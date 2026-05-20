using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuVitoria : MonoBehaviour
{
    public void Reiniciar()
    {
        // Garante que o jogo despause e o tempo volte a correr normalmente (1f = 100% da velocidade)
        Time.timeScale = 1f;

        // Carrega a fase novamente (Lembre-se de colocar o nome da sua cena aqui)
        SceneManager.LoadScene("JogoComMenu");
    }

    public void IrParaMenu()
    {
        // Coloque aqui o nome da sua cena do MENU INICIAL
        SceneManager.LoadScene("MenuInicial");
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saiu do jogo");
    }
}
