using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace SpellMender_03
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] GameObject objectToSpawn;
        [SerializeField] float kinematicFallSpeed;
        [SerializeField] float kinematicXSpeed;

        [SerializeField] int objectsPerSpawn;
        [SerializeField] float timeBetweenSpawns;
        [SerializeField] float timeBetweenObjects;
        [SerializeField] float despawnTime;
        [SerializeField] float initialSpawnDelay;

        List<GameObject> spawns = new List<GameObject>();

        float spawnTimer, dropTimer, despawnTimer;
        int dropCounter;

        // Start is called before the first frame update
        void Start()
        {
            spawnTimer = initialSpawnDelay;
            dropCounter = objectsPerSpawn;
        }

        // Update is called once per frame
        void Update()
        {
            Spawn();
        }

        void Spawn()
        {
            // Countdown timers
            if (spawnTimer > 0) spawnTimer -= Time.deltaTime;
            if (dropTimer > 0) dropTimer -= Time.deltaTime;
            if (despawnTimer > 0) despawnTimer -= Time.deltaTime;

            // Spawn objects
            if (spawnTimer <= 0 && dropTimer <= 0 && dropCounter > 0)
            {
                GameObject instance = Instantiate(objectToSpawn, transform);

                if (instance.TryGetComponent(out Rigidbody2D rb) && rb.isKinematic) rb.velocity = new Vector2(kinematicXSpeed, -kinematicFallSpeed);

                if (objectsPerSpawn > 1) dropTimer = timeBetweenObjects;

                spawns.Add(instance);

                Destroy(instance, despawnTime);

                dropCounter--;
            }

            // Reset for next spawn
            if (dropCounter <= 0)
            {
                spawnTimer = timeBetweenSpawns;
                dropCounter = objectsPerSpawn;
            }

        }

    }
}
