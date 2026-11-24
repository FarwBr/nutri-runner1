using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// O mapa: terreno, avenidas, praca, quadras, predios, mobiliario urbano,
/// hospital, pizzaria e a largada.
/// </summary>
public static partial class DemoSceneBuilder
{

    /// Um material por textura de kit. Antes era um material so, o que fazia o
    /// parque sair pintado com a textura da cidade.
    static readonly Dictionary<Texture, Material> materiaisPorTextura =
        new Dictionary<Texture, Material>();
    static readonly HashSet<string> jaConfigurado = new HashSet<string>();

    // --- materiais (preenchidos por CriarMateriais) ---
    static Material mGrama, mCalcada, mCalcadaClara, mAsfalto, mFaixaRua, mMuro;
    static Material mConcreto, mVidro, mTelhado, mTijolo;
    static Material mAgua2, mMetal, mBorracha, mPlaca;
    static Material mTronco, mCopaA, mCopaB, mFlorA, mFlorB, mBanco;
    static Material mCarroA, mCarroB, mCarroC;
    static Material mAura, mAuraForte;
    static Material mTreino, mTreinoForte;
    static Material mBanca, mToldoA, mToldoB, mCaixote, mLaranja, mVerdura;
    static Material mAcademia, mPizzaria, mMassa, mMolho, mCalabresa, mExaustor, mLacre;
    static Material mJaleco, mRival1, mRival2, mRival3, mRival4, mPista, mOuro;
    static Material mPele, mCabelo, mCamiseta, mCalca, mTenis, mOlho;
    static Material mMaca, mCabinho, mFolhaMaca, mGarrafa, mTampa, mRotulo;
    static Material mFrango, mOsso, mForminha, mGlace, mCereja, mPacote, mBatata;
    static Material mAuraBoa, mCheiro, mCheiro2;

    // ================= terreno, avenidas =================

    static GameObject CriarTerreno(Transform pai)
    {
        GameObject g = new GameObject("Cenario");
        g.transform.SetParent(pai);

        Solida(g.transform, "Chao", PrimitiveType.Cube,
               new Vector3(0f, -0.5f, 0f), new Vector3(LIMITE * 2f + 14f, 1f, LIMITE * 2f + 14f), mGrama);

        // avenida em anel: um disco de asfalto com a praca por cima
        Malha(g.transform, "AvenidaAnel", PrimitiveType.Cylinder,
              new Vector3(0f, 0.010f, 0f), new Vector3(RAIO_ANEL * 2f, 0.02f, RAIO_ANEL * 2f), mAsfalto);

        // avenidas radiais, do anel para fora
        float meio = (LIMITE + RAIO_ANEL) * 0.5f;
        float comp = LIMITE - RAIO_ANEL + 6f;
        Malha(g.transform, "AvenidaL", PrimitiveType.Cube,
              new Vector3(meio, 0.012f, 0f), new Vector3(comp, 0.02f, MEIA_AVENIDA * 2f), mAsfalto);
        Malha(g.transform, "AvenidaO", PrimitiveType.Cube,
              new Vector3(-meio, 0.012f, 0f), new Vector3(comp, 0.02f, MEIA_AVENIDA * 2f), mAsfalto);
        Malha(g.transform, "AvenidaN", PrimitiveType.Cube,
              new Vector3(0f, 0.012f, meio), new Vector3(MEIA_AVENIDA * 2f, 0.02f, comp), mAsfalto);
        Malha(g.transform, "AvenidaS", PrimitiveType.Cube,
              new Vector3(0f, 0.012f, -meio), new Vector3(MEIA_AVENIDA * 2f, 0.02f, comp), mAsfalto);


        // faixas tracejadas do anel central
        for (int i = 0; i < 28; i++)
        {
            float a = (i / 28f) * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * ((RAIO_PRACA + RAIO_ANEL) * 0.5f);
            GameObject t = Malha(g.transform, "Traco" + i, PrimitiveType.Cube,
                                 new Vector3(p.x, 0.030f, p.z), new Vector3(0.2f, 0.02f, 1.8f), mFaixaRua);
            t.transform.localRotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
        }

        // faixas de pedestre nas quatro saidas
        for (int i = 0; i < 5; i++)
        {
            float d = -3.2f + i * 1.6f;
            Malha(g.transform, "PedN" + i, PrimitiveType.Cube,
                  new Vector3(d, 0.034f, RAIO_PRACA + 3f), new Vector3(0.8f, 0.02f, 4f), mFaixaRua);
            Malha(g.transform, "PedS" + i, PrimitiveType.Cube,
                  new Vector3(d, 0.034f, -RAIO_PRACA - 3f), new Vector3(0.8f, 0.02f, 4f), mFaixaRua);
            Malha(g.transform, "PedL" + i, PrimitiveType.Cube,
                  new Vector3(RAIO_PRACA + 3f, 0.034f, d), new Vector3(4f, 0.02f, 0.8f), mFaixaRua);
            Malha(g.transform, "PedO" + i, PrimitiveType.Cube,
                  new Vector3(-RAIO_PRACA - 3f, 0.034f, d), new Vector3(4f, 0.02f, 0.8f), mFaixaRua);
        }

        // muros externos
        Solida(g.transform, "MuroN", PrimitiveType.Cube,
               new Vector3(0f, 6f, LIMITE), new Vector3(LIMITE * 2f + 14f, 12f, 4f), mMuro);
        Solida(g.transform, "MuroS", PrimitiveType.Cube,
               new Vector3(0f, 6f, -LIMITE), new Vector3(LIMITE * 2f + 14f, 12f, 4f), mMuro);
        Solida(g.transform, "MuroL", PrimitiveType.Cube,
               new Vector3(LIMITE, 6f, 0f), new Vector3(4f, 12f, LIMITE * 2f + 14f), mMuro);
        Solida(g.transform, "MuroO", PrimitiveType.Cube,
               new Vector3(-LIMITE, 6f, 0f), new Vector3(4f, 12f, LIMITE * 2f + 14f), mMuro);

        MalhaViaria(g.transform);

        // carros nas avenidas
        Carro(g.transform, "Carro0", new Vector3(-2.6f, 0f, 38f), 0f, mCarroA);
        Carro(g.transform, "Carro1", new Vector3(2.6f, 0f, -34f), 180f, mCarroB);
        Carro(g.transform, "Carro2", new Vector3(36f, 0f, 2.6f), 90f, mCarroC);
        Carro(g.transform, "Carro3", new Vector3(-40f, 0f, -2.6f), 270f, mCarroA);
        Carro(g.transform, "Carro4", new Vector3(-18f, 0f, RUA_QUADRO - 2.6f), 90f, mCarroB);
        Carro(g.transform, "Carro5", new Vector3(22f, 0f, -RUA_QUADRO + 2.6f), 270f, mCarroC);

        return g;
    }

    /// Assenta as pecas de asfalto do kit sobre as quatro avenidas radiais.
    /// No modelo a pista fica em y=0,07 e a calcada em y=0,10, entao afundamos
    /// a peca 0,07*escala para o asfalto ficar exatamente no nivel do chao.
    static void MalhaViaria(Transform pai)
    {
        if (!TemModelo("road_straight")) return;

        float y = -0.07f * ESCALA_RUA;
        float[] distancias = { 35f, 45f, 55f, 65f, 75f, 85f };

        for (int i = 0; i < distancias.Length; i++)
        {
            float d = distancias[i];
            // road_straight corre no eixo Z: norte e sul sem girar, leste e oeste a 90
            Peca(pai, "road_straight", new Vector3(0f, y, d), 0f, ESCALA_RUA);
            Peca(pai, "road_straight", new Vector3(d, y, 0f), 90f, ESCALA_RUA);
            Peca(pai, "road_straight", new Vector3(-d, y, 0f), 90f, ESCALA_RUA);

            // ao sul a avenida para na entrada do estadio
            if (d < -PISTA_Z - PISTA_RESERVA_Z)
                Peca(pai, "road_straight", new Vector3(0f, y, -d), 0f, ESCALA_RUA);
        }
    }

