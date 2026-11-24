using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Constroi a cidade inteira: Tools > Nutri Runner > Construir Cena Demo.
///
/// Aqui ficam os itens de menu, as constantes do mapa e a rotina que monta tudo.
/// O RESTO DA CLASSE esta nos arquivos Builder.*.cs, um por assunto:
///
///   Builder.Paleta.cs       materiais, texturas e os ajudantes de malha
///   Builder.Cidade.cs       terreno, avenidas, praca, predios, marcos
///   Builder.Estadio.cs      a pista de atletismo e as duas provas
///   Builder.Personagens.cs  Avatar, Animators, Alex, NPCs, rivais e a taca
///
/// Sao todos 'partial' do MESMO tipo: nada mudou de escopo na separacao, e cada
/// parte enxerga as outras exatamente como quando era um arquivo so. A divisao e
/// para encontrar as coisas, nao para isolar - um erro de compilacao em qualquer
/// um deles ainda derruba o menu Tools inteiro, porque e tudo o mesmo assembly.
///
/// Layout: praca circular no centro com fonte, avenida em anel contornando ela,
/// quatro avenidas radiais e aneis de quarteiroes fechando o horizonte.
///
/// Desempenho: sombreamento por vertice, sem sombras, predios distantes
/// simplificados e todo o cenario marcado como static para juntar draw calls.
/// </summary>
public static partial class DemoSceneBuilder
{
    const string PASTA = "Assets/NutriRunner";
    const string PASTA_MAT = PASTA + "/Materials";
    const string PASTA_MODELOS = PASTA + "/Modelos";   // coloque os .fbx aqui

    // --- medidas da cidade ---
    const float LIMITE = 95f;        // parede externa (cresceu para caber o estadio)

    // --- pista de atletismo (etapa 3) ---
    // O modelo "pista_atletismo" e uma pista oficial de 400 m: duas retas de
    // 84,39 m fechadas por semicirculos de 36,50 m de raio, com 8 raias de 1,22 m.
    // Medidas lidas do proprio arquivo, em metros:
    const float ARQ_RETA = 42.195f;      // meia-reta (a reta inteira tem 84,39)
    const float ARQ_R_INTERNO = 36.50f;  // do eixo ate o meio-fio
    const float ARQ_R_MEDIDA = 36.80f;   // raia de medicao: e nela que a volta da 400 m
    const float ARQ_R_EXTERNO = 46.26f;  // borda externa do asfalto
    const float ARQ_R_APRON = 49.26f;    // area cinza em volta
    const float ARQ_COMPRIMENTO = 182.91f; // vao total em X, usado para aferir a escala

    // Em escala 1:1 a pista teria 183 x 99 m e nao caberia na cidade (o mapa tem
    // 190 m de lado e a praca ocupa o meio). Reduzimos pela METADE: a volta passa
    // a ter 200 m exatos, e as proporcoes continuam as de uma pista de verdade.
    const float PISTA_ESCALA = 0.5f;
    const float PISTA_Z = -67f;          // o estadio fica ao sul da cidade

    const float PISTA_RETA = ARQ_RETA * PISTA_ESCALA;
    const float PISTA_R_INTERNO = ARQ_R_INTERNO * PISTA_ESCALA;
    const float PISTA_R_MEDIDA = ARQ_R_MEDIDA * PISTA_ESCALA;
    const float PISTA_R_EXTERNO = ARQ_R_EXTERNO * PISTA_ESCALA;
    const float PISTA_R_APRON = ARQ_R_APRON * PISTA_ESCALA;
    const float PISTA_RAIA = 1.22f * PISTA_ESCALA;        // largura de uma raia

    static readonly Vector3 PISTA_CENTRO = new Vector3(0f, 0f, PISTA_Z);

    const string MODELO_PISTA = "pista_atletismo";
    /// O modelo e um conjunto de planos de espessura zero. Levantamos 3 cm para o
    /// asfalto nao brigar com o chao da cidade, que tem o topo exatamente em y=0.
    const float PISTA_Y = 0.03f;

