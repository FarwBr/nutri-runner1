using UnityEngine;

/// <summary>
/// Etapa 3: a prova do campeonato, na pista de atletismo ao sul da cidade.
///
/// Toda a geometria vem do componente PistaAtletismo. Aqui ficam so as regras.
///
/// O progresso de todo mundo - jogador e rivais - e medido em METROS DE ARCO ao
/// longo do circuito, nao em angulo. A diferenca importa: com angulo, quem passava
/// rente ao centro da pista varria meia volta andando quase nada (neste tracado,
/// 100 m de credito por 37 m andados). Com arco, metro e metro.
///
/// E para fechar de vez o atalho existe o JUIZ: fora do asfalto o progresso
/// CONGELA, e ao voltar a contagem e religada na posicao de reentrada. Quem corta
/// pelo gramado perde o tempo do desvio e nao ganha um metro. Insistir desclassifica.
/// </summary>
public class CircuitoOlimpico : MonoBehaviour
{
    [Header("Pista")]
    public PistaAtletismo pista;
    public int voltas = 2;

    [Header("Rivais")]
    public Corredor[] rivais;

    [Header("Largada")]
    [Tooltip("Segundos de contagem antes de todos sairem")]
    public float contagem = 3f;

    [Header("Juiz de pista")]
    [Tooltip("Segundos acumulados fora do asfalto antes de desclassificar")]
    public float toleranciaFora = 5f;
    [Tooltip("Com que rapidez a tolerancia se recupera correndo direito")]
    public float recuperaTolerancia = 0.4f;
    [Tooltip("Segundos entre um aviso de fora de pista e o proximo")]
    public float intervaloAviso = 1.2f;

    Transform jogador;
    bool dentro;
    bool conversando;
    bool comecou;
    bool terminou;
    float relogioLargada;

    // progresso do jogador, em metros de arco
    float paramAnterior;
    float percorrido;
    float avisoSentido;

    // juiz
    bool foraAgora;
    float tempoFora;
    float proximoAviso;

    float Meta { get { return voltas * pista.Perimetro; } }

