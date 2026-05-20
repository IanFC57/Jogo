using System.Collections.Generic;
using UnityEngine;

public class PortaAutomatica : MonoBehaviour
{
    public float tempoAbertaAposInimigo = 1.5f;

    private Animator anim;
    private readonly HashSet<Collider> ocupantes = new HashSet<Collider>();
    private float manterAbertaAte;
    private bool estaAberta;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        LimparOcupantesInvalidos();

        if (estaAberta && DoorAccessRules.ShouldClose(ocupantes.Count, manterAbertaAte, Time.time))
        {
            DefinirAberta(false);
        }
    }

    public void AbrirPorInimigo(Vector3 posicaoAtor)
    {
        DefinirAberta(true);
        manterAbertaAte = Time.time + tempoAbertaAposInimigo;
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (!DoorActorUtility.IsDoorActor(outro))
            return;

        ocupantes.Add(outro);
        DefinirAberta(true);
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

    private void DefinirAberta(bool aberta)
    {
        estaAberta = aberta;
        if (anim != null)
        {
            anim.SetBool("estaAberta", aberta);
        }
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