    // terreno reservado em volta do estadio: nada de predio, rua ou mobiliario
    const float PISTA_MARGEM = 6f;
    const float PISTA_RESERVA_X = PISTA_RETA + PISTA_R_APRON + PISTA_MARGEM;
    const float PISTA_RESERVA_Z = PISTA_R_APRON + PISTA_MARGEM;

    // --- hospital (abertura) ---
    static readonly Vector3 HOSPITAL = new Vector3(-ANEL_A - 2f, 0f, 0f);
    const float RAIO_PRACA = 24f;    // calcadao central
    const float RAIO_ANEL = 30f;     // avenida circular
    const float ANEL_A = 38f;        // onde ficam a pizzaria e a largada
    const float RUA_QUADRO = 44f;    // rua em quadro entre os aneis
    const float ANEL_B = 51f;        // anel de predios na borda do mapa
    const float MEIA_AVENIDA = 5f;   // meia largura das avenidas radiais

    // --- aura dos checkpoints da corrida de rua ---
    const float AURA_RAIO = 2.6f;    // da para atravessar correndo sem mirar
    const float AURA_ALTURA = 9f;    // alta o bastante para ser vista por cima dos predios

    static bool sombreado = true;

    /// Kit de cidade em GRADE: cada predio com base ocupa exatamente 2 x 2 unidades,
    /// entao eles encaixam lado a lado sem medir nada. Pivo na base, Y para cima.
    static readonly string[] MODELOS_PREDIO = {
        "building_A", "building_B", "building_C", "building_D",
        "building_E", "building_F", "building_G", "building_H"
    };
    const string MODELO_LOTE = "base";    // piso 2x2 para lote vazio
    const string MODELO_ARBUSTO = "bush";

    const float TILE = 2f;             // planta do kit, em unidades do modelo
    const float ESCALA_PREDIO = 7f;    // 1,65-3,05 no arquivo -> 11,5 a 21 m em cena
    const float ESCALA_RUA = 5f;       // tile de 2 -> 10 m, a largura exata da avenida
    const float ESCALA_POSTE = 6f;
    const float RESERVA_MARCO = 19f;   // espaco livre em volta de pizzaria/largada

    // kit de parque: ja vem em metros, quase nao precisa escalar
    const float ESCALA_PARQUE = 1.35f;
    const float GIRO_FACHADA = 180f;   // vire para 0 se os predios ficarem de costas
    const float CHANCE_SEGUNDA_FILA = 0.62f;

    // --- personagem com esqueleto (Quaternius Universal Animation Library) ---
    const string MODELO_ALEX = "Mannequin_Medium";
    const string RIG_GERAL = "Rig_Medium_General";
    const string RIG_MOVIMENTO = "Rig_Medium_MovementBasic";
    const string CLIPE_PARADO = "Idle_A";
    const string CLIPE_ANDANDO = "Walking_A";
    const string CLIPE_CORRENDO = "Running_A";
    const string CLIPE_PULANDO = "Jump_Idle";

    // o pack de simulacao traz a comemoracao e o aceno dos NPCs
    const string RIG_SIMULACAO = "Rig_Medium_Simulation";
    const string CLIPE_COMEMORA = "Cheering";
    const string CLIPE_ACENA = "Waving";

    const float ALTURA_ALEX = 1.72f;   // o modelo e reescalado para bater com o collider
    const float ALTURA_NPC = 1.74f;

    // nomes dos estados dos controladores, usados tambem pelos scripts de runtime
    const string ESTADO_LOCOMOCAO = "Locomocao";
    const string ESTADO_ACENANDO = "Acenando";
    const string ESTADO_COMEMORANDO = "Comemorando";

    // ==================================================================
    [MenuItem("Tools/Nutri Runner/Construir Cena Demo")]
    public static void ConstruirSombreado() { sombreado = true; Construir(); }

    [MenuItem("Tools/Nutri Runner/Construir Cena Demo (modo leve, sem luz)")]
    public static void ConstruirLeve() { sombreado = false; Construir(); }

