using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortaInteligente1 : MonoBehaviour
{
    [Header("Arraste o objeto Porta_Eixo para cá:")]
    public Transform eixoDaPorta;

    public float anguloAbertura = 90f;
    public float velocidade = 5f;

    private Quaternion alvoRotacao;

    void Start()
    {
        // O alvo inicial é a porta totalmente fechada (0,0,0 localmente)
        alvoRotacao = Quaternion.identity;
    }

    void Update()
    {
        // Gira APENAS o Eixo, mantendo o sensor (esta Moldura) parado no lugar
        eixoDaPorta.localRotation = Quaternion.Slerp(eixoDaPorta.localRotation, alvoRotacao, Time.deltaTime * velocidade);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Calcula de qual lado o jogador vem (usando a frente da Moldura, que está estática)
            Vector3 direcaoJogador = outro.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, direcaoJogador);

            // Decide o lado que a porta foge de você
            float direcaoFinal = (dot >= 0) ? -anguloAbertura : anguloAbertura;

            // Define o alvo de rotação local
            alvoRotacao = Quaternion.Euler(0, direcaoFinal, 0);
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Volta para a posição zero (fechada)
            alvoRotacao = Quaternion.identity;
        }
    }
}