using UnityEngine;
using UnityEngine.AI;

public class MonsterFollow : MonoBehaviour
{
    [Header("Configuracoes de Perseguicao")]
    [Tooltip("Agora o script acha o jogador sozinho usando a Tag 'Player'")]
    public Transform jogador;
    public float intervaloRecalculoRota = 0.2f;
    public float distanciaMinimaParaRecalcular = 0.75f;
    public float raioBuscaJogadorNavMesh = 2f;
    public float raioAberturaPorta = 2.25f;
    public float intervaloBuscaPorta = 0.35f;

    [Header("Configuracoes de Combate")]
    public float distanciaDeAtaque = 2.0f;
    public float tempoDeRecargaAtaque = 3.0f;
    public int danoDoAtaque = 25;
    public float tempoParadoAoAtacar = 1.0f;

    [Header("Audio")]
    public bool usarAudioDeMonstro = true;
    public AudioClip somAmbienteMonstro;
    public AudioClip somAtaqueMonstro;
    public AudioClip somMorteMonstro;
    public float distanciaSomAudivel = MonsterAudioRules.DefaultMinimumAudibleDistance;
    public float distanciaVolumeMaximo = MonsterAudioRules.DefaultFullVolumeDistance;
    [Range(0f, 1f)] public float volumeMinimoMonstro = MonsterAudioRules.DefaultMinimumVolume;
    [Range(0f, 1f)] public float volumeMaximoMonstro = MonsterAudioRules.DefaultMaximumVolume;
    [Range(0f, 1f)] public float volumeAtaque = 0.95f;
    [Range(0f, 1f)] public float volumeMorte = 1f;
    public float variacaoPitch = 0.08f;

    [Header("Recuperacao de rota")]
    public float intervaloAvaliacaoTravamento = 0.5f;
    public float distanciaMinimaProgresso = 0.15f;
    public float velocidadeMinimaMovimento = 0.05f;
    public float tempoParaConsiderarTravado = 1.5f;

