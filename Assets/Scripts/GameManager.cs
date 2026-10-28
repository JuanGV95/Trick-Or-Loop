
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    //instancia para poder acceder a el script globalmente (singleton)
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int pointsToWin = 3;

    [Header("References")]
    [SerializeField] private HouseSpawner houseGenerator;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Transform player;

    [Header("Pista")]
    [SerializeField] private TextMeshProUGUI textoPista;

    [SerializeField] private UIManager uiManager;

    //poder acceder a la variable globalmente
    public int CandyCount { get; private set; }
    public bool IsGameWon { get; private set; }
    public bool IsRestarting { get; private set; }

    private void Awake()
    {
        // Evita tener mas de un GameManager activo
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

        if (uiManager != null)
            uiManager.UpdateCandyUI(CandyCount);

        if (CandyCount >= pointsToWin)
        {
            IsGameWon = true;
            SceneManager.LoadScene("Victory");
        }
    }

public void RestartRound()
{
    if (IsRestarting || IsGameWon)
        return;

    IsRestarting = true;
    CandyCount = 0;

        uiManager.ShowJumpscare();

        // Reinicia al jugador y genera una nueva ronda.
        ResetPlayerPosition();

    houseGenerator.GenerateHouses();

    IsRestarting = false;

    
}

    private void ResetPlayerPosition()
    {

        // Desactiva el controlador para evitar conflictos al cambiar la posicion
        if (playerController != null)
            playerController.enabled = false;

        player.position = new Vector3(-30f,-2.7f,0); //modificar el vector para cualquier posicion

        // Reactiva el controlador para continuar el movimiento
        if (playerController != null)
            playerController.enabled = true;

       
    }

    public void Pista()
    {
     

        StopAllCoroutines();
        StartCoroutine(MostrarPista());
    }

    private IEnumerator MostrarPista()
    {
        // Muestra la pista durante tres segundos
        textoPista.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        textoPista.gameObject.SetActive(false);
    }

    public void ResetGame()
    {
        // Restablece el progreso y el estado de la partida
        CandyCount = 0;
        IsGameWon = false;
        IsRestarting = false;

        if (houseGenerator != null)
            houseGenerator.GenerateHouses();
    }
}
