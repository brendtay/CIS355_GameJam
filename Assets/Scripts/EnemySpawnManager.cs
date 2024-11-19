using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    // Reference to the GameManager
    private GameManager gameManager;

    // Class to define a wave
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject[] enemies;       // Array of enemy prefabs for this wave
        public Transform[] spawnPoints;    // Array of spawn points for this wave
        public float delayBeforeWave = 2f; // Delay before spawning this wave
        public int repeatTimes;            // Amount of times the wave repeats 
        public bool repeatable = false;    // If true, this wave can be repeated
    }

    public Wave[] waves;                  // Array of waves for the level
    private int currentWaveIndex = 0;
    private bool isSpawningWave = false;


    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

       

        // Start the first wave
        if (waves.Length > 0)
        {
            StartCoroutine(SpawnNextWave());
        }
        else
        {
            Debug.LogWarning("No waves configured in EnemySpawnManager.");
        }
    }

    void Update()
    {
        // Check if we are not currently spawning a wave
        if (!isSpawningWave)
        {
            // Check if there are any enemies left
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                // All enemies defeated
                if (currentWaveIndex < waves.Length)
                {
                    // Start next wave
                    StartCoroutine(SpawnNextWave());
                }
                else
                {
                    // All waves completed
                    gameManager.levelComplete = true;

                    // Trigger chat messages from previous scripts                   
                     gameManager.levelComplete = true; 
                    
                }
            }
        }
    }

    private IEnumerator SpawnNextWave()
    {
        isSpawningWave = true;

        if (currentWaveIndex < waves.Length)
        {
            Wave currentWave = waves[currentWaveIndex];

            // Delay before starting the wave
            yield return new WaitForSeconds(currentWave.delayBeforeWave);

            // Spawn enemies for this wave
            for (int i = 0; i < currentWave.enemies.Length && i < currentWave.spawnPoints.Length; i++)
            {
                Instantiate(currentWave.enemies[i], currentWave.spawnPoints[i].position, currentWave.spawnPoints[i].rotation);
            }

            // Wait for enemies to be defeated before proceeding
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

            // If the wave is repeatable, do not increment the wave index
            if (currentWave.repeatable && currentWave.repeatTimes > 0)
            {
                currentWave.repeatTimes--;
            }
            else
            {
                currentWaveIndex++;
            }

            isSpawningWave = false;
        }
        else
        {
            // All waves have been spawned
            isSpawningWave = false;
        }
    }
}
