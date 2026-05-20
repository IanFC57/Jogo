using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GeradorDeInimigos : MonoBehaviour
{
    [Header("Configurações do Monstro")]
    public GameObject prefabInimigo; // Arraste o Prefab do seu monstro aqui
    public int limiteDeMonstros = 3; // Limite máximo na tela para não travar o celular

    [Header("Configurações de Tempo e Local")]
    public Transform[] pontosDeSpawn; // Lista de lugares onde ele pode nascer
    public float tempoEntreSpawns = 15f; // Segundos entre cada tentativa de spawn

    // Contador interno invisível
    private int monstrosAtuais = 0;

    void Start()
    {
        // Inicia o ciclo de spawn assim que a fase começa
        StartCoroutine(RotinaDeSpawn());
    }

    IEnumerator RotinaDeSpawn()
    {
        // O loop infinito mantém o spawner funcionando a fase toda
        while (true)
        {
            // Espera o tempo definido
            yield return new WaitForSeconds(tempoEntreSpawns);

            // Só gera um novo monstro se não tiver batido o limite
            if (monstrosAtuais < limiteDeMonstros)
            {
                GerarMonstro();
            }
        }
    }

    void GerarMonstro()
    {
        // Se a lista de pontos estiver vazia, cancela para não dar erro
        if (pontosDeSpawn.Length == 0) return;

        // Escolhe um ponto de spawn aleatório da sua lista
        int indiceAleatorio = Random.Range(0, pontosDeSpawn.Length);
        Transform pontoEscolhido = pontosDeSpawn[indiceAleatorio];

        // Cria o monstro no ponto escolhido
        Instantiate(prefabInimigo, pontoEscolhido.position, pontoEscolhido.rotation);

        monstrosAtuais++;
        Debug.Log("Novo monstro gerado! Total ativo: " + monstrosAtuais);
    }

    // Você vai chamar essa função futuramente a partir do script de Vida do Monstro quando ele morrer
    public void MonstroMorreu()
    {
        monstrosAtuais--;
    }
}
