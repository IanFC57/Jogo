using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortaAutomatica : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        // Pega o "cérebro" das animações da porta
        anim = GetComponent<Animator>();
    }

    // Quando algo ENTRA na zona invisível da porta
    private void OnTriggerEnter(Collider outro)
    {
        // Verifica se quem pisou na zona tem a etiqueta de "Player"
        if (outro.CompareTag("Player"))
        {
            anim.SetBool("estaAberta", true); // Abre a porta
        }
    }

    // Quando algo SAI da zona invisível da porta
    private void OnTriggerExit(Collider outro)
    {
        // Verifica se quem saiu foi o "Player"
        if (outro.CompareTag("Player"))
        {
            anim.SetBool("estaAberta", false); // Fecha a porta
        }
    }
}
