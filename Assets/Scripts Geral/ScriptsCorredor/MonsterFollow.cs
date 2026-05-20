using UnityEngine;
using UnityEngine.AI;

public class MonsterFollow : MonoBehaviour
{
    [Header("Configurações de Perseguição")]
    [Tooltip("Agora o script acha o jogador sozinho usando a Tag 'Player'")]
    public Transform jogador; // O alvo

    [Header("Configurações de Combate")]
    public float distanciaDeAtaque = 2.0f; // Quão perto ele precisa chegar para atacar
    public float tempoDeRecargaAtaque = 3.0f; // Segundos entre cada ataque
    public int danoDoAtaque = 25; // Quanto de vida ele tira por golpe

    private NavMeshAgent agente;
    private Animator anim;
    private float proximoTempoDeAtaque;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // --- NOVA SEÇÃO: BUSCA DINÂMICA ---
        // O monstro varre a fase inteira atrás do objeto com a etiqueta "Player"
        GameObject alvo = GameObject.FindGameObjectWithTag("Player");

        if (alvo != null)
        {
            jogador = alvo.transform;
        }
        else
        {
            Debug.LogError("Monstro não achou o jogador! Coloque a Tag 'Player' no seu personagem.");
        }
    }

    void Update()
    {
        // Trava de Segurança: Se não achar o jogador, cancela o Update para o jogo não quebrar
        if (jogador == null) return;

        // 1. Calcula a distância atual até o jogador
        float distanciaParaJogador = Vector3.Distance(transform.position, jogador.position);

        // 2. Tenta atacar se estiver perto e a recarga acabou
        if (distanciaParaJogador <= distanciaDeAtaque && Time.time >= proximoTempoDeAtaque)
        {
            Atacar();
        }
        else
        {
            // Se não está atacando, ele continua seguindo
            agente.SetDestination(jogador.position);

            // Controle da animação de andar
            if (agente.velocity.magnitude > 0.1f)
            {
                anim.SetBool("isWalking", true);
            }
            else
            {
                anim.SetBool("isWalking", false);
            }
        }
    }

    void Atacar()
    {
        proximoTempoDeAtaque = Time.time + tempoDeRecargaAtaque;

        anim.SetTrigger("attackTrigger");
        agente.isStopped = true;
        Invoke("ResetarMovimento", 1.0f);

        // Procura o script 'PlayerHealth' no objeto do jogador
        PlayerHealth vidaDoJogador = jogador.GetComponent<PlayerHealth>();

        // Se o script existir, o monstro causa o dano!
        if (vidaDoJogador != null)
        {
            vidaDoJogador.TomarDano(danoDoAtaque);
        }
    }

    void ResetarMovimento()
    {
        if (agente.enabled)
        {
            agente.isStopped = false;
        }
    }
}