using UnityEngine;

/// <summary>
/// O treinador corre o percurso junto com o Alex, no papel de PACER - nao de
/// adversario.
///
/// Por que ele existe: a etapa 1 tem quatro nuvens cacando o jogador e a etapa 3
/// tem quatro rivais do lado dele. A etapa 2 tinha uma pessoa sozinha numa cidade
/// deserta com um numero descendo no canto da tela. A sensacao de vazio era
/// literal - faltava alguem ali.
///
/// Ele nao pode ser vencido nem perdido: se o Alex fica para tras, o treinador
/// PARA E ESPERA, e a cobranca vem em palavra em vez de em derrota. A pressao
/// social pesa mais que o cronometro, e nao pune quem esta indo mal.
/// </summary>
public class TreinadorPacer : MonoBehaviour
{
    [Header("Percurso")]
    [Tooltip("Os mesmos pontos das auras amarelas, na ordem")]
    public Vector3[] pontos;

    [Header("Ritmo")]
    [Tooltip("Velocidade de cruzeiro, em m/s")]
    public float velocidade = 5.6f;
    [Tooltip("Distancia a partir da qual ele para e espera, em metros")]
    public float distanciaParaEsperar = 26f;
    [Tooltip("Distancia para voltar a correr depois de esperar")]
    public float distanciaParaRetomar = 16f;

    [Header("Animacao")]
    public Animator animator;
    public string parametroVelocidade = "Velocidade";

    [Header("Falas")]
    [Tooltip("Segundos entre uma cobranca e a seguinte")]
    public float intervaloDeFala = 6f;

    Transform jogador;
    int alvo;
    bool correndo;
    bool esperando;
    float relogioDeFala;
    int idVelocidade;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        idVelocidade = Animator.StringToHash(parametroVelocidade);
    }

    void Start()
    {
        ThirdPersonController j = Object.FindObjectOfType<ThirdPersonController>();
        if (j != null) jogador = j.transform;
        gameObject.SetActive(false);     // so entra em cena quando o treino comeca
    }

    /// Chamado pela Corrida quando o cronometro solta.
    public void Largar(Vector3 ondeComecar)
    {
        gameObject.SetActive(true);
        transform.position = ondeComecar;
        alvo = 0;
        correndo = true;
        esperando = false;
        relogioDeFala = intervaloDeFala;
    }

    public void Parar()
    {
        correndo = false;
        Avisar(0f);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (!correndo || gm == null || gm.Estado != EstadoJogo.Jogando) { Avisar(0f); return; }

        if (pontos == null || alvo >= pontos.Length) { Avisar(0f); return; }

        DecidirSeEspera(gm);

        if (esperando) { Avisar(0f); EncararJogador(); return; }

        Vector3 destino = pontos[alvo];
        destino.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, destino,
                                                 velocidade * Time.deltaTime);

        Vector3 frente = destino - transform.position;
        frente.y = 0f;
        if (frente.sqrMagnitude > 0.05f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(frente.normalized),
                                                  Time.deltaTime * 8f);

        Avisar(velocidade);

        // chegou neste ponto: segue para o proximo
        if (Vector3.Distance(transform.position, destino) < 1.2f)
        {
            alvo++;
            if (alvo >= pontos.Length) { correndo = false; Avisar(0f); }
        }
    }

    /// Histerese de proposito: ele para a 26 m e so volta a correr a 16 m. Com um
    /// limite so, ele piscaria entre andar e parar sempre que o Alex oscilasse em
    /// volta da distancia.
    void DecidirSeEspera(GameManager gm)
    {
        if (jogador == null) return;

        float d = Vector3.Distance(transform.position, jogador.position);

        if (!esperando && d > distanciaParaEsperar)
        {
            esperando = true;
            Falar(gm, "Nao fica para tras! Eu espero");
        }
        else if (esperando && d < distanciaParaRetomar)
        {
            esperando = false;
            Falar(gm, "Isso! Comigo agora");
        }
        else if (esperando)
        {
            relogioDeFala -= Time.deltaTime;
            if (relogioDeFala <= 0f) Falar(gm, "Vamos, Alex. O campeonato nao espera");
        }
    }

    void Falar(GameManager gm, string texto)
    {
        relogioDeFala = intervaloDeFala;
        gm.Notificar(texto);
        AudioDirector.Falar(0.9f);
    }

    void EncararJogador()
    {
        if (jogador == null) return;
        Vector3 d = jogador.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.05f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(d.normalized),
                                                  Time.deltaTime * 6f);
    }

    void Avisar(float v)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetFloat(idVelocidade, v);
    }
}