    static void Construir()
    {
        GameObject antigo = GameObject.Find("NutriRunner");
        if (antigo != null) Object.DestroyImmediate(antigo);

        Random.InitState(20260929);
        GameObject raiz = new GameObject("NutriRunner");

        // Primeira linha do Console, antes do barulho da construcao: o que existe
        // na pasta de modelos. Quase todo "ficou feio" comeca por um arquivo que
        // nao esta la.
        InventarioDeModelos();

        materiaisPorTextura.Clear();
        materialSemTextura = null;
        avatarHumano = null;
        controladorNpc = null;
        degrauDoOuro = null;   // senao a 2a construcao aponta para um objeto destruido
        avisouTextura = false;
        texturasDoProjeto = null;
        indiceDeModelos = null;
        jaConfigurado.Clear();
        ListarTexturas();
        CriarMateriais();

        GameObject cenario = CriarTerreno(raiz.transform);
        GameObject praca = CriarPraca(raiz.transform);
        GameObject quadras = CriarQuadras(raiz.transform);
        GameObject largada = CriarLargada(raiz.transform, new Vector3(ANEL_A, 0f, ANEL_A));
        CriarPessoa(largada.transform, "Treinador", new Vector3(-5f, 0f, 3.5f),
                    150f, mRival3, false, ESTADO_ACENANDO);
        CriarCorrida(raiz.transform, largada);

        GameObject hospital = CriarHospital(raiz.transform, HOSPITAL);
        GameObject circuito = CriarPista(raiz.transform);

        Pizzaria chefe;
        GameObject pizzaria = CriarPizzaria(raiz.transform, new Vector3(-ANEL_A, 0f, ANEL_A), out chefe);

        // nuvem sai da pizzaria
        GameObject nuvemGO = Nuvem(raiz.transform, "NuvemDeCheiro",
                                   new Vector3(-ANEL_A, 1.7f, ANEL_A - 12f), 1f, mCheiro);
        NuvemCheiro nuvemPrincipal = nuvemGO.GetComponent<NuvemCheiro>();
        nuvemPrincipal.principal = true;

        Vector3 portaDoHospital = HOSPITAL
                                + (Vector3.zero - HOSPITAL).normalized * 13f;
        portaDoHospital.y = 1f;
        GameObject alex = CriarAlex(raiz.transform, portaDoHospital);
        Camera cam = CriarCamera(raiz.transform, alex.transform);

        // ---------------- alimentos ----------------
        GameObject modelos = new GameObject("Modelos");
        modelos.transform.SetParent(raiz.transform);

        GameObject fruta    = Maca(modelos.transform);
        GameObject agua     = Garrafa(modelos.transform);
        GameObject proteina = Frango(modelos.transform);
        GameObject nuvemPeq = Nuvem(modelos.transform, "NuvemPequena", Vector3.zero, 0.62f, mCheiro2);
        chefe.modeloNuvemPequena = nuvemPeq;

        modelos.SetActive(false);

        GameObject spawnerGO = new GameObject("ItemSpawner");
        spawnerGO.transform.SetParent(raiz.transform);
        ItemSpawner spawner = spawnerGO.AddComponent<ItemSpawner>();
        spawner.modelosBons = new GameObject[] { fruta, agua, proteina };
        spawner.quantidadeBons = 10;
        spawner.raioArea = 30f;
        spawner.distanciaMinimaDoJogador = 14f;

        // ---------------- sistemas ----------------
        GameObject sistemas = new GameObject("Sistemas");
        sistemas.transform.SetParent(raiz.transform);
        sistemas.AddComponent<PerformanceSetup>();
        GameManager gm = sistemas.AddComponent<GameManager>();
        gm.largada = largada.transform;    // fim da etapa 1, partida da etapa 2
        gm.circuito = circuito.transform;  // partida da etapa 3
        gm.nuvem = nuvemPrincipal;
        gm.fabricaDeNuvens = chefe;        // solta nuvens extras a cada X alimentos
        sistemas.AddComponent<HUDController>();
        sistemas.AddComponent<AudioDirector>();
        // As falas vao para o Inspector ja preenchidas: dai em diante da para
        // reescrever qualquer linha sem recompilar.
        Dialogo dlg = sistemas.AddComponent<Dialogo>();
        dlg.falasAbertura = Dialogo.AberturaPadrao();
        dlg.falasTreinador = Dialogo.TreinadorPadrao();
        dlg.falasAntesDaProva = Dialogo.AntesDaProvaPadrao();
        sistemas.AddComponent<TelaInicial>();

        CriarLuzEAmbiente(raiz.transform, cam);

        // ---------------- o estadio manda no seu terreno ----------------
        // Rodado aqui, com tudo ja colocado: mede o que de fato ficou na cena,
        // em vez de confiar nos testes de reserva de cada funcao.
        LimparPista(quadras.transform, circuito.GetComponent<PistaAtletismo>());

        // ---------------- batching ----------------
        Estatico(cenario);
        Estatico(praca);
        Estatico(quadras);
        Estatico(hospital);

        Selection.activeGameObject = raiz;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Renderer[] r = Object.FindObjectsOfType<Renderer>();
        Debug.Log("Nutri Runner: cidade montada (" + (sombreado ? "sombreada" : "modo leve")
                + "). " + r.Length + " objetos desenhados. Salve a cena e aperte Play.");
    }

