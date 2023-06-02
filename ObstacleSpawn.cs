using System;
using TriInspector;
using UnityEngine;

namespace BlockyMapGen {
    public class ObstacleSpawn : MonoBehaviour {

        [SerializeField] SpawnItem[] spawnItems;
        [SerializeField] bool spawnOnEnable = true;
        [SerializeField] float dontSpawnUntilTime = 1;

        BlockyMapGenTime _time;

        void OnEnable() {
            _time = gameObject.GetRootGameObject().GetComponent<BlockyMapGenTime>();
            if (_time == null) throw new Exception("BlockyMapGenTime not found");
            if (spawnOnEnable && _time.Time > dontSpawnUntilTime) SpawnRandom( _time.Time );
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