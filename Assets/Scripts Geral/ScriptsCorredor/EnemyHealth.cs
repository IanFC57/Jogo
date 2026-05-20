using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Balanceamento de vida")]
    public float vidaMaxima = 270f;
    public float danoReferenciaPorTiro = EnemyDamageRules.DefaultReferenceShotDamage;
    public float tirosCorpoParaMatar = EnemyDamageRules.DefaultBodyShotsToKill;
    public float multiplicadorDanoNoPeito = EnemyDamageRules.DefaultChestMultiplier;
    public float multiplicadorDanoNaCabeca = EnemyDamageRules.DefaultHeadMultiplier;

    [Header("Hitboxes")]
    public bool criarHitboxesAutomaticamente = true;
    public bool desativarColisorRaizQuandoHitboxesAtivas = true;
    public float raioCabeca = 0.26f;
    public float raioPeito = 0.38f;
    public float raioCorpo = 0.45f;
    public float alturaCorpo = 1.25f;
    public Vector3 centroCorpo = new Vector3(0f, 0.55f, 0f);

    [Header("Morte")]
    public float tempoParaDesativarAposMorte = 10f;

    private float vidaAtual;
    private Animator anim;
    private NavMeshAgent agente;
    private MonsterFollow scriptDePerseguicao;
    private Collider colisor;
    private EnemyHitbox[] hitboxes = new EnemyHitbox[0];
    private GeradorDeInimigos gerador;
    private bool inicializado;
    private bool morreu;

    public float VidaAtual => vidaAtual;
    public bool EstaMorto => morreu;

    void Awake()
    {
        Inicializar();
    }

    void Start()
    {
        ResetarVida();
    }

    public void ConfigurarGerador(GeradorDeInimigos novoGerador)
    {
        gerador = novoGerador;
    }

    public void ResetarParaSpawn(Vector3 posicao, Quaternion rotacao, GeradorDeInimigos novoGerador)
    {
        Inicializar();
        gerador = novoGerador;
        transform.SetPositionAndRotation(posicao, rotacao);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        morreu = false;
        ResetarVida();

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        AtualizarColisoresDeCombate(true);

        if (agente != null)
        {
            if (!agente.enabled)
            {
                agente.enabled = true;
            }

            if (agente.isOnNavMesh)
            {
                agente.Warp(posicao);
                agente.ResetPath();
                agente.isStopped = false;
            }
        }

        if (scriptDePerseguicao != null)
        {
            scriptDePerseguicao.enabled = true;
            scriptDePerseguicao.RefreshTarget();
            scriptDePerseguicao.ResetarEstadoDeNavegacao();
        }
    }

    public void ApplyWeaponHit(float rawWeaponDamage, EnemyHitZone zona)
    {
        float danoFinal = EnemyDamageRules.CalculateZoneDamage(
            rawWeaponDamage,
            zona,
            multiplicadorDanoNaCabeca,
            multiplicadorDanoNoPeito);

        bool danoAplicado = !morreu && danoFinal > 0f;
        if (HeadshotFeedbackRules.ShouldPlayHeadshotFeedback(zona, danoAplicado))
        {
            HeadshotAudioFeedback.PlayAt(transform.position);
        }

        TomarDano(danoFinal, zona);
    }

    public void Damage(float quantidade)
    {
        TomarDano(Mathf.Abs(quantidade), EnemyHitZone.Body);
    }

    public void ChangeHealth(float quantidade)
    {
        if (quantidade < 0f)
        {
            TomarDano(-quantidade, EnemyHitZone.Body);
            return;
        }

        if (quantidade > 0f && !morreu)
        {
            vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantidade);
        }
    }

    private void Inicializar()
    {
        if (inicializado)
            return;

        anim = GetComponent<Animator>();
        agente = GetComponent<NavMeshAgent>();
        scriptDePerseguicao = GetComponent<MonsterFollow>();
        colisor = GetComponent<Collider>();

        if (criarHitboxesAutomaticamente)
        {
            CriarHitboxesAutomaticas();
        }

        hitboxes = GetComponentsInChildren<EnemyHitbox>(true);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].SetOwner(this);
        }

        AtualizarColisoresDeCombate(true);
        inicializado = true;
    }

    private void ResetarVida()
    {
        vidaMaxima = EnemyDamageRules.CalculateMaxHealth(danoReferenciaPorTiro, tirosCorpoParaMatar);
        vidaAtual = vidaMaxima;
    }

    private void TomarDano(float quantidade, EnemyHitZone zona)
    {
        if (morreu || quantidade <= 0f)
            return;

        vidaAtual -= quantidade;
        Debug.Log("Inimigo atingido (" + zona + "). Vida: " + vidaAtual + "/" + vidaMaxima);

        if (vidaAtual <= 0f)
        {
            Morrer();
        }
    }

    private void Morrer()
    {
        if (morreu)
            return;

        morreu = true;

        if (anim != null)
        {
            anim.SetTrigger("deathTrigger");
        }

        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        if (scriptDePerseguicao != null)
        {
            scriptDePerseguicao.TocarSomMorte();
            scriptDePerseguicao.enabled = false;
        }

        AtualizarColisoresDeCombate(false);

        if (gerador != null)
        {
            gerador.MonstroMorreu(this, tempoParaDesativarAposMorte);
        }
        else if (Application.isPlaying)
        {
            Destroy(gameObject, tempoParaDesativarAposMorte);
        }
    }

    private void AtualizarColisoresDeCombate(bool ativo)
    {
        bool temHitboxes = hitboxes != null && hitboxes.Length > 0;

        if (colisor != null)
        {
            colisor.enabled = ativo && (!desativarColisorRaizQuandoHitboxesAtivas || !temHitboxes);
        }

        if (hitboxes == null)
            return;

        for (int i = 0; i < hitboxes.Length; i++)
        {
            Collider hitboxCollider = hitboxes[i].GetComponent<Collider>();
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = ativo;
            }
        }
    }

    private void CriarHitboxesAutomaticas()
    {
        if (GetComponentsInChildren<EnemyHitbox>(true).Length > 0)
            return;

        CriarCapsula("Hitbox_Body", transform, EnemyHitZone.Body, centroCorpo, raioCorpo, alturaCorpo);

        Transform peito = EncontrarFilhoPorNome(transform, "spine_02.x");
        CriarEsfera("Hitbox_Chest", peito != null ? peito : transform, EnemyHitZone.Chest, Vector3.zero, raioPeito);

        Transform cabeca = EncontrarFilhoPorNome(transform, "head.x");
        CriarEsfera("Hitbox_Head", cabeca != null ? cabeca : transform, EnemyHitZone.Head, Vector3.zero, raioCabeca);
    }

    private void CriarCapsula(
        string nome,
        Transform pai,
        EnemyHitZone zona,
        Vector3 centro,
        float raio,
        float altura)
    {
        GameObject objeto = new GameObject(nome);
        objeto.layer = gameObject.layer;
        objeto.transform.SetParent(pai, false);

        EnemyHitbox hitbox = objeto.AddComponent<EnemyHitbox>();
        hitbox.Configure(this, zona);

        CapsuleCollider capsule = objeto.AddComponent<CapsuleCollider>();
        capsule.center = centro;
        capsule.radius = Mathf.Max(0.01f, raio);
        capsule.height = Mathf.Max(capsule.radius * 2f, altura);
        capsule.direction = 1;
    }

    private void CriarEsfera(
        string nome,
        Transform pai,
        EnemyHitZone zona,
        Vector3 centro,
        float raio)
    {
        GameObject objeto = new GameObject(nome);
        objeto.layer = gameObject.layer;
        objeto.transform.SetParent(pai, false);

        EnemyHitbox hitbox = objeto.AddComponent<EnemyHitbox>();
        hitbox.Configure(this, zona);

        SphereCollider sphere = objeto.AddComponent<SphereCollider>();
        sphere.center = centro;
        sphere.radius = Mathf.Max(0.01f, raio);
    }

    private static Transform EncontrarFilhoPorNome(Transform raiz, string nome)
    {
        if (raiz.name == nome)
            return raiz;

        for (int i = 0; i < raiz.childCount; i++)
        {
            Transform encontrado = EncontrarFilhoPorNome(raiz.GetChild(i), nome);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }
}
