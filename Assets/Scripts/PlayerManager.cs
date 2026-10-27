
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer = ~0;

    [Header("Press Settings")]
    [SerializeField] private int requiredPresses = 10;
    [SerializeField] private float pressInterval = 0.5f;

    private int currentPresses;
    private InteractableItem currentInteractable;
    private Coroutine resetCoroutine;

    public void RegisterInteractionPress()
    {
        InteractableItem nearestItem = FindNearestInteractable();

        if (nearestItem == null)
        {
            ResetInteraction();
            return;
        }

        // Si se cambia de casa, reiniciar el progreso anterior.
        if (currentInteractable != nearestItem)
        {
            ResetInteraction();

            currentInteractable = nearestItem;

            // Identificar la casa desde el primer toque.
            Debug.Log(
                "Casa identificada: " +
                currentInteractable.GetType().Name
            );
        }

        currentPresses++;

        Debug.Log(
            $"Interacción con {currentInteractable.GetType().Name}: " +
            $"{currentPresses}/{requiredPresses}"
        );

        // Reiniciar el temporizador en cada pulsación.
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(ResetAfterDelay());

        if (currentPresses >= requiredPresses)
        {
            CompleteInteraction();
        }
    }

    private InteractableItem FindNearestInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            interactionLayer
        );

        InteractableItem nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            // Obtener el componente específico de la casa
            // desde el collider o cualquiera de sus padres.
            InteractableItem item =
                col.GetComponentInParent<InteractableItem>();

            if (item == null || item.HasInteracted)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                item.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    private void CompleteInteraction()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        InteractableItem completedItem = currentInteractable;

        // Limpiar el estado antes de ejecutar el efecto.
        currentPresses = 0;
        currentInteractable = null;

        if (completedItem == null)
            return;

        bool success = completedItem.TryCollect();

        if (success)
        {
            Debug.Log(
                "Interacción completada: " +
                completedItem.GetType().Name
            );
        }
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(pressInterval);

        currentPresses = 0;
        currentInteractable = null;
        resetCoroutine = null;

        Debug.Log("Tiempo agotado. Progreso reiniciado.");
    }

    private void ResetInteraction()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        currentPresses = 0;
        currentInteractable = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRadius
        );
    }
}
