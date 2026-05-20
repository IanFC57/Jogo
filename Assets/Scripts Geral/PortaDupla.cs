using System.Collections.Generic;
using UnityEngine;

public class PortaDupla : MonoBehaviour
{
    [Header("Arraste os dois eixos das portas aqui:")]
    public Transform eixoEsquerdo;
    public Transform eixoDireito;

    public float anguloAbertura = 90f;
    public float velocidade = 5f;
    public float tempoAbertaAposInimigo = 1.5f;

    private Quaternion rotacaoFechadaEsq;
    private Quaternion rotacaoFechadaDir;
    private Quaternion alvoRotacaoEsq;
    private Quaternion alvoRotacaoDir;
    private readonly HashSet<Collider> ocupantes = new HashSet<Collider>();
    private float manterAbertaAte;
    private bool estaAberta;

    void Awake()
    {
        rotacaoFechadaEsq = eixoEsquerdo != null ? eixoEsquerdo.localRotation : Quaternion.identity;
        rotacaoFechadaDir = eixoDireito != null ? eixoDireito.localRotation : Quaternion.identity;

        alvoRotacaoEsq = rotacaoFechadaEsq;
        alvoRotacaoDir = rotacaoFechadaDir;
    }

    void Update()
    {
        LimparOcupantesInvalidos();

        if (estaAberta && DoorAccessRules.ShouldClose(ocupantes.Count, manterAbertaAte, Time.time))
        {
            Fechar();
        }

        if (eixoEsquerdo != null)
        {
            eixoEsquerdo.localRotation = Quaternion.Slerp(eixoEsquerdo.localRotation, alvoRotacaoEsq, Time.deltaTime * velocidade);
        }

        if (eixoDireito != null)
        {
            eixoDireito.localRotation = Quaternion.Slerp(eixoDireito.localRotation, alvoRotacaoDir, Time.deltaTime * velocidade);
        }
    }

    public void AbrirPorInimigo(Vector3 posicaoAtor)
    {
        AbrirPara(posicaoAtor);
        manterAbertaAte = Time.time + tempoAbertaAposInimigo;
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (!DoorActorUtility.IsDoorActor(outro))
            return;

        ocupantes.Add(outro);
        AbrirPara(DoorActorUtility.GetActorPosition(outro));
    }

    private void OnTriggerExit(Collider outro)
    {
        if (!DoorActorUtility.IsDoorActor(outro))
            return;

        ocupantes.Remove(outro);
        if (ocupantes.Count == 0)
        {
            manterAbertaAte = Time.time + tempoAbertaAposInimigo;
        }
    }

    private void AbrirPara(Vector3 posicaoAtor)
    {
        Vector3 direcaoAtor = posicaoAtor - transform.position;
        float dot = Vector3.Dot(transform.forward, direcaoAtor);
        float multiplicador = dot >= 0f ? -1f : 1f;

        alvoRotacaoEsq = rotacaoFechadaEsq * Quaternion.Euler(0f, anguloAbertura * multiplicador, 0f);
        alvoRotacaoDir = rotacaoFechadaDir * Quaternion.Euler(0f, -anguloAbertura * multiplicador, 0f);
        manterAbertaAte = Time.time + tempoAbertaAposInimigo;
        estaAberta = true;
    }

    private void Fechar()
    {
        alvoRotacaoEsq = rotacaoFechadaEsq;
        alvoRotacaoDir = rotacaoFechadaDir;
        estaAberta = false;
    }

    private void LimparOcupantesInvalidos()
    {
        if (ocupantes.Count == 0)
            return;

        ocupantes.RemoveWhere(colisor => colisor == null || !colisor.enabled || !colisor.gameObject.activeInHierarchy);
        if (ocupantes.Count == 0)
        {
            manterAbertaAte = Mathf.Max(manterAbertaAte, Time.time + tempoAbertaAposInimigo);
        }
    }
}
