using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortaInteligente : MonoBehaviour
{
    public float anguloAbertura = 90f;
    public float velocidade = 5f;

    private Quaternion rotacaoFechada;
    private Quaternion alvoRotacao;

    void Start()
    {
        // Guarda a rotação inicial exata, não importa para onde a porta esteja virada no mapa
        rotacaoFechada = transform.localRotation;
        alvoRotacao = rotacaoFechada;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, alvoRotacao, Time.deltaTime * velocidade);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            Vector3 direcaoJogador = outro.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, direcaoJogador);

            float direcaoFinal = (dot >= 0) ? -anguloAbertura : anguloAbertura;

            // A MÁGICA ACONTECE AQUI:
            // Multiplicar a "rotacaoFechada" pelo novo ângulo faz a Unity SOMAR as rotações.
            // Assim ela abre 90 graus a partir de onde já estava!
            alvoRotacao = rotacaoFechada * Quaternion.Euler(0, direcaoFinal, 0);
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Volta exatamente para a rotação original salva no Start
            alvoRotacao = rotacaoFechada;
        }
    }
}