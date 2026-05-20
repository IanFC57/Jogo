using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // BIBLIOTECA ESSENCIAL: Permite trocar de cenas

public class MenuPrincipal : MonoBehaviour
{
    // Criamos uma variável para você digitar o nome da sua cena de jogo no Unity
    public string nomeDaCenaDoJogo;

    public void Comecar()
    {
        // Esse comando procura a cena pelo nome e a carrega
        SceneManager.LoadScene(nomeDaCenaDoJogo);
    }

    public void SairDoJogo()
    {
        // Esse comando fecha o aplicativo (funciona no celular após o build)
        Application.Quit();
        Debug.Log("O jogador saiu do jogo.");
    }
}
