using System;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class BlockyMapGenTime : MonoBehaviour {
        [SerializeField] bool useNormalTime = true;
        
        [ShowInInspector, NonSerialized] public float Time;
        [ShowInInspector, NonSerialized] public float DeltaTime;

        void Update() {
            if (useNormalTime) {
                DeltaTime = UnityEngine.Time.deltaTime;
                Time += DeltaTime;
            }
        }

        public void Reset() => Time = DeltaTime = 0;
    }
}