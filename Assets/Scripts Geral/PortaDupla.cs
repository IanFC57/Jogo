using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortaDupla : MonoBehaviour
{
    [Header("Arraste os dois eixos das portas aqui:")]
    public Transform eixoEsquerdo;
    public Transform eixoDireito;

    public float anguloAbertura = 90f;
    public float velocidade = 5f;

    private Quaternion rotacaoFechadaEsq;
    private Quaternion rotacaoFechadaDir;

    private Quaternion alvoRotacaoEsq;
    private Quaternion alvoRotacaoDir;

    void Start()
    {
        // Salva a rotação original das duas portas assim que a fase carrega
        rotacaoFechadaEsq = eixoEsquerdo.localRotation;
        rotacaoFechadaDir = eixoDireito.localRotation;

        alvoRotacaoEsq = rotacaoFechadaEsq;
        alvoRotacaoDir = rotacaoFechadaDir;
    }

    void Update()
    {
        // Movimenta as duas portas simultaneamente de forma suave
        eixoEsquerdo.localRotation = Quaternion.Slerp(eixoEsquerdo.localRotation, alvoRotacaoEsq, Time.deltaTime * velocidade);
        eixoDireito.localRotation = Quaternion.Slerp(eixoDireito.localRotation, alvoRotacaoDir, Time.deltaTime * velocidade);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Calcula de qual lado o jogador está vindo em relação ao sensor invisível
            Vector3 direcaoJogador = outro.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, direcaoJogador);

            // Define se abre para "frente" ou para "trás"
            float multiplicador = (dot >= 0) ? -1f : 1f;

            // A porta esquerda gira para um lado, a direita gira para o lado inverso
            alvoRotacaoEsq = rotacaoFechadaEsq * Quaternion.Euler(0, anguloAbertura * multiplicador, 0);
            alvoRotacaoDir = rotacaoFechadaDir * Quaternion.Euler(0, -anguloAbertura * multiplicador, 0);
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Manda as duas portas de volta para a posição original
            alvoRotacaoEsq = rotacaoFechadaEsq;
            alvoRotacaoDir = rotacaoFechadaDir;
        }
    }
}
