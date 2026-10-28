using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void Reintentar()
    {
        SceneManager.LoadScene("Escenario_1");
    }

    public void VolverAInicio()
    {
        SceneManager.LoadScene("MainMenu");
    }


}
