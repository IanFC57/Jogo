using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Necessário para mexer com textos
using System.Collections; // Necessário para o temporizador

public class MensagemInicial : MonoBehaviour
{
    [Header("Arraste o seu TextoDeAviso do Canvas para cá:")]
    public Text textoAvisoDaTela;

    [Header("Configurações da Mensagem")]
    public string mensagemObjetivo = "OBJETIVO: Encontre a chave principal para escapar do hospital!";
    public float tempoNaTela = 5f; // Fica 5 segundos na tela para dar tempo de ler

    void Start()
    {
        // Assim que o jogo começa, ele joga o texto na tela
        if (textoAvisoDaTela != null)
        {
            textoAvisoDaTela.text = mensagemObjetivo;
            StartCoroutine(ApagarTexto());
        }
    }

    IEnumerator ApagarTexto()
    {
        // Espera o tempo definido e depois limpa a tela
        yield return new WaitForSeconds(tempoNaTela);
        textoAvisoDaTela.text = "";
    }
}
