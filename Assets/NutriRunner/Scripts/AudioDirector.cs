using UnityEngine;

/// <summary>
/// Gera a trilha e os efeitos por codigo (nenhum arquivo de audio no projeto).
/// A musica acelera conforme a fase avanca, como pede o GameDoc.
/// Se voce quiser usar musica propria, arraste um AudioClip em "musicaOpcional".
/// </summary>
public class AudioDirector : MonoBehaviour
{
    public static AudioDirector Instance;

    public AudioClip musicaOpcional;
    [Range(0f, 1f)] public float volumeMusica = 0.22f;
    [Range(0f, 1f)] public float volumeEfeitos = 0.5f;

    AudioSource musica, efeitos;
    AudioClip clipBom, clipRuim;

    const int TAXA = 22050; // taxa baixa = pouco processamento e memoria

    void Awake()
    {
        Instance = this;

        musica = gameObject.AddComponent<AudioSource>();
        musica.loop = true;
        musica.playOnAwake = false;
        musica.volume = volumeMusica;

        efeitos = gameObject.AddComponent<AudioSource>();
        efeitos.playOnAwake = false;
        efeitos.volume = volumeEfeitos;

        clipBom = Bip(680f, 0.12f, true);
        clipRuim = Bip(190f, 0.22f, false);

        musica.clip = (musicaOpcional != null) ? musicaOpcional : GerarLoop();
        musica.Play();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        float tensao = Mathf.Max(GameManager.Instance.Progresso,
                                 GameManager.Instance.Perigo);
        float alvo = Mathf.Lerp(1f, 1.55f, tensao);
        musica.pitch = Mathf.Lerp(musica.pitch, alvo, Time.deltaTime);
    }

    public static void Tocar(bool ruim)
    {
        if (Instance == null) return;
        Instance.efeitos.PlayOneShot(ruim ? Instance.clipRuim : Instance.clipBom);
    }

    /// Zumbido grave e oscilante da nuvem de cheiro, gerado por codigo.
    /// As frequencias sao multiplas de 0,5 Hz para o loop de 2 s emendar sem estalo.
    static AudioClip zumbido;
    public static AudioClip Zumbido()
    {
        if (zumbido != null) return zumbido;

        const int taxa = 22050;
        const float duracao = 2f;
        int n = Mathf.RoundToInt(taxa * duracao);
        float[] dados = new float[n];

        float f1 = 58f, f2 = 61.5f, f3 = 121f;   // graves levemente desafinados
        float p1 = 0f, p2 = 0f, p3 = 0f;
        System.Random rnd = new System.Random(7);
        float ruido = 0f;

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / taxa;

            p1 += 2f * Mathf.PI * f1 / taxa;
            p2 += 2f * Mathf.PI * f2 / taxa;
            p3 += 2f * Mathf.PI * f3 / taxa;

            // ruido filtrado: da a textura de "gas" em cima dos graves
            ruido = ruido * 0.97f + ((float)rnd.NextDouble() * 2f - 1f) * 0.03f;

            float ondulacao = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * 1f * t);

