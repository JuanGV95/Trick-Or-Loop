
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private EnemyManager enemyManager;

    [Header("Escaneo de cabeza")]
    [SerializeField] private float scanAngle = 45f;
    [SerializeField] private float scanSpeed = 2f;

    [Header("Raycast")]
    [SerializeField] private float detectionDistance = 15f;
    //Detecta todas las capas de layers disponibles
    [SerializeField] private LayerMask detectionLayers = ~0;

    private Quaternion initialRotation;
    private bool isScanning = true;

    private void Awake()
    {
        // Guarda la rotación inicial de la cabeza
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        // Evita ejecutar el escaneo y el raycast cuando estan desactivados
        if (!isScanning)
            return;

        ScanHead();
        DetectPlayer();
    }

    private void ScanHead()
    {
        // Oscila suavemente entre -scanAngle y +scanAngle 
        float angle = Mathf.Sin(Time.time * scanSpeed) * scanAngle;
        // Aplica el giro relativo a la rotacion inicial
        transform.localRotation = initialRotation * Quaternion.Euler(0f, angle, 0f);
    }

    private void DetectPlayer()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin,direction,out RaycastHit hit,detectionDistance,detectionLayers,QueryTriggerInteraction.Ignore))
        {
            Transform player = hit.collider.transform;

            if (player.CompareTag("Player"))
            {
                enemyManager.StartChase(player);
            }
        }
    }

    public void SetScanning(bool scanning)
    {
        isScanning = scanning;

        if (isScanning)
        {
            initialRotation = transform.localRotation;
        }
    }

    private void OnDrawGizmosSelected()
    {
        //esta funcion borrar antes del build final 
        Gizmos.color = Color.red;

        Gizmos.DrawRay(
            transform.position,
            transform.forward * detectionDistance
        );
    }
}
