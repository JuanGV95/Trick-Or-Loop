
using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    private bool hasInteracted = false;

    public bool HasInteracted => hasInteracted;

    // Se ejecuta cuando el jugador completa la interacci
    public bool TryCollect()
    {
        if (hasInteracted)
        {
            return false;
        }

        // Bloquear antes de aplicar el efecto
        hasInteracted = true;

        ApplyEffect();

        return true;
    }

   
    protected abstract void ApplyEffect();
}
