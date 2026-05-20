using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControleLanterna : MonoBehaviour
{
    [Header("Arraste a luz da sua lanterna aqui:")]
    public GameObject luzDaLanterna;

    // Essa é a função que o botão da tela vai chamar
    public void AlternarLanterna()
    {
        if (luzDaLanterna != null)
        {
            // O comando "!luzDaLanterna.activeSelf" inverte o estado atual. 
            // Se estiver ligada, desliga. Se estiver desligada, liga.
            luzDaLanterna.SetActive(!luzDaLanterna.activeSelf);
        }
    }
}
