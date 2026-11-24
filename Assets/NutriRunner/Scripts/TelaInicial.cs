using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menu inicial. Constroi o proprio Canvas por codigo, como o resto da interface.
///
/// Em vez de criar um estado novo no GameManager (o que obrigaria a mexer em todos
/// os scripts que checam EstadoJogo), ele apenas zera o Time.timeScale. Com
/// deltaTime em zero ninguem anda, a nuvem nao persegue e as barras nao drenam,
/// sem uma linha alterada em nenhum outro arquivo.
/// </summary>
public class TelaInicial : MonoBehaviour
{
    public string titulo = "NUTRI RUNNER";

    enum Pagina { Menu, ComoJogar }

    static readonly string[] OPCOES = { "JOGAR", "COMO JOGAR", "SAIR" };

    Canvas canvas;
    RectTransform painelMenu, painelAjuda;
    Text[] itens;
    Text dica;
    Font fonte;

    Pagina pagina = Pagina.Menu;
    int escolha;
    bool aberto;
    float eixoAnterior;

    /// Estatico sobrevive ao recarregamento da cena: apertar R nao traz o menu de volta.
    static bool jaMostrou;

    void Start()
    {
        if (jaMostrou)
        {
            Time.timeScale = 1f;
            OrbitCamera.TravarCursor(true);
            return;
        }
        jaMostrou = true;

        Montar();
        Abrir();
    }

    void Update()
    {
        if (!aberto) return;

        // O OrbitCamera tenta travar o cursor no clique; aqui ele fica livre.
        if (Cursor.lockState == CursorLockMode.Locked) OrbitCamera.TravarCursor(false);

        if (pagina == Pagina.ComoJogar)
        {
            if (Entrada.PulouAgora || Entrada.EscAgora) MostrarPagina(Pagina.Menu);
            return;
        }

        Navegar();
        if (Entrada.PulouAgora) Confirmar();
    }

    // ---------------- navegacao ----------------

    /// O eixo vertical e continuo; so contamos quando ele SAI do zero,
    /// senao segurar a tecla varreria a lista inteira num frame.
    void Navegar()
    {
        float eixo = Entrada.Vertical;

        if (Mathf.Abs(eixo) > 0.5f && Mathf.Abs(eixoAnterior) <= 0.5f)
        {
            escolha += (eixo > 0f) ? -1 : 1;        // W sobe na lista
            if (escolha < 0) escolha = OPCOES.Length - 1;
            if (escolha >= OPCOES.Length) escolha = 0;
            Pintar();
        }
        eixoAnterior = eixo;
    }

    void Confirmar()
    {
        if (escolha == 0) Jogar();
        else if (escolha == 1) MostrarPagina(Pagina.ComoJogar);
        else Sair();
    }

    void Jogar()
    {
        aberto = false;
        if (canvas != null) canvas.gameObject.SetActive(false);

        // a historia comeca no consultorio da nutricionista
        if (Dialogo.Instance != null)
        {
            Dialogo.Instance.Abrir(Dialogo.Abertura(), Comecar);
            return;
        }
        Comecar();
    }

    void Comecar()
    {
        Time.timeScale = 1f;
        OrbitCamera.TravarCursor(true);
    }

    void Sair()
    {
        Application.Quit();
#if UNITY_EDITOR
        // No Editor, Application.Quit nao faz nada: paramos o Play na mao.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void Abrir()
    {
        aberto = true;
        escolha = 0;
        Time.timeScale = 0f;
        OrbitCamera.TravarCursor(false);
        MostrarPagina(Pagina.Menu);
    }

    void MostrarPagina(Pagina p)
    {
        pagina = p;
        painelMenu.gameObject.SetActive(p == Pagina.Menu);
        painelAjuda.gameObject.SetActive(p == Pagina.ComoJogar);
        dica.text = (p == Pagina.Menu)
                  ? "W / S  escolher          ESPACO  confirmar"
                  : "ESPACO  voltar ao menu";
        if (p == Pagina.Menu) Pintar();
    }

    void Pintar()
    {
        for (int i = 0; i < itens.Length; i++)
        {
            bool sel = (i == escolha);
            itens[i].text = (sel ? ">  " : "    ") + OPCOES[i];
            itens[i].color = sel ? new Color(1f, 0.93f, 0.55f)
                                 : new Color(0.62f, 0.68f, 0.76f);
            itens[i].fontSize = sel ? 38 : 32;
        }
    }

    // ---------------- construcao ----------------

    void Montar()
    {
        fonte = CarregarFonte();

        GameObject go = new GameObject("MenuInicial_Canvas", typeof(RectTransform));
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;          // por cima do HUD

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform raiz = go.GetComponent<RectTransform>();

        Fundo(raiz, new Color(0.04f, 0.05f, 0.10f, 0.93f));

        Texto(raiz, titulo, 78, new Vector2(0f, 240f),
              new Color(1f, 0.93f, 0.55f), 900f);
        Texto(raiz, "fuja do cheiro, recupere o corpo", 23, new Vector2(0f, 180f),
              new Color(0.70f, 0.78f, 0.88f), 900f);

        // ---- pagina do menu ----
        painelMenu = Painel(raiz);
        itens = new Text[OPCOES.Length];
        for (int i = 0; i < OPCOES.Length; i++)
            itens[i] = Texto(painelMenu, OPCOES[i], 32,
                             new Vector2(0f, 40f - i * 58f), Color.white, 600f);

        // ---- pagina de instrucoes ----
        painelAjuda = Painel(raiz);
        Texto(painelAjuda,
              "Alex recebeu um diagnostico de obesidade e risco cardiaco.\n"
            + "O cheiro da pizzaria nao larga do pe dele.",
              22, new Vector2(0f, 115f), new Color(0.88f, 0.90f, 0.95f), 1000f);

        Texto(painelAjuda,
              "FASE 1    Colete todos os alimentos saudaveis sem ser alcancado.\n"
            + "          Cada alimento deixa o Alex mais leve, e mais leve e mais\n"
            + "          rapido. Mas o cheiro tambem acelera quando voce emagrece.\n\n"
            + "FASE 2    Va ate a LARGADA e complete a corrida de rua no tempo.",
              21, new Vector2(0f, 5f), new Color(0.82f, 0.88f, 0.94f), 1000f);

        Texto(painelAjuda,
              "W A S D  andar        MOUSE  camera        SHIFT  correr\n"
            + "ESPACO  pular         ESC  liberar cursor        R  reiniciar",
              20, new Vector2(0f, -120f), new Color(0.66f, 0.74f, 0.84f), 1000f);

        dica = Texto(raiz, "", 21, new Vector2(0f, -265f),
                     new Color(0.45f, 0.95f, 0.55f), 900f);
    }

    RectTransform Painel(RectTransform pai)
    {
        GameObject go = new GameObject("Pagina", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -40f);
        rt.sizeDelta = new Vector2(1000f, 330f);
        return rt;
    }

    void Fundo(RectTransform pai, Color cor)
    {
        GameObject go = new GameObject("Fundo", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
    }

    Text Texto(RectTransform pai, string conteudo, int tamanho,
               Vector2 pos, Color cor, float largura)
    {
        GameObject go = new GameObject("Texto", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(largura, 150f);

        Text t = go.AddComponent<Text>();
        t.font = fonte;
        t.fontSize = tamanho;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = cor;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 1.18f;
        t.text = conteudo;
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
