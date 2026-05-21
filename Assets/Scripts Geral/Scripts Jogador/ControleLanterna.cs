using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleLanterna : MonoBehaviour
{
    [Header("Configurações da Lanterna")]
    public Light luzLanterna; // Arraste a sua luz aqui no Inspector
    public float tempoMaximoBateria = 15f; // Tempo em segundos

    // Variáveis internas para controle
    private float tempoAtual;
    private bool temCarga = true;

    void Start()
    {
        // A lanterna começa com a bateria cheia
        tempoAtual = tempoMaximoBateria;
    }

    void Update()
    {
        // Se a luz estiver ligada, a bateria vai descarregando
        if (luzLanterna.enabled == true)
        {
            // Time.deltaTime é o tempo real em segundos que passa entre cada frame
            tempoAtual -= Time.deltaTime;

            // Quando o tempo zera, apaga a luz e bloqueia o uso
            if (tempoAtual <= 0)
            {
                luzLanterna.enabled = false;
                temCarga = false;
                tempoAtual = 0;
            }
        }
    }

    // Essa é a função que você vai colocar no BOTÃO DA TELA do celular
    public void BotaoLigarDesligar()
    {
        // Só permite ligar se ainda tiver carga
        if (temCarga == true)
        {
            // Inverte o estado da luz (se está ligada, desliga. Se está desligada, liga)
            luzLanterna.enabled = !luzLanterna.enabled;
        }
    }

    // Essa função será chamada quando pegarmos a pilha
    public void RecarregarPilha()
    {
        tempoAtual = tempoMaximoBateria;
        temCarga = true;
        Debug.Log("Pilha recarregada! Tempo restaurado para 15s.");
    }
}
