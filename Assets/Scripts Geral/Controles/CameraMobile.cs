using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMobile : MonoBehaviour
{
    public const float DefaultTouchSensitivity = MobileCameraRules.DefaultTouchSensitivity;

    [Header("Joystick de camera (opcional):")]
    public Joystick joystickCamera;
    public bool usarJoystickCamera = false;

    [Header("Corpo principal do jogador:")]
    public Transform corpoDoJogador;

    public float sensibilidade = 2f;

    [Header("Arraste direto na tela")]
    public bool usarToqueDireto = true;
    [Range(0.01f, 1f)]
    public float sensibilidadeToque = DefaultTouchSensitivity;
    [Range(0f, 1f)]
    public float areaCameraInicioX = 0f;
    [Range(0f, 1f)]
    public float areaCameraFimX = 1f;
    [Range(0f, 1f)]
    public float areaCameraInicioY = 0f;
    [Range(0f, 1f)]
    public float areaCameraFimY = 1f;

    private float rotacaoX = 0f;
    private readonly MobileCameraTouchPolicy touchPolicy = new MobileCameraTouchPolicy();

    void Update()
    {
        if (corpoDoJogador == null) return;

        Vector2 movimentoCamera = Vector2.zero;

        if (usarJoystickCamera && joystickCamera != null && joystickCamera.isActiveAndEnabled)
        {
            movimentoCamera += new Vector2(
                joystickCamera.Horizontal * sensibilidade,
                joystickCamera.Vertical * sensibilidade
            );
        }

        if (usarToqueDireto)
        {
            movimentoCamera += LerArrasteDireto() * sensibilidadeToque;
        }

        if (movimentoCamera.sqrMagnitude < 0.0001f) return;

        corpoDoJogador.Rotate(Vector3.up * movimentoCamera.x);

        rotacaoX -= movimentoCamera.y;
        rotacaoX = Mathf.Clamp(rotacaoX, -80f, 80f);
        transform.localRotation = Quaternion.Euler(rotacaoX, 0f, 0f);
    }

    private Vector2 LerArrasteDireto()
    {
        if (Input.touchCount == 0)
        {
            touchPolicy.Reset();
            return Vector2.zero;
        }

        Vector2 delta = Vector2.zero;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Rect safeArea = Screen.safeArea;
        Rect cameraArea = new Rect(
            areaCameraInicioX,
            areaCameraInicioY,
            areaCameraFimX - areaCameraInicioX,
            areaCameraFimY - areaCameraInicioY);

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch toque = Input.GetTouch(i);
            MobileTouchSample sample = new MobileTouchSample(toque.fingerId, toque.position, toque.phase);
            bool capturedBySceneUi = MobileTouchInputBridge.IsTouchCapturedByMobileUi(toque.fingerId) ||
                                     MobileTouchInputBridge.IsScreenPositionReservedForMobileUi(toque.position);
            bool pointerOverEventSystem =
                EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(toque.fingerId);

            delta += touchPolicy.ProcessTouch(
                sample,
                screenSize,
                safeArea,
                cameraArea,
                capturedBySceneUi,
                pointerOverEventSystem);
        }

        return delta;
    }
}
