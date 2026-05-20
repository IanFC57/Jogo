using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections; // NOVA LINHA: Necessária para usar temporizadores (Coroutines)

public class SistemaDeVasculhar : MonoBehaviour
{
    [Header("Interface (UI)")]
    public GameObject botaoVasculhar;
    public Text textoAvisoDaTela; // NOVA VARIÁVEL: O texto que vai mostrar o loot

    private Armario armarioAtual;
    private InventarioJogador inventario;

    void Start()
    {
        inventario = GetComponent<InventarioJogador>();
        botaoVasculhar.SetActive(false);

        // APAGAMOS A LINHA QUE LIMPAVA O TEXTO AQUI!
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Armario"))
        {
            armarioAtual = outro.GetComponent<Armario>();
            if (armarioAtual != null && !armarioAtual.jaFoiVasculhado)
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
            botaoVasculhar.SetActive(false);
        }
    }

    public void ClicarNoBotaoVasculhar()
    {
        if (armarioAtual != null && !armarioAtual.jaFoiVasculhado)
        {
            armarioAtual.jaFoiVasculhado = true;
            botaoVasculhar.SetActive(false);

            string mensagemLoot = ""; // Cria um texto em branco para irmos preenchendo
            bool achouAlgumaCoisa = false;

            // 1. Verifica a chave
            if (armarioAtual.temAChave)
            {
                inventario.temChavePrincipal = true;
                mensagemLoot += "Você achou a CHAVE DA SAÍDA!\n"; // O \n pula uma linha
                achouAlgumaCoisa = true;
            }

            // 2. Verifica as balas
            if (armarioAtual.quantidadeDeBalas > 0)
            {
                inventario.balasNoBolso += armarioAtual.quantidadeDeBalas;
                mensagemLoot += "Você achou " + armarioAtual.quantidadeDeBalas + " balas!\n";
                achouAlgumaCoisa = true;
            }

            // 3. Verifica as pilhas
            if (armarioAtual.quantidadeDePilhas > 0)
            {
                inventario.pilhasNoBolso += armarioAtual.quantidadeDePilhas;
                mensagemLoot += "Você achou " + armarioAtual.quantidadeDePilhas + " pilhas!\n";
                achouAlgumaCoisa = true;
            }

            // 4. Se estiver vazio
            if (!achouAlgumaCoisa)
            {
                mensagemLoot = "Apenas poeira e papéis velhos...";
            }

            // Manda o texto para a tela e ativa o temporizador para apagar
            if (textoAvisoDaTela != null)
            {
                textoAvisoDaTela.text = mensagemLoot;

                // Para qualquer temporizador antigo (caso o jogador vasculhe 2 coisas muito rápido)
                StopAllCoroutines();

                // Inicia o relógio para apagar o texto
                StartCoroutine(ApagarTextoDepoisDeUmTempo());
            }
        }
    }

    // A FUNÇÃO TEMPORIZADORA
    IEnumerator ApagarTextoDepoisDeUmTempo()
    {
        // Espera exatos 3 segundos de tempo no jogo
        yield return new WaitForSeconds(3f);

        // Apaga o texto da tela
        textoAvisoDaTela.text = "";
    }
}
