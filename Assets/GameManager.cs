
using TMPro;
using UnityEngine;
using System.Collections;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int pointsToWin = 3;

    [Header("References")]
    [SerializeField] private HouseSpawner houseGenerator;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform player;

    [Header("Pista")]
    [SerializeField] private TextMeshProUGUI textoPista;

    public int CandyCount { get; private set; }
    public bool IsGameWon { get; private set; }
    public bool IsRestarting { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CollectCandy()
    {
        if (IsGameWon || IsRestarting)
            return;

        CandyCount++;

        Debug.Log($"Dulces: {CandyCount}/{pointsToWin}");

        if (CandyCount >= pointsToWin)
        {
            IsGameWon = true;
            Debug.Log("¡Has ganado la partida!");
        }
    }

public void RestartRound()
{
    if (IsRestarting || IsGameWon)
        return;

    IsRestarting = true;
    CandyCount = 0;

    if (houseGenerator == null)
    {
        Debug.LogError("HouseGenerator no está asignado.");
        IsRestarting = false;
        return;
    }

    ResetPlayerPosition();

    houseGenerator.GenerateHouses();

    IsRestarting = false;

    Debug.Log("Nueva ronda: casas regeneradas y jugador reiniciado.");
}

    private void ResetPlayerPosition()
    {
        if (player == null)
        {
            Debug.LogError("Player no está asignado en el GameManager.");
            return;
        }

       
        if (playerController != null)
            playerController.enabled = false;

        player.position = Vector3.zero;

        
        if (playerController != null)
            playerController.enabled = true;

       
    }

    public void Pista()
    {
        if (textoPista == null) return;

        StopAllCoroutines();
        StartCoroutine(MostrarPista());
    }

    private IEnumerator MostrarPista()
    {
        textoPista.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        textoPista.gameObject.SetActive(false);
    }

    public void ResetGame()
    {
        CandyCount = 0;
        IsGameWon = false;
        IsRestarting = false;

        if (houseGenerator != null)
            houseGenerator.GenerateHouses();
    }
}