    // ================= onde os modelos moram =================
    //
    // Historia curta: a pasta Modelos ficava DENTRO de Assets/NutriRunner, e eu
    // mandei apagar essa pasta antes de colar uma versao nova. Resultado: os .fbx
    // do usuario foram junto, e a cena inteira caiu para primitivas.
    //
    // A correcao nao e so avisar: e parar de depender de um caminho fixo. O
    // construtor indexa TODO modelo e TODA textura do projeto uma vez, por nome
    // de arquivo, e procura por nome. Agora tanto faz onde os arquivos estejam -
    // Assets/Modelos, Assets/NutriRunner/Modelos, Assets/Kenney/Kit -, basta que
    // estejam em algum lugar dentro de Assets.

    static Dictionary<string, string> indiceDeModelos;

    /// Reaproveita um material por textura: todos os predios de um kit acabam
    /// dividindo o mesmo material, o que mantem os draw calls baixos.
    static Material MaterialDoKit(Texture tex)
    {
        if (tex == null)
        {
            if (!avisouTextura)
            {
                avisouTextura = true;
                Debug.LogWarning("Algum modelo ficou SEM TEXTURA (vai aparecer cinza). "
                               + "Copie os PNG dos packs para dentro de " + PASTA
                               + " - a linha 'Texturas encontradas' acima mostra o que ele achou.");
            }
            if (materialSemTextura == null)
                materialSemTextura = MatComShader("KitSemTextura",
                                                  new Color(0.75f, 0.75f, 0.75f), ShaderPadrao());
            return materialSemTextura;
        }

        Material m;
        if (materiaisPorTextura.TryGetValue(tex, out m) && m != null) return m;

        m = MatComShader("Kit_" + tex.name, Color.white, ShaderPadrao());
        m.mainTexture = tex;
        materiaisPorTextura[tex] = m;
        return m;
    }

    static GameObject Peca(Transform pai, string nome, Vector3 pos, float giro, float escala)
    {
        GameObject g = InstanciarModelo(pai, nome, escala);
        if (g == null) return null;
        Posicionar(g, pos, giro);
        g.name = nome;
        return g;
    }

    // ================= praca central =================

    static GameObject CriarPraca(Transform pai)
    {
        GameObject g = new GameObject("Praca");
        g.transform.SetParent(pai);

        Malha(g.transform, "Calcadao", PrimitiveType.Cylinder,
              new Vector3(0f, 0.014f, 0f), new Vector3(RAIO_PRACA * 2f, 0.02f, RAIO_PRACA * 2f), mCalcada);
        Malha(g.transform, "Gramado", PrimitiveType.Cylinder,
              new Vector3(0f, 0.020f, 0f), new Vector3(RAIO_PRACA * 1.62f, 0.02f, RAIO_PRACA * 1.62f), mGrama);
        Malha(g.transform, "Largo", PrimitiveType.Cylinder,
              new Vector3(0f, 0.026f, 0f), new Vector3(15f, 0.02f, 15f), mCalcadaClara);

        // quatro caminhos radiais ligando o largo as saidas
        for (int i = 0; i < 4; i++)
        {
            GameObject c = Malha(g.transform, "Caminho" + i, PrimitiveType.Cube,
                                 new Vector3(0f, 0.024f, 0f), new Vector3(4.5f, 0.02f, RAIO_PRACA * 2f), mCalcadaClara);
            c.transform.localRotation = Quaternion.Euler(0f, i * 45f + 22.5f, 0f);
        }

        // ---- fonte no centro ----
        GameObject fonte = new GameObject("Fonte");
        fonte.transform.SetParent(g.transform);
        Malha(fonte.transform, "Borda", PrimitiveType.Cylinder,
              new Vector3(0f, 0.45f, 0f), new Vector3(9f, 0.45f, 9f), mCalcada);
        BoxCollider colFonte = fonte.AddComponent<BoxCollider>();
        colFonte.center = new Vector3(0f, 0.6f, 0f);
        colFonte.size = new Vector3(8.2f, 1.2f, 8.2f);
        Malha(fonte.transform, "Agua", PrimitiveType.Cylinder,
              new Vector3(0f, 0.78f, 0f), new Vector3(8.1f, 0.06f, 8.1f), mAgua2);
        Malha(fonte.transform, "Pedestal", PrimitiveType.Cylinder,
               new Vector3(0f, 1.1f, 0f), new Vector3(2.2f, 0.7f, 2.2f), mCalcada);
        Malha(fonte.transform, "Taca", PrimitiveType.Cylinder,
              new Vector3(0f, 1.9f, 0f), new Vector3(4.2f, 0.18f, 4.2f), mCalcada);
        Malha(fonte.transform, "Haste", PrimitiveType.Cylinder,
              new Vector3(0f, 2.5f, 0f), new Vector3(0.5f, 0.6f, 0.5f), mCalcada);
        Malha(fonte.transform, "Jato", PrimitiveType.Cylinder,
              new Vector3(0f, 3.4f, 0f), new Vector3(0.35f, 0.9f, 0.35f), mAgua2);
        Malha(fonte.transform, "Respingo", PrimitiveType.Sphere,
              new Vector3(0f, 4.3f, 0f), new Vector3(1.4f, 1.0f, 1.4f), mAgua2);

        // ---- canteiros floridos em volta da fonte ----
        for (int i = 0; i < 8; i++)
        {
            float a = (i / 8f) * Mathf.PI * 2f + 0.39f;
            Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 13.5f;

            GameObject cant = new GameObject("Canteiro" + i);
            cant.transform.SetParent(g.transform);
            cant.transform.position = p;
            cant.transform.rotation = Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);

            Malha(cant.transform, "Borda", PrimitiveType.Cylinder,
                  new Vector3(0f, 0.24f, 0f), new Vector3(4.2f, 0.24f, 4.2f), mCalcada);
            Malha(cant.transform, "Terra", PrimitiveType.Cylinder,
                  new Vector3(0f, 0.5f, 0f), new Vector3(3.6f, 0.04f, 3.6f), mGrama);
            bool temParque = TemModelo("flower_A") || TemModelo("bush");
            for (int f = 0; f < 5; f++)
            {
                float fa = (f / 5f) * Mathf.PI * 2f;
                Vector3 pf = new Vector3(Mathf.Cos(fa) * 1.1f, 0.5f, Mathf.Sin(fa) * 1.1f);

                if (temParque)
                {
                    string[] plantas = { "flower_A", "flower_B", "bush", "grass_A" };
                    string planta = plantas[Random.Range(0, plantas.Length)];
                    if (!TemModelo(planta)) planta = "bush";
                    if (TemModelo(planta))
                    {
                        Peca(cant.transform, planta,
                             cant.transform.position + pf,
                             Random.Range(0f, 360f), ESCALA_PARQUE);
                        continue;
                    }
                }

                Malha(cant.transform, "Flor" + f, PrimitiveType.Sphere,
                      new Vector3(pf.x, 0.75f, pf.z),
                      new Vector3(1.3f, 0.7f, 1.3f), (i + f) % 2 == 0 ? mFlorA : mFlorB);
            }
        }

        // ---- arvores no gramado ----
        for (int i = 0; i < 12; i++)
        {
            float a = (i / 12f) * Mathf.PI * 2f + 0.13f;
            Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 19f;
            Arvore(g.transform, "Arvore" + i, p);
        }

        // ---- bancos voltados para a fonte ----
        for (int i = 0; i < 8; i++)
        {
            float a = (i / 8f) * Mathf.PI * 2f;
            Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 8.5f;
            Banco(g.transform, "Banco" + i, p, -a * Mathf.Rad2Deg + 90f);
        }

