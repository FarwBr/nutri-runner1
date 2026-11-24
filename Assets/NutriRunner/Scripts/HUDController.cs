using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD montado por codigo: barras, objetivo da fase, aviso de perigo da nuvem,
/// cronometro da corrida e um marcador que aponta o proximo destino.
/// </summary>
public class HUDController : MonoBehaviour
{
    const float LARGURA = 240f;
    const float ALTURA = 20f;

    Font fonte;
    RectTransform canvasRect;
    Image vinheta;
    RectTransform fillColesterol, fillDiabetes, fillPeso, fillFolego;
    Image fundoFolego;
    Text txtInfo, txtObjetivo, txtAviso, txtFim, txtMarcador, txtPerigo, txtCronometro;
    Text txtRelatorio;
    RectTransform painelRelatorio;

    [Header("Final")]
    [Tooltip("Segundos de comemoracao antes do relatorio cobrir a tela")]
    public float esperaDoRelatorio = 4.5f;
    float tempoDesdeOFim;
    ItemSpawner spawner;

    void Start()
    {
        fonte = CarregarFonte();
        spawner = Object.FindObjectOfType<ItemSpawner>();

        GameObject canvasGO = new GameObject("HUD_Canvas", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasGO.GetComponent<RectTransform>();

        // vinheta vermelha do cheiro: primeiro filho, fica atras de tudo
        GameObject vinGO = new GameObject("Vinheta", typeof(RectTransform));
        RectTransform vrt = vinGO.GetComponent<RectTransform>();
        vrt.SetParent(canvasRect, false);
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        vinheta = vinGO.AddComponent<Image>();
        vinheta.color = new Color(0.6f, 0.1f, 0.35f, 0f);
        vinheta.raycastTarget = false;

        fillColesterol = Barra(canvasRect, "COLESTEROL", 0, new Color(1.00f, 0.72f, 0.20f));
        fillDiabetes   = Barra(canvasRect, "DIABETES",   1, new Color(1.00f, 0.38f, 0.45f));
        fillPeso       = Barra(canvasRect, "PESO",       2, new Color(0.70f, 0.70f, 0.75f));

        // O folego so aparece na prova final, entao nasce desligado.
        fillFolego = Barra(canvasRect, "FOLEGO", 3, new Color(0.45f, 0.85f, 1.00f));
        fundoFolego = fillFolego.parent.GetComponent<Image>();
        fillFolego.parent.gameObject.SetActive(false);

        txtInfo = Texto(canvasRect, "Info", 20, TextAnchor.UpperRight,
                        new Vector2(1f, 1f), new Vector2(-20f, -18f), new Vector2(420f, 90f));

        txtObjetivo = Texto(canvasRect, "Objetivo", 22, TextAnchor.UpperCenter,
                            new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(900f, 40f));
        txtObjetivo.color = new Color(1f, 0.95f, 0.6f);

        txtPerigo = Texto(canvasRect, "Perigo", 24, TextAnchor.UpperCenter,
                          new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(900f, 40f));
        txtPerigo.color = new Color(1f, 0.45f, 0.45f);

        txtAviso = Texto(canvasRect, "Aviso", 20, TextAnchor.LowerCenter,
                         new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(900f, 40f));

        txtMarcador = Texto(canvasRect, "Marcador", 18, TextAnchor.MiddleCenter,
                            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260f, 30f));
        txtMarcador.color = new Color(0.60f, 1f, 0.65f);

        txtCronometro = Texto(canvasRect, "Cronometro", 44, TextAnchor.UpperCenter,
                              new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(400f, 60f));
        txtCronometro.text = "";

        Texto(canvasRect, "Ajuda", 16, TextAnchor.LowerLeft,
              new Vector2(0f, 0f), new Vector2(20f, 18f), new Vector2(980f, 30f))
            .text = "W A S D = andar   MOUSE = camera   SHIFT = correr   ESPACO = pular   ESC = cursor   R = reiniciar";

        // painel de relatorio: fica atras do texto de fim, escondido ate o jogo acabar
        painelRelatorio = Caixa(canvasRect, "PainelRelatorio", Vector2.zero,
                                new Vector2(640f, 440f), new Color(0.05f, 0.06f, 0.12f, 0.9f));
        painelRelatorio.anchorMin = painelRelatorio.anchorMax =
            painelRelatorio.pivot = new Vector2(0.5f, 0.5f);
        painelRelatorio.anchoredPosition = new Vector2(0f, -40f);

        txtRelatorio = Texto(painelRelatorio, "Relatorio", 20, TextAnchor.UpperLeft,
                             new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(580f, 380f));
        txtRelatorio.color = new Color(0.88f, 0.92f, 0.96f);
        txtRelatorio.lineSpacing = 1.25f;
        painelRelatorio.gameObject.SetActive(false);

        txtFim = Texto(canvasRect, "Fim", 30, TextAnchor.MiddleCenter,
                       new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1000f, 90f));
        txtFim.text = "";
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        Preencher(fillColesterol, gm.Colesterol / gm.colesterolMax, LARGURA);
        Preencher(fillDiabetes, gm.Diabetes / gm.diabetesMax, LARGURA);
        Preencher(fillPeso, gm.Peso / 100f, LARGURA);
        AtualizarFolego(gm);

