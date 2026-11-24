using UnityEngine;

/// <summary>
/// Alex em terceira pessoa. WASD anda na direcao que a camera esta olhando,
/// Shift corre, Espaco pula. Correr queima colesterol nas duas primeiras etapas
/// e gasta folego na prova final.
/// Usa CharacterController: barato, previsivel e ja resolve colisao com predios.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Velocidade (m/s)")]
    public float velCaminhada = 4.4f;
    public float velCorrida = 7.8f;
    public float suavidadeGiro = 14f;

    [Header("Pulo e gravidade")]
    public float forcaPulo = 6.0f;
    public float gravidade = -20f;

    CharacterController cc;
    Transform visual;
    Transform cam;
    float velY;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        visual = transform.Find("Visual");
    }

    void Start()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.Estado != EstadoJogo.Jogando)
        {
            // Continua aplicando gravidade para nao ficar flutuando na tela de fim.
            velY += gravidade * Time.deltaTime;
            cc.Move(Vector3.up * velY * Time.deltaTime);
            return;
        }

        float h = Entrada.Horizontal; // A / D
        float v = Entrada.Vertical;   // W / S
        Vector3 entrada = new Vector3(h, 0f, v);
        if (entrada.sqrMagnitude > 1f) entrada.Normalize();
        bool andando = entrada.sqrMagnitude > 0.01f;

        // Direcao relativa a camera: e isso que faz o WASD "sentir" natural.
        Vector3 direcao = entrada;
        if (cam != null)
        {
            Vector3 frente = cam.forward; frente.y = 0f; frente.Normalize();
            Vector3 lado = cam.right; lado.y = 0f; lado.Normalize();
            direcao = frente * entrada.z + lado * entrada.x;
        }

        float peso01 = 0f;
        if (gm != null) peso01 = Mathf.InverseLerp(15f, 100f, gm.Peso);

        // Nas etapas 1 e 2 correr nao gasta nada: o desafio vem das nuvens e do
        // cronometro. Na prova final o folego entra, e e o peso que diz quanto
        // dele existe - e assim que a dieta paga na pista.
        bool querCorrer = andando && Entrada.Correndo;
        bool correndo = querCorrer;

        if (gm != null && gm.FolegoAtivo)
        {
            if (querCorrer && gm.GastarFolego(Time.deltaTime)) correndo = true;
            else
            {
                // Querendo correr sem folego, ou andando: nos dois casos recupera.
                correndo = false;
                gm.RecuperarFolego(Time.deltaTime);
            }
        }

        // A velocidade vem do peso (etapa 1) e do condicionamento (etapa 2).
        // Comer deixa o Alex mais rapido; treinar tambem.
        float fatorPeso = Mathf.Lerp(1.15f, 0.50f, peso01);
        if (gm != null) fatorPeso *= gm.FatorDeCondicionamento;
        float vel = (correndo ? velCorrida : velCaminhada) * fatorPeso;

        if (gm != null) gm.VelocidadeCorridaAtual = velCorrida * fatorPeso;

        // Giro do corpo para onde esta indo
        if (andando && direcao.sqrMagnitude > 0.001f)
        {
            Quaternion alvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, alvo,
                                                  Time.deltaTime * suavidadeGiro);
        }

        // Vertical
        if (cc.isGrounded)
        {
            if (velY < 0f) velY = -2f;
            if (Entrada.PulouAgora)
                velY = forcaPulo * Mathf.Lerp(1.1f, 0.65f, peso01);
        }
        else
        {
            velY += gravidade * Time.deltaTime;
        }

        Vector3 movimento = direcao * vel + Vector3.up * velY;
        cc.Move(movimento * Time.deltaTime);

        if (gm != null)
        {
            gm.ReportarEsforco(!andando ? 0f : (correndo ? 1f : 0.5f));
            gm.VelocidadeJogador = andando ? vel : 0f;
            Vector3 plano = new Vector3(movimento.x, 0f, movimento.z);
            gm.RegistrarDistancia(plano.magnitude * Time.deltaTime);
        }

        AtualizarCorpo(peso01);
    }

    /// Deformacao do corpo pelo peso. O balanco de bracos e pernas fica no AnimadorSimples.
    void AtualizarCorpo(float peso01)
    {
        if (visual == null) return;

        float largura = Mathf.Lerp(0.82f, 1.45f, peso01);
        Vector3 alvo = new Vector3(largura, 1f, largura);
        visual.localScale = Vector3.Lerp(visual.localScale, alvo, Time.deltaTime * 4f);
    }
}
