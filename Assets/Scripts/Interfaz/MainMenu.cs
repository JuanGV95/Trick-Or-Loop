using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private GameObject panelAudio;

    public Button botonIniciar;

    [SerializeField] private InputAction esc;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        esc.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (esc.triggered)
        {
            VolverAtras();
        }
    }

    public void IniciarJuego()
    {
        SceneManager.LoadScene("Escenario_1");
    }

    public void AbrirInstrucciones()
    {
        
        panelInstrucciones.SetActive(true);
    }

    public void AbrirSonido()
    {

        panelAudio.SetActive(true);
    }

    public void VolverAtras()
    {

        if(panelInstrucciones.activeSelf == true)
        {
            panelInstrucciones.SetActive(false);

        } else if(panelAudio.activeSelf == true)
        {
            panelAudio.SetActive(false);
        }
        
    }

    public void Cerrar()
    {
        Application.Quit();
    }
}
