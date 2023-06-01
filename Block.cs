using AnimFlex.Sequencer;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockyMapGen {
    public class Block : MonoBehaviour {
        public Bounds bounds;
        [SerializeField] SequenceAnim activateSeq, deactivateSeq;
        
        public bool TargetInside { get; private set; }
        public bool TargetPassed { get; private set; }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            Handles.color = Color.blue;
            Handles.DrawWireCube( transform.TransformPoint( bounds.center ), bounds.size );
        }
#endif

        public bool Tick(MapTarget mapTarget) {
            if (TargetPassed) return false;
            if (TargetInside) {
                if (!new Bounds( transform.TransformPoint( bounds.center ), bounds.size ).Contains( mapTarget.GetPoint() )) {
                    TargetPassed = true;
                    activateSeq.StopSequence();
                    deactivateSeq.PlaySequence();
                    Debug.Log( $"block {name} deactivated".Color( Color.red ), this );
                }
                return false;
            }

            if (new Bounds( transform.TransformPoint( bounds.center ), bounds.size ).Contains( mapTarget.GetPoint() )) {
                TargetInside = true;
                activateSeq.PlaySequence();
                Debug.Log( $"block {name} activated".Color( Color.green ), this );
                return true;
            }

            return false;
        }
    }
}