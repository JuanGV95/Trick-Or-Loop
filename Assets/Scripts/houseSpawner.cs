
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

    // Guarda las casas creadas para eliminarlas al generar una nueva ronda
    private readonly List<GameObject> spawnedHouses = new List<GameObject>();

    private void Start()
    {
        GenerateHouses();
    }

    public void GenerateHouses()
    {
        // Elimina las casas de la ronda anterior
        foreach (GameObject house in spawnedHouses)
        {
            if (house != null)
                Destroy(house);
        }

        spawnedHouses.Clear();

        // Define la cantidad aleatoria de casas rojas y verdes
        int redCount = Random.Range(2, 5); 
        int greenCount = Random.Range(3, 8 - redCount);

        // Completa las ocho casas con las casas blancas restantes
        int whiteCount = 8 - redCount - greenCount;

        // Crea una lista con los prefabs segun las cantidades elegidas
        List<GameObject> housePool = new List<GameObject>();

        for (int i = 0; i < greenCount; i++)
            housePool.Add(greenHousePrefab);

        for (int i = 0; i < redCount; i++)
            housePool.Add(redHousePrefab);

        for (int i = 0; i < whiteCount; i++)
            housePool.Add(whiteHousePrefab);

        // Mezcla aleatoriamente las casas para distribuir los colores
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

        
    }
}
