using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class Opening : MonoBehaviour {
        public Block fromBlock;
        public bool open;
        public GameObject openContainer, closeContainer;
        [SerializeField] OpeningType type;
        [SerializeField] int specialId;
        [SerializeField] List<int> blockIds = new();

        [NonSerialized, ShowInInspector, ReadOnly]
        public Opening connectedOpening;

        void Start() => updateView();

        void updateView() {
            if (openContainer)  openContainer.SetActive( open );
            if (closeContainer) closeContainer.SetActive( !open );
        }

        public bool CanConnectTo(Opening other) => other.type == type && !blockIds.Contains( other.specialId );

        public Vector3 GetOffsetPosition() => transform.position - fromBlock.transform.position;

        public enum OpeningType {
            AlongX, AlongY, AlongZ
        }
    }
}