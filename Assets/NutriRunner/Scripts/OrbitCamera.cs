using UnityEngine;

/// <summary>
/// Camera de terceira pessoa. O mouse orbita em volta de Alex.
/// Scroll aproxima/afasta. ESC libera o cursor (util no Editor).
/// Tem recuo automatico quando encosta em parede, para nao entrar dentro do predio.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo;
    public float alturaAlvo = 1.5f;

    [Header("Mouse")]
    public float sensibilidade = 3.0f;
    public bool inverterY = false;
    public float anguloMin = -25f;
    public float anguloMax = 65f;

    [Header("Distancia")]
    public float distancia = 6.0f;
    public float distanciaMin = 2.5f;
    public float distanciaMax = 10f;
    public float raioColisao = 0.3f;

    [Header("Comemoracao")]
    public float giroDaVitoria = 18f;     // graus por segundo em volta do heroi
    public float distanciaDaVitoria = 4.6f;
    public float alturaDaVitoria = 1.5f;

    float yaw;
    float pitch = 18f;
    float distanciaAtual;
    bool comemorando;

    void Start()
    {
        distanciaAtual = distancia;

        // Se o alvo nao foi ligado no Inspector, acha o Alex sozinho.
        if (alvo == null)
        {
            ThirdPersonController jogador = Object.FindObjectOfType<ThirdPersonController>();
            if (jogador != null) alvo = jogador.transform;
        }

        if (alvo == null)
        {
            Debug.LogWarning("OrbitCamera: nenhum alvo encontrado. Arraste o Alex no campo 'alvo'.");
            return;
        }

        yaw = alvo.eulerAngles.y;
        TravarCursor(true);
        Posicionar(true);
    }

    void Update()
    {
        if (Entrada.EscAgora) TravarCursor(false);
        if (Entrada.CliqueAgora && Cursor.lockState != CursorLockMode.Locked)
            TravarCursor(true);
    }

    void LateUpdate()
    {
        if (alvo == null) return;

        if (comemorando)
        {
            // gira sozinha em volta do Alex: nada de mouse no final
            yaw += giroDaVitoria * Time.deltaTime;
            pitch = Mathf.Lerp(pitch, 8f, Time.deltaTime * 2f);
            distancia = Mathf.Lerp(distancia, distanciaDaVitoria, Time.deltaTime * 2f);
            Posicionar(false);
            return;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Entrada.MouseX * sensibilidade;
            float dy = Entrada.MouseY * sensibilidade;
            pitch += inverterY ? dy : -dy;
            pitch = Mathf.Clamp(pitch, anguloMin, anguloMax);

            float scroll = Entrada.Scroll;
            if (Mathf.Abs(scroll) > 0.001f)
                distancia = Mathf.Clamp(distancia - scroll * 4f, distanciaMin, distanciaMax);
        }

        Posicionar(false);
    }

    void Posicionar(bool imediato)
    {
        Quaternion rotacao = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 foco = alvo.position + Vector3.up * alturaAlvo;
        Vector3 desejada = foco - rotacao * Vector3.forward * distancia;

        // Nao deixa a camera atravessar parede
        float alvoDist = distancia;
        RaycastHit hit;
        if (Physics.SphereCast(foco, raioColisao, (desejada - foco).normalized, out hit,
                               distancia, ~0, QueryTriggerInteraction.Ignore))
        {
            alvoDist = Mathf.Max(distanciaMin * 0.6f, hit.distance - 0.1f);
        }

        distanciaAtual = imediato ? alvoDist
                                  : Mathf.Lerp(distanciaAtual, alvoDist, Time.deltaTime * 12f);

        transform.position = foco - rotacao * Vector3.forward * distanciaAtual;
        transform.rotation = rotacao;
    }

    /// Entra no plano da vitoria: camera baixa, mais perto e girando sozinha.
    public void Comemorar(Transform heroi)
    {
        comemorando = true;
        if (heroi != null) alvo = heroi;
        alturaAlvo = alturaDaVitoria;
        TravarCursor(false);
    }

    public static void TravarCursor(bool travar)
    {
        Cursor.lockState = travar ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !travar;
    }
}
