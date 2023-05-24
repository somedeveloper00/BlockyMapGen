using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class MapGenerator : MonoBehaviour {
        [SerializeField] MapTarget target;
        [SerializeField] ChunkSpawn[] chunkSpawns;
        [SerializeField] Chunk startingChunk;
        [SerializeField] float updateInterval = 0.1f;

        [ShowInInspector, ReadOnly] readonly List<Chunk> _chunks = new();
        [ShowInInspector, ReadOnly] readonly List<Chunk> _endedChunks = new();
        float _lastUpdateTime = -1;
        
        [Button]
        public void ResetMap() {
            foreach (var chunk in GetComponentsInChildren<Chunk>())
                if (chunk != startingChunk) safeDestroy( chunk.gameObject );
            _chunks.Clear();
            _chunks.Add( startingChunk );
        }

        void Start() => ResetMap();

        [Button]
        void Update() {
            bool shouldUpdateChunkInstances = Time.time - _lastUpdateTime >= updateInterval;
#if UNITY_EDITOR
            shouldUpdateChunkInstances |= !Application.isPlaying; 
#endif
            if (shouldUpdateChunkInstances)
                if (updateChunks() | updateEndingChunks())
                    updateChunkNames();

        }

        bool updateChunks() {
            List<(Vector3 pos, Chunk.ConnectionType type)> npoint = new();
            for (var i = 0; i < _chunks.Count; i++) {
                if (_chunks[i].Tick( target, out var nextPoint )) {
                    npoint.Add( nextPoint.Value );
                    _endedChunks.Add( _chunks[i] );
                    _chunks.RemoveAt( i-- );
                }
            }

            foreach (var point in npoint) instantiateChunk( getRandomChunk(point.type), point.pos );
            return npoint.Count > 0;
        }

        bool updateEndingChunks() {
            bool c = false;
            for (var i = 0; i < _endedChunks.Count; i++) {
                _endedChunks[i].Tick( target, out _ );
                if (!target.IsInHotSpot( _endedChunks[i].WorldBounds )) {
                    c = true;
                    safeDestroy( _endedChunks[i].gameObject );
                    _endedChunks.RemoveAt( i-- );
                }
            }
            return c;
        }

        Chunk getRandomChunk(Chunk.ConnectionType connectionType) => chunkSpawns
            .Where( c => c.chunk.connectionType == connectionType )
            .Random( c => c.chance.Evaluate( Time.time ) ).chunk;

        void updateChunkNames() {
            for (int i = 0; i < _chunks.Count; i++) {
                _chunks[i].name = $"chunk {i}";
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


        Chunk instantiateChunk(Chunk chunk, Vector3 pos) {
            var c = Instantiate( chunk, pos, Quaternion.identity, transform );
            _chunks.Add( c );
            return c;
        }
    }

    [Serializable]
    public struct ChunkSpawn {
        public Chunk chunk;
        [PropertyTooltip("over seconds")]
        public AnimationCurve chance;
    }
}