using System;
using System.Linq;
using TriInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockyMapGen {
    public class Chunk : MonoBehaviour {
        public ConnectionType connectionType;
        [SerializeField, ReadOnly] Block[] blocks = Array.Empty<Block>();
        [SerializeField] EndPoint[] endPoints = Array.Empty<EndPoint>();
        [SerializeField] Bounds bounds;

        public Bounds WorldBounds { get; private set; }
        public event Action<Block> onMapTargetReachBlock;
        bool _ended;

#if UNITY_EDITOR
        void OnValidate() {
            var cblocks = GetComponentsInChildren<Block>();
            if (!cblocks.SequenceEqual( blocks )) blocks = cblocks;
        }
#endif

#if UNITY_EDITOR
        void OnDrawGizmos() {
            foreach (var endPoint in endPoints) {
                var np = transform.TransformPoint( endPoint.nextSpawnPoint );
                var cp = transform.TransformPoint( endPoint.condition.center );
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube( cp, endPoint.condition.size );
                Gizmos.DrawLine( cp, np );
                Gizmos.DrawSphere( np, 0.5f );
                Handles.Label( np + transform.up * 2, endPoint.connectionType.ToString() );
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube( transform.TransformPoint( bounds.center ), bounds.size );
            Gizmos.DrawCube( transform.position, new Vector3( 0.25f, bounds.size.y, 0.25f ) );
        }
#endif

        void OnEnable() {
            for (var i = 0; i < endPoints.Length; i++)
                endPoints[i].worldPosCondition = new Bounds( transform.TransformPoint( endPoints[i].condition.center ), endPoints[i].condition.size );
            WorldBounds = new Bounds( transform.TransformPoint( bounds.center ), bounds.size );
        }

        [Serializable]
        public struct EndPoint {
            public Bounds condition;
            [NonSerialized] public Bounds worldPosCondition;
            public Vector3 nextSpawnPoint;
            public ConnectionType connectionType;
        }
        
        public enum ConnectionType {
            AlongX, AlongZ
        }
        
        public bool Tick(MapTarget mapTarget, out (Vector3 pos, ConnectionType type)? nextPoint) {
            foreach (var block in blocks)
                if (block.Tick( mapTarget ))
                    onMapTargetReachBlock?.Invoke( block );

            if (_ended) {
                nextPoint = null;
                return false;
            }

            // end check
            foreach (var endPoint in endPoints)
                if (endPoint.worldPosCondition.Contains( mapTarget.GetPoint() )) { 
                    nextPoint = new(transform.TransformPoint( endPoint.nextSpawnPoint ), endPoint.connectionType);
                    _ended = true;
                    return true;
                }

            nextPoint = null;
            return false;
        }
    }
}