            dados[i] = (Mathf.Sin(p1) * 0.50f
                      + Mathf.Sin(p2) * 0.34f
                      + Mathf.Sin(p3) * 0.11f
                      + ruido * 1.6f) * ondulacao * 0.5f;
        }

        zumbido = AudioClip.Create("zumbido", n, 1, taxa, false);
        zumbido.SetData(dados, 0);
        return zumbido;
    }

    // ---------------- vozes de dialogo ----------------
    // Som de fala sem palavra nenhuma, no estilo dos aldeoes: uma nota curta com
    // harmonicos e uma dobra de altura. Cada personagem usa o mesmo clipe com
    // um pitch diferente, entao basta UM clipe na memoria para todas as vozes.

    static AudioClip vozClipe;
    static AudioSource vozFonte;

    static AudioClip ClipeDeVoz()
    {
        if (vozClipe != null) return vozClipe;

        const int taxa = 22050;
        const float duracao = 0.13f;
        int n = Mathf.RoundToInt(taxa * duracao);
        float[] dados = new float[n];

        float fase = 0f, fase2 = 0f, fase3 = 0f;

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;

            // a altura sobe e volta: e isso que da o "jeito de fala"
            float f = 310f * (1f + 0.22f * Mathf.Sin(t * Mathf.PI));

            fase  += 2f * Mathf.PI * f / taxa;
            fase2 += 2f * Mathf.PI * f * 2f / taxa;
            fase3 += 2f * Mathf.PI * f * 3f / taxa;

            // harmonicos dao o timbre de voz em vez de apito puro
            float onda = Mathf.Sin(fase) * 0.62f
                       + Mathf.Sin(fase2) * 0.26f
                       + Mathf.Sin(fase3) * 0.12f;

            // ataque rapido, queda suave
            float env = Mathf.Clamp01(t * 14f) * Mathf.Clamp01((1f - t) * 3.2f);

            dados[i] = onda * env * 0.5f;
        }

        vozClipe = AudioClip.Create("voz", n, 1, taxa, false);
        vozClipe.SetData(dados, 0);
        return vozClipe;
    }

    /// Uma silaba. "altura" muda o personagem: grave = medico, agudo = Alex.
    public static void Falar(float altura)
    {
        if (Instance == null) return;

        if (vozFonte == null)
        {
            vozFonte = Instance.gameObject.AddComponent<AudioSource>();
            vozFonte.playOnAwake = false;
            vozFonte.volume = 0.32f;
        }

        vozFonte.pitch = altura * Random.Range(0.93f, 1.07f);
        vozFonte.PlayOneShot(ClipeDeVoz());
    }

    // ---------------- passos ----------------

    static AudioClip passoClipe;
    static AudioSource passoFonte;

    /// Pisada: um baque grave somado a um estalo de ruido, bem curto.
    static AudioClip ClipeDePasso()
    {
        if (passoClipe != null) return passoClipe;

        const int taxa = 22050;
        const float duracao = 0.1f;
        int n = Mathf.RoundToInt(taxa * duracao);
        float[] dados = new float[n];

        System.Random rnd = new System.Random(19);
        float fase = 0f, ruido = 0f;

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;

            // o baque cai de altura rapido: e o que soa como peso no chao
            float f = Mathf.Lerp(150f, 55f, t);
            fase += 2f * Mathf.PI * f / taxa;

            ruido = ruido * 0.72f + ((float)rnd.NextDouble() * 2f - 1f) * 0.28f;

            float env = Mathf.Exp(-16f * t);
            dados[i] = (Mathf.Sin(fase) * 0.7f + ruido * 0.45f) * env * 0.6f;
        }

        passoClipe = AudioClip.Create("passo", n, 1, taxa, false);
        passoClipe.SetData(dados, 0);
        return passoClipe;
    }

    /// "forca" 0..1: andando soa leve, correndo soa pesado.
    public static void Passo(float forca)
    {
        if (Instance == null) return;

        if (passoFonte == null)
        {
            passoFonte = Instance.gameObject.AddComponent<AudioSource>();
            passoFonte.playOnAwake = false;
        }

        passoFonte.volume = Mathf.Lerp(0.10f, 0.26f, Mathf.Clamp01(forca));
        passoFonte.pitch = Random.Range(0.88f, 1.12f);
        passoFonte.PlayOneShot(ClipeDePasso());
    }

    AudioClip Bip(float freq, float duracao, bool sobe)
    {
        int n = Mathf.CeilToInt(TAXA * duracao);
        float[] dados = new float[n];
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = sobe ? Mathf.Lerp(freq, freq * 2f, t) : Mathf.Lerp(freq, freq * 0.4f, t);
            fase += 2f * Mathf.PI * f / TAXA;
            dados[i] = Mathf.Sin(fase) * Mathf.Exp(-4f * t) * 0.55f;
        }
        AudioClip c = AudioClip.Create("bip", n, 1, TAXA, false);
        c.SetData(dados, 0);
        return c;
    }

    AudioClip GerarLoop()
    {
        int[] notas = { 0, 7, 3, 10, 0, 7, 5, 10 }; // baixinho estilo arcade
        float duracaoNota = 0.26f;
        int amostrasNota = Mathf.RoundToInt(TAXA * duracaoNota);
        float[] dados = new float[amostrasNota * notas.Length];
        float fase = 0f;

        for (int n = 0; n < notas.Length; n++)
        {
            float freq = 110f * Mathf.Pow(2f, notas[n] / 12f);
            for (int i = 0; i < amostrasNota; i++)
            {
                float t = (float)i / amostrasNota;
                fase += 2f * Mathf.PI * freq / TAXA;
                float onda = Mathf.Sin(fase) >= 0f ? 1f : -1f; // onda quadrada
                float env = Mathf.Clamp01(1f - t) * Mathf.Clamp01(t * 12f);
                dados[n * amostrasNota + i] = onda * env * 0.20f;
            }
        }

        AudioClip clip = AudioClip.Create("trilha", dados.Length, 1, TAXA, false);
        clip.SetData(dados, 0);
        return clip;
    }
}
