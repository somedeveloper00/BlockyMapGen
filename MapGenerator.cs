using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class MapGenerator : MonoBehaviour {
        [SerializeField] MapTarget target;
        [SerializeField] int maxUnpassedBlocks = 5;
        [SerializeField] BlockSpawn[] spawnableBlockPrefabs;
        [SerializeField] BlockSpawn[] startingBlockPrefabs;
        [SerializeField] int updateDelayFrames = 5;

        [ShowInInspector]
        readonly List<Block> _blocks = new();

        int _lastUpdateFrame = -100;

        [Button]
        public void ResetMap() {
            _blocks.ForEach( b => safeDestroy( b ? b.gameObject : null ) );
            _blocks.Clear();
            foreach (var b in GetComponentsInChildren<Block>()) safeDestroy( b.gameObject );
            _blocks.Add( Instantiate( startingBlockPrefabs.Random( s => s.chance ).prefab, target.GetPoint(), Quaternion.identity, transform ) );
        }

        void Start() {
            ResetMap();
            flushOpeningConnections();
        }

        [Button]
        void Update() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                updateBlocks();
                flushOpeningConnections();
                return;
            }
#endif
            if (Time.frameCount - _lastUpdateFrame > updateDelayFrames) {
                _lastUpdateFrame = Time.frameCount;
                if (updateBlocks()) {
                    flushOpeningConnections();
                    destroyIsolatedBlocks();
                    updateBlockNames();
                }
            }
        }

        bool updateBlocks() {
            var point = target.GetPoint();
            var changed = false;

            int notPassedCount = _blocks.Count( b => !b.passed );
                
            for (int i = 0; i < _blocks.Count; i++) {
                _blocks[i].Tick( target );
                if ( !_blocks[i] || !_blocks[i].enabled) {
                    _blocks.RemoveAt( i-- );
                    continue;
                }
                
                if (notPassedCount >= maxUnpassedBlocks) continue;
                
                // adding new blocks
                foreach (var opening in _blocks[i].openings) {
                    if (opening.connectedOpening) continue;
                    if (Vector3.Dot( opening.transform.position - point, target.GetDirection() ) < 0) continue;
                    var nBlock = spawnBlock( opening.transform.position, opening );
                    if (nBlock) {
                        _blocks.Add( nBlock );
                        notPassedCount++;
                        changed = true;
                    }
                    
                    if (notPassedCount >= maxUnpassedBlocks) continue;
                }
            }
            
            return changed;
        }

        bool destroyIsolatedBlocks() {
            var visited = new List<Block>();
            var check = new List<Block>();
            check.Add( _blocks.Find( b => b.ContainsTarget ) );
            while (check.Count > 0) {
                if (check[0] is null) break;
                visited.Add( check[0] );
                foreach (var opening in check[0].openings) {
                    if (!opening.connectedOpening) continue;
                    if (!visited.Contains( opening.connectedOpening.fromBlock ) ) {
                        check.Add( opening.connectedOpening.fromBlock );
                    }
                }
                check.RemoveAt( 0 );
            }

            for (var i = 0; i < _blocks.Count; i++) {
                if (!visited.Contains( _blocks[i] )) {
                    safeDestroy( _blocks[i].gameObject );
                    _blocks.RemoveAt( i-- );
                }
            }

            return visited.Count == 0;
        }
        
        [Button]
        void flushOpeningConnections() {
            var openings = _blocks.SelectMany( b => b.openings ).ToList();
            openings.ForEach( o => o.connectedOpening = null );
            for (var i = 0; i < openings.Count - 1; i++) {
                for (var j = i + 1; j < openings.Count; j++) {
                    if (!openings[i].CanConnectTo(openings[j])) continue;
                    var dist = (openings[i].transform.position - openings[j].transform.position).sqrMagnitude;
                    if (dist > 0.1f) continue;
                    openings[i].connectedOpening = openings[j];
                    openings[j].connectedOpening = openings[i];
                    break;
                }
                openings[i].UpdateView();
            }
        }

        void updateBlockNames() {
            for (int i = 0; i < _blocks.Count; i++) _blocks[i].name = $"Block {i}";
        }

        void safeDestroy(GameObject go) {
            if (!go) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate( go );
            else 
#endif
                Destroy( go );
        }

        Block spawnBlock(Vector3 position, Opening opening) {
            List<(BlockSpawn blockSpawn, List<Vector3> pos)> availableSpawns = new();

            foreach (var blockSpawn in spawnableBlockPrefabs) {
                (BlockSpawn blockSpawn, List<Vector3> pos) tuple = (blockSpawn, new List<Vector3>());
                foreach (var newOpening in blockSpawn.prefab.openings) {
                    if (!opening.CanConnectTo( newOpening )) continue;
                    var pos = position - newOpening.GetOffsetPosition();
                    if (_blocks.Any(b => b.WillIntersectWithPrefabAtPos( blockSpawn.prefab, pos ) ))
                        continue;
                    tuple.pos.Add( pos );
                }
                if (tuple.pos.Count > 0) availableSpawns.Add( tuple );
            }

            if (availableSpawns.Count == 0) return null;

            var selectedSpawn = availableSpawns.Random( asp => asp.blockSpawn.chance );
            var selectedBlock = selectedSpawn.blockSpawn.prefab;
            var selectedPos = selectedSpawn.pos.Random();
            
            var newBlock = Instantiate( selectedBlock, selectedPos, Quaternion.identity, transform );
            return newBlock;
        }
        
        [Serializable]
        public struct BlockSpawn {
            public Block prefab;
            [Min(0)] public float chance;
        }
    }
}