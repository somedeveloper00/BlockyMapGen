using AnimFlex.Sequencer;
using UnityEditor;
using UnityEngine;

namespace BlockyMapGen {
    public class Block : MonoBehaviour {
        public Bounds bounds;
        [SerializeField] SequenceAnim activateSeq, deactivateSeq;
        
        public bool TargetInside { get; private set; }
        public bool TargetPassed { get; private set; }

        void OnDrawGizmos() {
            Handles.color = Color.blue;
            Handles.DrawWireCube( transform.TransformPoint( bounds.center ), bounds.size );
        }

        public void Tick(MapTarget mapTarget) {
            if (TargetPassed) return;
            if (TargetInside) {
                if (!new Bounds( transform.TransformPoint( bounds.center ), bounds.size ).Contains( mapTarget.GetPoint() )) {
                    TargetPassed = true;
                    activateSeq.StopSequence();
                    deactivateSeq.PlaySequence();
                    Debug.Log( $"block {name} deactivated".Color( Color.red ), this );
                }
                return;
            }

            if (new Bounds( transform.TransformPoint( bounds.center ), bounds.size ).Contains( mapTarget.GetPoint() )) {
                TargetInside = true;
                activateSeq.PlaySequence();
                Debug.Log( $"block {name} activated".Color( Color.green ), this );
            }
        }
    }
}