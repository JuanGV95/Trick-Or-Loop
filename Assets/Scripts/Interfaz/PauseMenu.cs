using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private GameObject MenuPausa;
    [SerializeField] private GameObject panelInstrucciones;
    [SerializeField] private GameObject panelAudio;



    [SerializeField] private PlayerController1 player;
    [SerializeField] private InputAction esc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
        esc.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (esc.triggered && MenuPausa.activeSelf == false)
        {
            AbrirCerrarPausa();

        }else if(esc.triggered && MenuPausa.activeSelf == true && panelInstrucciones.activeSelf == false && panelAudio.activeSelf == false)
        {
            AbrirCerrarPausa();
        }
        else if (esc.triggered && panelInstrucciones.activeSelf == true)
        {
            VolverAtras();

        } else if(esc.triggered && panelAudio.activeSelf == true)
        {
            VolverAtras();
        }
    }

    public void VolverAInicio()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
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

        if (panelInstrucciones.activeSelf == true)
        {
            panelInstrucciones.SetActive(false);

        }
        else if (panelAudio.activeSelf == true)
        {
            panelAudio.SetActive(false);
        }

    }
    public void AbrirCerrarPausa()
    {

        if (MenuPausa.activeSelf == true)
        {
            MenuPausa.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            player.enabled = true;

        }
        else if (MenuPausa.activeSelf == false)
        {
            MenuPausa.SetActive(true);
        
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            player.enabled = false;
        }

    }
}
