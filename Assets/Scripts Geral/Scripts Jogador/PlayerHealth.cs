using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int vidaMaxima = 100;
    public int regeneracaoVida = PlayerHealthRules.DefaultRegenerationAmount;
    public float intervaloRegeneracao = PlayerHealthRules.DefaultRegenerationIntervalSeconds;
    public Text textoDeVida;
    public TMP_Text textoDeVidaTmp;

    private int vidaAtual;
    private float temporizadorRegeneracao;

    public int VidaAtual => vidaAtual;

    void Start()
    {
        ResolverTextoDeVida();
        vidaAtual = vidaMaxima;
        AtualizarTextoDaTela();
    }

    void Update()
    {
        temporizadorRegeneracao += Time.deltaTime;
        if (!PlayerHealthRules.ShouldRegenerate(temporizadorRegeneracao, intervaloRegeneracao, vidaAtual, vidaMaxima))
            return;

        temporizadorRegeneracao = 0f;
        RecuperarVida(regeneracaoVida);
    }

    public void TomarDano(int quantidadeDeDano)
    {
        vidaAtual = PlayerHealthRules.ApplyDamage(vidaAtual, quantidadeDeDano);
        temporizadorRegeneracao = 0f;
        AtualizarTextoDaTela();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    public void RecuperarVida(int quantidade)
    {
        int novaVida = PlayerHealthRules.ApplyRegeneration(vidaAtual, vidaMaxima, quantidade);
        if (novaVida == vidaAtual)
            return;

        vidaAtual = novaVida;
        AtualizarTextoDaTela();
    }

    void AtualizarTextoDaTela()
    {
        ResolverTextoDeVida();
        string texto = "Vida: " + vidaAtual;

        if (textoDeVidaTmp != null)
        {
            textoDeVidaTmp.text = texto;
        }

        if (textoDeVida != null)
        {
            textoDeVida.text = texto;
        }
    }

    private void ResolverTextoDeVida()
    {
        if (textoDeVida != null || textoDeVidaTmp != null)
            return;

        GameObject objetoTexto = GameObject.Find("Texto_Vida");
        if (objetoTexto == null)
            return;

        textoDeVida = objetoTexto.GetComponent<Text>();
        textoDeVidaTmp = objetoTexto.GetComponent<TMP_Text>();
    }

    void Morrer()
    {
        Debug.Log("O JOGADOR MORREU!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
