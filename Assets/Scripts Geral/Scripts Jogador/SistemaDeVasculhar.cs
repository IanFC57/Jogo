using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SistemaDeVasculhar : MonoBehaviour
{
    [Header("Interface")]
    public GameObject botaoVasculhar;
    public Text textoAvisoDaTela;
    public TMP_Text textoAvisoDaTelaTmp;

    private Armario armarioAtual;
    private InventarioJogador inventario;

    void Start()
    {
        inventario = GetComponent<InventarioJogador>();
        if (botaoVasculhar != null)
        {
            botaoVasculhar.SetActive(false);
        }

        ResolverTextoAviso();
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Armario"))
        {
            armarioAtual = outro.GetComponent<Armario>();
            if (armarioAtual != null && !armarioAtual.jaFoiVasculhado && botaoVasculhar != null)
            {
                botaoVasculhar.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Armario"))
        {
            armarioAtual = null;
            if (botaoVasculhar != null)
            {
                botaoVasculhar.SetActive(false);
            }
        }
    }

    public void ClicarNoBotaoVasculhar()
    {
        if (armarioAtual == null || armarioAtual.jaFoiVasculhado)
            return;

        armarioAtual.jaFoiVasculhado = true;
        if (botaoVasculhar != null)
        {
            botaoVasculhar.SetActive(false);
        }

        string mensagemLoot = "";
        bool achouAlgumaCoisa = false;

        if (armarioAtual.temAChave)
        {
            inventario.temChavePrincipal = true;
            mensagemLoot += "Você achou a CHAVE DA SAÍDA!\n";
            achouAlgumaCoisa = true;
        }

        if (armarioAtual.quantidadeDeBalas > 0)
        {
            inventario.balasNoBolso += armarioAtual.quantidadeDeBalas;
            mensagemLoot += "Você achou " + armarioAtual.quantidadeDeBalas + " balas!\n";
            achouAlgumaCoisa = true;
        }

        if (armarioAtual.quantidadeDePilhas > 0)
        {
            // 1. Guarda a pilha no bolso (como você já tinha feito)
            inventario.pilhasNoBolso += armarioAtual.quantidadeDePilhas;
            mensagemLoot += "Você achou " + armarioAtual.quantidadeDePilhas + " pilhas!\n";
            achouAlgumaCoisa = true;

            // 2. --- NOVA PARTE: RECARREGA A LANTERNA NA HORA ---
            // Procura o script ControleLanterna no próprio Jogador (ou na Câmera dele)
            ControleLanterna lanterna = GetComponentInChildren<ControleLanterna>();

            // Se achou a lanterna, restaura os 15 segundos!
            if (lanterna != null)
            {
                lanterna.RecarregarPilha();
            }
        }

        if (!achouAlgumaCoisa)
        {
            mensagemLoot = "Apenas poeira e papéis velhos...";
        }

        ResolverTextoAviso();
        if (TemTextoAviso())
        {
            EscreverTextoAviso(mensagemLoot);
            StopAllCoroutines();
            StartCoroutine(ApagarTextoDepoisDeUmTempo());
        }
    }

    IEnumerator ApagarTextoDepoisDeUmTempo()
    {
        yield return new WaitForSeconds(3f);
        EscreverTextoAviso("");
    }

    private void ResolverTextoAviso()
    {
        if (textoAvisoDaTela != null || textoAvisoDaTelaTmp != null)
            return;

        GameObject objetoTexto = GameObject.Find("TextoAviso");
        if (objetoTexto == null)
            return;

        textoAvisoDaTela = objetoTexto.GetComponent<Text>();
        textoAvisoDaTelaTmp = objetoTexto.GetComponent<TMP_Text>();
    }

    private bool TemTextoAviso()
    {
        return textoAvisoDaTela != null || textoAvisoDaTelaTmp != null;
    }

    private void EscreverTextoAviso(string texto)
    {
        if (textoAvisoDaTelaTmp != null)
        {
            textoAvisoDaTelaTmp.text = texto;
        }

        if (textoAvisoDaTela != null)
        {
            textoAvisoDaTela.text = texto;
        }
    }
}
