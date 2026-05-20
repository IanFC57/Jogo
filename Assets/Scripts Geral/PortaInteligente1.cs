using System.Collections.Generic;
using UnityEngine;

public class PortaInteligente1 : MonoBehaviour
{
    [Header("Arraste o objeto Porta_Eixo para ca:")]
    public Transform eixoDaPorta;

    public float anguloAbertura = 90f;
    public float velocidade = 5f;
    public float tempoAbertaAposInimigo = 1.5f;

    private Quaternion rotacaoFechada;
    private Quaternion alvoRotacao;
    private readonly HashSet<Collider> ocupantes = new HashSet<Collider>();
    private float manterAbertaAte;
    private bool estaAberta;

    void Awake()
    {
        rotacaoFechada = eixoDaPorta != null ? eixoDaPorta.localRotation : Quaternion.identity;
        alvoRotacao = rotacaoFechada;
    }

    void Update()
    {
        LimparOcupantesInvalidos();

        if (estaAberta && DoorAccessRules.ShouldClose(ocupantes.Count, manterAbertaAte, Time.time))
        {
            Fechar();
        }

        if (eixoDaPorta == null)
            return;

        eixoDaPorta.localRotation = Quaternion.Slerp(eixoDaPorta.localRotation, alvoRotacao, Time.deltaTime * velocidade);
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
        float direcaoFinal = dot >= 0f ? -anguloAbertura : anguloAbertura;

        alvoRotacao = rotacaoFechada * Quaternion.Euler(0f, direcaoFinal, 0f);
        manterAbertaAte = Time.time + tempoAbertaAposInimigo;
        estaAberta = true;
    }

    private void Fechar()
    {
        alvoRotacao = rotacaoFechada;
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
