using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // BIBLIOTECA ESSENCIAL: Permite trocar de cenas

public class MenuPrincipal : MonoBehaviour
{
    // Criamos uma variável para você digitar o nome da sua cena de jogo no Unity
    public string nomeDaCenaDoJogo = "JogoComMenu";
    public GameObject menuPrincipal;
    public GameObject menuOpcoes;

    public void Comecar()
    {
        Jogar();
    }

    public void Jogar()
    {
        // Esse comando procura a cena pelo nome e a carrega
        SceneManager.LoadScene(nomeDaCenaDoJogo);
    }

    public void AbrirOpcoes()
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(false);
        if (menuOpcoes != null) menuOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        if (menuOpcoes != null) menuOpcoes.SetActive(false);
        if (menuPrincipal != null) menuPrincipal.SetActive(true);
    }

    public void SairDoJogo()
    {
        SairJogo();
    }

    public void SairJogo()
    {
        // Esse comando fecha o aplicativo (funciona no celular após o build)
        Application.Quit();
        Debug.Log("O jogador saiu do jogo.");
    }
}
