using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Caixa de dialogo da abertura: a nutricionista explica por que Alex precisa
/// emagrecer para disputar o campeonato.
///
/// Como o menu, congela o jogo com Time.timeScale = 0 em vez de criar um estado
/// novo no GameManager, entao nenhum outro script precisa saber que ela existe.
/// </summary>
public class Dialogo : MonoBehaviour
{
    [System.Serializable]
    public struct Fala
    {
        public string quem;
        [TextArea] public string texto;
        public float voz;      // altura da voz: grave < 1 < agudo
    }

    public static Dialogo Instance;

    // ---- as falas, editaveis no Inspector ----
    //
    // O texto e o conteudo educativo deste jogo, entao ele nao devia morar dentro
    // do codigo: mudar uma frase do medico exigia recompilar. O construtor enche
    // estes arrays com o texto padrao, e dai em diante da para reescrever qualquer
    // linha pelo Inspector, com o jogo rodando inclusive.
    //
    // Array vazio = cai no texto escrito em codigo, entao nada quebra se alguem
    // apagar tudo sem querer.
    [Header("Falas (edite aqui, sem recompilar)")]
    public Fala[] falasAbertura;
    public Fala[] falasTreinador;
    public Fala[] falasAntesDaProva;

    public static Fala[] Abertura() { return Escolher(Dono(d => d.falasAbertura), AberturaPadrao()); }
    public static Fala[] Treinador() { return Escolher(Dono(d => d.falasTreinador), TreinadorPadrao()); }
    public static Fala[] AntesDaProva() { return Escolher(Dono(d => d.falasAntesDaProva), AntesDaProvaPadrao()); }

    static Fala[] Dono(System.Func<Dialogo, Fala[]> campo)
    {
        return Instance != null ? campo(Instance) : null;
    }

    static Fala[] Escolher(Fala[] doInspector, Fala[] padrao)
    {
        return (doInspector != null && doInspector.Length > 0) ? doInspector : padrao;
    }

    Canvas canvas;
    Text txtQuem, txtFala, txtDica;
    [Header("Ritmo da fala")]
    public float letrasPorSegundo = 42f;
    [Tooltip("Uma silaba a cada N letras reveladas")]
    public int letrasPorSilaba = 3;

    Fala[] falas;
    int atual;
    bool ativo;
    System.Action aoTerminar;

    // maquina de escrever
    string alvo = "";
    float reveladas;
    int ultimaSilaba;
    bool escrevendo;

    void Awake()
    {
        Instance = this;
        Montar();
        canvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!ativo) return;

        bool apertou = Entrada.PulouAgora || Entrada.CliqueAgora;

        if (escrevendo)
        {
            // Espaco no meio da fala completa a linha em vez de pular para a proxima:
            // assim ninguem perde texto por apertar rapido demais.
            if (apertou) { Completar(); return; }
            Escrever();
            return;
        }