    // ================= diagnostico =================

    [MenuItem("Tools/Nutri Runner/Verificar Cena")]
    public static void Verificar()
    {
        int problemas = 0;

        if (Object.FindObjectOfType<ThirdPersonController>() == null)
        { Debug.LogError("FALTA: Alex (ThirdPersonController)."); problemas++; }

        Camera[] cams = Object.FindObjectsOfType<Camera>();
        if (cams.Length > 1)
        { Debug.LogWarning("ATENCAO: " + cams.Length + " cameras na cena. Apague as extras."); problemas++; }

        if (Object.FindObjectOfType<OrbitCamera>() == null)
        { Debug.LogError("FALTA: OrbitCamera."); problemas++; }

        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("FALTA: GameManager."); problemas++; }
        else
        {
            if (gm.largada == null) Debug.LogWarning("GameManager sem referencia da largada.");
            if (gm.nuvem == null) Debug.LogWarning("GameManager sem referencia da nuvem.");
        }

        if (Object.FindObjectOfType<ItemSpawner>() == null)
        { Debug.LogError("FALTA: ItemSpawner."); problemas++; }
        CircuitoOlimpico circ = Object.FindObjectOfType<CircuitoOlimpico>();
        if (circ == null) { Debug.LogError("FALTA: CircuitoOlimpico (etapa 3)."); problemas++; }
        else Debug.Log("Circuito com " + circ.rivais.Length + " rivais, "
                     + circ.voltas + " voltas, tolerancia de "
                     + circ.toleranciaFora + " s fora da pista.");

