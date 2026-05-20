using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // NOVA LINHA
using UnityEngine.SceneManagement;
using System.Collections; // NOVA LINHA

public class PortaDeFuga : MonoBehaviour
{
    [Header("Arraste o seu TextoDeAviso do Canvas para cá:")]
    public Text textoAvisoDaTela;

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            InventarioJogador inventario = outro.GetComponent<InventarioJogador>();

            // Verifica se tem o inventário e se a chave principal é verdadeira
            if (inventario != null && inventario.temChavePrincipal == true)
            {
                textoAvisoDaTela.text = "VOCÊ ESCAPOU DO HOSPITAL!";

                // Recarrega a fase (ou você pode carregar uma cena de Vitória aqui)
                SceneManager.LoadScene("Final");
            }
            else
            {
                // Se NÃO tem a chave, mostra o aviso na tela!
                if (textoAvisoDaTela != null)
                {
                    textoAvisoDaTela.text = "A porta está trancada... Você precisa encontrar a chave!";

                    // Inicia o temporizador para apagar essa mensagem
                    StopAllCoroutines();
                    StartCoroutine(ApagarTextoTrancado());
                }
            }
        }
    }

    IEnumerator ApagarTextoTrancado()
    {
        // Espera 3 segundos e apaga o aviso da porta
        yield return new WaitForSeconds(3f);
        textoAvisoDaTela.text = "";
    }
}
