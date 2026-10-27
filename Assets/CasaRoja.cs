
using UnityEngine;

public class CasaRoja : InteractableItem
{
    protected override void ApplyEffect()
    {
        
        GameManager.Instance.RestartRound();
    }
}