        // ---- a pista e o juiz ----
        PistaAtletismo pst = Object.FindObjectOfType<PistaAtletismo>();
        if (pst == null)
        { Debug.LogError("FALTA: PistaAtletismo. Sem ela a prova nao mede nada."); problemas++; }
        else
        {
            Debug.Log("Pista: volta de " + pst.Perimetro.ToString("F1")
                    + " m, faixa valida de " + pst.raioInterno.ToString("F2")
                    + " a " + pst.raioExterno.ToString("F2") + " m do eixo, reta de "
                    + (pst.reta * 2f).ToString("F1") + " m.");

            if (circ != null && circ.pista == null)
            { Debug.LogError("CircuitoOlimpico sem referencia da pista."); problemas++; }

            Corredor[] cs = Object.FindObjectsOfType<Corredor>(true);
            int semPista = 0;
            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i].pista == null) { semPista++; continue; }
                float d = cs[i].raia;
                if (d < pst.raioInterno || d > pst.raioExterno)
                    Debug.LogWarning("Rival " + cs[i].nome + " esta na raia " + d.ToString("F2")
                                   + " m, fora da faixa de asfalto.");
            }
            if (semPista > 0)
            { Debug.LogError(semPista + " rival(is) sem referencia da pista."); problemas++; }

            // confere que o modelo (ou a reserva de codigo) cobre a geometria usada
            if (!TemArquivo(MODELO_PISTA, ".obj") && !TemArquivo(MODELO_PISTA, ".fbx"))
                Debug.LogWarning("Sem " + MODELO_PISTA + ".obj em " + PASTA_MODELOS
                               + ": a pista esta desenhada por codigo. Jogavel, mas sem"
                               + " as 8 raias pintadas do modelo.");
        }

        if (Object.FindObjectOfType<Dialogo>() == null)
            Debug.LogWarning("Sem Dialogo: a conversa de abertura nao vai aparecer.");

        Corrida corridaCena = Object.FindObjectOfType<Corrida>();
        if (corridaCena == null) { Debug.LogError("FALTA: Corrida (fase 2)."); problemas++; }
        else Debug.Log("Corrida com " + corridaCena.checkpoints.Length + " porticos.");
        if (Object.FindObjectOfType<NuvemCheiro>() == null)
        { Debug.LogError("FALTA: NuvemCheiro."); problemas++; }

        Collectible[] itens = Object.FindObjectsOfType<Collectible>();
        Debug.Log("Alimentos na cena: " + itens.Length);
        for (int i = 0; i < itens.Length && i < 4; i++)
        {
            Renderer[] rs = itens[i].GetComponentsInChildren<Renderer>(true);
            string tam = "SEM RENDERER";
            if (rs.Length > 0)
            {
                Bounds bb = rs[0].bounds;
                for (int k = 1; k < rs.Length; k++) bb.Encapsulate(rs[k].bounds);
                tam = bb.size.ToString("F2") + " em " + bb.center.ToString("F1");
            }
            Debug.Log("   " + itens[i].name + " (" + rs.Length + " malhas) tamanho " + tam);
        }

        InventarioDeModelos();

        Debug.Log("Objetos desenhados: " + Object.FindObjectsOfType<Renderer>().Length
                + " (o batching static junta boa parte deles)");

        // ---- modelos de personagem e o final com a taca ----
        if (!TemModelo(MODELO_ALEX))
        {
            Debug.LogError("FALTA o modelo " + MODELO_ALEX + ".fbx em " + PASTA_MODELOS
                         + ". Alex e os NPCs ficam como bonecos de primitivas.");
            problemas++;
        }
        if (!TemModelo(RIG_GERAL))
        {
            Debug.LogError("FALTA " + RIG_GERAL + ".fbx: sem ele ninguem anda nem corre.");
            problemas++;
        }
        if (!TemModelo(RIG_SIMULACAO))
        {
            Debug.LogError("FALTA " + RIG_SIMULACAO + ".fbx: e dele que vem o '"
                         + CLIPE_COMEMORA + "' (Alex erguendo a taca no final) e o '"
                         + CLIPE_ACENA + "' dos NPCs. Copie para " + PASTA_MODELOS + ".");
            problemas++;
        }

        int humanoides = 0;
        Animator[] ans = Object.FindObjectsOfType<Animator>();
        for (int i = 0; i < ans.Length; i++) if (ans[i].isHuman) humanoides++;
        Debug.Log("Personagens com esqueleto humanoide: " + humanoides + " de " + ans.Length
                + " Animators (o esperado e todos).");

        Comemoracao com = Object.FindObjectOfType<Comemoracao>();
        if (com == null)
        { Debug.LogError("FALTA: Comemoracao no Alex (o final da taca)."); problemas++; }
        else
        {
            if (com.taca == null)
            { Debug.LogError("Comemoracao sem a taca."); problemas++; }
            if (com.animator == null)
                Debug.LogWarning("Comemoracao sem Animator: a taca aparece, mas sem a pose.");
            if (com.podio == null)
                Debug.LogWarning("Comemoracao sem o podio: o Alex comemora onde parou.");
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Debug.Log("Entrada: Input System NOVO.");
#else
        Debug.Log("Entrada: Input Manager ANTIGO.");
#endif

        Debug.Log(problemas == 0 ? "Verificacao: nenhum problema grave."
                                 : "Verificacao: " + problemas + " problema(s).");
    }
}
