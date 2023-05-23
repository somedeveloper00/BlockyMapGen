using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class MapGenerator : MonoBehaviour {
        [SerializeField] MapTarget target;
        [SerializeField] BlockSpawn[] spawnableBlockPrefabs;
        [SerializeField] BlockSpawn[] startingBlockPrefabs;
        [SerializeField] int updateDelayFrames = 5;

        [ShowInInspector]
        readonly List<Block> _blocks = new();

        int _lastUpdateFrame = -100;

        [Button]
        public void ResetMap() {
            foreach (var block in GetComponentsInChildren<Block>()) safeDestroy( block.gameObject );
            _blocks.Clear();
            _blocks.Add( Instantiate( startingBlockPrefabs.Random( s => s.chance ).prefab, transform ) );
        }

        void Start() {
            _blocks.Clear();
            _blocks.AddRange( GetComponentsInChildren<Block>() );
            flushOpeningConnections();
            updateBlockNames();
        }

        [Button]
        void Update() {
            if (Time.frameCount - _lastUpdateFrame > updateDelayFrames) {
                _lastUpdateFrame = Time.frameCount;
                if (updateBlocks()) {
                    flushOpeningConnections();
                    updateBlockNames();
                }
            }
        }

        bool updateBlocks() {
            bool changed = false;
            for (int i = 0; i < _blocks.Count; i++) {
                var block = _blocks[i];
                
                bool anyOpeningInside = false;
                
                // check for adding neighbors
                for (int j = 0; j < block.openings.Length; j++) {
                    var opening = block.openings[j];
                    
                    var inside = target.IsInsideTargetView( opening.transform.position );
                    if (!inside) continue;
                    anyOpeningInside = true;
                    
                    if (opening.connectedOpening) continue;
                    
                    // spawn new block here
                    var nblock = spawnBlock( opening.transform.position, opening );
                    if (nblock) {
                        _blocks.Add( nblock );
                        nblock.name = "block " + i + "-" + j;
                        changed = true;
                    }
                }
                
                // check for deletion
                if (!anyOpeningInside) {
                    safeDestroy( _blocks[i].gameObject );
                    _blocks.RemoveAt( i-- );
                    changed = true;
                }
            }

            return changed;
        }

        [Button]
        void flushOpeningConnections() {
            var openings = _blocks.SelectMany( b => b.openings ).ToList();
            for (var i = 0; i < openings.Count; i++) {
                bool connectionFound = false;
                for (var j = i + 1; j < openings.Count; j++) {
                    if (!openings[i].CanConnectTo(openings[j])) continue;
                    var dist = (openings[i].transform.position - openings[j].transform.position).sqrMagnitude;
                    if (dist > 0.1f) continue;
                    openings[i].connectedOpening = openings[j];
                    openings[j].connectedOpening = openings[i];
                    connectionFound = true;
                    break;
                }

                if (!connectionFound) {
                    openings[i].connectedOpening = null;
                }
            }
        }

        void updateBlockNames() {
            for (int i = 0; i < _blocks.Count; i++) {
                _blocks[i].name = $"block {i}";
            }
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