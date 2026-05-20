using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- NOVA LINHA: Biblioteca obrigatória para mexer com Canvas/Textos

public class PlayerHealth : MonoBehaviour
{
    public int vidaMaxima = 100;
    private int vidaAtual;

    public Text textoDeVida; // <-- NOVA LINHA: Uma "caixa" vazia para colocarmos o nosso texto da tela

    void Start()
    {
        vidaAtual = vidaMaxima;
        AtualizarTextoDaTela(); // Já arruma o texto assim que o jogo começar
    }

    public void TomarDano(int quantidadeDeDano)
    {
        vidaAtual -= quantidadeDeDano;
        AtualizarTextoDaTela(); // Arruma o texto sempre que apanhar

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    // Esta função muda a palavra na tela para mostrar a vida certa
    void AtualizarTextoDaTela()
    {
        if (textoDeVida != null)
        {
            textoDeVida.text = "Vida: " + vidaAtual;
        }
    }

    void Morrer()
    {
        Debug.Log("O JOGADOR MORREU!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}