        if (gm.Fase == FaseJogo.Circuito && gm.Estado == EstadoJogo.Jogando)
        {
            txtCronometro.text = gm.Posicao + "o";
            txtCronometro.color = (gm.Posicao <= 3)
                                ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 0.6f, 0.5f);
        }

        txtInfo.text = "PONTOS  " + gm.Pontos
                     + "\nSaudaveis " + gm.ItensBons + " / " + gm.TotalItensBons
                     + "\nPeso " + Mathf.RoundToInt(gm.Peso) + " kg";

        txtObjetivo.text = gm.ObjetivoTexto;
        txtAviso.text = gm.Aviso;

        AtualizarPerigo(gm);
        AtualizarCronometro(gm);
        AtualizarMarcador(gm);

        bool acabou = (gm.Estado != EstadoJogo.Jogando);

        if (!acabou)
        {
            tempoDesdeOFim = 0f;
            txtFim.text = "";
            if (painelRelatorio.gameObject.activeSelf)
                painelRelatorio.gameObject.SetActive(false);
        }
        else
        {
            tempoDesdeOFim += Time.deltaTime;

            txtFim.text = gm.MensagemFim;
            txtFim.color = (gm.Estado == EstadoJogo.Vitoria)
                         ? new Color(0.55f, 1f, 0.6f) : new Color(1f, 0.55f, 0.55f);

            // Na vitoria o relatorio espera: ele cobre o meio da tela, e e ali
            // que o Alex esta erguendo a taca. Na derrota aparece na hora.
            bool podeMostrar = (gm.Estado != EstadoJogo.Vitoria)
                            || tempoDesdeOFim >= esperaDoRelatorio;

            if (podeMostrar && !painelRelatorio.gameObject.activeSelf)
            {
                painelRelatorio.gameObject.SetActive(true);
                txtRelatorio.text = gm.Relatorio()
                                  + "\n\n\nPressione R para jogar de novo";
            }
        }
    }

    void AtualizarPerigo(GameManager gm)
    {
        float p = gm.Perigo;
        vinheta.color = new Color(0.6f, 0.1f, 0.35f, p * 0.38f);

        // Na prova nao existe mais cheiro: o mesmo espaco da tela passa a ser
        // ocupado pelo juiz de pista.
        if (gm.Fase == FaseJogo.Circuito && gm.Estado == EstadoJogo.Jogando)
        {
            AtualizarJuiz(gm);
            return;
        }

        if (gm.Estado != EstadoJogo.Jogando || gm.Fase == FaseJogo.CorridaRua
                                           || gm.Fase == FaseJogo.Circuito)
        {
            txtPerigo.text = "";
            vinheta.color = new Color(0.6f, 0.1f, 0.35f, 0f);
            return;
        }

        string barra = "";
        int cheios = Mathf.RoundToInt(p * 10f);
        for (int i = 0; i < 10; i++) barra += (i < cheios) ? "|" : ".";

        txtPerigo.text = "CHEIRO  " + barra + "   " + Mathf.RoundToInt(gm.DistanciaNuvem) + " m"
                       + "      nuvem " + gm.VelocidadeNuvem.ToString("0.0")
                       + " m/s   voce " + gm.VelocidadeJogador.ToString("0.0") + " m/s";
    }

    /// A barra de folego aparece so na prova, e pisca em vermelho quando zera.
    /// E a leitura que o jogador precisa ter o tempo todo na pista, porque e ela
    /// que diz se ele pode sustentar o sprint ate a chegada.
    void AtualizarFolego(GameManager gm)
    {
        if (fillFolego == null) return;

        GameObject caixa = fillFolego.parent.gameObject;
        if (caixa.activeSelf != gm.FolegoAtivo) caixa.SetActive(gm.FolegoAtivo);
        if (!gm.FolegoAtivo) return;

        Preencher(fillFolego, gm.Folego, LARGURA);

        Image img = fillFolego.GetComponent<Image>();
        if (img != null)
        {
            if (gm.SemFolego)
            {
                float pulso = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 8f));
                img.color = new Color(1f, 0.3f * pulso, 0.3f * pulso);
            }
            else img.color = Color.Lerp(new Color(1f, 0.6f, 0.25f),
                                        new Color(0.45f, 0.85f, 1f),
                                        Mathf.Clamp01(gm.Folego * 2.2f));
        }

        if (fundoFolego != null)
            fundoFolego.color = gm.SemFolego
                ? new Color(0.35f, 0f, 0f, 0.65f)
                : new Color(0f, 0f, 0f, 0.55f);
    }

    /// Mostra ao jogador o que o juiz de pista esta vendo: quantos metros ja
    /// contaram, e - quando ele sai do asfalto - que o progresso parou de contar
    /// e quanto falta para a desclassificacao.
    void AtualizarJuiz(GameManager gm)
    {
        string metros = Mathf.FloorToInt(gm.MetrosCorridos) + " / "
                      + Mathf.RoundToInt(gm.MetrosDaProva) + " m";

        if (!gm.ForaDaPista)
        {
            vinheta.color = new Color(0.6f, 0.1f, 0.35f, 0f);
            txtPerigo.color = new Color(0.72f, 0.82f, 0.9f);
            txtPerigo.text = metros;
            return;
        }

        float f = gm.ForaDaPistaFracao;

        // pisca em ambar, e mais rapido conforme a tolerancia acaba
        float pulso = 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(Time.time * (5f + f * 8f)));
        vinheta.color = new Color(0.8f, 0.45f, 0.05f, (0.14f + f * 0.3f) * pulso);

        string barra = "";
        int cheios = Mathf.RoundToInt(f * 8f);
        for (int i = 0; i < 8; i++) barra += (i < cheios) ? "|" : ".";

        txtPerigo.color = new Color(1f, 0.75f - 0.35f * f, 0.25f);
        txtPerigo.text = (gm.LadoQueSaiu < 0f ? "CORTOU A PISTA" : "FORA DAS RAIAS")
                       + "   nao conta   " + barra + "   " + metros;
    }

    void AtualizarCronometro(GameManager gm)
    {
        if (gm.Fase != FaseJogo.CorridaRua || gm.Estado != EstadoJogo.Jogando)
        {
            txtCronometro.text = "";
            return;
        }

        int seg = Mathf.CeilToInt(gm.TempoRestante);
        txtCronometro.text = (seg / 60) + ":" + (seg % 60).ToString("00");

        // fica vermelho e pulsa nos ultimos 10 segundos
        if (gm.TempoRestante <= 10f)
        {
            float pulso = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
            txtCronometro.color = new Color(1f, 0.3f * pulso, 0.3f * pulso);
        }
        else txtCronometro.color = Color.white;
    }

    void AtualizarMarcador(GameManager gm)
    {
        if (Camera.main == null || gm.Estado != EstadoJogo.Jogando)
        {
            txtMarcador.text = "";
            return;
        }

        Transform alvo = null;
        string rotulo = "";

        if (gm.Fase == FaseJogo.CorridaRua && gm.AlvoCorrida != null)
        {
            alvo = gm.AlvoCorrida;
            rotulo = "[ PORTICO " + gm.CheckpointAtual + " ] ";
        }
        else if (gm.Fase == FaseJogo.Dieta && spawner != null)
        {
            alvo = spawner.MaisProximo(Camera.main.transform.position);
            rotulo = "[ saudavel ] ";
        }
        if (alvo == null && gm.Fase == FaseJogo.IrParaCircuito && gm.circuito != null)
        {
            alvo = gm.circuito;
            rotulo = "[ CIRCUITO ] ";
        }
        if (alvo == null && gm.largada != null && gm.Fase != FaseJogo.Circuito)
        {
            alvo = gm.largada;
            rotulo = "[ LARGADA ] ";
        }
        if (alvo == null) { txtMarcador.text = ""; return; }

        Vector3 ponto = alvo.position + Vector3.up * 2.5f;
        float distancia = Vector3.Distance(Camera.main.transform.position, ponto);
        Vector3 tela = Camera.main.WorldToScreenPoint(ponto);

        if (tela.z < 0f)
        {
            tela.x = Screen.width - tela.x;
            tela.y = 0f;
        }

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, new Vector2(tela.x, tela.y), null, out local);

        float limiteX = canvasRect.rect.width * 0.5f - 140f;
        float limiteY = canvasRect.rect.height * 0.5f - 70f;
        local.x = Mathf.Clamp(local.x, -limiteX, limiteX);
        local.y = Mathf.Clamp(local.y, -limiteY, limiteY);

        txtMarcador.rectTransform.anchoredPosition = local;
        txtMarcador.text = rotulo + Mathf.RoundToInt(distancia) + " m";
    }

    // ---------- construcao ----------

    RectTransform Barra(RectTransform pai, string rotulo, int indice, Color cor)
    {
        float y = -18f - indice * (ALTURA + 8f);

        RectTransform fundo = Caixa(pai, "Barra_" + rotulo, new Vector2(20f, y),
                                    new Vector2(LARGURA, ALTURA), new Color(0f, 0f, 0f, 0.55f));

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        RectTransform fill = fillGO.GetComponent<RectTransform>();
        fill.SetParent(fundo, false);
        fill.anchorMin = new Vector2(0f, 0f);
        fill.anchorMax = new Vector2(0f, 1f);
        fill.pivot = new Vector2(0f, 0.5f);
        fill.anchoredPosition = new Vector2(2f, 0f);
        fill.sizeDelta = new Vector2(LARGURA - 4f, -4f);
        fillGO.AddComponent<Image>().color = cor;

        Text t = Texto(fundo, "Rotulo", 13, TextAnchor.MiddleLeft,
                       new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(LARGURA, ALTURA));
        t.text = rotulo;

        return fill;
    }

    RectTransform Caixa(RectTransform pai, string nome, Vector2 pos, Vector2 tamanho, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = tamanho;
        Image img = go.AddComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return rt;
    }

    Text Texto(RectTransform pai, string nome, int tamanho, TextAnchor alinhamento,
               Vector2 ancora, Vector2 pos, Vector2 caixa)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = ancora;
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

        Outline o = go.AddComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.85f);
        o.effectDistance = new Vector2(1.5f, -1.5f);
        return t;
    }

    static void Preencher(RectTransform rt, float pct, float largura)
    {
        if (rt == null) return;
        Vector2 sd = rt.sizeDelta;
        sd.x = (largura - 4f) * Mathf.Clamp01(pct);
        rt.sizeDelta = sd;
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
