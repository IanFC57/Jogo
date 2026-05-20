using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleRecarregamento : MonoBehaviour
{
    [Header("Arraste o seu Jogador (Inventário) aqui:")]
    public InventarioJogador inventario;

    [Header("Arraste a sua Arma aqui:")]
    public GameObject armaEquipada;

    [Header("Configuração")]
    public int tamanhoDoPente = 12; // Quantas balas entram na arma por recarga

    public void TentarRecarregar()
    {
        // O Porteiro verifica: Tem bala no bolso?
        if (inventario.balasNoBolso > 0)
        {
            // 1. Manda a ordem para o Easy Weapons fazer a animação e o som
            armaEquipada.SendMessage("Reload", SendMessageOptions.DontRequireReceiver);

            // 2. Desconta do nosso bolso o valor do pente
            inventario.balasNoBolso -= tamanhoDoPente;

            // 3. Garante que a matemática não deixe suas balas negativas
            if (inventario.balasNoBolso < 0)
            {
                inventario.balasNoBolso = 0;
            }

            Debug.Log("Recarregou! Balas no bolso: " + inventario.balasNoBolso);
        }
        else
        {
            Debug.Log("Sem balas no inventário! Vá vasculhar os armários!");
        }
    }
}
