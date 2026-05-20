using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Essa linha garante que a Unity não deixe você esquecer de colocar o componente de física no jogador
[RequireComponent(typeof(Rigidbody))]
public class MovimentoMobile : MonoBehaviour
{
    [Header("Arraste o Joystick Visível (Esquerdo) aqui:")]
    public Joystick joystickMovimento;

    public float velocidadeDeCaminhada = 5f;

    private Rigidbody rb;

    void Start()
    {
        // Pega o corpo físico do jogador
        rb = GetComponent<Rigidbody>();

        // Congela a rotação para o jogador não cair de cara no chão como um pino de boliche
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (joystickMovimento == null) return;

        // Lê para onde o jogador está arrastando o dedo no joystick esquerdo
        float direcaoX = joystickMovimento.Horizontal;
        float direcaoZ = joystickMovimento.Vertical;

        // Calcula a direção levando em conta para onde o corpo está virado.
        // Assim, se ele olhar para a direita e apertar "frente", ele vai para a direita do mapa!
        Vector3 movimento = MobileMovementVector.CalculateWorldDirection(
            transform.right,
            transform.forward,
            direcaoX,
            direcaoZ);

        // Aplica a força da caminhada. O "rb.velocity.y" mantém a gravidade normal funcionando.
        rb.velocity = new Vector3(movimento.x * velocidadeDeCaminhada, rb.velocity.y, movimento.z * velocidadeDeCaminhada);
    }
}
