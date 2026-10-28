
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Imagenes de dulces")]
    [SerializeField] private Image[] candyImages = new Image[3];

    [Header("Sprites")]
    [SerializeField] private Sprite darkCandySprite;
    [SerializeField] private Sprite collectedCandySprite;

    private int previousCandyCount = 0;


    [Header("Barra de interaccion")]
    [SerializeField] private GameObject interactionBarContainer;
    [SerializeField] private Slider interactionBar;

    [Header("Jumpscare")]
    [SerializeField] private GameObject jumpscareContainer;
    [SerializeField] private float jumpscareDuration = 2f;



    private Coroutine hintCoroutine;
    private Coroutine jumpscareCoroutine;

    private void Awake()
    {
        InitializeUI();

       
  
    }

    public void InitializeUI()
    {
;

        // La barra comienza oculta y vacia.
        SetInteractionProgress(0f);
        ShowInteractionBar(false);

        // Ocultar el jumpscare.
        if (jumpscareContainer != null)
            jumpscareContainer.SetActive(false);

        // Las tres imágenes comienzan oscuras.
        foreach (Image candyImage in candyImages)
        {
            if (candyImage != null)
                candyImage.sprite = darkCandySprite;
        }


    }

    // =========================
    // DULCES
    // =========================
    public void UpdateCandyUI(int candyCount)
    {
        candyCount = Mathf.Clamp(candyCount, 0, candyImages.Length);

        for (int i = 0; i < candyImages.Length; i++)
        {
            if (candyImages[i] == null)
                continue;

            candyImages[i].sprite = i < candyCount
                ? collectedCandySprite
                : darkCandySprite;
        }

        previousCandyCount = candyCount;
    }

    // =========================
    // BARRA DE INTERACCION
    // =========================

    public void ShowInteractionBar(bool show)
    {
        if (interactionBarContainer != null)
            interactionBarContainer.SetActive(show);
    }

    public void SetInteractionProgress(float progress)
    {
        if (interactionBar == null)
            return;

        interactionBar.value = Mathf.Clamp01(progress);
    }

    // =========================
    // JUMPSCARE
    // =========================

    public void ShowJumpscare()
    {
        if (jumpscareContainer == null)
            return;

        if (jumpscareCoroutine != null)
            StopCoroutine(jumpscareCoroutine);

        jumpscareCoroutine = StartCoroutine(
            JumpscareRoutine()
        );
    }

    private IEnumerator JumpscareRoutine()
    {
        jumpscareContainer.SetActive(true);

        yield return new WaitForSeconds(jumpscareDuration);

        jumpscareContainer.SetActive(false);
        jumpscareCoroutine = null;
    }

}
