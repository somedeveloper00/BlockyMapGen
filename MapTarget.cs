using UnityEngine;

namespace BlockyMapGen {
    public class MapTarget : MonoBehaviour {
        [SerializeField] Vector3 hotSpot;

        void OnDrawGizmos() {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube( transform.position, hotSpot );
        }

        public Vector3 GetDirection() => transform.forward;
        public Vector3 GetPoint() => transform.position;

        public bool IsInHotSpot(Vector3 point) => new Bounds( transform.position, hotSpot ).Contains( point );
        public bool IsInHotSpot(Bounds bounds) => new Bounds( transform.position, hotSpot ).Intersects( bounds );
    }
}