using UnityEngine;

/// <summary>
/// O final: quando o Alex ganha a prova, ele sobe ao podio e ergue a taca.
///
/// Sao tres tempos, nessa ordem:
///   1. desliga o controle, acende a pose de comemorar e desliza ate o degrau do ouro;
///   2. chegando la, espera um instante;
///   3. a taca aparece pendurada no OSSO DA MAO, que o Avatar Humanoid entrega
///      pronto - assim ela acompanha a animacao sem nenhum encaixe feito na mao.
///
/// A pose entra ja no comeco do deslocamento, nao no fim: sao sete decimos de
/// segundo de deslize com o corpo em festa, o que le como corte de cena. Trocar a
/// pose so na chegada daria um Alex parado deslizando feito estatua.
/// </summary>
public class Comemoracao : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;
    public GameObject taca;
    public OrbitCamera cameraOrbital;

    [Header("Podio")]
    [Tooltip("Marcador no topo do degrau do ouro. Vazio = comemora onde parou.")]
    public Transform podio;
    [Tooltip("Segundos do deslocamento ate o podio")]
    public float tempoDeSubida = 0.7f;

    [Header("Encaixe da taca na mao")]
    [Tooltip("Ajuste fino caso a taca fique torta ou atravessando a mao")]
    public Vector3 deslocamento = new Vector3(0.02f, 0.06f, 0.03f);
    public Vector3 rotacao = new Vector3(0f, 0f, 10f);
    public float escala = 1f;

    [Header("Ritmo")]
    [Tooltip("Espera depois de chegar ao podio, antes de erguer a taca")]
    public float esperaAntesDeErguer = 0.45f;

    const string ESTADO = "Comemorando";

    enum Etapa { Esperando, Subindo, NoPodio, Erguida }
    Etapa etapa = Etapa.Esperando;

    float relogio;
    Vector3 origem;
    Quaternion giroOrigem;
    Vector3 destino;
    Quaternion giroDestino;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (cameraOrbital == null) cameraOrbital = Object.FindObjectOfType<OrbitCamera>();
        if (taca != null) taca.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        switch (etapa)
        {
            case Etapa.Esperando:
                if (gm.Estado == EstadoJogo.Vitoria) Iniciar();
                break;

            case Etapa.Subindo:
                Subir();
                break;

            case Etapa.NoPodio:
                relogio += Time.deltaTime;
                if (relogio >= esperaAntesDeErguer) Erguer();
                break;
        }
    }

    void Iniciar()
    {
        // Solta o controle do jogador: dali em diante quem move o Alex e este script.
        ThirdPersonController controle = GetComponent<ThirdPersonController>();
        if (controle != null) controle.enabled = false;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.CrossFade(ESTADO, 0.25f);

        if (cameraOrbital != null) cameraOrbital.Comemorar(transform);
        AudioDirector.Tocar(false);

        relogio = 0f;

        if (podio == null)
        {
            // Sem podio na cena, comemora onde parou. O jogo nao quebra por isso.
            etapa = Etapa.NoPodio;
            return;
        }

        origem = transform.position;
        giroOrigem = transform.rotation;
        destino = podio.position;
        giroDestino = podio.rotation;
        etapa = Etapa.Subindo;
    }

    void Subir()
    {
        relogio += Time.deltaTime;
        float t = Mathf.Clamp01(relogio / Mathf.Max(0.01f, tempoDeSubida));

        // suaviza a entrada e a saida, para nao parecer teletransporte
        float s = t * t * (3f - 2f * t);

        transform.position = Vector3.Lerp(origem, destino, s);
        transform.rotation = Quaternion.Slerp(giroOrigem, giroDestino, s);

        if (t >= 1f)
        {
            etapa = Etapa.NoPodio;
            relogio = 0f;
        }
    }

    void Erguer()
    {
        etapa = Etapa.Erguida;
        if (taca == null) return;

        Transform mao = null;
        if (animator != null && animator.isHuman)
            mao = animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (mao != null)
        {
            taca.transform.SetParent(mao, false);
            taca.transform.localPosition = deslocamento;
            taca.transform.localRotation = Quaternion.Euler(rotacao);
            taca.transform.localScale = Vector3.one * escala;
        }
        else
        {
            // sem esqueleto humanoide (boneco de primitivas): segura acima da cabeca
            taca.transform.SetParent(transform, false);
            taca.transform.localPosition = new Vector3(0.35f, 2.15f, 0.1f);
            taca.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
        }

        taca.SetActive(true);
        AudioDirector.Tocar(false);
    }
}
