
using System.Collections.Generic;
using UnityEngine;

public class HouseSpawner: MonoBehaviour
{
    [Header("Spawner positions (exactly 8)")]
    [SerializeField] private Transform[] spawners = new Transform[8];

    [Header("House prefabs")]
    [SerializeField] private GameObject greenHousePrefab;
    [SerializeField] private GameObject redHousePrefab;
    [SerializeField] private GameObject whiteHousePrefab;

    private readonly List<GameObject> spawnedHouses = new List<GameObject>();

    private void Start()
    {
        GenerateHouses();
    }

    public void GenerateHouses()
    {
        
        foreach (GameObject house in spawnedHouses)
        {
            if (house != null)
                Destroy(house);
        }

        spawnedHouses.Clear();

        if (spawners == null || spawners.Length != 8)
        {
            Debug.LogError("You must assign exactly 8 spawners.");
            return;
        }

        if (greenHousePrefab == null ||
            redHousePrefab == null ||
            whiteHousePrefab == null)
        {
            Debug.LogError("Assign all three house prefabs.");
            return;
        }

        foreach (Transform spawner in spawners)
        {
            if (spawner == null)
            {
                Debug.LogError("A spawner has not been assigned.");
                return;
            }
        }

        
        int redCount = Random.Range(2, 5); 

       
        int greenCount = Random.Range(3, 8 - redCount);

        
        int whiteCount = 8 - redCount - greenCount;

        List<GameObject> housePool = new List<GameObject>();

        for (int i = 0; i < greenCount; i++)
            housePool.Add(greenHousePrefab);

        for (int i = 0; i < redCount; i++)
            housePool.Add(redHousePrefab);

        for (int i = 0; i < whiteCount; i++)
            housePool.Add(whiteHousePrefab);

        
        for (int i = housePool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            GameObject temp = housePool[i];
            housePool[i] = housePool[j];
            housePool[j] = temp;
        }

       
        for (int i = 0; i < spawners.Length; i++)
        {
            Transform point = spawners[i];

            GameObject house = Instantiate(
                housePool[i],
                point.position,
                point.rotation
            );

            spawnedHouses.Add(house);
        }

        Debug.Log(
            $"Generation complete: {greenCount} green, " +
            $"{redCount} red, {whiteCount} white."
        );
    }
}
