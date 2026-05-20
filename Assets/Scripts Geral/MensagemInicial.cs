using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MensagemInicial : MonoBehaviour
{
    [Header("Texto de aviso do Canvas")]
    public Text textoAvisoDaTela;
    public TMP_Text textoAvisoDaTelaTmp;

    [Header("Configurações da mensagem")]
    public string mensagemObjetivo = "OBJETIVO: Encontre a chave principal para escapar do hospital!";
    public float tempoNaTela = 5f;

    void Start()
    {
        ResolverTextoAviso();
        if (TemTextoAviso())
        {
            EscreverTextoAviso(mensagemObjetivo);
            StartCoroutine(ApagarTexto());
        }
    }

    IEnumerator ApagarTexto()
    {
        yield return new WaitForSeconds(tempoNaTela);
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
