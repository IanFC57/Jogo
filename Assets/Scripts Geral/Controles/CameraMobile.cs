using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMobile : MonoBehaviour
{
    [Header("Arraste o Joystick_Camera Invisível aqui:")]
    public Joystick joystickCamera;

    [Header("Arraste o CORPO PRINCIPAL do Jogador aqui:")]
    public Transform corpoDoJogador; // NOVA VARIÁVEL: Dizemos exatamente quem ele deve girar

    public float sensibilidade = 2f;

    private float rotacaoX = 0f;

    void Update()
    {
        // Se faltar o joystick ou o corpo, ele não faz nada para evitar erros
        if (joystickCamera == null || corpoDoJogador == null) return;

        float movimentoDedoX = joystickCamera.Horizontal * sensibilidade;
        float movimentoDedoY = joystickCamera.Vertical * sensibilidade;

        // 1. Gira O CORPO EXATO que você arrastou lá no Inspector para os lados (Esquerda/Direita)
        corpoDoJogador.Rotate(Vector3.up * movimentoDedoX);

        // 2. Gira APENAS A CABEÇA (Câmera) para cima e para baixo
        rotacaoX -= movimentoDedoY;
        rotacaoX = Mathf.Clamp(rotacaoX, -80f, 80f);
        transform.localRotation = Quaternion.Euler(rotacaoX, 0f, 0f);
    }
}
