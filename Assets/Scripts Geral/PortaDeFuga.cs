using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortaDeFuga : MonoBehaviour
{
    [Header("Texto de aviso do Canvas")]
    public Text textoAvisoDaTela;
    public TMP_Text textoAvisoDaTelaTmp;

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        ResolverTextoAviso();
        InventarioJogador inventario = outro.GetComponent<InventarioJogador>();
        if (inventario != null && inventario.temChavePrincipal)
        {
            EscreverTextoAviso("VOCÊ ESCAPOU DO HOSPITAL!");
            SceneManager.LoadScene("Final");
            return;
        }

        if (TemTextoAviso())
        {
            EscreverTextoAviso("A porta está trancada... Você precisa encontrar a chave!");
            StopAllCoroutines();
            StartCoroutine(ApagarTextoTrancado());
        }
    }

    IEnumerator ApagarTextoTrancado()
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
