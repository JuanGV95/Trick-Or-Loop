
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    public enum EnemyState
    {
        Scanning,
        Chasing,
        Returning,
        GameOver
    }

    [Header("Referencias")]
    [SerializeField] private Enemy enemyHead;
    [SerializeField] private Transform centerPosition;
    [SerializeField] private GameObject gameOverWindow;

    [Header("Movimiento")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float returnSpeed = 4f;
    [SerializeField] private float chaseDuration = 5f;

    private EnemyState currentState = EnemyState.Scanning;
    private Transform playerTarget;
    private float chaseTimer;

    // Permite consultar el estado actual desde otros scripts
    public EnemyState CurrentState => currentState;

    private void Start()
    {
        if (gameOverWindow != null)
            gameOverWindow.SetActive(false);

        // Activa el escaneo inicial de la cabeza.
        if (enemyHead != null)
            enemyHead.SetScanning(true);
    }

    private void Update()
    {
        // Ejecuta la logica correspondiente al estado actual
        switch (currentState)
        {
            // La cabeza gestiona el escaneo.
            case EnemyState.Scanning:
                break;

            case EnemyState.Chasing:
                UpdateChase();
                break;

            case EnemyState.Returning:
                ReturnToCenter();
                break;

            case EnemyState.GameOver:
                break;
        }
    }

    public void StartChase(Transform player)
    {
        // Solo permite iniciar la persecucion desde el estado de escaneo
        if (currentState != EnemyState.Scanning)
            return;

        playerTarget = player;
        chaseTimer = chaseDuration;
        currentState = EnemyState.Chasing;

        // Detiene el escaneo mientras el enemigo persigue al jugador
        if (enemyHead != null)
            enemyHead.SetScanning(false);
    }

    private void UpdateChase()
    {
        chaseTimer -= Time.deltaTime;

        // Mantiene la posicion del enemigo durante la persecucion.
        Vector3 targetPosition = playerTarget.position;

        // El enemigo se mueve horizontalmente
        targetPosition.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position,targetPosition,chaseSpeed * Time.deltaTime);

        // Inicia el regreso cuando termina el tiempo de persecucion.
        if (chaseTimer <= 0f)
        {
            BeginReturn();
        }
    }

    private void BeginReturn()
    {
        playerTarget = null;
        currentState = EnemyState.Returning;
    }

private void ReturnToCenter()
    {
        // Mueve al enemigo hacia su posicion inicial.
        transform.position = Vector3.MoveTowards(transform.position,centerPosition.position,returnSpeed * Time.deltaTime);

        // Reactiva el escaneo al llegar al centro.
        if (transform.position == centerPosition.position)
        {
            currentState = EnemyState.Scanning;

            if (enemyHead != null)
                enemyHead.SetScanning(true);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        CheckPlayerCollision(collision.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPlayerCollision(other.transform);
    }

    private void CheckPlayerCollision(Transform other)
    {
        Transform player = other.CompareTag("Player")
            ? other
            : other.root.CompareTag("Player")
                ? other.root
                : other.GetComponentInParent<CharacterController>()?.transform;

        if (player != null && player.CompareTag("Player"))
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (currentState == EnemyState.GameOver)
            return;

        currentState = EnemyState.GameOver;

        if (enemyHead != null)
            enemyHead.SetScanning(false);

        SceneManager.LoadScene("GameOver");

    }
}