    private NavMeshAgent agente;
    private Animator anim;
    private AudioSource audioMonstro;
    private AudioSource audioAtaqueMonstro;
    private float proximoTempoDeAtaque;
    private float proximoRecalculoRota;
    private float proximaBuscaPorta;
    private float proximaBuscaJogador;
    private float tempoRetomarMovimento;
    private float proximaAvaliacaoTravamento;
    private float ultimoTempoAvaliacaoTravamento;
    private float tempoTravado;
    private Vector3 ultimaPosicaoProgresso;
    private Vector3 ultimoDestinoJogador;
    private NavMeshPath caminhoCalculado;
    private float pitchAmbiente = 1f;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private bool ambienteAudivelReportado;
    private bool ataqueReportado;
#endif
    private static AudioClip somAmbienteRecurso;
    private static AudioClip somAtaqueRecurso;
    private static AudioClip somMorteRecurso;
    private static bool tentouCarregarAmbienteRecurso;
    private static bool tentouCarregarAtaqueRecurso;
    private static bool tentouCarregarMorteRecurso;
    private static AudioClip somAmbienteFallback;
    private static AudioClip somAtaqueFallback;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        GarantirFontesDeAudio();
        caminhoCalculado = new NavMeshPath();
        ConfigurarAgente();
        ConfigurarAudio();
    }

    void Start()
    {
        RefreshTarget();
    }

    void OnEnable()
    {
        if (agente == null)
        {
            agente = GetComponent<NavMeshAgent>();
        }

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        GarantirFontesDeAudio();
        caminhoCalculado ??= new NavMeshPath();
        pitchAmbiente = Random.Range(0.92f, 1.04f);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ambienteAudivelReportado = false;
        ataqueReportado = false;
#endif
        ConfigurarAgente();
        ConfigurarAudio();
        ResetarEstadoDeNavegacao();

        if (jogador == null)
        {
            RefreshTarget();
        }
    }

    void OnDisable()
    {
        PararAudioDeMonstro();
    }

    public void RefreshTarget()
    {
        GameObject alvo = GameObject.FindGameObjectWithTag("Player");

        if (alvo != null)
        {
            jogador = alvo.transform;
        }
        else
        {
            Debug.LogError("Monstro nao achou o jogador! Coloque a Tag 'Player' no seu personagem.");
        }
    }

    public void ResetarEstadoDeNavegacao()
    {
        tempoTravado = 0f;
        proximoRecalculoRota = 0f;
        proximaBuscaPorta = 0f;
        tempoRetomarMovimento = 0f;
        ultimoDestinoJogador = Vector3.positiveInfinity;
        ultimaPosicaoProgresso = transform.position;
        ultimoTempoAvaliacaoTravamento = Time.time;
        proximaAvaliacaoTravamento = Time.time + intervaloAvaliacaoTravamento;

        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.ResetPath();
            agente.isStopped = false;
        }
    }

    public void PararAudioDeMonstro()
    {
        AudioSource[] fontes = GetComponents<AudioSource>();
        for (int i = 0; i < fontes.Length; i++)
        {
            AudioSource fonte = fontes[i];
            if (fonte == null)
                continue;

            fonte.Stop();
            fonte.volume = 0f;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        ambienteAudivelReportado = false;
        ataqueReportado = false;
#endif
    }

    public void TocarSomMorte()
    {
        if (!usarAudioDeMonstro)
            return;

        AudioClip clip = somMorteMonstro != null ? somMorteMonstro : ObterSomMorteFallback();
        PararAudioDeMonstro();

        if (clip == null)
            return;

        TocarOneShotEspacial(clip, transform.position, Mathf.Clamp01(volumeMorte), Random.Range(0.92f, 1.04f));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log($"MonsterAudio: death horror growl played. clip={clip.name}, volume={Mathf.Clamp01(volumeMorte):0.00}");
#endif
    }

    void Update()
    {
        if (jogador == null)
        {
            if (Time.time >= proximaBuscaJogador)
            {
                proximaBuscaJogador = Time.time + 1f;
                RefreshTarget();
            }

            AtualizarAnimacao(false);
            AtualizarAudioAmbiente(0f, false);
            return;
        }

        if (agente == null || !agente.enabled)
            return;

        if (!agente.isOnNavMesh)
        {
            TentarReposicionarNaNavMesh();
            return;
        }

        float distanciaParaJogador = Vector3.Distance(transform.position, jogador.position);
        AtualizarAudioAmbiente(distanciaParaJogador, true);

        if (distanciaParaJogador <= distanciaDeAtaque && Time.time >= proximoTempoDeAtaque)
        {
            Atacar(distanciaParaJogador);
            return;
        }

        if (Time.time < tempoRetomarMovimento)
        {
            OlharParaJogador();
            AtualizarAnimacao(false);
            return;
        }

        if (agente.isStopped)
        {
            agente.isStopped = false;
        }

        if (Time.time >= proximaBuscaPorta)
        {
            proximaBuscaPorta = Time.time + Mathf.Max(0.05f, intervaloBuscaPorta);
            TentarAbrirPortasProximas();
        }

        AtualizarDestino();
        AvaliarTravamento();
        AtualizarAnimacao(agente.velocity.magnitude > 0.1f);
    }

    void Atacar(float distanciaParaJogador)
    {
        proximoTempoDeAtaque = Time.time + tempoDeRecargaAtaque;
        tempoRetomarMovimento = Time.time + Mathf.Max(0f, tempoParadoAoAtacar);
        OlharParaJogador();
        TocarSomAtaque(distanciaParaJogador);

        if (anim != null)
        {
            anim.SetTrigger("attackTrigger");
        }

        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }

        PlayerHealth vidaDoJogador = jogador.GetComponent<PlayerHealth>();
        if (vidaDoJogador != null)
        {
            vidaDoJogador.TomarDano(danoDoAtaque);
        }
    }

    private void AtualizarDestino()
    {
        bool destinoMoveu = (jogador.position - ultimoDestinoJogador).sqrMagnitude >=
                            distanciaMinimaParaRecalcular * distanciaMinimaParaRecalcular;
        bool semCaminho = !agente.hasPath && !agente.pathPending;
        bool caminhoInvalido = !agente.pathPending && agente.pathStatus != NavMeshPathStatus.PathComplete;

        if (!EnemyNavigationRules.ShouldRepath(Time.time, proximoRecalculoRota, destinoMoveu, semCaminho, caminhoInvalido))
            return;

        proximoRecalculoRota = Time.time + Mathf.Max(0.05f, intervaloRecalculoRota);

        if (TryObterDestino(out Vector3 destino))
        {
            agente.SetDestination(destino);
            ultimoDestinoJogador = jogador.position;
        }
        else
        {
            TentarAbrirPortasProximas();
        }
    }

    private bool TryObterDestino(out Vector3 destino)
    {
        destino = jogador.position;

        if (!NavMesh.SamplePosition(jogador.position, out NavMeshHit hit, Mathf.Max(0.1f, raioBuscaJogadorNavMesh), NavMesh.AllAreas))
            return false;

        destino = hit.position;
        caminhoCalculado ??= new NavMeshPath();

        if (!NavMesh.CalculatePath(transform.position, destino, NavMesh.AllAreas, caminhoCalculado))
            return false;

        if (caminhoCalculado.status == NavMeshPathStatus.PathComplete)
            return true;

        if (caminhoCalculado.status == NavMeshPathStatus.PathPartial && caminhoCalculado.corners.Length > 1)
        {
            destino = caminhoCalculado.corners[caminhoCalculado.corners.Length - 1];
            TentarAbrirPortasProximas();
            return true;
        }

        return false;
    }

    private void AvaliarTravamento()
    {
        if (Time.time < proximaAvaliacaoTravamento)
            return;

        float agora = Time.time;
        float elapsed = Mathf.Max(0.001f, agora - ultimoTempoAvaliacaoTravamento);
        float deslocamento = Vector3.Distance(transform.position, ultimaPosicaoProgresso);
        bool deveriaMover = agente.hasPath && !agente.pathPending && agente.remainingDistance > agente.stoppingDistance + 0.25f;

        if (deveriaMover &&
            EnemyNavigationRules.IsStuck(deslocamento, distanciaMinimaProgresso, agente.velocity.magnitude, velocidadeMinimaMovimento, elapsed))
        {
            tempoTravado += elapsed;
        }
        else
        {
            tempoTravado = 0f;
        }

        ultimaPosicaoProgresso = transform.position;
        ultimoTempoAvaliacaoTravamento = agora;
        proximaAvaliacaoTravamento = agora + Mathf.Max(0.1f, intervaloAvaliacaoTravamento);

        if (EnemyNavigationRules.ShouldRecoverFromStuck(tempoTravado, tempoParaConsiderarTravado))
        {
            RecuperarDeTravamento();
        }
    }

    private void RecuperarDeTravamento()
    {
        tempoTravado = 0f;
        TentarAbrirPortasProximas();

        if (!agente.enabled)
            return;

        if (!agente.isOnNavMesh)
        {
            TentarReposicionarNaNavMesh();
            return;
        }

        agente.ResetPath();
        proximoRecalculoRota = 0f;

        if (TryObterDestino(out Vector3 destino))
        {
            agente.SetDestination(destino);
        }
    }

    private void TentarAbrirPortasProximas()
    {
        Collider[] proximos = Physics.OverlapSphere(
            transform.position,
            Mathf.Max(0.1f, raioAberturaPorta),
            ~0,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < proximos.Length; i++)
        {
            Collider colisor = proximos[i];
            if (colisor == null || colisor.transform == transform)
                continue;

            colisor.SendMessageUpwards("AbrirPorInimigo", transform.position, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void TentarReposicionarNaNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agente.Warp(hit.position);
            agente.ResetPath();
            proximoRecalculoRota = 0f;
        }
    }

    private void OlharParaJogador()
    {
        if (jogador == null)
            return;

        Vector3 direcao = jogador.position - transform.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direcao.normalized, Vector3.up),
            Time.deltaTime * 8f);
    }

    private void AtualizarAnimacao(bool andando)
    {
        if (anim != null)
        {
            anim.SetBool("isWalking", andando);
        }
    }

    private void ConfigurarAgente()
    {
        if (agente == null)
            return;

        agente.autoRepath = true;
        agente.autoBraking = false;
        agente.stoppingDistance = Mathf.Max(0.1f, distanciaDeAtaque * 0.8f);
    }

    private void ConfigurarAudio()
    {
        if (!usarAudioDeMonstro)
            return;

        GarantirFontesDeAudio();

        ConfigurarFonteMonstro(audioMonstro, loop: true, priority: 32);
        ConfigurarFonteMonstro(audioAtaqueMonstro, loop: false, priority: 24);

        AudioClip clipAmbiente = somAmbienteMonstro != null ? somAmbienteMonstro : ObterSomAmbienteFallback();
        if (audioMonstro.clip == null || audioMonstro.clip != clipAmbiente)
        {
            audioMonstro.clip = clipAmbiente;
        }

        audioMonstro.pitch = Mathf.Clamp(pitchAmbiente, 0.75f, 1.25f);
    }

    private void AtualizarAudioAmbiente(float distanciaParaJogador, bool temJogador)
    {
        if (!usarAudioDeMonstro)
            return;

        ConfigurarAudio();
        if (audioMonstro == null || audioMonstro.clip == null)
            return;

        float volume = temJogador
            ? MonsterAudioRules.CalculateProximityVolume(
                distanciaParaJogador,
                distanciaVolumeMaximo,
                distanciaSomAudivel,
                volumeMinimoMonstro,
                volumeMaximoMonstro)
            : 0f;

        audioMonstro.volume = volume;
        if (volume > 0f)
        {
            if (!audioMonstro.isPlaying)
            {
                audioMonstro.Play();
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (!ambienteAudivelReportado && volume >= 0.15f)
            {
                ambienteAudivelReportado = true;
                Debug.Log($"MonsterAudio: ambient horror growl audible. clip={audioMonstro.clip.name}, distance={distanciaParaJogador:0.0}, volume={volume:0.00}");
            }
#endif
        }
        else if (audioMonstro.isPlaying)
        {
            audioMonstro.Stop();
        }
    }

    private void TocarSomAtaque(float distanciaParaJogador)
    {
        if (!usarAudioDeMonstro || !MonsterAudioRules.ShouldPlayAttackSound(jogador != null, distanciaParaJogador, distanciaDeAtaque))
            return;

        ConfigurarAudio();
        if (audioAtaqueMonstro == null)
            return;

        AudioClip clip = somAtaqueMonstro != null ? somAtaqueMonstro : ObterSomAtaqueFallback();
        if (clip == null)
            return;

        audioAtaqueMonstro.pitch = Random.Range(1f - Mathf.Abs(variacaoPitch), 1f + Mathf.Abs(variacaoPitch));
        audioAtaqueMonstro.PlayOneShot(clip, Mathf.Clamp01(volumeAtaque));

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (!ataqueReportado)
        {
            ataqueReportado = true;
            Debug.Log($"MonsterAudio: attack horror growl played. clip={clip.name}, distance={distanciaParaJogador:0.0}, volume={Mathf.Clamp01(volumeAtaque):0.00}");
        }
#endif
    }

    private void GarantirFontesDeAudio()
    {
        AudioSource[] fontes = GetComponents<AudioSource>();
        if (fontes.Length == 0)
        {
            audioMonstro = gameObject.AddComponent<AudioSource>();
            audioAtaqueMonstro = gameObject.AddComponent<AudioSource>();
            return;
        }

        audioMonstro = fontes[0];
        audioAtaqueMonstro = fontes.Length >= 2 ? fontes[1] : gameObject.AddComponent<AudioSource>();
    }

    public bool TodasAsFontesDeAudioParadas()
    {
        AudioSource[] fontes = GetComponents<AudioSource>();
        for (int i = 0; i < fontes.Length; i++)
        {
            AudioSource fonte = fontes[i];
            if (fonte != null && (fonte.isPlaying || fonte.volume > 0f))
                return false;
        }

        return true;
    }

    private void ConfigurarFonteMonstro(AudioSource fonte, bool loop, int priority)
    {
        if (fonte == null)
            return;

        fonte.playOnAwake = false;
        fonte.loop = loop;
        fonte.spatialBlend = 0.78f;
        fonte.rolloffMode = AudioRolloffMode.Linear;
        fonte.minDistance = Mathf.Max(0.1f, distanciaVolumeMaximo);
        fonte.maxDistance = Mathf.Max(fonte.minDistance + 0.1f, distanciaSomAudivel);
        fonte.dopplerLevel = 0f;
        fonte.spread = 55f;
        fonte.priority = priority;
        fonte.reverbZoneMix = 0f;
    }

    private void TocarOneShotEspacial(AudioClip clip, Vector3 posicao, float volume, float pitch)
    {
        GameObject audioObject = new GameObject("MonsterDeathGrowl_OneShot");
        audioObject.transform.position = posicao;

        AudioSource fonte = audioObject.AddComponent<AudioSource>();
        ConfigurarFonteMonstro(fonte, loop: false, priority: 20);
        fonte.clip = clip;
        fonte.volume = volume;
        fonte.pitch = Mathf.Clamp(pitch, 0.75f, 1.25f);
        fonte.Play();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(audioObject);
            return;
        }
#endif

        Object.Destroy(audioObject, clip.length / Mathf.Max(0.01f, fonte.pitch) + 0.25f);
    }

    private static AudioClip ObterSomAmbienteFallback()
    {
        AudioClip recurso = ObterSomAmbienteRecurso();
        if (recurso != null)
            return recurso;

        if (somAmbienteFallback != null)
            return somAmbienteFallback;

        const int sampleRate = 22050;
        const float duration = 1.8f;
        int totalSamples = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 0.55f + Mathf.Sin(2f * Mathf.PI * 0.8f * t) * 0.25f;
            float lowGrowl = Mathf.Sin(2f * Mathf.PI * 58f * t);
            float roughness = Mathf.Sin(2f * Mathf.PI * 91f * t + Mathf.Sin(2f * Mathf.PI * 3.1f * t));
            samples[i] = (lowGrowl * 0.33f + roughness * 0.19f) * envelope;
        }

        somAmbienteFallback = AudioClip.Create("OriginalMonsterPresence", totalSamples, 1, sampleRate, false);
        somAmbienteFallback.SetData(samples, 0);
        return somAmbienteFallback;
    }

    private static AudioClip ObterSomAtaqueFallback()
    {
        AudioClip recurso = ObterSomAtaqueRecurso();
        if (recurso != null)
            return recurso;

        if (somAtaqueFallback != null)
            return somAtaqueFallback;

        const int sampleRate = 22050;
        const float duration = 0.55f;
        int totalSamples = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - t / duration);
            float snarl = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 72f, t / duration) * t);
            float bite = t < 0.08f ? Mathf.Sin(2f * Mathf.PI * 420f * t) : 0f;
            samples[i] = (snarl * 0.45f + bite * 0.28f) * envelope;
        }

        somAtaqueFallback = AudioClip.Create("OriginalMonsterAttack", totalSamples, 1, sampleRate, false);
        somAtaqueFallback.SetData(samples, 0);
        return somAtaqueFallback;
    }

    private static AudioClip ObterSomMorteFallback()
    {
        AudioClip recurso = ObterSomMorteRecurso();
        if (recurso != null)
            return recurso;

        const int sampleRate = 22050;
        const float duration = 1.8f;
        int totalSamples = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - t / duration);
            float throat = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(135f, 55f, t / duration) * t);
            float rattle = Mathf.Sin(2f * Mathf.PI * 82f * t + Mathf.Sin(2f * Mathf.PI * 19f * t) * 2f);
            samples[i] = (throat * 0.34f + rattle * 0.24f) * envelope * envelope;
        }

        AudioClip fallback = AudioClip.Create("OriginalMonsterDeath", totalSamples, 1, sampleRate, false);
        fallback.SetData(samples, 0);
        return fallback;
    }

    private static AudioClip ObterSomAmbienteRecurso()
    {
        if (!tentouCarregarAmbienteRecurso)
        {
            tentouCarregarAmbienteRecurso = true;
            somAmbienteRecurso = Resources.Load<AudioClip>(MonsterAudioRules.AmbientGrowlResourcePath);
        }

        return somAmbienteRecurso;
    }

    private static AudioClip ObterSomAtaqueRecurso()
    {
        if (!tentouCarregarAtaqueRecurso)
        {
            tentouCarregarAtaqueRecurso = true;
            somAtaqueRecurso = Resources.Load<AudioClip>(MonsterAudioRules.AttackGrowlResourcePath);
        }

        return somAtaqueRecurso;
    }

    private static AudioClip ObterSomMorteRecurso()
    {
        if (!tentouCarregarMorteRecurso)
        {
            tentouCarregarMorteRecurso = true;
            somMorteRecurso = Resources.Load<AudioClip>(MonsterAudioRules.DeathGrowlResourcePath);
        }

        return somMorteRecurso;
    }

    void ResetarMovimento()
    {
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = false;
        }
    }
}
