
using UnityEngine;

public class CasaVerde : InteractableItem
{
    protected override void ApplyEffect()
    {
        
        GameManager.Instance.CollectCandy();
    }
}