        // ---- postes ----
        for (int i = 0; i < 10; i++)
        {
            float a = (i / 10f) * Mathf.PI * 2f + 0.3f;
            Vector3 p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 22f;
            Poste(g.transform, "Poste" + i, p, -a * Mathf.Rad2Deg + 180f);
        }

        DetalhesDoParque(g.transform);
        return g;
    }

    /// Moitas, tufos de grama, pedras e passaros espalhados pelo gramado da praca.
    /// Tudo rasteiro: nada disso chega na altura dos alimentos.
    static void DetalhesDoParque(Transform pai)
    {
        string[] rasteiros = { "bush", "bush_large", "grass_A", "flower_A", "flower_B",
                               "cobble_stones", "cobble_stones_large" };

        for (int i = 0; i < 46; i++)
        {
            float a = Random.Range(0f, Mathf.PI * 2f);
            float r = Random.Range(10f, 22f);
            Vector3 pos = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);

            string nome = rasteiros[Random.Range(0, rasteiros.Length)];
            if (!TemModelo(nome)) continue;

            Peca(pai, nome, pos, Random.Range(0f, 360f), ESCALA_PARQUE);
        }

        if (TemModelo("bird"))
        {
            for (int i = 0; i < 5; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(11f, 20f);
                Peca(pai, "bird",
                     new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r),
                     Random.Range(0f, 360f), ESCALA_PARQUE);
            }
        }

        if (TemModelo("hedge_corner"))
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                Peca(pai, "hedge_corner",
                     new Vector3(Mathf.Cos(a) * 21f, 0f, Mathf.Sin(a) * 21f),
                     -a * Mathf.Rad2Deg, ESCALA_PARQUE);
            }
        }
    }

    // ================= quarteiroes =================

    static GameObject CriarQuadras(Transform pai)
    {
        GameObject g = new GameObject("Quadras");
        g.transform.SetParent(pai);

        // Nada de predio no miolo: eles escondiam a comida durante a fuga.
        // Toda a massa construida fica encostada na borda do mapa.
        if (TemModelosDePredio())
        {
            AnelDeModelos(g.transform);
        }
        else
        {
            Vector3[] reservados = new Vector3[0];
            AnelDePredios(g.transform, "B", ANEL_B,
                          new float[] { -44f, -32f, -21f, -10f, 10f, 21f, 32f, 44f },
                          8f, 15f, 30f, true, reservados);

            Vector3[] cantos = {
                new Vector3(ANEL_B, 0f, ANEL_B), new Vector3(-ANEL_B, 0f, ANEL_B),
                new Vector3(ANEL_B, 0f, -ANEL_B), new Vector3(-ANEL_B, 0f, -ANEL_B)
            };
            for (int i = 0; i < cantos.Length; i++)
                Predio(g.transform, "Torre" + i, cantos[i], 11f, 11f, Random.Range(22f, 30f),
                       mFachadas[i % mFachadas.Length], true);
        }

        // ---- miolo aberto: so mobiliario baixo, que nao esconde alimento ----
        for (int i = 0; i < 8; i++)
        {
            float a = (i / 8f) * Mathf.PI * 2f + 0.39f;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));

            Poste(g.transform, "PosteMeio" + i, dir * 34f, -a * Mathf.Rad2Deg + 180f);
            Poste(g.transform, "PosteFora" + i, dir * 46f, -a * Mathf.Rad2Deg + 180f);

            GameObject cant = new GameObject("CanteiroMeio" + i);
            cant.transform.SetParent(g.transform);
            cant.transform.position = dir * 39f;
            Malha(cant.transform, "Borda", PrimitiveType.Cylinder,
                  new Vector3(0f, 0.16f, 0f), new Vector3(6f, 0.16f, 6f), mCalcada);
            Malha(cant.transform, "Terra", PrimitiveType.Cylinder,
                  new Vector3(0f, 0.34f, 0f), new Vector3(5.4f, 0.03f, 5.4f), mGrama);
            for (int f = 0; f < 5; f++)
            {
                float fa = (f / 5f) * Mathf.PI * 2f;
                Malha(cant.transform, "Flor" + f, PrimitiveType.Sphere,
                      new Vector3(Mathf.Cos(fa) * 1.5f, 0.5f, Mathf.Sin(fa) * 1.5f),
                      new Vector3(1.5f, 0.55f, 1.5f), (i + f) % 2 == 0 ? mFlorA : mFlorB);
            }
        }

        return g;
    }

    static bool TemModelosDePredio()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(
                   CaminhoDoModelo(MODELOS_PREDIO[0], ".fbx")) != null;
    }

    /// Monta a borda em GRADE: como cada peca do kit ocupa 2 x 2, basta andar de
    /// passo em passo. Duas fileiras de profundidade, a de tras sempre cheia e a da
    /// frente sorteada, o que da um recorte irregular de quarteirao.
    static void AnelDeModelos(Transform pai)
    {
        float passo = TILE * ESCALA_PREDIO;
        float externo = LIMITE - 2.2f - passo * 0.5f;   // encostada no muro
        float meio = externo - passo;
        float interno = meio - passo;                   // de frente para a praca

        // fileira, recuo, chance de ter predio (o resto vira lote vazio)
        float[] recuos = { externo, meio, interno };
        float[] chances = { 1f, CHANCE_SEGUNDA_FILA, 1f };

        for (int lado = 0; lado < 4; lado++)
        {
            bool eixoX = (lado < 2);

            for (int fila = 0; fila < recuos.Length; fila++)
            {
                // leste/oeste param antes para nao bater nas pontas de norte/sul
                float alcance = eixoX ? externo : recuos[fila] - passo;

                int n = Mathf.FloorToInt((alcance * 2f) / passo) + 1;
                float inicio = -((n - 1) * 0.5f) * passo;

                for (int i = 0; i < n; i++)
                {
                    float t = inicio + i * passo;
                    if (Mathf.Abs(t) < MEIA_AVENIDA + passo * 0.4f) continue; // avenida

                    Vector3 onde = Local(lado, t, recuos[fila]);
                    if (Reservado(onde, passo * 0.5f)) continue;

                    if (Random.value < chances[fila])
                        ColocarPeca(pai, lado, t, recuos[fila], Sorteado(MODELOS_PREDIO));
                    else
                        ColocarPeca(pai, lado, t, recuos[fila], MODELO_LOTE);
                }
            }
        }

        ArbustosDaBorda(pai, interno - passo * 0.72f);
        PropsDaRua(pai, interno - passo * 0.6f);
    }

    /// Mobiliario urbano do kit, todo na faixa da calcada em frente aos predios,
    /// FORA do raio onde os alimentos nascem. Nada disso esconde comida.
    static void PropsDaRua(Transform pai, float raio)
    {
        // semaforos nas quatro bocas de avenida
        if (TemModelo("trafficlight_B"))
        {
            float d = RAIO_ANEL + 2.5f;
            Peca(pai, "trafficlight_B", new Vector3(-MEIA_AVENIDA - 1.5f, 0f, d), 180f, 5f);
            Peca(pai, "trafficlight_B", new Vector3(MEIA_AVENIDA + 1.5f, 0f, -d), 0f, 5f);
            Peca(pai, "trafficlight_B", new Vector3(d, 0f, MEIA_AVENIDA + 1.5f), 270f, 5f);
            Peca(pai, "trafficlight_B", new Vector3(-d, 0f, -MEIA_AVENIDA - 1.5f), 90f, 5f);
        }

        string[] miudos = { "firehydrant", "trash_A", "trash_B", "dumpster" };
        for (int lado = 0; lado < 4; lado++)
        {
            for (float t = -raio + 8f; t <= raio - 8f; t += 9f)
            {
                if (Mathf.Abs(t) < MEIA_AVENIDA + 4f) continue;
                if (Random.value > 0.45f) continue;

                string nome = miudos[Random.Range(0, miudos.Length)];
                if (!TemModelo(nome)) continue;

                Vector3 p = Local(lado, t + Random.Range(-2f, 2f), raio);
                if (Reservado(p, 2.5f)) continue;
                Peca(pai, nome, p, Random.Range(0f, 360f), 5f);
            }
        }

        // caixas d'agua no alto dos lotes de canto
        if (TemModelo("watertower"))
        {
            float c = raio - 6f;
            Vector3[] cantos = { new Vector3(c, 0f, c), new Vector3(-c, 0f, c),
                                 new Vector3(c, 0f, -c), new Vector3(-c, 0f, -c) };
            float[] giros = { 0f, 90f, 180f, 270f };
            for (int i = 0; i < cantos.Length; i++)
                if (!Reservado(cantos[i], 6f))
                    Peca(pai, "watertower", cantos[i], giros[i], 8f);
        }
    }

    /// Posicao no mundo de uma peca (lado, deslocamento, recuo).
    static Vector3 Local(int lado, float t, float recuo)
    {
        if (lado == 0) return new Vector3(t, 0f, recuo);
        if (lado == 1) return new Vector3(t, 0f, -recuo);
        if (lado == 2) return new Vector3(recuo, 0f, t);
        return new Vector3(-recuo, 0f, t);
    }

    /// Pizzaria e largada ocupam lotes do quarteirao: nada de predio por cima.
    static bool Reservado(Vector3 p) { return Reservado(p, 0f); }

    /// "raioDaPeca" = metade do tamanho do que vai ser colocado. Sem isso o teste
    /// olha so o CENTRO da peca, e um predio de 14 m centrado a 6 m da reserva
    /// ainda invade 1 m dela. Foi assim que foram parar arbustos e lixeiras em
    /// cima da pista: eles nem chegavam a consultar esta funcao.
    static bool Reservado(Vector3 p, float raioDaPeca)
    {
        Vector3[] marcos = {
            new Vector3(-ANEL_A, 0f, ANEL_A),   // pizzaria
            new Vector3(ANEL_A, 0f, ANEL_A),    // largada do treino
            HOSPITAL                            // hospital
        };
        for (int i = 0; i < marcos.Length; i++)
            if (Vector3.Distance(p, marcos[i]) < RESERVA_MARCO + raioDaPeca) return true;

        return NaReservaDaPista(p, raioDaPeca);
    }

    /// Coloca uma peca do kit na posicao (lado, deslocamento ao longo do lado, recuo).
    static void ColocarPeca(Transform pai, int lado, float t, float recuo, string modelo)
    {
        GameObject inst = InstanciarModelo(pai, modelo, ESCALA_PREDIO);
        if (inst == null) return;

        Bounds b = BoundsDe(inst);

        // lote e arbusto nao precisam bloquear o jogador
        if (modelo != MODELO_LOTE && b.size.y > 1f)
        {
            BoxCollider bc = inst.AddComponent<BoxCollider>();
            bc.center = b.center / ESCALA_PREDIO;
            bc.size = b.size / ESCALA_PREDIO;
        }

        Vector3 pos = Local(lado, t, recuo);
        float giro = GIRO_FACHADA + lado * 90f;
        if (lado == 1) giro = GIRO_FACHADA + 180f;
        else if (lado == 2) giro = GIRO_FACHADA + 90f;
        else if (lado == 3) giro = GIRO_FACHADA + 270f;

        // o lote e so um piso de 0,1: afunda para ficar rente ao gramado
        if (modelo == MODELO_LOTE) pos.y = -0.1f * ESCALA_PREDIO + 0.03f;

        Posicionar(inst, pos, giro);
        inst.name = modelo;
    }

    /// Fileira de arbustos na calcada, na frente dos predios.
    /// Fica fora da area onde os alimentos nascem, entao nao esconde nada.
    static void ArbustosDaBorda(Transform pai, float raio)
    {
        if (CarregarModelo(MODELO_ARBUSTO, ".fbx") == null) return;

        for (int lado = 0; lado < 4; lado++)
        {
            for (float t = -raio; t <= raio; t += 6f)
            {
                if (Mathf.Abs(t) < MEIA_AVENIDA + 4f) continue;
                if (Random.value > 0.5f) continue;

                GameObject bush = InstanciarModelo(pai, MODELO_ARBUSTO, 4f);
                if (bush == null) return;

                Vector3 pos;
                if (lado == 0) pos = new Vector3(t, 0f, raio);
                else if (lado == 1) pos = new Vector3(t, 0f, -raio);
                else if (lado == 2) pos = new Vector3(raio, 0f, t);
                else pos = new Vector3(-raio, 0f, t);

                if (Reservado(pos, 2.5f)) { Object.DestroyImmediate(bush); continue; }

                Posicionar(bush, pos, Random.Range(0f, 360f));
                bush.name = "Arbusto";
            }
        }
    }

    static void TorresDeAgua(Transform pai)
    {
        // opcional: so entra se o arquivo existir na pasta de modelos
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PASTA_MODELOS + "/water-tower.fbx") == null)
            return;

        Vector3[] pos = {
            new Vector3(41f, 0f, 41f), new Vector3(-41f, 0f, 41f),
            new Vector3(41f, 0f, -41f), new Vector3(-41f, 0f, -41f)
        };
        for (int i = 0; i < pos.Length; i++)
        {
            GameObject t = InstanciarModelo(pai, "water-tower", 5.5f);
            if (t == null) return;
            Bounds b = BoundsDe(t);
            BoxCollider bc = t.AddComponent<BoxCollider>();
            bc.center = b.center / 5.5f;
            bc.size = b.size / 5.5f;
            Posicionar(t, pos[i], i * 90f);
            t.name = "CaixaDagua" + i;
        }
    }

    /// Igual ao Peca, mas gera um BoxCollider a partir do bounding box.
    static GameObject PecaSolida(Transform pai, string nome, Vector3 pos, float giro, float escala)
    {
        GameObject g = InstanciarModelo(pai, nome, escala);
        if (g == null) return null;

        Bounds b = BoundsDe(g);
        BoxCollider bc = g.AddComponent<BoxCollider>();
        bc.center = b.center / escala;
        bc.size = b.size / escala;

        Posicionar(g, pos, giro);
        g.name = nome;
        return g;
    }

    /// Distribui predios nos quatro lados de um quadrado, virados para o centro.
    static void AnelDePredios(Transform pai, string prefixo, float raio, float[] posicoes,
                              float profundidade, float alturaMin, float alturaMax,
                              bool detalhado, Vector3[] reservados)
    {
        int n = 0;
        for (int lado = 0; lado < 4; lado++)
        {
            for (int i = 0; i < posicoes.Length; i++)
            {
                float t = posicoes[i];
                if (Mathf.Abs(t) < MEIA_AVENIDA + 3f) continue; // nao tampa a avenida

                Vector3 p;
                if (lado == 0) p = new Vector3(t, 0f, raio);
                else if (lado == 1) p = new Vector3(t, 0f, -raio);
                else if (lado == 2) p = new Vector3(raio, 0f, t);
                else p = new Vector3(-raio, 0f, t);

                bool ocupado = false;
                for (int r = 0; r < reservados.Length; r++)
                    if (Vector3.Distance(p, reservados[r]) < 14f) ocupado = true;
                if (ocupado) continue;

                float larg = Random.Range(8f, 9.5f);
                float alt = Random.Range(alturaMin, alturaMax);
                Predio(pai, prefixo + n, p, larg, profundidade, alt,
                       mFachadas[(n + lado) % mFachadas.Length], detalhado);
                n++;
            }
        }
    }

    /// Predio. "detalhado" acrescenta faixas de janela, marquise e caixa d'agua;
    /// os do anel externo saem simplificados porque so compoem o horizonte.
    static void Predio(Transform pai, string nome, Vector3 pos,
                       float larg, float prof, float alt, Material fachada, bool detalhado)
    {
        GameObject g = new GameObject("Predio" + nome);
        g.transform.SetParent(pai);
        g.transform.position = pos;

        Vector3 paraCentro = new Vector3(-pos.x, 0f, -pos.z).normalized;
        if (paraCentro.sqrMagnitude > 0.01f)
            g.transform.rotation = Quaternion.LookRotation(paraCentro);

        Solida(g.transform, "Corpo", PrimitiveType.Cube,
               new Vector3(0f, alt * 0.5f, 0f), new Vector3(larg, alt, prof), fachada);

        Malha(g.transform, "Cornija", PrimitiveType.Cube,
              new Vector3(0f, alt + 0.18f, 0f), new Vector3(larg + 0.8f, 0.36f, prof + 0.8f), mConcreto);
        Malha(g.transform, "Platibanda", PrimitiveType.Cube,
              new Vector3(0f, alt + 0.75f, 0f), new Vector3(larg + 0.2f, 0.8f, prof + 0.2f), mTelhado);

        if (!detalhado) return;

        Malha(g.transform, "Terreo", PrimitiveType.Cube,
              new Vector3(0f, 1.3f, 0f), new Vector3(larg + 0.3f, 2.6f, prof + 0.3f), mConcreto);

        int andares = Mathf.FloorToInt((alt - 4.4f) / 2.8f);
        for (int i = 0; i < andares; i++)
        {
            float y = 4.4f + i * 2.8f;
            if (y > alt - 1.5f) break;
            Malha(g.transform, "Janelas" + i, PrimitiveType.Cube,
                  new Vector3(0f, y, 0f), new Vector3(larg + 0.16f, 1.2f, prof + 0.16f), mVidro);
        }

        Malha(g.transform, "CaixaDagua", PrimitiveType.Cylinder,
              new Vector3(larg * 0.22f, alt + 1.8f, prof * 0.18f), new Vector3(1.4f, 0.7f, 1.4f), mConcreto);
        Malha(g.transform, "Porta", PrimitiveType.Cube,
              new Vector3(0f, 1.15f, prof * 0.5f + 0.24f), new Vector3(2f, 2.3f, 0.16f), mVidro);
        Malha(g.transform, "Marquise", PrimitiveType.Cube,
              new Vector3(0f, 2.8f, prof * 0.5f + 0.6f), new Vector3(3.6f, 0.22f, 1.4f), mConcreto);
    }

    // ================= mobiliario urbano =================

    static void Arvore(Transform pai, string nome, Vector3 pos)
    {
        if (TemModelo("tree"))
        {
            string modelo = (Random.value < 0.35f && TemModelo("tree_large"))
                          ? "tree_large" : "tree";

            GameObject t = Peca(pai, modelo, pos, Random.Range(0f, 360f), ESCALA_PARQUE);
            if (t != null)
            {
                // collider so no tronco: uma caixa do tamanho da copa faria o Alex
                // esbarrar no ar a tres metros do tronco
                Bounds b = BoundsDe(t);
                CapsuleCollider cap = t.AddComponent<CapsuleCollider>();
                cap.radius = 0.22f;
                cap.height = b.size.y / ESCALA_PARQUE;
                cap.center = new Vector3(0f, cap.height * 0.5f, 0f);
            }
            return;
        }

        GameObject a = new GameObject(nome);
        a.transform.SetParent(pai);
        a.transform.position = pos;

        Solida(a.transform, "Tronco", PrimitiveType.Cylinder,
               new Vector3(0f, 1.6f, 0f), new Vector3(0.44f, 1.6f, 0.44f), mTronco);
        Malha(a.transform, "Copa1", PrimitiveType.Sphere,
              new Vector3(0f, 3.7f, 0f), new Vector3(3.2f, 2.8f, 3.2f), mCopaA);
        Malha(a.transform, "Copa2", PrimitiveType.Sphere,
              new Vector3(0.8f, 3.1f, 0.5f), new Vector3(2.1f, 1.9f, 2.1f), mCopaB);
        Malha(a.transform, "Copa3", PrimitiveType.Sphere,
              new Vector3(-0.7f, 3.3f, -0.6f), new Vector3(1.9f, 1.7f, 1.9f), mCopaB);
    }

    static void Banco(Transform pai, string nome, Vector3 pos, float giro)
    {
        if (TemModelo("bench"))
        {
            PecaSolida(pai, "bench", pos, giro, ESCALA_PARQUE);
            return;
        }

        GameObject b = new GameObject(nome);
        b.transform.SetParent(pai);
        b.transform.position = pos;
        b.transform.rotation = Quaternion.Euler(0f, giro, 0f);

        Solida(b.transform, "Assento", PrimitiveType.Cube,
               new Vector3(0f, 0.45f, 0f), new Vector3(2.4f, 0.12f, 0.62f), mBanco);
        MalhaRot(b.transform, "Encosto", PrimitiveType.Cube,
                 new Vector3(0f, 0.76f, -0.29f), new Vector3(-15f, 0f, 0f),
                 new Vector3(2.4f, 0.12f, 0.5f), mBanco);
        Solida(b.transform, "PeE", PrimitiveType.Cube,
               new Vector3(-1.0f, 0.22f, 0f), new Vector3(0.12f, 0.45f, 0.58f), mMetal);
        Solida(b.transform, "PeD", PrimitiveType.Cube,
               new Vector3(1.0f, 0.22f, 0f), new Vector3(0.12f, 0.45f, 0.58f), mMetal);
    }

    static void Poste(Transform pai, string nome, Vector3 pos, float giro)
    {
        if (TemModelo("street_lantern"))
        {
            Peca(pai, "street_lantern", pos, giro, ESCALA_PARQUE);
            return;
        }
        if (TemModelo("streetlight"))
        {
            Peca(pai, "streetlight", pos, giro, ESCALA_POSTE);
            return;
        }

        GameObject p = new GameObject(nome);
        p.transform.SetParent(pai);
        p.transform.position = pos;
        p.transform.rotation = Quaternion.Euler(0f, giro, 0f);

        Solida(p.transform, "Base", PrimitiveType.Cylinder,
               new Vector3(0f, 0.16f, 0f), new Vector3(0.38f, 0.16f, 0.38f), mMetal);
        Solida(p.transform, "Haste", PrimitiveType.Cylinder,
               new Vector3(0f, 2.6f, 0f), new Vector3(0.14f, 2.6f, 0.14f), mMetal);
        MalhaRot(p.transform, "Braco", PrimitiveType.Cylinder,
                 new Vector3(0.5f, 5.1f, 0f), new Vector3(0f, 0f, 90f),
                 new Vector3(0.10f, 0.6f, 0.10f), mMetal);
        Malha(p.transform, "Luminaria", PrimitiveType.Cube,
              new Vector3(1.0f, 5.0f, 0f), new Vector3(0.7f, 0.22f, 0.4f), mPlaca);
    }

    static void Carro(Transform pai, string nome, Vector3 pos, float giro, Material cor)
    {
        GameObject c = new GameObject(nome);
        c.transform.SetParent(pai);
        c.transform.position = pos;
        c.transform.rotation = Quaternion.Euler(0f, giro, 0f);

        Solida(c.transform, "Lataria", PrimitiveType.Cube,
               new Vector3(0f, 0.62f, 0f), new Vector3(1.9f, 0.7f, 4.2f), cor);
        Solida(c.transform, "Cabine", PrimitiveType.Cube,
               new Vector3(0f, 1.18f, -0.2f), new Vector3(1.7f, 0.62f, 2.1f), mVidro);
        Malha(c.transform, "Teto", PrimitiveType.Cube,
              new Vector3(0f, 1.48f, -0.2f), new Vector3(1.72f, 0.1f, 2.0f), cor);

        float[] rx = { -0.98f, 0.98f };
        float[] rz = { 1.35f, -1.35f };
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                MalhaRot(c.transform, "Roda" + i + j, PrimitiveType.Cylinder,
                         new Vector3(rx[i], 0.34f, rz[j]), new Vector3(0f, 0f, 90f),
                         new Vector3(0.34f, 0.10f, 0.34f), mBorracha);
    }

    // ================= pontos de interesse =================

    static GameObject CriarMercado(Transform pai, Vector3 pos)
    {
        GameObject g = new GameObject("Mercado");
        g.transform.SetParent(pai);
        g.transform.position = pos;
        g.transform.rotation = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z).normalized);

        Malha(g.transform, "Piso", PrimitiveType.Cube,
              new Vector3(0f, 0.06f, 0f), new Vector3(22f, 0.12f, 20f), mCalcadaClara);

        for (int i = 0; i < 6; i++)
        {
            float x = -7f + (i % 3) * 7f;
            float z = (i < 3) ? -4.5f : 4.5f;
            GameObject b = new GameObject("Banca" + i);
            b.transform.SetParent(g.transform);
            b.transform.localPosition = new Vector3(x, 0f, z);

            Solida(b.transform, "Balcao", PrimitiveType.Cube,
                   new Vector3(0f, 0.55f, 0f), new Vector3(4f, 1.1f, 2.2f), mBanca);
            Solida(b.transform, "Poste1", PrimitiveType.Cylinder,
                   new Vector3(-1.8f, 1.25f, -0.9f), new Vector3(0.12f, 1.25f, 0.12f), mMetal);
            Solida(b.transform, "Poste2", PrimitiveType.Cylinder,
                   new Vector3(1.8f, 1.25f, -0.9f), new Vector3(0.12f, 1.25f, 0.12f), mMetal);

            for (int t = 0; t < 5; t++)
                Malha(b.transform, "Listra" + t, PrimitiveType.Cube,
                      new Vector3(-1.8f + t * 0.9f, 2.55f, 0f), new Vector3(0.9f, 0.14f, 3f),
                      t % 2 == 0 ? mToldoA : mToldoB);

            for (int c = 0; c < 2; c++)
            {
                float cx = -1.0f + c * 2.0f;
                Malha(b.transform, "Caixote" + c, PrimitiveType.Cube,
                      new Vector3(cx, 1.28f, 0f), new Vector3(1.4f, 0.36f, 1.2f), mCaixote);
                Malha(b.transform, "Produto" + c, PrimitiveType.Sphere,
                      new Vector3(cx, 1.5f, 0f), new Vector3(1.1f, 0.32f, 0.9f),
                      (i + c) % 2 == 0 ? mLaranja : mVerdura);
            }
        }
        return g;
    }

    /// Portico de largada da prova de rua.
    ///
    /// Antes aqui havia um predio de academia que nao tinha funcao nenhuma de jogo
    /// e so ocupava espaco. Virou um portico aberto: ele CARREGA o componente
    /// Corrida e o gatilho da largada, entao a estrutura agora se explica sozinha.
    static GameObject CriarLargada(Transform pai, Vector3 pos)
    {
        GameObject g = new GameObject("Largada");
        g.transform.SetParent(pai);
        g.transform.position = pos;
        // de frente para a praca, que e de onde o jogador chega
        g.transform.rotation = Quaternion.LookRotation(new Vector3(-pos.x, 0f, -pos.z).normalized);

        // ---- pista sob o portico ----
        Malha(g.transform, "Pista", PrimitiveType.Cube,
              new Vector3(0f, 0.03f, 0f), new Vector3(16f, 0.06f, 12f), mAsfalto);
        Malha(g.transform, "CalcadaE", PrimitiveType.Cube,
              new Vector3(-8.6f, 0.08f, 0f), new Vector3(1.6f, 0.16f, 12f), mCalcada);
        Malha(g.transform, "CalcadaD", PrimitiveType.Cube,
              new Vector3(8.6f, 0.08f, 0f), new Vector3(1.6f, 0.16f, 12f), mCalcada);

        // ---- linha quadriculada de largada ----
        for (int f = 0; f < 2; f++)
            for (int c = 0; c < 12; c++)
                Malha(g.transform, "Quadro" + f + c, PrimitiveType.Cube,
                      new Vector3(-7.3f + c * 1.33f, 0.07f, -0.7f + f * 1.4f),
                      new Vector3(1.33f, 0.04f, 1.4f),
                      ((f + c) % 2 == 0) ? mFaixaRua : mBorracha);

        // ---- arco ----
        Solida(g.transform, "PilarE", PrimitiveType.Cube,
               new Vector3(-8.2f, 3.4f, 0f), new Vector3(1.1f, 6.8f, 1.1f), mMetal);
        Solida(g.transform, "PilarD", PrimitiveType.Cube,
               new Vector3(8.2f, 3.4f, 0f), new Vector3(1.1f, 6.8f, 1.1f), mMetal);
        Malha(g.transform, "Viga", PrimitiveType.Cube,
              new Vector3(0f, 7.3f, 0f), new Vector3(18f, 1.1f, 1.1f), mAcademia);
        Malha(g.transform, "Letreiro", PrimitiveType.Cube,
              new Vector3(0f, 8.4f, 0f), new Vector3(13f, 1.4f, 0.3f), mPlaca);

        // faixas penduradas na viga, alternando as cores do evento
        for (int i = 0; i < 9; i++)
            Malha(g.transform, "Bandeira" + i, PrimitiveType.Cube,
                  new Vector3(-7.2f + i * 1.8f, 6.3f, 0f), new Vector3(1.1f, 0.9f, 0.08f),
                  (i % 2 == 0) ? mToldoA : mToldoB);

        // ---- cones delimitando a pista ----
        for (int i = 0; i < 5; i++)
        {
            float z = -5f + i * 2.5f;
            Malha(g.transform, "ConeE" + i, PrimitiveType.Cylinder,
                  new Vector3(-7.4f, 0.4f, z), new Vector3(0.5f, 0.4f, 0.5f), mToldoA);
            Malha(g.transform, "ConeD" + i, PrimitiveType.Cylinder,
                  new Vector3(7.4f, 0.4f, z), new Vector3(0.5f, 0.4f, 0.5f), mToldoA);
        }

        // ---- gatilho da largada ----
        BoxCollider zona = g.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.center = new Vector3(0f, 2f, 0f);
        zona.size = new Vector3(16f, 5f, 10f);

        return g;
    }

    /// O hospital onde a historia comeca. Alex sai daqui atras dos alimentos.
    static GameObject CriarHospital(Transform pai, Vector3 pos)
    {
        GameObject g = new GameObject("Hospital");
        g.transform.SetParent(pai);
        g.transform.position = pos;
        // de frente para a praca
        // a fachada deste predio fica no -Z local, entao o +Z tem que apontar
        // para FORA do mapa para ela encarar a praca
        g.transform.rotation = Quaternion.LookRotation(pos.normalized);

        Malha(g.transform, "Piso", PrimitiveType.Cube,
              new Vector3(0f, 0.07f, 0f), new Vector3(22f, 0.14f, 18f), mCalcada);

        Solida(g.transform, "ParedeFundo", PrimitiveType.Cube,
               new Vector3(0f, 3.4f, 7.5f), new Vector3(20f, 6.8f, 1f), mJaleco);
        Solida(g.transform, "ParedeL", PrimitiveType.Cube,
               new Vector3(9.5f, 3.4f, 0f), new Vector3(1f, 6.8f, 16f), mJaleco);
        Solida(g.transform, "ParedeO", PrimitiveType.Cube,
               new Vector3(-9.5f, 3.4f, 0f), new Vector3(1f, 6.8f, 16f), mJaleco);
        Malha(g.transform, "Laje", PrimitiveType.Cube,
              new Vector3(0f, 7f, 0.5f), new Vector3(20.6f, 0.5f, 17.6f), mConcreto);

        // fachada aberta para a rua, com recepcao
        Solida(g.transform, "PilarE", PrimitiveType.Cube,
               new Vector3(-7.5f, 3.4f, -8f), new Vector3(1.2f, 6.8f, 1.2f), mConcreto);
        Solida(g.transform, "PilarD", PrimitiveType.Cube,
               new Vector3(7.5f, 3.4f, -8f), new Vector3(1.2f, 6.8f, 1.2f), mConcreto);
        Malha(g.transform, "VidroE", PrimitiveType.Cube,
              new Vector3(-4f, 2.8f, -8f), new Vector3(4.6f, 5.2f, 0.2f), mVidro);
        Malha(g.transform, "VidroD", PrimitiveType.Cube,
              new Vector3(4f, 2.8f, -8f), new Vector3(4.6f, 5.2f, 0.2f), mVidro);

        // cruz do hospital
        Malha(g.transform, "Letreiro", PrimitiveType.Cube,
              new Vector3(0f, 7.9f, -7.8f), new Vector3(15f, 1.6f, 0.4f), mJaleco);
        Malha(g.transform, "CruzV", PrimitiveType.Cube,
              new Vector3(-5.2f, 7.9f, -8.1f), new Vector3(0.45f, 1.3f, 0.2f), mToldoA);
        Malha(g.transform, "CruzH", PrimitiveType.Cube,
              new Vector3(-5.2f, 7.9f, -8.1f), new Vector3(1.3f, 0.45f, 0.2f), mToldoA);

        // consultorio: mesa, maca e as duas cadeiras
        Solida(g.transform, "Mesa", PrimitiveType.Cube,
               new Vector3(0f, 0.65f, 3f), new Vector3(4.2f, 0.18f, 1.8f), mBanco);
        Solida(g.transform, "Maca", PrimitiveType.Cube,
               new Vector3(6f, 0.55f, 2f), new Vector3(2f, 0.3f, 4.5f), mJaleco);
        Malha(g.transform, "CadeiraA", PrimitiveType.Cube,
              new Vector3(-1.4f, 0.45f, 0.6f), new Vector3(0.9f, 0.14f, 0.9f), mMetal);
        Malha(g.transform, "CadeiraB", PrimitiveType.Cube,
              new Vector3(1.4f, 0.45f, 0.6f), new Vector3(0.9f, 0.14f, 0.9f), mMetal);

        // o medico atras da mesa, a nutricionista ao lado
        CriarPessoa(g.transform, "DrMoreira", new Vector3(-0.8f, 0f, 4.6f),
                    180f, mJaleco, false, "");
        CriarPessoa(g.transform, "DraHelena", new Vector3(2.6f, 0f, 4.2f),
                    200f, mJaleco, true, "");

        return g;
    }

    static GameObject CriarPizzaria(Transform pai, Vector3 pos, out Pizzaria chefe)
    {
        GameObject g = new GameObject("Pizzaria");
        g.transform.SetParent(pai);
        g.transform.position = pos;
        // mesma regra do hospital: a fachada esta no -Z local
        g.transform.rotation = Quaternion.LookRotation(pos.normalized);

        Malha(g.transform, "Piso", PrimitiveType.Cube,
              new Vector3(0f, 0.07f, 0f), new Vector3(20f, 0.14f, 18f), mCalcada);

        Solida(g.transform, "ParedeFundo", PrimitiveType.Cube,
               new Vector3(0f, 3.4f, 6.5f), new Vector3(17f, 6.8f, 1f), mTijolo);
        Solida(g.transform, "ParedeL", PrimitiveType.Cube,
               new Vector3(8f, 3.4f, 0f), new Vector3(1f, 6.8f, 14f), mTijolo);
        Solida(g.transform, "ParedeO", PrimitiveType.Cube,
               new Vector3(-8f, 3.4f, 0f), new Vector3(1f, 6.8f, 14f), mTijolo);
        Malha(g.transform, "Laje", PrimitiveType.Cube,
              new Vector3(0f, 7f, 0f), new Vector3(17.6f, 0.5f, 15.6f), mPizzaria);

        Solida(g.transform, "Balcao", PrimitiveType.Cube,
               new Vector3(0f, 0.6f, -6.5f), new Vector3(15f, 1.2f, 1.2f), mPizzaria);
        Malha(g.transform, "Vitrine", PrimitiveType.Cube,
              new Vector3(0f, 3.4f, -6.6f), new Vector3(14.6f, 3.6f, 0.2f), mVidro);
        for (int i = 0; i < 8; i++)
            Malha(g.transform, "Toldo" + i, PrimitiveType.Cube,
                  new Vector3(-6.3f + i * 1.8f, 5.3f, -7.4f), new Vector3(1.8f, 0.18f, 2.6f),
                  i % 2 == 0 ? mPizzaria : mToldoB);

        Malha(g.transform, "Letreiro", PrimitiveType.Cube,
              new Vector3(0f, 7.9f, -6.6f), new Vector3(14f, 1.9f, 0.4f), mPizzaria);
        MalhaRot(g.transform, "PizzaMassa", PrimitiveType.Cylinder,
                 new Vector3(-4.8f, 7.9f, -6.95f), new Vector3(90f, 0f, 0f),
                 new Vector3(1.6f, 0.1f, 1.6f), mMassa);
        MalhaRot(g.transform, "PizzaMolho", PrimitiveType.Cylinder,
                 new Vector3(-4.8f, 7.9f, -7.05f), new Vector3(90f, 0f, 0f),
                 new Vector3(1.3f, 0.06f, 1.3f), mMolho);
        for (int i = 0; i < 5; i++)
        {
            float a = i * Mathf.PI * 0.4f + 0.3f;
            MalhaRot(g.transform, "Calabresa" + i, PrimitiveType.Cylinder,
                     new Vector3(-4.8f + Mathf.Cos(a) * 0.48f, 7.9f + Mathf.Sin(a) * 0.48f, -7.12f),
                     new Vector3(90f, 0f, 0f), new Vector3(0.3f, 0.04f, 0.3f), mCalabresa);
        }

        Solida(g.transform, "Chamine", PrimitiveType.Cylinder,
               new Vector3(4f, 8.6f, 2f), new Vector3(1.6f, 1.7f, 1.6f), mTijolo);
        Malha(g.transform, "ChamineBoca", PrimitiveType.Cylinder,
              new Vector3(4f, 10.4f, 2f), new Vector3(1.8f, 0.2f, 1.8f), mExaustor);
        Malha(g.transform, "FumacaChamine", PrimitiveType.Sphere,
              new Vector3(4f, 11.6f, 2f), new Vector3(2.6f, 2.0f, 2.6f), mCheiro2);

        Transform[] pontosNuvem = new Transform[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject pt = new GameObject("PontoNuvem" + i);
            pt.transform.SetParent(g.transform);
            pt.transform.localPosition = new Vector3(i == 0 ? -4.5f : 4.5f, 1.7f, -3f);
            pontosNuvem[i] = pt.transform;
        }

        chefe = g.AddComponent<Pizzaria>();
        chefe.pontosNuvem = pontosNuvem;
        return g;
    }

    static Transform Membro(Transform pai, string nome, Vector3 posPivo, bool perna)
    {
        GameObject pivo = new GameObject(nome);
        pivo.transform.SetParent(pai);
        pivo.transform.localPosition = posPivo;
        pivo.transform.localRotation = Quaternion.identity;

        if (perna)
        {
            Malha(pivo.transform, "Coxa", PrimitiveType.Capsule,
                  new Vector3(0f, -0.34f, 0f), new Vector3(0.21f, 0.33f, 0.21f), mCalca);
            Malha(pivo.transform, "Tenis", PrimitiveType.Cube,
                  new Vector3(0f, -0.72f, 0.06f), new Vector3(0.24f, 0.14f, 0.38f), mTenis);
        }
        else
        {
            Malha(pivo.transform, "Braco", PrimitiveType.Capsule,
                  new Vector3(0f, -0.25f, 0f), new Vector3(0.17f, 0.24f, 0.17f), mPele);
            Malha(pivo.transform, "Mao", PrimitiveType.Sphere,
                  new Vector3(0f, -0.50f, 0f), new Vector3(0.15f, 0.15f, 0.15f), mPele);
        }
        return pivo.transform;
    }

    static void CriarLuzEAmbiente(Transform pai, Camera cam)
    {
        Light luz = Object.FindObjectOfType<Light>();
        if (luz == null)
        {
            GameObject luzGO = new GameObject("Sol");
            luzGO.transform.SetParent(pai);
            luz = luzGO.AddComponent<Light>();
            luz.type = LightType.Directional;
        }
        luz.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        luz.intensity = 1.05f;
        luz.color = new Color(1f, 0.97f, 0.90f);
        luz.shadows = LightShadows.None;
        luz.renderMode = LightRenderMode.ForceVertex;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.46f, 0.50f, 0.56f);
        RenderSettings.fog = sombreado;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = cam.backgroundColor;
        RenderSettings.fogStartDistance = 85f;
        RenderSettings.fogEndDistance = 205f;
    }

    // ================= alimentos =================

    static GameObject Maca(Transform pai)
    {
        Transform m;
        GameObject raiz = BaseItem(pai, "Fruta", TipoItem.Fruta, out m);

        // o construtor mede o modelo e normaliza para 0,85 m, entao nao importa
        // com que escala o Unity importou o FBX
        if (Modelo(m, "Apple", 0.85f, new Vector3(0f, 15f, 0f),
                   new string[] { "DarkRed", "DarkBrown", "DarkGreen" },
                   new Material[] { mMaca, mCabinho, mFolhaMaca }))
            return raiz;

        m.localScale = Vector3.one * 1.45f; // as primitivas sao menores
        Malha(m, "Corpo", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.62f, 0.58f, 0.62f), mMaca);
        Malha(m, "Bochecha", PrimitiveType.Sphere, new Vector3(0.12f, -0.02f, 0f),
              new Vector3(0.46f, 0.50f, 0.52f), mMaca);
        Malha(m, "Cabinho", PrimitiveType.Cylinder, new Vector3(0f, 0.34f, 0f),
              new Vector3(0.05f, 0.12f, 0.05f), mCabinho);
        MalhaRot(m, "Folha", PrimitiveType.Sphere, new Vector3(0.14f, 0.40f, 0f),
                 new Vector3(0f, 0f, -25f), new Vector3(0.26f, 0.05f, 0.13f), mFolhaMaca);
        return raiz;
    }

    static GameObject Garrafa(Transform pai)
    {
        Transform m;
        GameObject raiz = BaseItem(pai, "Agua", TipoItem.Agua, out m);

        // o pivo original fica na base da garrafa; o construtor recentraliza sozinho.
        // Os materiais originais sao de garrafa verde escura (parece cerveja);
        // repintamos para virar garrafa de agua.
        if (Modelo(m, "Bottle1", 0.95f, Vector3.zero,
                   new string[] { "DarkGreen", "LightBrown", "BottleCap" },
                   new Material[] { mGarrafa, mRotulo, mTampa }))
            return raiz;

        m.localScale = Vector3.one * 1.45f; // as primitivas sao menores
        Malha(m, "Corpo", PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0f),
              new Vector3(0.21f, 0.28f, 0.21f), mGarrafa);
        Malha(m, "Rotulo", PrimitiveType.Cylinder, new Vector3(0f, -0.06f, 0f),
              new Vector3(0.225f, 0.10f, 0.225f), mRotulo);
        Malha(m, "Ombro", PrimitiveType.Cylinder, new Vector3(0f, 0.26f, 0f),
              new Vector3(0.14f, 0.05f, 0.14f), mGarrafa);
        Malha(m, "Gargalo", PrimitiveType.Cylinder, new Vector3(0f, 0.35f, 0f),
              new Vector3(0.09f, 0.06f, 0.09f), mGarrafa);
        Malha(m, "Tampa", PrimitiveType.Cylinder, new Vector3(0f, 0.44f, 0f),
              new Vector3(0.11f, 0.05f, 0.11f), mTampa);
        return raiz;
    }

    static GameObject Frango(Transform pai)
    {
        Transform m;
        GameObject raiz = BaseItem(pai, "Proteina", TipoItem.Proteina, out m);

        // normaliza para 0,9 m no maior lado e inclina um pouco
        if (Modelo(m, "ChickenLeg", 0.9f, new Vector3(0f, 20f, 25f),
                   new string[] { "DarkBrown", "Beige" },
                   new Material[] { mFrango, mOsso }))
            return raiz;

        m.localScale = Vector3.one * 1.45f; // as primitivas sao menores
        Malha(m, "Carne", PrimitiveType.Sphere, new Vector3(-0.06f, 0.06f, 0f),
              new Vector3(0.46f, 0.40f, 0.40f), mFrango);
        Malha(m, "Carne2", PrimitiveType.Sphere, new Vector3(0.10f, -0.04f, 0f),
              new Vector3(0.32f, 0.30f, 0.30f), mFrango);
        MalhaRot(m, "Osso", PrimitiveType.Cylinder, new Vector3(0.26f, -0.18f, 0f),
                 new Vector3(0f, 0f, -38f), new Vector3(0.07f, 0.16f, 0.07f), mOsso);
        Malha(m, "PontaOsso", PrimitiveType.Sphere, new Vector3(0.38f, -0.30f, 0f),
              new Vector3(0.14f, 0.14f, 0.14f), mOsso);
        return raiz;
    }

    static GameObject BaseItem(Transform pai, string nome, TipoItem tipo, out Transform malha)
    {
        GameObject raiz = new GameObject(nome);
        raiz.transform.SetParent(pai);
        raiz.transform.localPosition = Vector3.zero;

        BoxCollider box = raiz.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(1.5f, 2.0f, 1.5f);

        raiz.AddComponent<Collectible>().tipo = tipo;

        Malha(raiz.transform, "Aura", PrimitiveType.Cylinder,
              new Vector3(0f, -1.14f, 0f), new Vector3(1.1f, 0.015f, 1.1f), mAuraBoa);

        GameObject no = new GameObject("Malha");
        no.transform.SetParent(raiz.transform);
        no.transform.localPosition = Vector3.zero;
        no.transform.localRotation = Quaternion.identity;
        no.transform.localScale = Vector3.one; // a escala vem de cada modelo/primitiva
        malha = no.transform;
        return raiz;
    }

    static GameObject Nuvem(Transform pai, string nome, Vector3 pos, float escala, Material mat)
    {
        GameObject raiz = new GameObject(nome);
        raiz.transform.SetParent(pai);
        raiz.transform.position = pos;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(raiz.transform);
        visual.transform.localPosition = Vector3.zero;

        float[,] bolhas = {
            {  0.0f,  0.0f,  0.0f, 2.6f }, {  1.3f,  0.3f,  0.4f, 1.9f },
            { -1.2f,  0.2f, -0.4f, 2.0f }, {  0.3f,  0.9f, -1.1f, 1.7f },
            { -0.4f, -0.6f,  1.1f, 1.6f }, {  0.9f, -0.5f, -0.9f, 1.4f }
        };
        for (int i = 0; i < bolhas.GetLength(0); i++)
            Malha(visual.transform, "Bolha" + i, PrimitiveType.Sphere,
                  new Vector3(bolhas[i, 0], bolhas[i, 1], bolhas[i, 2]) * escala,
                  Vector3.one * bolhas[i, 3] * escala, mat);

        raiz.AddComponent<NuvemCheiro>();
        return raiz;
    }

    static GameObject Primitiva(Transform pai, string nome, PrimitiveType forma,
                                Vector3 posLocal, Vector3 euler, Vector3 escala, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(forma);
        go.name = nome;
        go.transform.SetParent(pai);
        go.transform.localPosition = posLocal;
        go.transform.localRotation = Quaternion.Euler(euler);
        go.transform.localScale = escala;

        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        return go;
    }
}
