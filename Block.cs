using System;
using AnimFlex.Sequencer;
using UnityEditor;
using UnityEngine;

namespace BlockyMapGen {
    public class Block : MonoBehaviour {
        public Opening[] openings;
        public Bounds bounds;
        [SerializeField] SequenceAnim activateSeq, deactivateSeq;

        [NonSerialized] public bool passed = false;
        bool _active = false;

        public bool ContainsTarget { get; private set; } = false;

        void OnDrawGizmos() {
            Handles.color = Color.blue;
            Handles.DrawWireCube( transform.TransformPoint( bounds.center ), bounds.size );
        }

        public void Tick(MapTarget mapTarget) {
            if (activateSeq.sequence.IsPlaying() || deactivateSeq.sequence.IsPlaying()) return;
            
            if (!passed) {
                ContainsTarget = containsPoint( mapTarget.GetPoint() );
                if (_active == ContainsTarget) return;
                if (ContainsTarget) {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                        activateSeq.PlaySequence();
#else
                    activateSeq.PlaySequence();
#endif
                }
                else {
                    ContainsTarget = false;
#if UNITY_EDITOR
                    if (Application.isPlaying) {
                        deactivateSeq.PlaySequence();
                        deactivateSeq.sequence.onComplete += () => passed = true;
                    }
                    else {
                        passed = true;
                    }
#else
                    deactivateSeq.PlaySequence();
                    deactivateSeq.sequence.onComplete += () => passed = true;
#endif
                }
                _active = ContainsTarget;
            }
            else {
                var cbound = new Bounds( transform.TransformPoint( bounds.center ), bounds.size );
                if (!mapTarget.IsInHotSpot( cbound )) {
                    Destroy( gameObject );
                    Debug.Log( $"destroyed {name}".Color( Color.red ) );
                }
            }
        }

        bool containsPoint(Vector3 point) => bounds.Contains( transform.InverseTransformPoint( point ) );


        public bool WillIntersectWithPrefabAtPos(Block prefab, Vector3 pos) {
            var bound1 = new Bounds( transform.TransformPoint( bounds.center ), bounds.size );
            var bound2 = new Bounds( prefab.bounds.center + pos, prefab.bounds.size );
            return bound1.Intersects( bound2 ) || bound1 == bound2;
        }
    }
}