        if (apertou) Avancar();
    }

    /// O dialogo roda com Time.timeScale = 0, entao o relogio tem que ser o
    /// nao-escalado, senao nenhuma letra apareceria.
    void Escrever()
    {
        reveladas += Time.unscaledDeltaTime * letrasPorSegundo;

        int n = Mathf.Min(Mathf.FloorToInt(reveladas), alvo.Length);
        txtFala.text = alvo.Substring(0, n);

        if (n - ultimaSilaba >= letrasPorSilaba && n < alvo.Length)
        {
            ultimaSilaba = n;
            char c = alvo[n - 1];
            if (c != ' ' && c != '\n') AudioDirector.Falar(falas[atual].voz);
        }

        if (n >= alvo.Length) Completar();
    }

    void Completar()
    {
        escrevendo = false;
        txtFala.text = alvo;
        txtDica.text = (atual == falas.Length - 1)
                     ? "ESPACO  continuar"
                     : "ESPACO  proxima          " + (atual + 1) + " / " + falas.Length;
    }

    /// Abre a conversa. O callback roda quando a ultima fala sai da tela.
    public void Abrir(Fala[] conversa, System.Action quandoTerminar)
    {
        if (conversa == null || conversa.Length == 0)
        {
            if (quandoTerminar != null) quandoTerminar();
            return;
        }

        falas = conversa;
        aoTerminar = quandoTerminar;
        atual = -1;
        ativo = true;

        Time.timeScale = 0f;
        OrbitCamera.TravarCursor(false);
        canvas.gameObject.SetActive(true);
        Avancar();
    }

    void Avancar()
    {
        atual++;

        if (atual >= falas.Length)
        {
            Fechar();
            return;
        }

        txtQuem.text = falas[atual].quem;

        alvo = falas[atual].texto;
        txtFala.text = "";
        reveladas = 0f;
        ultimaSilaba = 0;
        escrevendo = true;
        txtDica.text = "ESPACO  passar";
    }

    void Fechar()
    {
        ativo = false;
        canvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
        OrbitCamera.TravarCursor(true);

        if (aoTerminar != null)
        {
            System.Action cb = aoTerminar;
            aoTerminar = null;
            cb();
        }
    }

    // Alturas de voz por personagem. O clipe e o mesmo para todos;
    // so o pitch muda, entao cada um soa diferente sem custar memoria.
    const float VOZ_DOUTOR = 0.72f;
    const float VOZ_NUTRI = 1.18f;
    const float VOZ_ALEX = 0.96f;
    const float VOZ_TREINO = 0.80f;

    /// Etapa 0: o consultorio do hospital.
    public static Fala[] AberturaPadrao()
    {
        return new Fala[]
        {
            Nova("DR. MOREIRA  -  clinico geral", VOZ_DOUTOR,
                 "Sentou, Alex? Entao vamos aos exames.\n\n"
               + "Colesterol alto, glicemia no limite, 78 quilos.\n"
               + "Isso aqui nao e susto, e aviso."),

            Nova("ALEX", VOZ_ALEX,
                 "Doutor, eu me inscrevi no campeonato da cidade."),

            Nova("DR. MOREIRA  -  clinico geral", VOZ_DOUTOR,
                 "Do jeito que voce esta, nao termina a prova.\n\n"
               + "Mas nao vim te proibir de nada. Vim te dar um caminho.\n"
               + "Chamei a Helena para comecar por onde se comeca."),

            Nova("DRA. HELENA  -  nutricionista", VOZ_NUTRI,
                 "Comeca pelo prato, Alex. Fruta, agua e proteina.\n\n"
               + "Cada escolha boa tira peso. E peso e exatamente\n"
               + "o que esta te deixando lento."),

            Nova("ALEX", VOZ_ALEX,
                 "E a pizzaria da esquina? Aquele cheiro me persegue a rua inteira."),

            Nova("DRA. HELENA  -  nutricionista", VOZ_NUTRI,
                 "Vai perseguir mesmo. E quanto mais leve voce ficar,\n"
               + "mais rapido ele vem atras.\n\n"
               + "Nao da para fugir parado. Corre."),

            Nova("DR. MOREIRA  -  clinico geral", VOZ_DOUTOR,
                 "Primeiro a dieta. Depois o treino na rua.\n"
               + "So entao o campeonato.\n\n"
               + "Pode ir. Te espero de volta mais leve.")
        };
    }

    /// Etapa 2: o treinador, no portico de largada.
    public static Fala[] TreinadorPadrao()
    {
        return new Fala[]
        {
            Nova("SEU ANTUNES  -  treinador", VOZ_TREINO,
                 "Entao e voce o teimoso que a Helena me falou.\n\n"
               + "Peso voce ja perdeu. Falta o resto."),

            Nova("ALEX", VOZ_ALEX,
                 "O resto?"),

            Nova("SEU ANTUNES  -  treinador", VOZ_TREINO,
                 "Condicionamento. Dieta sozinha nao faz atleta.\n\n"
               + "Pega a rua, cruza os porticos no tempo. Nada de pista,\n"
               + "nada de publico. So voce, o asfalto e o relogio."),

            Nova("SEU ANTUNES  -  treinador", VOZ_TREINO,
                 "Fez isso, eu te inscrevo no campeonato.\n"
               + "Vai.")
        };
    }

    /// Etapa 3: os dois na entrada da pista, antes da prova.
    public static Fala[] AntesDaProvaPadrao()
    {
        return new Fala[]
        {
            Nova("SEU ANTUNES  -  treinador", VOZ_TREINO,
                 "Chegou o dia, garoto. A pista e essa."),

            Nova("DRA. HELENA  -  nutricionista", VOZ_NUTRI,
                 "Eu vi seus exames de hoje. Nao e mais o mesmo corpo\n"
               + "que entrou no consultorio."),

            Nova("ALEX", VOZ_ALEX,
                 "Eu lembro do dia que o doutor disse que eu nao terminava a prova."),

            Nova("SEU ANTUNES  -  treinador", VOZ_TREINO,
                 "Pois entao. Voce batalhou por isso cada dia.\n\n"
               + "Agora nao tem mais dieta, nao tem mais treino.\n"
               + "Tem a raia e os outros quatro."),

            Nova("DRA. HELENA  -  nutricionista", VOZ_NUTRI,
                 "Faz o teu melhor, Alex. O resto ja esta feito.")
        };
    }

    static Fala Nova(string quem, float voz, string texto)
    {
        Fala f = new Fala();
        f.quem = quem;
        f.voz = voz;
        f.texto = texto;
        return f;
    }

    // ---------------- construcao ----------------

    void Montar()
    {
        Font fonte = CarregarFonte();

        GameObject go = new GameObject("Dialogo_Canvas", typeof(RectTransform));
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;          // por cima do menu e do HUD

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform raiz = go.GetComponent<RectTransform>();

        // escurece a cena sem escondê-la de todo
        Caixa(raiz, Vector2.zero, new Vector2(0f, 0f), true,
              new Color(0.03f, 0.04f, 0.08f, 0.78f));

        // placa do consultorio
        RectTransform painel = Caixa(raiz, new Vector2(0.5f, 0.5f), new Vector2(1060f, 300f),
                                     false, new Color(0.08f, 0.10f, 0.16f, 0.97f));
        painel.anchoredPosition = new Vector2(0f, -40f);

        RectTransform faixa = Caixa(painel, new Vector2(0.5f, 1f), new Vector2(1060f, 52f),
                                    false, new Color(0.18f, 0.46f, 0.38f, 1f));
        faixa.anchoredPosition = Vector2.zero;

        txtQuem = Texto(faixa, fonte, 23, TextAnchor.MiddleLeft,
                        new Vector2(36f, 0f), new Vector2(1000f, 52f));
        txtQuem.color = new Color(1f, 0.96f, 0.80f);

        txtFala = Texto(painel, fonte, 22, TextAnchor.UpperLeft,
                        new Vector2(40f, -78f), new Vector2(980f, 180f));
        txtFala.color = new Color(0.90f, 0.93f, 0.97f);
        txtFala.lineSpacing = 1.35f;

        txtDica = Texto(painel, fonte, 18, TextAnchor.LowerRight,
                        new Vector2(-40f, 24f), new Vector2(980f, 40f));
        txtDica.color = new Color(0.45f, 0.95f, 0.55f);
    }

    RectTransform Caixa(RectTransform pai, Vector2 ancora, Vector2 tamanho,
                        bool esticar, Color cor)
    {
        GameObject go = new GameObject("Caixa", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);

        if (esticar)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
            rt.sizeDelta = tamanho;
            rt.anchoredPosition = Vector2.zero;
        }

        Image img = go.AddComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return rt;
    }

    Text Texto(RectTransform pai, Font fonte, int tamanho, TextAnchor alinhamento,
               Vector2 pos, Vector2 caixa)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);

        // ancora no canto que o alinhamento sugere, para a caixa crescer para dentro
        Vector2 a = new Vector2(0f, 1f);
        if (alinhamento == TextAnchor.MiddleLeft) a = new Vector2(0f, 0.5f);
        else if (alinhamento == TextAnchor.LowerRight) a = new Vector2(1f, 0f);

        rt.anchorMin = rt.anchorMax = rt.pivot = a;
        rt.anchoredPosition = pos;
        rt.sizeDelta = caixa;

        Text t = go.AddComponent<Text>();
        t.font = fonte;
        t.fontSize = tamanho;
        t.alignment = alinhamento;
        t.color = Color.white;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static Font CarregarFonte()
    {
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (f == null) { try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
        if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 16);
        return f;
    }
}
