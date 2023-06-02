using UnityEngine;

namespace BlockyMapGen {
    public static class ComponentExtensions {
        public static GameObject GetRootGameObject(this GameObject gameObject) {
            var root = gameObject.transform;
            while (root.parent != null) root = root.parent;
            return root.gameObject;
        }
    }
}