    void Start()
    {
        ThirdPersonController j = Object.FindObjectOfType<ThirdPersonController>();
        if (j != null) jogador = j.transform;

        if (pista == null) pista = GetComponentInParent<PistaAtletismo>();
        if (pista == null) pista = Object.FindObjectOfType<PistaAtletismo>();
        if (pista == null)
            Debug.LogError("CircuitoOlimpico sem PistaAtletismo: a prova nao vai funcionar.");

        for (int i = 0; i < rivais.Length; i++)
            if (rivais[i] != null) rivais[i].gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() == null) return;
        dentro = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() == null) return;
        dentro = false;
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || pista == null || gm.Estado != EstadoJogo.Jogando) return;

        if (conversando) return;

        if (!comecou)
        {
            if (!dentro) return;

            if (gm.Fase == FaseJogo.Dieta || gm.Fase == FaseJogo.IrParaLargada)
            {
                gm.Notificar("A prova do campeonato e a ultima etapa. Faca o treino antes");
                return;
            }
            if (gm.Fase == FaseJogo.IrParaCircuito) Conversar();
            return;
        }

        if (terminou) return;

        // ---- contagem regressiva ----
        if (relogioLargada > 0f)
        {
            relogioLargada -= Time.deltaTime;
            int n = Mathf.CeilToInt(relogioLargada);
            gm.Notificar(n > 0 ? "LARGADA EM " + n : "VAI!");

            if (relogioLargada <= 0f)
            {
                for (int i = 0; i < rivais.Length; i++)
                    if (rivais[i] != null) rivais[i].Largar();

                paramAnterior = ParametroDoJogador();
            }
            return;
        }

        Julgar(gm);
        Classificar(gm);
    }

    /// Treinador e nutricionista falam antes da prova comecar.
    void Conversar()
    {
        if (Dialogo.Instance != null)
        {
            conversando = true;
            Dialogo.Instance.Abrir(Dialogo.AntesDaProva(), Largar);
            return;
        }
        Largar();
    }

    void Largar()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        conversando = false;
        comecou = true;
        relogioLargada = contagem;

        DispersarCheiro();

        // os rivais entram em cena alinhados com o jogador, cada um na sua raia
        float t0 = ParametroDoJogador();
        for (int i = 0; i < rivais.Length; i++)
        {
            if (rivais[i] == null) continue;
            rivais[i].gameObject.SetActive(true);
            rivais[i].pista = pista;
            rivais[i].Preparar(t0);
        }

        paramAnterior = t0;
        percorrido = 0f;
        tempoFora = 0f;
        foraAgora = false;

        gm.MetrosDaProva = Meta;
        gm.MetrosCorridos = 0f;
        gm.IniciarCircuito(voltas, rivais.Length + 1);
    }

    /// Na prova nao ha mais cheiro: o adversario agora sao os outros corredores.
    void DispersarCheiro()
    {
        NuvemCheiro[] nuvens = Object.FindObjectsOfType<NuvemCheiro>();
        for (int i = 0; i < nuvens.Length; i++)
            nuvens[i].gameObject.SetActive(false);
    }

    float ParametroDoJogador()
    {
        if (jogador == null) return 0f;
        return pista.Parametro(jogador.position);
    }

    // ================= o juiz =================

    /// Mede o progresso e, ao mesmo tempo, fiscaliza se o Alex esta na pista.
    /// As duas coisas moram juntas de proposito: e a mesma decisao por frame.
    void Julgar(GameManager gm)
    {
        if (jogador == null) return;

        bool naPista = pista.NaPista(jogador.position);
        float t = pista.Parametro(jogador.position);

        if (naPista)
        {
            if (foraAgora)
            {
                // Religa a contagem no ponto de REENTRADA. E o coracao da regra: o
                // trecho andado por fora simplesmente nao existe para a prova, nem
                // a favor nem contra.
                foraAgora = false;
                paramAnterior = t;
                gm.Notificar("DE VOLTA A PISTA");
            }

            percorrido += PistaAtletismo.Delta(paramAnterior, t) * pista.Perimetro;
            paramAnterior = t;

            tempoFora = Mathf.Max(0f, tempoFora - Time.deltaTime * recuperaTolerancia);
        }
        else
        {
            if (!foraAgora)
            {
                foraAgora = true;
                proximoAviso = 0f;
                AudioDirector.Tocar(true);
            }

            tempoFora += Time.deltaTime;

            proximoAviso -= Time.deltaTime;
            if (proximoAviso <= 0f)
            {
                proximoAviso = intervaloAviso;
                gm.Notificar(pista.QuantoFora(jogador.position) < 0f
                           ? "FORA DA PISTA - cortar pelo gramado nao conta"
                           : "FORA DA PISTA - volte para as raias");
            }

            if (tempoFora >= toleranciaFora)
            {
                gm.Desclassificar("Voce correu fora da pista");
                return;
            }
        }

        gm.ForaDaPista = foraAgora;
        gm.LadoQueSaiu = pista.QuantoFora(jogador.position);
        gm.ForaDaPistaFracao = Mathf.Clamp01(tempoFora / Mathf.Max(0.01f, toleranciaFora));
        gm.MetrosCorridos = Mathf.Max(0f, percorrido);

        Voltas(gm);
    }

    void Voltas(GameManager gm)
    {
        // Os rivais correm no sentido do parametro crescente. Se o jogador for para
        // o outro lado ele nunca fecha a volta, entao avisamos em vez de deixar a
        // pessoa correndo a esmo.
        avisoSentido -= Time.deltaTime;
        if (percorrido < -3f && avisoSentido <= 0f)
        {
            avisoSentido = 2.5f;
            gm.Notificar("SENTIDO ERRADO - siga os outros corredores");
        }

        int voltaAgora = Mathf.Clamp(
            Mathf.FloorToInt(percorrido / pista.Perimetro) + 1, 1, voltas + 1);

        if (voltaAgora > gm.Volta && voltaAgora <= voltas)
            gm.RegistrarVolta();

        if (percorrido >= Meta) Terminar(gm);
    }

    void Classificar(GameManager gm)
    {
        int lugar = 1;
        for (int i = 0; i < rivais.Length; i++)
            if (rivais[i] != null && rivais[i].Percorrido > percorrido) lugar++;

        gm.ReportarPosicao(lugar);
    }

    void Terminar(GameManager gm)
    {
        terminou = true;

        int lugar = 1;
        for (int i = 0; i < rivais.Length; i++)
        {
            if (rivais[i] == null) continue;
            if (rivais[i].Percorrido > percorrido) lugar++;
            rivais[i].Parar();
        }

        gm.ForaDaPista = false;
        gm.ForaDaPistaFracao = 0f;
        gm.TerminarCircuito(lugar);
    }
}
