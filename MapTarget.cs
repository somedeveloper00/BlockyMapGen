using UnityEngine;

namespace BlockyMapGen {
    public class MapTarget : MonoBehaviour {
        public Vector3 viewArea;

        public bool IsInsideTargetView(Vector3 position) {
            var pos = transform.position;
            return position.x >= pos.x - viewArea.x / 2f &&
                   position.x <= pos.x + viewArea.x / 2f &&
                   position.y >= pos.y - viewArea.y / 2f &&
                   position.y <= pos.y + viewArea.y / 2f &&
                   position.z >= pos.z - viewArea.z / 2f &&
                   position.z <= pos.z + viewArea.z / 2f;
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube( transform.position, viewArea );
        }
    }
}