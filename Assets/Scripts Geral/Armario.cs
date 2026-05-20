using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armario : MonoBehaviour
{
    [Header("O que tem aqui dentro?")]
    public bool temAChave = false;
    public int quantidadeDeBalas = 0;

    // NOVA VARIÁVEL: Quantas pilhas esse móvel esconde?
    public int quantidadeDePilhas = 0;

    [HideInInspector]
    public bool jaFoiVasculhado = false;
}
