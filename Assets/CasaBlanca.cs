
using UnityEngine;

public class CasaBlanca : InteractableItem
{
    protected override void ApplyEffect()
    {
        GameManager.Instance.Pista();
    }
}
