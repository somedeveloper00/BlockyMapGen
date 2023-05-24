using System;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class ObstacleSpawn : MonoBehaviour {

        [SerializeField] SpawnItem[] spawnItems;
        [SerializeField] bool spawnOnEnable = true;
        [SerializeField] float dontSpawnUntilTime = 1;

        void OnEnable() {
            if (spawnOnEnable && Time.time > dontSpawnUntilTime) SpawnRandom( Time.time );
        }

        public void SpawnRandom(float time) {
            spawnItems.Random( s => s.chanceOverTime.Evaluate( time ) ).obstacleObject.gameObject.SetActive( true );
        }
        
        [Serializable]
        public struct SpawnItem {
            public Obstacle obstacleObject;
            [PropertyTooltip("Time is in seconds")]
            public AnimationCurve chanceOverTime;
        }
    }
}