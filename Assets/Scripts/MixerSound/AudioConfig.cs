using UnityEngine;
using UnityEngine.Audio;   // para usar AudioMixer
using UnityEngine.UI;      // para usar Slider
using TMPro;               // para usar los textos de TextMeshPro

public class AudioConfig : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider sliderGeneral;
    [SerializeField] private Slider sliderEfectos;

    [Header("Mute")]
    [SerializeField] private Toggle mute;

    [Header("Textos de porcentaje")]
    [SerializeField] private TMP_Text textoGeneral;
    [SerializeField] private TMP_Text textoEfectos;

    void Start()
    {
        // Se inicia con el valor que este aplicado desde el inspecto o por defecto
        CambiarVolumenGeneral(sliderGeneral.value);
        CambiarVolumenEfectos(sliderEfectos.value);
    }

    // Llama el slider de Volumen general cada vez que se mueve
    public void CambiarVolumenGeneral(float valor)
    {
        audioMixer.SetFloat("MasterVolume", ADecibeles(valor));
        textoGeneral.text = Mathf.RoundToInt(valor * 100) + "%";
    }

    // Llama el slider de Efectos cada vez que se mueve
    public void CambiarVolumenEfectos(float valor)
    {
        audioMixer.SetFloat("MasterSFX", ADecibeles(valor));
        textoEfectos.text = Mathf.RoundToInt(valor * 100) + "%";
    }

    // Convierte el valor del slider (0 a 1) a decibeles (-80 a 0)
    private float ADecibeles(float valor)
    {
        if (valor <= 0.0001f)
        {
            return -80f;   // 0 en el slider = silencio total
        }
        return Mathf.Log10(valor) * 20f;
    }

    public void Silenciar(bool silenciado)
    {
        if (silenciado)
        {
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        else
        {
            // Volvemos al volumen que tiene el slider en este momento
            audioMixer.SetFloat("MasterVolume", ADecibeles(sliderGeneral.value));
        }
    }
}