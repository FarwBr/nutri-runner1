using UnityEngine;

/// <summary>
/// A nuvem de cheiro da pizzaria. Persegue o Alex em linha reta e ATRAVESSA PAREDES
/// (cheiro passa por tudo) - alem de ser tematico, elimina qualquer necessidade de
/// pathfinding, que seria caro. Fica mais rapida com o tempo e quando Alex come porcaria.
/// </summary>
public class NuvemCheiro : MonoBehaviour
{
    [Header("Perseguicao")]
    public Transform alvo;

    [Tooltip("Velocidade quando Alex ainda esta pesado")]
    public float velocidadeInicial = 3.0f;

    [Tooltip("Velocidade quando Alex chega no peso ideal")]
    public float velocidadeFinal = 5.9f;

    [Tooltip("Teto: fracao da velocidade de corrida do Alex que a nuvem pode atingir. " +
             "Garante que fugir sempre seja possivel. 1 = pode empatar.")]
    [Range(0.5f, 1.1f)] public float tetoDaCorrida = 0.85f;

    [Tooltip("Quantos segundos a frente ela mira. Corta a curva em vez de te seguir " +
             "pelo mesmo caminho, o que acaba com a manobra de dar a volta nela.")]
    public float antecipacao = 0.55f;

    [Tooltip("Aceleracao extra por segundo de partida, independente do peso")]
    public float aceleracaoPorTempo = 0f;

    public float altura = 1.7f;

    [Header("Dano")]
    public float raioDano = 2.8f;
    public float raioAviso = 16f;
    public float intensidade = 1f;

    [Header("Identidade")]
    public bool principal = true;

    [Header("Som")]
    public float volumeMaximo = 0.75f;
    public float alcanceSom = 42f;

    float bonus;
    float tempoRepelida;
    float relogio;
    Transform visual;
    AudioSource som;
    CharacterController ccAlvo;

    void Start()
    {
        if (alvo == null)
        {
            ThirdPersonController j = Object.FindObjectOfType<ThirdPersonController>();
            if (j != null) alvo = j.transform;
        }
        visual = transform.Find("Visual");
        if (alvo != null) ccAlvo = alvo.GetComponent<CharacterController>();
        PrepararSom();
    }

    /// Zumbido grave em 3D: da para ouvir de que lado ela vem antes de ver.
    void PrepararSom()
    {
        som = gameObject.AddComponent<AudioSource>();
        som.clip = AudioDirector.Zumbido();
        som.loop = true;
        som.playOnAwake = false;
        som.spatialBlend = 1f;              // totalmente posicional
        som.rolloffMode = AudioRolloffMode.Linear;
        som.minDistance = 4f;
        som.maxDistance = alcanceSom;
        som.volume = 0f;
        som.dopplerLevel = 0f;
        if (som.clip != null) som.Play();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (alvo == null || gm == null) return;

        Pulsar();

        if (gm.Estado != EstadoJogo.Jogando) return;

        relogio += Time.deltaTime;

        // AQUI ESTA O CORACAO DO JOGO:
        // quanto mais leve o Alex fica, mais rapido o cheiro corre atras dele.
        float vel = Mathf.Lerp(velocidadeInicial, velocidadeFinal, gm.Leveza)
                  + bonus + aceleracaoPorTempo * relogio;

        // TETO: por mais leve que o Alex fique, a nuvem nunca chega a ser mais
        // rapida do que ele correndo. Sem isso a fase vira impossivel no fim.
        float teto = gm.VelocidadeCorridaAtual * tetoDaCorrida;
        if (teto > 1f) vel = Mathf.Min(vel, teto);

        if (principal) gm.VelocidadeNuvem = vel;

        // mira onde o Alex VAI estar, nao onde ele esta
        Vector3 mira = alvo.position;
        if (ccAlvo != null)
        {
            Vector3 v = ccAlvo.velocity;
            v.y = 0f;
            mira += v * antecipacao;
        }

        Vector3 posAlvo = new Vector3(mira.x, transform.position.y, mira.z);
        Vector3 dir = posAlvo - transform.position;
        dir.y = 0f;

        Vector3 real = alvo.position - transform.position;
        real.y = 0f;
        float distancia = real.magnitude;

        bool recuando = tempoRepelida > 0f;
        if (recuando)
        {
            tempoRepelida -= Time.deltaTime;
            dir = -dir;
            vel *= 1.8f;
        }

        if (dir.sqrMagnitude > 0.001f)
            transform.position += dir.normalized * vel * Time.deltaTime;

        // flutuacao
        Vector3 p = transform.position;
        p.y = altura + Mathf.Sin(Time.time * 1.4f) * 0.22f;
        transform.position = p;

        if (!recuando && distancia < raioDano)
            gm.DanoCheiro(intensidade);

        float aperto = Mathf.Clamp01(1f - (distancia - raioDano) / raioAviso);
        if (recuando) aperto *= 0.3f;

        if (principal) gm.ReportarPerigo(aperto, distancia);

        if (som != null)
        {
            float alvoVol = Mathf.Lerp(0.18f, volumeMaximo, aperto) * (principal ? 1f : 0.6f);
            som.volume = Mathf.Lerp(som.volume, alvoVol, Time.deltaTime * 3f);
            som.pitch = Mathf.Lerp(0.85f, 1.3f, aperto);
        }
    }

    void Pulsar()
    {
        if (visual == null) return;
        float s = 1f + Mathf.Sin(Time.time * 2.3f) * 0.07f;
        visual.localScale = new Vector3(s, s * 0.92f, s);
        visual.Rotate(0f, 16f * Time.deltaTime, 0f, Space.World);
    }

    /// Alex comeu porcaria: o cheiro fica mais forte de vez.
    public void Acelerar(float valor) { bonus += valor; }

    /// Agua dispersa o cheiro por alguns segundos.
    public void Repelir(float segundos) { tempoRepelida = Mathf.Max(tempoRepelida, segundos); }
}
