using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GeradorDeInimigos : MonoBehaviour
{
    [Header("Configuracoes do monstro")]
    public GameObject prefabInimigo;
    public int limiteDeMonstros = 3;

    [Header("Tempo e locais")]
    public Transform[] pontosDeSpawn;
    public bool spawnImediatoAoIniciar = true;
    public bool usarNavMeshInteiraComoSpawn = true;
    public bool exigirCaminhoCompletoAteJogador = true;
    public bool exigirSpawnForaDoCampoDeVisao = true;
    public bool usarPontosFixosComoFallback = true;
    public float tempoAntesDoPrimeiroSpawn = EnemySpawnRules.DefaultSpawnIntervalSeconds;
    public float tempoEntreSpawns = EnemySpawnRules.DefaultSpawnIntervalSeconds;
    public float raioBuscaNavMesh = 3f;
    public float raioBuscaJogadorNavMesh = 3f;
    public float distanciaMinimaDoJogador = EnemySpawnRules.DefaultMinimumSpawnDistanceFromPlayer;
    public float margemCampoVisaoSpawn = EnemySpawnRules.DefaultViewSafetyViewportMargin;
    public float alturaVisualInimigoParaSpawn = 2.2f;
    public int tentativasSpawnNavMesh = 128;

    [Header("Performance")]
    public bool usarPool = true;
    public int tamanhoInicialPool = 3;

    private int monstrosAtuais = 0;
    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly HashSet<GameObject> monstrosVivos = new HashSet<GameObject>();
    private readonly HashSet<GameObject> monstrosAguardandoPool = new HashSet<GameObject>();
    private NavMeshPath caminhoTeste;
    private Transform jogador;
    private Camera cameraJogador;
    private EnemySpawnExclusionZone[] zonasExclusaoSpawn = new EnemySpawnExclusionZone[0];
    private NavMeshTriangulation triangulacaoNavMesh;
    private float[] pesoAcumuladoTriangulos = new float[0];
    private float pesoTotalTriangulos;

    void Start()
    {
        if (prefabInimigo == null)
        {
            Debug.LogWarning("Spawner de inimigos sem prefab configurado.");
            enabled = false;
            return;
        }

        if (!usarNavMeshInteiraComoSpawn && (pontosDeSpawn == null || pontosDeSpawn.Length == 0))
        {
            Debug.LogWarning("Spawner sem NavMesh global e sem pontos fixos de spawn.");
            enabled = false;
            return;
        }

        tempoAntesDoPrimeiroSpawn = EnemySpawnRules.SanitizeSpawnDelay(tempoAntesDoPrimeiroSpawn);
        tempoEntreSpawns = EnemySpawnRules.SanitizeSpawnDelay(tempoEntreSpawns);
        distanciaMinimaDoJogador = EnemySpawnRules.SanitizeMinimumSpawnDistance(distanciaMinimaDoJogador);
        margemCampoVisaoSpawn = EnemySpawnRules.SanitizeViewSafetyMargin(margemCampoVisaoSpawn);
        alturaVisualInimigoParaSpawn = Mathf.Max(0.1f, alturaVisualInimigoParaSpawn);
        tentativasSpawnNavMesh = Mathf.Max(1, tentativasSpawnNavMesh);
        PrepararValidacaoSpawn();

        if (usarPool)
        {
            PrepararPool();
        }

        StartCoroutine(RotinaDeSpawn());
    }

    IEnumerator RotinaDeSpawn()
    {
        float atrasoInicial = EnemySpawnRules.GetFirstSpawnDelay(spawnImediatoAoIniciar, tempoAntesDoPrimeiroSpawn);
        if (atrasoInicial > 0f)
        {
            yield return new WaitForSeconds(atrasoInicial);
        }

        while (true)
        {
            if (monstrosAtuais < limiteDeMonstros)
            {
                GerarMonstro();
            }

            yield return new WaitForSeconds(tempoEntreSpawns);
        }
    }

    void GerarMonstro()
    {
        if (!EnemySpawnRules.CanSpawn(monstrosAtuais, limiteDeMonstros)) return;

        if (!TryGetValidatedSpawnPose(out Vector3 posicao, out Quaternion rotacao))
        {
            Debug.LogWarning("Spawner nao encontrou um ponto de NavMesh com caminho completo ate o jogador.");
            return;
        }

        GameObject monstro = ObterMonstro();
        EnemyHealth vida = monstro.GetComponent<EnemyHealth>();

        if (vida != null)
        {
            vida.ResetarParaSpawn(posicao, rotacao, this);
        }
        else
        {
            monstro.transform.SetPositionAndRotation(posicao, rotacao);
            monstro.SetActive(true);
        }

        monstrosVivos.Add(monstro);
        monstrosAtuais++;
        Debug.Log("Novo monstro gerado com rota valida ate o jogador. Total ativo: " + monstrosAtuais);
    }

    public void MonstroMorreu()
    {
        monstrosAtuais = Mathf.Max(0, monstrosAtuais - 1);
    }

    public void MonstroMorreu(EnemyHealth monstro, float atrasoParaPool)
    {
        if (monstro == null)
            return;

        GameObject objeto = monstro.gameObject;
        if (!monstrosVivos.Remove(objeto))
            return;

        monstrosAtuais = Mathf.Max(0, monstrosAtuais - 1);
        Debug.Log("Monstro morreu. Total ativo: " + monstrosAtuais);

        if (monstrosAguardandoPool.Add(objeto))
        {
            StartCoroutine(DesativarOuDestruirDepois(objeto, Mathf.Max(0f, atrasoParaPool)));
        }
    }

    private void PrepararPool()
    {
        int quantidade = EnemySpawnRules.SanitizePoolSize(tamanhoInicialPool, limiteDeMonstros);
        for (int i = 0; i < quantidade; i++)
        {
            GameObject monstro = CriarMonstroParaPool();
            monstro.SetActive(false);
            pool.Enqueue(monstro);
        }
    }

    private GameObject ObterMonstro()
    {
        if (usarPool && pool.Count > 0)
        {
            GameObject monstro = pool.Dequeue();
            monstrosAguardandoPool.Remove(monstro);
            return monstro;
        }

        return CriarMonstroParaPool();
    }

    private GameObject CriarMonstroParaPool()
    {
        GameObject monstro = Instantiate(prefabInimigo);
        EnemyHealth vida = monstro.GetComponent<EnemyHealth>();
        if (vida != null)
        {
            vida.ConfigurarGerador(this);
        }

        return monstro;
    }

    private IEnumerator DesativarOuDestruirDepois(GameObject monstro, float atraso)
    {
        if (atraso > 0f)
        {
            yield return new WaitForSeconds(atraso);
        }

        if (monstro == null)
            yield break;

        monstrosAguardandoPool.Remove(monstro);

        if (usarPool)
        {
            monstro.SetActive(false);
            pool.Enqueue(monstro);
        }
        else
        {
            Destroy(monstro);
        }
    }

    public bool TryGetValidatedSpawnPose(out Vector3 posicao, out Quaternion rotacao)
    {
        PrepararValidacaoSpawn();

        posicao = Vector3.zero;
        rotacao = Quaternion.identity;

        if (usarNavMeshInteiraComoSpawn && TryObterSpawnNaNavMesh(distanciaMinimaDoJogador, out posicao))
        {
            rotacao = ObterRotacaoOlhandoParaJogador(posicao, Quaternion.identity);
            return true;
        }

        if (usarPontosFixosComoFallback && TryObterSpawnEmPontosFixos(distanciaMinimaDoJogador, out posicao, out rotacao))
        {
            return true;
        }

        return false;
    }

    private bool TryObterSpawnNaNavMesh(float distanciaMinima, out Vector3 posicao)
    {
        posicao = Vector3.zero;

        if (pesoTotalTriangulos <= 0f || triangulacaoNavMesh.vertices == null || triangulacaoNavMesh.vertices.Length == 0)
        {
            RecarregarTriangulacaoNavMesh();
        }

        if (pesoTotalTriangulos <= 0f)
            return false;

        for (int tentativa = 0; tentativa < tentativasSpawnNavMesh; tentativa++)
        {
            Vector3 candidato = SortearPontoNaTriangulacao();
            if (TryValidarCandidato(candidato, distanciaMinima, out posicao))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryObterSpawnEmPontosFixos(float distanciaMinima, out Vector3 posicao, out Quaternion rotacao)
    {
        posicao = Vector3.zero;
        rotacao = Quaternion.identity;

        if (pontosDeSpawn == null || pontosDeSpawn.Length == 0)
            return false;

        int inicio = Random.Range(0, pontosDeSpawn.Length);
        for (int i = 0; i < pontosDeSpawn.Length; i++)
        {
            int indice = (inicio + i) % pontosDeSpawn.Length;
            Transform ponto = pontosDeSpawn[indice];
            if (ponto == null)
                continue;

            if (TryValidarCandidato(ponto.position, distanciaMinima, out posicao))
            {
                rotacao = ObterRotacaoOlhandoParaJogador(posicao, ponto.rotation);
                return true;
            }
        }

        return false;
    }

    private bool TryValidarCandidato(Vector3 posicaoOriginal, float distanciaMinima, out Vector3 posicaoNavMesh)
    {
        posicaoNavMesh = Vector3.zero;

        if (!NavMesh.SamplePosition(posicaoOriginal, out NavMeshHit hit, Mathf.Max(0.1f, raioBuscaNavMesh), NavMesh.AllAreas))
        {
            return false;
        }

        bool temCaminhoCompleto = !exigirCaminhoCompletoAteJogador;
        float distancia = float.MaxValue;

        if (TryObterPosicaoJogadorNaNavMesh(out Vector3 posicaoJogadorNavMesh))
        {
            caminhoTeste ??= new NavMeshPath();
            distancia = Vector3.Distance(hit.position, posicaoJogadorNavMesh);
            temCaminhoCompleto = NavMesh.CalculatePath(hit.position, posicaoJogadorNavMesh, NavMesh.AllAreas, caminhoTeste) &&
                                  caminhoTeste.status == NavMeshPathStatus.PathComplete;
        }

        bool dentroDeZonaProibida = EstaDentroDeZonaDeExclusao(hit.position);
        bool dentroDoCampoDeVisao = EstaDentroDoCampoDeVisaoDoJogador(hit.position);
        if (!EnemySpawnRules.IsCandidateAllowed(
                isOnNavMesh: true,
                hasCompletePath: temCaminhoCompleto,
                distanceToPlayer: distancia,
                minDistanceFromPlayer: distanciaMinima,
                requireCompletePath: exigirCaminhoCompletoAteJogador,
                isInsideForbiddenArea: dentroDeZonaProibida,
                isInsidePlayerView: dentroDoCampoDeVisao))
        {
            return false;
        }

        posicaoNavMesh = hit.position;
        return true;
    }

    private bool TryObterPosicaoJogadorNaNavMesh(out Vector3 posicaoJogadorNavMesh)
    {
        posicaoJogadorNavMesh = Vector3.zero;
        AtualizarJogador();

        if (jogador == null)
            return false;

        if (NavMesh.SamplePosition(jogador.position, out NavMeshHit hit, Mathf.Max(0.1f, raioBuscaJogadorNavMesh), NavMesh.AllAreas))
        {
            posicaoJogadorNavMesh = hit.position;
            return true;
        }

        return false;
    }

    private void AtualizarJogador()
    {
        if (jogador != null && jogador.gameObject.activeInHierarchy)
            return;

        GameObject objetoJogador = GameObject.FindGameObjectWithTag("Player");
        jogador = objetoJogador != null ? objetoJogador.transform : null;
        cameraJogador = null;
    }

    private void AtualizarCameraJogador()
    {
        AtualizarJogador();

        if (cameraJogador != null && cameraJogador.isActiveAndEnabled)
            return;

        cameraJogador = null;
        if (jogador != null)
        {
            cameraJogador = jogador.GetComponentInChildren<Camera>();
        }

        if (cameraJogador == null)
        {
            cameraJogador = Camera.main;
        }
    }

    private void AtualizarZonasExclusao()
    {
        zonasExclusaoSpawn = Object.FindObjectsByType<EnemySpawnExclusionZone>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
    }

    private void PrepararValidacaoSpawn()
    {
        caminhoTeste ??= new NavMeshPath();

        if (zonasExclusaoSpawn == null || zonasExclusaoSpawn.Length == 0)
        {
            AtualizarZonasExclusao();
        }

        if (pesoTotalTriangulos <= 0f)
        {
            RecarregarTriangulacaoNavMesh();
        }

        AtualizarJogador();
        AtualizarCameraJogador();
    }

    private bool EstaDentroDeZonaDeExclusao(Vector3 posicao)
    {
        if (zonasExclusaoSpawn == null || zonasExclusaoSpawn.Length == 0)
            return false;

        for (int i = 0; i < zonasExclusaoSpawn.Length; i++)
        {
            EnemySpawnExclusionZone zona = zonasExclusaoSpawn[i];
            if (zona != null && zona.isActiveAndEnabled && zona.Contains(posicao))
            {
                return true;
            }
        }

        return false;
    }

    private bool EstaDentroDoCampoDeVisaoDoJogador(Vector3 posicao)
    {
        if (!exigirSpawnForaDoCampoDeVisao)
            return false;

        AtualizarCameraJogador();
        if (cameraJogador == null)
            return false;

        float margem = EnemySpawnRules.SanitizeViewSafetyMargin(margemCampoVisaoSpawn);
        float altura = Mathf.Max(0.1f, alturaVisualInimigoParaSpawn);
        Vector3 basePos = posicao + Vector3.up * 0.25f;
        Vector3 centroPos = posicao + Vector3.up * (altura * 0.5f);
        Vector3 topoPos = posicao + Vector3.up * altura;

        return EnemySpawnRules.IsViewportPointInsidePlayerView(cameraJogador.WorldToViewportPoint(basePos), margem) ||
               EnemySpawnRules.IsViewportPointInsidePlayerView(cameraJogador.WorldToViewportPoint(centroPos), margem) ||
               EnemySpawnRules.IsViewportPointInsidePlayerView(cameraJogador.WorldToViewportPoint(topoPos), margem);
    }

    private Quaternion ObterRotacaoOlhandoParaJogador(Vector3 posicao, Quaternion fallback)
    {
        AtualizarJogador();
        if (jogador == null)
            return fallback;

        Vector3 direcao = jogador.position - posicao;
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
            return fallback;

        return Quaternion.LookRotation(direcao.normalized, Vector3.up);
    }

    private void RecarregarTriangulacaoNavMesh()
    {
        triangulacaoNavMesh = NavMesh.CalculateTriangulation();
        int totalTriangulos = triangulacaoNavMesh.indices != null ? triangulacaoNavMesh.indices.Length / 3 : 0;

        if (pesoAcumuladoTriangulos.Length != totalTriangulos)
        {
            pesoAcumuladoTriangulos = new float[totalTriangulos];
        }

        pesoTotalTriangulos = 0f;
        for (int i = 0; i < totalTriangulos; i++)
        {
            Vector3 a = triangulacaoNavMesh.vertices[triangulacaoNavMesh.indices[i * 3]];
            Vector3 b = triangulacaoNavMesh.vertices[triangulacaoNavMesh.indices[i * 3 + 1]];
            Vector3 c = triangulacaoNavMesh.vertices[triangulacaoNavMesh.indices[i * 3 + 2]];

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            pesoTotalTriangulos += Mathf.Max(0.0001f, area);
            pesoAcumuladoTriangulos[i] = pesoTotalTriangulos;
        }
    }

    private Vector3 SortearPontoNaTriangulacao()
    {
        if (pesoTotalTriangulos <= 0f || pesoAcumuladoTriangulos.Length == 0)
            return transform.position;

        float escolha = Random.Range(0f, pesoTotalTriangulos);
        int indiceTriangulo = 0;

        for (int i = 0; i < pesoAcumuladoTriangulos.Length; i++)
        {
            if (escolha <= pesoAcumuladoTriangulos[i])
            {
                indiceTriangulo = i;
                break;
            }
        }

        int indiceA = triangulacaoNavMesh.indices[indiceTriangulo * 3];
        int indiceB = triangulacaoNavMesh.indices[indiceTriangulo * 3 + 1];
        int indiceC = triangulacaoNavMesh.indices[indiceTriangulo * 3 + 2];

        Vector3 a = triangulacaoNavMesh.vertices[indiceA];
        Vector3 b = triangulacaoNavMesh.vertices[indiceB];
        Vector3 c = triangulacaoNavMesh.vertices[indiceC];

        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;

        return (1f - r1) * a + r1 * (1f - r2) * b + r1 * r2 * c;
    }
}
