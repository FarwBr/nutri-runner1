using UnityEngine;
using UnityEngine.SceneManagement;

public enum EstadoJogo { Jogando, Vitoria, Derrota }
public enum FaseJogo { Dieta, IrParaLargada, CorridaRua, IrParaCircuito, Circuito }
public enum TipoItem { Fruta, Agua, Proteina, Doce, Fritura }

/// <summary>
/// Cerebro do jogo. A historia tem tres etapas, nesta ordem:
///
/// 1. DIETA      - a nutricionista manda emagrecer para disputar o campeonato.
///                 Alex monta a dieta fugindo do cheiro da pizzaria.
/// 2. CORRIDA DE RUA - treino amador cronometrado, para queimar caloria.
/// 3. CIRCUITO   - a prova do campeonato, contra outros corredores.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referencias")]
    public Transform largada;
    public Transform circuito;
    public NuvemCheiro nuvem;

    [Header("Limites das barras")]
    public float aguaMax = 100f;
    public float colesterolMax = 100f;
    public float diabetesMax = 100f;

    [Header("Peso (controla a velocidade da nuvem)")]
    public float pesoInicial = 78f;
    public float pesoMinimo = 30f;

    [Header("Ritmo")]
    [Tooltip("Correr queima colesterol e glicemia: e assim que se recupera de um susto")]
    public float queimaCorrendo = 3.4f;

    [Header("Dano do cheiro (por segundo)")]
    public float danoColesterol = 9f;
    public float danoDiabetes = 7.5f;
    [Tooltip("Quilos por segundo dentro da nuvem. E o que faz a etapa 1 pesar na etapa 3.")]
    public float ganhoDePesoNoCheiro = 1.1f;

    [Header("Pressao")]
    [Tooltip("A cada quantos alimentos a pizzaria solta mais uma nuvem")]
    public int itensPorNuvemExtra = 3;
    public int maximoNuvensExtras = 3;
    public Pizzaria fabricaDeNuvens;

    [HideInInspector] public float Agua;
    [HideInInspector] public float Colesterol;
    [HideInInspector] public float Diabetes;
    [HideInInspector] public float Peso;
    [HideInInspector] public float Distancia;
    [HideInInspector] public int Pontos;
    [HideInInspector] public int ItensBons;
    [HideInInspector] public int TotalItensBons = 10;
    [HideInInspector] public int ItensRuins;
    [HideInInspector] public int Frutas;
    [HideInInspector] public int Aguas;
    [HideInInspector] public int Proteinas;
    [HideInInspector] public float TempoDeJogo;
    [HideInInspector] public EstadoJogo Estado = EstadoJogo.Jogando;
    [HideInInspector] public FaseJogo Fase = FaseJogo.Dieta;
    [HideInInspector] public string MensagemFim = "";
    [HideInInspector] public string Aviso = "";

    // informacoes so para o HUD
    [HideInInspector] public float Perigo;          // 0..1, quanto a nuvem esta perto
    [HideInInspector] public float DistanciaNuvem = 999f;
    // ---- corrida (fase 2) ----
    [HideInInspector] public float TempoRestante;
    float tempoDoTreino;        // o limite com que o treino comecou, para medir a sobra
    [HideInInspector] public int CheckpointAtual;
    [HideInInspector] public int TotalCheckpoints;
    [HideInInspector] public Transform AlvoCorrida;

    // ---- circuito (fase 3) ----
    [HideInInspector] public int Volta;
    [HideInInspector] public int TotalVoltas;
    [HideInInspector] public int Posicao = 1;
    [HideInInspector] public int TotalCorredores = 1;
    [HideInInspector] public float VelocidadeNuvem;
    [HideInInspector] public float VelocidadeJogador;
    /// Velocidade que o Alex teria correndo AGORA. A nuvem usa isso como teto.
    [HideInInspector] public float VelocidadeCorridaAtual = 7f;

    // ---- juiz da pista: o que o HUD precisa saber sobre sair da raia ----
    /// True enquanto o Alex nao esta sobre o asfalto da pista.
    [HideInInspector] public bool ForaDaPista;
    /// Para que lado ele saiu: negativo cortou pelo gramado, positivo passou da borda.
    [HideInInspector] public float LadoQueSaiu;
    /// 0 a 1: quanto do tempo tolerado fora da pista ja foi gasto.
    [HideInInspector] public float ForaDaPistaFracao;
    /// Metros de prova ja validados, e o total a cumprir.
    [HideInInspector] public float MetrosCorridos;
    [HideInInspector] public float MetrosDaProva;

    // ---------------- folego (so na prova final) ----------------
    // E aqui que a dieta cobra a conta. Nas duas primeiras etapas o peso decide
    // VELOCIDADE; na prova ele decide tambem por QUANTO TEMPO o Alex sustenta
    // essa velocidade. Sem isso, quem fez a dieta direito e quem raspou chegavam
    // na pista em pe de igualdade, e as tres etapas eram uma sequencia em vez de
    // um sistema.
    [Header("Folego da prova final (segundos de corrida continua)")]
    [Tooltip("Com o Alex no peso ideal. A prova de 400 m leva uns 51 s correndo.")]
    public float folegoLeve = 60f;
    [Tooltip("Sem ter feito a dieta")]
    public float folegoPesado = 22f;
    [Tooltip("Segundos para recuperar o folego do zero, no peso ideal")]
    public float recuperaLeve = 11f;
    public float recuperaPesado = 22f;
    [Tooltip("Fracao de folego necessaria para poder correr de novo")]
    public float folegoParaRetomar = 0.33f;

    [Header("Condicionamento (ganho na etapa 2)")]
    [Tooltip("Segundos de sprint que um treino de rua perfeito acrescenta")]
    public float bonusMaximoDeFolego = 18f;
    [Tooltip("Peso das auras de treino contra o tempo que sobrou no relogio")]
    public float pesoDasAuras = 0.65f;
    [Tooltip("Ganho de velocidade de corrida com o condicionamento cheio")]
    public float ganhoDeVelocidade = 0.08f;

    /// 0 a 1: o quanto o Alex se preparou na etapa 2. Fechado quando o treino acaba.
    [HideInInspector] public float Condicionamento;

    /// Multiplicador de velocidade vindo do condicionamento.
    ///
    /// O folego sozinho nao bastava: quem fechou a dieta perfeita ja tinha folego
    /// de sobra para os 400 m, entao o treino nao mudava nada justamente para
    /// quem jogou bem. Estar em forma precisa deixar o Alex mais RAPIDO, nao so
    /// aguentar mais tempo - que alias e o que acontece de verdade.
    public float FatorDeCondicionamento
    {
        get { return 1f + ganhoDeVelocidade * Mathf.Clamp01(Condicionamento); }
    }
    [HideInInspector] public int TreinosColetados;
    [HideInInspector] public int TreinosTotal;

    /// 0 a 1. O HUD desenha, o controlador gasta.
    [HideInInspector] public float Folego = 1f;
    /// True enquanto o Alex precisa diminuir o ritmo para recuperar.
    [HideInInspector] public bool SemFolego;

    /// O folego so existe na prova. Nas duas primeiras etapas correr e livre -
    /// la o adversario e a nuvem, e tirar a corrida do jogador seria crueldade.
    public bool FolegoAtivo { get { return Fase == FaseJogo.Circuito; } }

    /// Quantos segundos de corrida continua o Alex tem.
    ///
    /// Duas parcelas, uma por etapa: o PESO (etapa 1) define a base, e o
    /// CONDICIONAMENTO (etapa 2) soma por cima. E o que faz as tres etapas se
    /// somarem em vez de se substituirem - antes o treino de rua nao deixava
    /// marca nenhuma na prova.
    public float SegundosDeFolego
    {
        get
        {
            return Mathf.Lerp(folegoPesado, folegoLeve, Leveza)
                 + bonusMaximoDeFolego * Mathf.Clamp01(Condicionamento);
        }
    }

    /// Gasta folego por um frame de corrida. Devolve false quando acabou -
    /// quem chamou deve cair para a velocidade de caminhada.
    public bool GastarFolego(float dt)
    {
        if (!FolegoAtivo) return true;
        if (SemFolego) return false;

        Folego -= dt / Mathf.Max(1f, SegundosDeFolego);
        if (Folego <= 0f)
        {
            Folego = 0f;
            SemFolego = true;
            Notificar("SEM FOLEGO - diminua o ritmo para recuperar");
            return false;
        }
        return true;
    }

    public void RecuperarFolego(float dt)
    {
        if (!FolegoAtivo) { Folego = 1f; SemFolego = false; return; }

        float tempo = Mathf.Lerp(recuperaPesado, recuperaLeve, Leveza);
        Folego = Mathf.Min(1f, Folego + dt / Mathf.Max(1f, tempo));

        // Histerese: so libera a corrida com um terco do folego de volta. Sem
        // isso o Alex recuperaria um fio e gastaria no mesmo frame, piscando
        // entre correr e andar.
        if (SemFolego && Folego >= folegoParaRetomar) SemFolego = false;
    }

    float esforco;
    int nuvensExtras;
    float tempoAviso;
    float perigoFrame;

    /// 0 = Alex pesado no inicio, 1 = Alex no peso ideal.
    /// A nuvem le esse valor: quanto mais leve o Alex, mais rapido o cheiro.
    public float Leveza
    {
        get { return Mathf.Clamp01(Mathf.InverseLerp(pesoInicial, pesoMinimo, Peso)); }
    }

    public float Progresso
    {
        get { return Mathf.Clamp01((float)ItensBons / Mathf.Max(1, TotalItensBons)); }
    }

    public string ObjetivoTexto
    {
        get
        {
            if (Estado != EstadoJogo.Jogando) return "";
            switch (Fase)
            {
                case FaseJogo.Dieta:
                    return "ETAPA 1  MONTE A DIETA, fugindo do cheiro   "
                         + ItensBons + " / " + TotalItensBons;
                case FaseJogo.IrParaLargada:
                    return "Dieta fechada! VA ATE A LARGADA para o treino de rua";
                case FaseJogo.CorridaRua:
                    return "ETAPA 2  TREINO DE RUA   portico "
                         + CheckpointAtual + " / " + TotalCheckpoints;
                case FaseJogo.IrParaCircuito:
                    return "Treino concluido! VA ATE O CIRCUITO para a prova";
                default:
                    return "ETAPA 3  CAMPEONATO   volta " + Volta + " / " + TotalVoltas
                         + "     " + Posicao + "o lugar";
            }
        }
    }

    void Awake()
    {
        Instance = this;
        Reiniciar();
    }

    public void Reiniciar()
    {
        Agua = aguaMax;
        Colesterol = colesterolMax * 0.45f;
        Diabetes = diabetesMax * 0.40f;
        Peso = pesoInicial;
        Distancia = 0f;
        Pontos = 0;
        ItensBons = 0;
        ItensRuins = 0;
        Frutas = 0;
        Aguas = 0;
        Proteinas = 0;
        TempoDeJogo = 0f;
        CheckpointAtual = 0;
        TotalCheckpoints = 0;
        TempoRestante = 0f;
        AlvoCorrida = null;
        Volta = 0;
        TotalVoltas = 0;
        Posicao = 1;
        nuvensExtras = 0;
        Perigo = 0f;
        Fase = FaseJogo.Dieta;
        Aviso = "";
        MensagemFim = "";
        Estado = EstadoJogo.Jogando;
    }

    void Update()
    {
        if (tempoAviso > 0f)
        {
            tempoAviso -= Time.deltaTime;
            if (tempoAviso <= 0f) Aviso = "";
        }

        // valores reportados por outros scripts valem so por um frame
        Perigo = Mathf.Lerp(Perigo, perigoFrame, Time.deltaTime * 8f);
        perigoFrame = 0f;

        if (Estado != EstadoJogo.Jogando)
        {
            if (Entrada.ReiniciarAgora) RecarregarCena();
            return;
        }

        float dt = Time.deltaTime;
        TempoDeJogo += dt;

        // O exercicio limpa colesterol e glicemia. O PESO so muda comendo:
        // e o peso que controla a velocidade da nuvem, entao precisa ser previsivel.
        float queima = queimaCorrendo * Mathf.Lerp(0.15f, 1f, esforco);
        Colesterol -= queima * dt;
        Diabetes -= queima * dt;

        if (Fase == FaseJogo.CorridaRua)
        {
            TempoRestante -= dt;
            if (TempoRestante <= 0f)
            {
                TempoRestante = 0f;
                Estado = EstadoJogo.Derrota;
                MensagemFim = "TEMPO ESGOTADO - ALEX NAO COMPLETOU A PROVA";
                return;
            }
        }

        esforco = 0f;
        Limitar();
        VerificarFim();
    }

    public void ReportarEsforco(float intensidade) { esforco = Mathf.Clamp01(intensidade); }

    public void RegistrarDistancia(float metros)
    {
        if (Estado != EstadoJogo.Jogando) return;
        Distancia += metros;
    }

    public void DefinirTotalItens(int total)
    {
        TotalItensBons = Mathf.Max(1, total);
    }

    // ---------------- coleta ----------------

    public void Coletar(TipoItem tipo)
    {
        if (Estado != EstadoJogo.Jogando) return;

        switch (tipo)
        {
            case TipoItem.Fruta:
                Colesterol -= 12f; Peso -= 4.0f; Pontos += 40; ItensBons++; Frutas++;
                Notificar("+ Fruta: mais leve, mas o cheiro acelerou");
                break;

            case TipoItem.Agua:
                Diabetes -= 12f; Peso -= 3.0f; Pontos += 35; ItensBons++; Aguas++;
                Notificar("+ Agua: o cheiro se dispersou!");
                if (nuvem != null) nuvem.Repelir(2.4f);   // agua espanta o cheiro
                break;

            case TipoItem.Proteina:
                Peso -= 5.5f; Colesterol -= 6f; Pontos += 50; ItensBons++; Proteinas++;
                Notificar("+ Proteina: mais forte e mais rapido");
                break;

            // Doce e Fritura nao nascem mais no mapa nesta versao.
            case TipoItem.Doce:
                Diabetes += 22f; Peso += 6f; Pontos -= 60; ItensRuins++;
                Notificar("- Voce alimentou o cheiro! Ele acelerou");
                if (nuvem != null) nuvem.Acelerar(0.45f);
                break;

            case TipoItem.Fritura:
                Colesterol += 20f; Peso += 7f; Pontos -= 60; ItensRuins++;
                Notificar("- Voce alimentou o cheiro! Ele acelerou");
                if (nuvem != null) nuvem.Acelerar(0.45f);
                break;
        }

        Limitar();

        if (Fase == FaseJogo.Dieta)
        {
            if (ItensBons >= TotalItensBons) ConcluirColeta();
            else TalvezSoltarNuvem();
        }

        VerificarFim();
    }

    /// O cheiro se multiplica conforme Alex avanca: no fim da fase existem varios
    /// perseguidores vindo de angulos diferentes, e correr reto deixa de bastar.
    void TalvezSoltarNuvem()
    {
        if (fabricaDeNuvens == null) return;
        if (nuvensExtras >= maximoNuvensExtras) return;
        if (ItensBons % Mathf.Max(1, itensPorNuvemExtra) != 0) return;

        nuvensExtras++;
        fabricaDeNuvens.SoltarNuvemExtra();
        Notificar("A pizzaria soltou mais cheiro na rua!");
    }

    void ConcluirColeta()
    {
        Fase = FaseJogo.IrParaLargada;
        Pontos += 300;
        Notificar("Dieta fechada! A nutricionista liberou o treino de rua");
        if (nuvem != null) nuvem.Repelir(6f);
    }

    // ---------------- nuvem ----------------

    public void DanoCheiro(float intensidade)
    {
        if (Estado != EstadoJogo.Jogando) return;

        float dt = Time.deltaTime;
        Colesterol += danoColesterol * intensidade * dt;
        Diabetes += danoDiabetes * intensidade * dt;

        // Ser pego pelo cheiro tambem ENGORDA, e isso tem tres efeitos de uma vez:
        //  1. tematicamente e o que deveria acontecer - ele cedeu ao cheiro;
        //  2. da variacao ao peso com que ele chega na prova final, que e o que faz
        //     o folego da etapa 3 significar algo (fechar a dieta exige os 10 itens,
        //     entao sem isto todo mundo chegaria na pista com o mesmo peso);
        //  3. engordar deixa a nuvem mais LENTA, porque a velocidade dela segue a
        //     leveza do Alex - um elastico que evita a espiral de morte.
        Peso += ganhoDePesoNoCheiro * intensidade * dt;

        Limitar();
        VerificarFim();
    }

    public void ReportarPerigo(float perigo01, float distancia)
    {
        perigoFrame = Mathf.Max(perigoFrame, Mathf.Clamp01(perigo01));
        DistanciaNuvem = distancia;
    }

    // ---------------- corrida ----------------

    public void IniciarCorrida(int totalCheckpoints, float tempo)
    {
        if (Fase == FaseJogo.CorridaRua) return;
        Fase = FaseJogo.CorridaRua;
        TotalCheckpoints = Mathf.Max(1, totalCheckpoints);
        CheckpointAtual = 0;
        TempoRestante = tempo;
        tempoDoTreino = tempo;
    }

    public void DefinirAlvoCorrida(Transform alvo, int numero)
    {
        AlvoCorrida = alvo;
        CheckpointAtual = numero;
    }

    /// Uma aura azul coletada no treino de rua.
    public void RegistrarTreino()
    {
        TreinosColetados++;
        Pontos += 60;
        Notificar("CONDICIONAMENTO +  (" + TreinosColetados + " de " + TreinosTotal + ")");
    }

    public void PassarCheckpoint(float bonusDeTempo)
    {
        TempoRestante += bonusDeTempo;
        Pontos += 120;
        Notificar("Portico " + CheckpointAtual + " cruzado!  +"
                + Mathf.RoundToInt(bonusDeTempo) + "s");
    }

    public void TerminarCorrida()
    {
        AlvoCorrida = null;
        Fase = FaseJogo.IrParaCircuito;
        Pontos += Mathf.RoundToInt(TempoRestante) * 20;   // sobrou tempo, vale ponto

        FecharCondicionamento();

        Notificar("Treino concluido! +" + Mathf.RoundToInt(FolegoGanhoNoTreino)
                + " s de folego para a prova");
    }

    /// Quanto folego o treino rendeu, em segundos. So para a mensagem e o relatorio.
    public float FolegoGanhoNoTreino
    {
        get { return bonusMaximoDeFolego * Mathf.Clamp01(Condicionamento); }
    }

    /// Duas fontes: as auras de treino que ele desviou para pegar, e o tempo que
    /// sobrou no relogio. A primeira pesa mais, porque custou uma decisao; a
    /// segunda e consequencia de ter corrido bem.
    void FecharCondicionamento()
    {
        float porAuras = (TreinosTotal > 0)
                       ? (float)TreinosColetados / TreinosTotal
                       : 0f;
        float porTempo = (tempoDoTreino > 0.01f)
                       ? Mathf.Clamp01(TempoRestante / tempoDoTreino)
                       : 0f;

        float w = Mathf.Clamp01(pesoDasAuras);
        Condicionamento = Mathf.Clamp01(porAuras * w + porTempo * (1f - w));
    }

    // ---------------- circuito (fase 3) ----------------

    public void IniciarCircuito(int voltas, int corredores)
    {
        if (Fase == FaseJogo.Circuito) return;
        Fase = FaseJogo.Circuito;
        TotalVoltas = Mathf.Max(1, voltas);
        TotalCorredores = Mathf.Max(1, corredores);
        Volta = 1;
        Posicao = TotalCorredores;
        TempoRestante = 0f;
    }

    public void RegistrarVolta()
    {
        Volta++;
        Pontos += 150;
        if (Volta <= TotalVoltas) Notificar("Volta " + Volta + " de " + TotalVoltas);
    }

    public void ReportarPosicao(int lugar)
    {
        Posicao = Mathf.Clamp(lugar, 1, TotalCorredores);
    }

    public void TerminarCircuito(int lugar)
    {
        Posicao = Mathf.Clamp(lugar, 1, TotalCorredores);

        if (lugar <= 3)
        {
            Estado = EstadoJogo.Vitoria;
            Pontos += (4 - lugar) * 500;
            MensagemFim = (lugar == 1)
                ? "CAMPEAO! ALEX GANHOU O PODIO"
                : lugar + "o LUGAR - ALEX SUBIU AO PODIO";
        }
        else
        {
            Estado = EstadoJogo.Derrota;
            MensagemFim = "FORA DO PODIO - " + lugar + "o LUGAR";
        }
    }

    /// Fim de prova por infracao, nao por classificacao. No atletismo de verdade
    /// cortar a raia e desclassificacao, nao perda de tempo - aqui tambem.
    public void Desclassificar(string motivo)
    {
        if (Estado != EstadoJogo.Jogando) return;

        ForaDaPista = false;
        ForaDaPistaFracao = 0f;
        Estado = EstadoJogo.Derrota;
        MensagemFim = "DESCLASSIFICADO\n" + motivo;
    }

    // ---------------- util ----------------

    /// Fechamento educativo: o que o Alex consumiu e como o corpo dele terminou.
    public string Relatorio()
    {
        int minutos = Mathf.FloorToInt(TempoDeJogo / 60f);
        int segundos = Mathf.FloorToInt(TempoDeJogo % 60f);
        float perdido = pesoInicial - Peso;

        string r = "TEMPO  " + minutos + ":" + segundos.ToString("00")
                 + "      DISTANCIA  " + Mathf.FloorToInt(Distancia) + " m\n\n";

        r += "PESO   " + Mathf.RoundToInt(pesoInicial) + " kg  ->  "
           + Mathf.RoundToInt(Peso) + " kg";
        if (perdido > 0.5f) r += "   (-" + perdido.ToString("0.#") + " kg)";
        r += "\n\n";

        r += "NA DIETA\n";
        r += "   Frutas     " + Frutas + "\n";
        r += "   Agua       " + Aguas + "\n";
        r += "   Proteina   " + Proteinas + "\n\n";

        r += "COLESTEROL " + Mathf.RoundToInt(Colesterol / colesterolMax * 100f) + "%"
           + "      GLICEMIA " + Mathf.RoundToInt(Diabetes / diabetesMax * 100f) + "%\n\n";

        // Fecha o arco: mostra que o peso com que ele chegou na pista foi o que
        // definiu por quanto tempo ele conseguiu sustentar o sprint na prova.
        if (Fase == FaseJogo.Circuito)
        {
            r += "NA PROVA\n";
            r += "   Folego     " + Mathf.RoundToInt(SegundosDeFolego) + " s de sprint"
               + "   (" + Mathf.RoundToInt(Mathf.Lerp(folegoPesado, folegoLeve, Leveza))
               + " s do peso + " + Mathf.RoundToInt(FolegoGanhoNoTreino) + " s do treino)\n";
            r += "   Treino     " + TreinosColetados + " de " + TreinosTotal
               + " auras azuis   ->  +"
               + Mathf.RoundToInt((FatorDeCondicionamento - 1f) * 100f) + "% de velocidade\n";
            if (MetrosDaProva > 1f)
                r += "   Percurso   " + Mathf.FloorToInt(MetrosCorridos) + " de "
                   + Mathf.RoundToInt(MetrosDaProva) + " m\n";
            r += "\n";
        }

        r += Comentario();
        return r;
    }

    /// Comentario de nutricionista, conforme o estado em que o Alex terminou.
    string Comentario()
    {
        if (Estado == EstadoJogo.Derrota)
        {
            if (Colesterol >= colesterolMax)
                return "O cheiro ganhou. Gordura em excesso eleva o colesterol e\n"
                     + "sobrecarrega o coracao. Exercicio reduz esse numero.";
            if (Diabetes >= diabetesMax)
                return "Glicemia no limite. Acucar em excesso leva o corpo a\n"
                     + "resistir a insulina. Agua e fibras ajudam a controlar.";
            return "A prova exige preparo. Cada alimento saudavel deixa o Alex\n"
                 + "mais leve, e mais leve e mais rapido.";
        }

        bool equilibrada = (Frutas > 0 && Aguas > 0 && Proteinas > 0);

        if (!equilibrada)
            return "Voce venceu, mas a dieta foi desequilibrada.\n"
                 + "Fruta, agua e proteina cumprem papeis diferentes no corpo.";

        if (Colesterol < colesterolMax * 0.3f && Diabetes < diabetesMax * 0.3f)
            return "Dieta equilibrada e indices sob controle.\n"
                 + "E assim que se reconstroi a saude: escolha boa, todo dia.";

        return "Prova concluida com uma dieta variada.\n"
             + "Os indices ainda pedem atencao: siga se exercitando.";
    }

    public void Notificar(string texto)
    {
        Aviso = texto;
        tempoAviso = 2.5f;
    }

    void Limitar()
    {
        Colesterol = Mathf.Clamp(Colesterol, 0f, colesterolMax);
        Diabetes = Mathf.Clamp(Diabetes, 0f, diabetesMax);
        Peso = Mathf.Clamp(Peso, pesoMinimo, 100f);
        if (Pontos < 0) Pontos = 0;
    }

    void VerificarFim()
    {
        if (Estado != EstadoJogo.Jogando) return;

        if (Colesterol >= colesterolMax)
        {
            Estado = EstadoJogo.Derrota;
            MensagemFim = "COLESTEROL NO LIMITE - O CHEIRO VENCEU";
        }
        else if (Diabetes >= diabetesMax)
        {
            Estado = EstadoJogo.Derrota;
            MensagemFim = "GLICEMIA NO LIMITE - O CHEIRO VENCEU";
        }
    }

    public void RecarregarCena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
