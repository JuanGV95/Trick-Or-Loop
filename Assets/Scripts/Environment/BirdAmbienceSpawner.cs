using System.Collections;
using UnityEngine;

public sealed class BirdAmbienceSpawner : MonoBehaviour
{
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private float spawnRadius = 28f;
    [SerializeField] private float altitude = 9f;
    [SerializeField] private float initialDelay = 6f;
    [SerializeField] private Vector2 spawnIntervalSeconds = new Vector2(22f, 38f);
    [SerializeField] private Vector2Int birdsPerWave = new Vector2Int(2, 4);
    [SerializeField] private int maxActiveBirds = 10;
    [SerializeField] private Vector2 flightSpeed = new Vector2(7f, 10f);

    private int activeBirds;

    private void Start()
    {
        if (ambienceSource != null && ambienceSource.clip != null && !ambienceSource.isPlaying)
            ambienceSource.Play();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialDelay);
        while (true)
        {
            SpawnWave();
            yield return new WaitForSeconds(Random.Range(spawnIntervalSeconds.x, spawnIntervalSeconds.y));
        }
    }

    private void SpawnWave()
    {
        if (birdPrefab == null || activeBirds >= maxActiveBirds) return;
        int count = Random.Range(birdsPerWave.x, birdsPerWave.y + 1);
        count = Mathf.Min(count, maxActiveBirds - activeBirds);
        for (int i = 0; i < count; i++)
        {
            Vector2 heading = Random.insideUnitCircle.normalized;
            if (heading.sqrMagnitude < 0.1f) heading = Vector2.right;
            Vector2 start = heading * Random.Range(spawnRadius * 0.65f, spawnRadius);
            Vector2 end = -heading * Random.Range(spawnRadius * 0.65f, spawnRadius);
            Vector3 spawnPosition = transform.position + new Vector3(start.x, altitude + Random.Range(-1f, 2f), start.y);
            Vector3 destination = transform.position + new Vector3(end.x, spawnPosition.y, end.y);
            GameObject bird = Instantiate(birdPrefab, spawnPosition, Quaternion.identity);
            activeBirds++;
            BirdFlyBy flight = bird.GetComponent<BirdFlyBy>();
            if (flight != null)
                flight.Initialize(destination, Random.Range(flightSpeed.x, flightSpeed.y), OnBirdFinished, transform.position, spawnRadius);
            else
            {
                activeBirds--;
                Destroy(bird);
            }
        }
    }

    private void OnBirdFinished()
    {
        activeBirds = Mathf.Max(0, activeBirds - 1);
    }
}
