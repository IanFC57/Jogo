using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float vidaMaxima = 100;
    private float vidaAtual;

    private Animator anim;
    private NavMeshAgent agente;
    private MonsterFollow scriptDePerseguicao;
    private Collider colisor;

    void Start()
    {
        vidaAtual = vidaMaxima;
        anim = GetComponent<Animator>();
        agente = GetComponent<NavMeshAgent>();
        scriptDePerseguicao = GetComponent<MonsterFollow>();
        colisor = GetComponent<Collider>();
    }

    // O Easy Weapons procura por uma função chamada "Damage" para enviar o dano
    public void Damage(float quantidade)
    {
        if (vidaAtual <= 0) return;

        vidaAtual -= quantidade;
        Debug.Log("Inimigo atingido! Vida: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    void Morrer()
    {
        anim.SetTrigger("deathTrigger");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        if (scriptDePerseguicao != null) scriptDePerseguicao.enabled = false;
        if (colisor != null) colisor.enabled = false;

        Destroy(gameObject, 10f);
    }
}