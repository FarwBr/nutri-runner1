using UnityEngine;

/// <summary>
/// Corredor rival do campeonato. Corre pelo circuito a uma velocidade propria,
/// numa raia so dele para ninguem se sobrepor.
///
/// Nao usa pathfinding nem fisica: a PistaAtletismo converte "o quanto ja corri"
/// em posicao e direcao, entao basta somar metros. E barato e totalmente previsivel,
/// o que importa numa prova em que o jogador precisa conseguir ler quem esta na frente.
///
/// Detalhe que faz diferenca: a raia de fora e mais longa que a de dentro. O rival
/// avanca pelo perimetro DA RAIA DELE, entao mantem a velocidade real em m/s; mas o
/// progresso comparavel (Percorrido) sai convertido para a raia de medicao, que e a
/// mesma regua usada no jogador. Sem isso, quem estivesse na raia de fora pareceria
/// estar ganhando quando na verdade so estava rodando mais metros.
/// </summary>
public class Corredor : MonoBehaviour
{
    [Header("Pista")]
    public PistaAtletismo pista;
    [Tooltip("Distancia do eixo central da pista, em metros: a raia deste corredor")]
    public float raia = 19f;

    [Header("Ritmo")]
    public float velocidade = 6f;
    [Tooltip("Variacao de ritmo: corredor de verdade nao mantem passo exato")]
    public float oscilacao = 0.5f;

    [Header("Identidade")]
    public string nome = "Rival";

    [Header("Animacao")]
    public Animator animator;
    [Tooltip("Nome do float do blend tree de locomocao")]
    public string parametroVelocidade = "Velocidade";

    /// Metros de prova percorridos, na regua da raia de medicao.
    /// E o que decide a classificacao.
    public float Percorrido { get; private set; }

    public float Voltas
    {
        get
        {
            if (pista == null) return 0f;
            return Percorrido / pista.Perimetro;
        }
    }

    float t;              // posicao no circuito, 0 a 1
    float fase;
    bool correndo;
    Transform visual;
    float balanco;

    int idVelocidade;

    void Awake()
    {
        visual = transform.Find("Visual");
        fase = Random.Range(0f, 10f);

        if (animator == null) animator = GetComponentInChildren<Animator>();
        idVelocidade = Animator.StringToHash(parametroVelocidade);
    }

    /// Coloca o corredor no ponto de largada, parado.
    public void Preparar(float parametroInicial)
    {
        t = parametroInicial;
        Percorrido = 0f;
        correndo = false;
        Posicionar();
    }

    public void Largar() { correndo = true; }
    public void Parar() { correndo = false; }

    void Update()
    {
        if (pista == null) return;

        if (correndo)
        {
            // ritmo com uma leve variacao, para nao parecer um trilho
            float v = velocidade + Mathf.Sin(Time.time * 0.7f + fase) * oscilacao;
            float andou = v * Time.deltaTime;
            float perimetroDaRaia = Mathf.Max(1f, pista.PerimetroDaRaia(raia));

            // Divide pelo perimetro DA RAIA: assim o corredor cobre exatamente
            // "andou" metros no chao, e nao mais nem menos por estar na raia de fora.
            t = Mathf.Repeat(t + andou / perimetroDaRaia, 1f);

            // mas o progresso comparavel e medido na raia oficial
            Percorrido += andou * (pista.Perimetro / perimetroDaRaia);

            Posicionar();

            balanco += Time.deltaTime * (4f + v * 0.9f);
            AvisarAnimator(v);
        }
        else AvisarAnimator(0f);

        // o balanco so vale para o boneco de primitivas; com o modelo animado
        // quem inclina o corpo e a propria animacao
        if (visual != null && animator == null)
        {
            float inclina = correndo ? Mathf.Sin(balanco) * 7f : 0f;
            visual.localRotation = Quaternion.Euler(0f, 0f, inclina);
        }
    }

    void AvisarAnimator(float v)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.SetFloat(idVelocidade, v);
    }

    void Posicionar()
    {
        transform.position = pista.Ponto(t, raia);

        Vector3 frente = pista.Tangente(t);
        if (frente.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(frente.normalized);
    }
}
