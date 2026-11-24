using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Materiais, shaders, texturas e os ajudantes de malha e de modelo.
/// Nada aqui sabe o que e o jogo: so sabe pintar e instanciar.
/// </summary>
public static partial class DemoSceneBuilder
{
    static Material materialSemTextura;
    static bool avisouTextura;
    static Texture2D[] texturasDoProjeto;
    static Material[] mFachadas;

    /// Lista, em UMA linha de Console, todo modelo que o construtor procura e se
    /// achou. Existe porque "o personagem esta quadrado" tem quatro causas
    /// possiveis, e so olhando a tela nao da para saber qual delas e.
    static void InventarioDeModelos()
    {
        List<string> faltando = new List<string>();
        List<string> achados = new List<string>();

        string[] personagem = { MODELO_ALEX, RIG_GERAL, RIG_MOVIMENTO, RIG_SIMULACAO };
        string[] comida = { "Apple", "Bottle1", "ChickenLeg" };
        string[] cenario = { MODELO_LOTE, MODELO_ARBUSTO, "road_straight", "streetlight",
                             "bench", "tree", "watertower" };

        Conferir(personagem, achados, faltando);
        Conferir(MODELOS_PREDIO, achados, faltando);
        Conferir(comida, achados, faltando);
        Conferir(cenario, achados, faltando);

        bool temPista = TemArquivo(MODELO_PISTA, ".obj") || TemArquivo(MODELO_PISTA, ".fbx");
        if (temPista) achados.Add(MODELO_PISTA); else faltando.Add(MODELO_PISTA + "(.obj)");

        Debug.Log("MODELOS: " + achados.Count + " encontrados no projeto, "
                + faltando.Count + " faltando. Podem estar em qualquer pasta dentro de"
                + " Assets; o preferido e " + PASTA_MODELOS + "."
                + (faltando.Count > 0 ? "\n   FALTAM: " + string.Join(", ", faltando.ToArray()) : "")
                + "\n   ACHADOS: " + string.Join(", ", achados.ToArray()));

        if (!TemModelo(MODELO_ALEX) || !TemModelo(RIG_GERAL))
            Debug.LogError("Sem " + MODELO_ALEX + ".fbx e " + RIG_GERAL + ".fbx em"
                         + " LUGAR NENHUM do projeto, TODOS os personagens ficam"
                         + " quadrados. Copie os dois para " + PASTA_MODELOS
                         + " (ou qualquer pasta dentro de Assets) e reconstrua a cena.");
    }

    static void Conferir(string[] nomes, List<string> achados, List<string> faltando)
    {
        for (int i = 0; i < nomes.Length; i++)
        {
            if (TemModelo(nomes[i])) achados.Add(nomes[i]);
            else faltando.Add(nomes[i]);
        }
    }

    static void IndexarModelos()
    {
        indiceDeModelos = new Dictionary<string, string>();

        string[] guids = AssetDatabase.FindAssets("t:Model");
        for (int i = 0; i < guids.Length; i++)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guids[i]);
            string chave = System.IO.Path.GetFileName(caminho).ToLower();
            if (!indiceDeModelos.ContainsKey(chave)) indiceDeModelos[chave] = caminho;
        }
    }

    /// Caminho do modelo, ou null. Procura primeiro na pasta oficial, para o caso
    /// de existirem dois arquivos com o mesmo nome no projeto.
    static string CaminhoDoModelo(string nome, string extensao)
    {
        string oficial = PASTA_MODELOS + "/" + nome + extensao;
        if (AssetDatabase.LoadAssetAtPath<GameObject>(oficial) != null) return oficial;

        if (indiceDeModelos == null) IndexarModelos();

        string achado;
        if (indiceDeModelos.TryGetValue((nome + extensao).ToLower(), out achado))
            return achado;

        return null;
    }

    static GameObject CarregarModelo(string nome, string extensao)
    {
        string caminho = CaminhoDoModelo(nome, extensao);
        if (caminho == null) return null;
        return AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
    }

    static bool TemModelo(string nome)
    {
        return TemArquivo(nome, ".fbx");
    }

    static bool TemArquivo(string nome, string extensao)
    {
        return CaminhoDoModelo(nome, extensao) != null;
    }

    static string Sorteado(string[] lista)
    {
        return lista[Random.Range(0, lista.Length)];
    }

    static void Posicionar(GameObject go, Vector3 pos, float giro)
    {
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, giro, 0f);
    }

    static Bounds BoundsDe(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    /// Instancia um .fbx do kit na origem, aplica escala e troca todos os materiais
    /// por um unico material com a textura colormap - o que deixa a cidade inteira
    /// num punhado de draw calls.
    /// Unity recente importa FBX SEM criar material por padrao, e ai o modelo
    /// aparece cinza. Forcamos a criacao do material embutido uma vez por arquivo.
    static void ConfigurarImportador(string caminho)
    {
        if (jaConfigurado.Contains(caminho)) return;
        jaConfigurado.Add(caminho);

        ModelImporter mi = AssetImporter.GetAtPath(caminho) as ModelImporter;
        if (mi == null) return;

        bool mudou = false;
        if (mi.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
        {
            mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            mudou = true;
        }
        if (mi.materialLocation != ModelImporterMaterialLocation.InPrefab)
        {
            mi.materialLocation = ModelImporterMaterialLocation.InPrefab;
            mudou = true;
        }
        if (mudou) mi.SaveAndReimport();
    }

    static void Juntar(List<Texture2D> lista, string[] guids)
    {
        for (int i = 0; i < guids.Length; i++)
        {
            string c = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (c.Contains("/Editor/") || c.StartsWith("Packages/")) continue;
            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>(c);
            if (t != null) lista.Add(t);
        }
    }

    static void ListarTexturas()
    {
        // Procura em PASTA primeiro e, se nao achar nada, no projeto inteiro:
        // a textura dos kits costuma vir numa subpasta "Textures" que a pessoa
        // solta em qualquer canto de Assets.
        List<Texture2D> lista = new List<Texture2D>();
        Juntar(lista, AssetDatabase.IsValidFolder(PASTA)
                      ? AssetDatabase.FindAssets("t:Texture2D", new string[] { PASTA })
                      : new string[0]);

        if (lista.Count == 0) Juntar(lista, AssetDatabase.FindAssets("t:Texture2D"));

        texturasDoProjeto = lista.ToArray();

        string nomes = "";
        for (int i = 0; i < texturasDoProjeto.Length; i++)
            nomes += (i > 0 ? ", " : "") + texturasDoProjeto[i].name;
        Debug.Log("Texturas encontradas em " + PASTA + ": " + texturasDoProjeto.Length
                + (nomes.Length > 0 ? "  (" + nomes + ")" : ""));
    }

    /// Ultimo recurso: acha o PNG pelo nome do material do FBX.
    static Texture AcharTexturaPorNome(string nomeMaterial)
    {
        if (texturasDoProjeto == null || texturasDoProjeto.Length == 0) return null;
        if (string.IsNullOrEmpty(nomeMaterial)) nomeMaterial = "";

        string alvo = nomeMaterial.ToLower();
        for (int i = 0; i < texturasDoProjeto.Length; i++)
        {
            string n = texturasDoProjeto[i].name.ToLower();
            if (n.Length > 2 && (alvo.Contains(n) || n.Contains(alvo))) return texturasDoProjeto[i];
        }

        // so existe uma textura no projeto: e obviamente essa
        if (texturasDoProjeto.Length == 1) return texturasDoProjeto[0];
        return null;
    }

    static GameObject InstanciarModelo(Transform pai, string arquivo, float escala)
    {
        string caminhoFbx = CaminhoDoModelo(arquivo, ".fbx");
        if (caminhoFbx == null) return null;
        ConfigurarImportador(caminhoFbx);

        GameObject fonte = AssetDatabase.LoadAssetAtPath<GameObject>(caminhoFbx);
        if (fonte == null)
        {
            Debug.LogWarning("Modelo nao encontrado: " + arquivo);
            return null;
        }


        GameObject inst = Object.Instantiate(fonte);
        inst.transform.SetParent(pai, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one * escala;

        Collider[] cols = inst.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) Object.DestroyImmediate(cols[i]);

        Renderer[] rends = inst.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer r = rends[i];

            Material[] slots = r.sharedMaterials;
            for (int k = 0; k < slots.Length; k++)
            {
                Texture tex = (slots[k] != null) ? slots[k].mainTexture : null;
                string nomeMat = (slots[k] != null) ? slots[k].name : arquivo;
                if (tex == null) tex = AcharTexturaPorNome(nomeMat);
                if (tex == null) tex = AcharTexturaPorNome(arquivo);
                slots[k] = MaterialDoKit(tex);
            }
            r.sharedMaterials = slots;

            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        return inst;
    }

    /// Instancia um .fbx de PASTA_MODELOS sob "pai".
    ///
    /// Em vez de confiar numa escala fixa, MEDE o modelo importado e normaliza para
    /// "tamanhoAlvo" no maior lado, alem de recentralizar o pivo. Isso deixa o
    /// resultado igual independente de como o Unity importou o arquivo (Convert Units
    /// ligado ou desligado, Scale Factor diferente, pivo na base ou no centro).
    /// Devolve false se o arquivo nao existir, para o construtor cair nas primitivas.
    static bool Modelo(Transform pai, string arquivo, float tamanhoAlvo, Vector3 euler,
                       string[] nomesOriginais, Material[] substitutos)
    {
        string caminho = CaminhoDoModelo(arquivo, ".fbx");
        if (caminho == null) return false;
        ConfigurarImportador(caminho);

        GameObject fonte = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        if (fonte == null)
        {
            Debug.LogWarning("Modelo nao encontrado em " + caminho
                           + " - usando a versao feita de primitivas.");
            return false;
        }

        // pivo intermediario: guarda a rotacao, para o recentramento nao ser girado junto
        GameObject pivo = new GameObject(arquivo);
        pivo.transform.SetParent(pai, false);
        pivo.transform.localPosition = Vector3.zero;
        pivo.transform.localRotation = Quaternion.Euler(euler);
        pivo.transform.localScale = Vector3.one;

        GameObject inst = Object.Instantiate(fonte);
        inst.name = "Malha_" + arquivo;
        inst.transform.SetParent(pivo.transform, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        Collider[] cols = inst.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) Object.DestroyImmediate(cols[i]);

        Renderer[] rends = inst.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0)
        {
            Debug.LogWarning(arquivo + " nao tem malha visivel. Usando primitivas.");
            Object.DestroyImmediate(pivo);
            return false;
        }

        // ---- mede e normaliza ----
        Bounds b = BoundsDe(inst);
        float maior = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        float k = (maior > 0.0001f) ? tamanhoAlvo / maior : 1f;

        inst.transform.localScale = Vector3.one * k;
        inst.transform.localPosition = -(b.center - inst.transform.position) * k;

        Debug.Log("Modelo " + arquivo + ": tamanho importado " + b.size.ToString("F2")
                + " -> escala " + k.ToString("F3") + " (alvo " + tamanhoAlvo + " m)");

        // ---- repinta ----
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer r = rends[i];
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            Material[] slots = r.sharedMaterials;
            for (int j = 0; j < slots.Length; j++)
            {
                Material escolhido = null;

                if (slots[j] != null)
                {
                    string nome = slots[j].name.Replace(" (Instance)", "").Trim();
                    for (int q = 0; q < nomesOriginais.Length; q++)
                        if (string.Equals(nome, nomesOriginais[q],
                                          System.StringComparison.OrdinalIgnoreCase))
                            escolhido = substitutos[q];
                }

                if (escolhido == null)
                    escolhido = substitutos[Mathf.Min(j, substitutos.Length - 1)];

                slots[j] = escolhido;
            }
            r.sharedMaterials = slots;
        }

        return true;
    }

    // ================= auxiliares =================

    static void Estatico(GameObject raiz)
    {
        Transform[] todos = raiz.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < todos.Length; i++)
            GameObjectUtility.SetStaticEditorFlags(todos[i].gameObject, StaticEditorFlags.BatchingStatic);
    }

    static GameObject Malha(Transform pai, string nome, PrimitiveType forma,
                            Vector3 posLocal, Vector3 escala, Material mat)
    {
        return MalhaRot(pai, nome, forma, posLocal, Vector3.zero, escala, mat);
    }

    static GameObject MalhaRot(Transform pai, string nome, PrimitiveType forma,
                               Vector3 posLocal, Vector3 euler, Vector3 escala, Material mat)
    {
        GameObject go = Primitiva(pai, nome, forma, posLocal, euler, escala, mat);
        Collider col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        return go;
    }

    static GameObject Solida(Transform pai, string nome, PrimitiveType forma,
                             Vector3 posLocal, Vector3 escala, Material mat)
    {
        return Primitiva(pai, nome, forma, posLocal, Vector3.zero, escala, mat);
    }

    // ================= materiais =================

    static void CriarMateriais()
    {
        mGrama        = Mat("Grama",        new Color(0.36f, 0.50f, 0.28f));
        mCalcada      = Mat("Calcada",      new Color(0.66f, 0.65f, 0.61f));
        mCalcadaClara = Mat("CalcadaClara", new Color(0.78f, 0.76f, 0.70f));
        mAsfalto      = Mat("Asfalto",      new Color(0.24f, 0.24f, 0.27f));
        mFaixaRua     = Mat("FaixaRua",     new Color(0.92f, 0.92f, 0.88f));
        mMuro         = Mat("Muro",         new Color(0.34f, 0.36f, 0.42f));

        mFachadas = new Material[] {
            Mat("FachadaA", new Color(0.80f, 0.76f, 0.68f)),
            Mat("FachadaB", new Color(0.60f, 0.66f, 0.74f)),
            Mat("FachadaC", new Color(0.74f, 0.58f, 0.50f)),
            Mat("FachadaD", new Color(0.55f, 0.63f, 0.58f)),
            Mat("FachadaE", new Color(0.70f, 0.66f, 0.76f))
        };

        mConcreto  = Mat("Concreto",  new Color(0.52f, 0.52f, 0.54f));
        mVidro     = Mat("Vidro",     new Color(0.30f, 0.45f, 0.58f));
        mTelhado   = Mat("Telhado",   new Color(0.40f, 0.30f, 0.27f));
        mTijolo    = Mat("Tijolo",    new Color(0.52f, 0.28f, 0.22f));

        mAgua2     = Mat("AguaFonte", new Color(0.35f, 0.66f, 0.88f));
        mMetal     = Mat("Metal",     new Color(0.70f, 0.72f, 0.76f));
        mBorracha  = Mat("Borracha",  new Color(0.14f, 0.14f, 0.16f));
        mPlaca     = Mat("Placa",     new Color(0.95f, 0.80f, 0.25f));

        mTronco    = Mat("Tronco",    new Color(0.38f, 0.27f, 0.18f));
        mCopaA     = Mat("CopaA",     new Color(0.24f, 0.55f, 0.26f));
        mCopaB     = Mat("CopaB",     new Color(0.33f, 0.65f, 0.33f));
        mFlorA     = Mat("FlorA",     new Color(0.90f, 0.35f, 0.45f));
        mFlorB     = Mat("FlorB",     new Color(0.95f, 0.75f, 0.30f));
        mBanco     = Mat("Banco",     new Color(0.52f, 0.36f, 0.22f));

        mCarroA    = Mat("CarroA",    new Color(0.80f, 0.25f, 0.25f));
        mCarroB    = Mat("CarroB",    new Color(0.25f, 0.42f, 0.78f));
        mCarroC    = Mat("CarroC",    new Color(0.92f, 0.92f, 0.92f));

        mBanca     = Mat("Banca",     new Color(0.78f, 0.66f, 0.48f));
        mToldoA    = Mat("ToldoA",    new Color(0.88f, 0.32f, 0.28f));
        mToldoB    = Mat("ToldoB",    new Color(0.95f, 0.93f, 0.88f));
        mCaixote   = Mat("Caixote",   new Color(0.60f, 0.44f, 0.28f));
        mLaranja   = Mat("Laranja",   new Color(0.95f, 0.60f, 0.15f));
        mVerdura   = Mat("Verdura",   new Color(0.30f, 0.62f, 0.28f));

        mAcademia  = Mat("Academia",  new Color(0.22f, 0.58f, 0.40f));
        mJaleco    = Mat("Jaleco",    new Color(0.95f, 0.96f, 0.98f));
        mOuro      = Mat("Ouro",      new Color(0.96f, 0.78f, 0.20f));
        mRival1    = Mat("Rival1",    new Color(0.85f, 0.25f, 0.30f));
        mRival2    = Mat("Rival2",    new Color(0.95f, 0.70f, 0.20f));
        mRival3    = Mat("Rival3",    new Color(0.55f, 0.35f, 0.85f));
        mRival4    = Mat("Rival4",    new Color(0.20f, 0.72f, 0.68f));
        mPista     = Mat("PistaTartan", new Color(0.62f, 0.26f, 0.22f));
        mPizzaria  = Mat("Pizzaria",  new Color(0.62f, 0.14f, 0.14f));
        mMassa     = Mat("Massa",     new Color(0.93f, 0.80f, 0.52f));
        mMolho     = Mat("Molho",     new Color(0.82f, 0.20f, 0.14f));
        mCalabresa = Mat("Calabresa", new Color(0.60f, 0.12f, 0.12f));
        mExaustor  = Mat("Exaustor",  new Color(0.45f, 0.45f, 0.48f));
        mLacre     = Mat("Lacre",     new Color(0.30f, 0.80f, 0.40f));

        mPele      = Mat("Pele",      new Color(0.94f, 0.74f, 0.58f));
        mCabelo    = Mat("Cabelo",    new Color(0.20f, 0.14f, 0.10f));
        mCamiseta  = Mat("Camiseta",  new Color(0.20f, 0.48f, 0.88f));
        mCalca     = Mat("Calca",     new Color(0.22f, 0.24f, 0.32f));
        mTenis     = Mat("Tenis",     new Color(0.92f, 0.92f, 0.94f));
        mOlho      = Mat("Olho",      new Color(0.10f, 0.10f, 0.12f));

        mMaca      = Mat("Maca",      new Color(0.85f, 0.15f, 0.15f));
        mCabinho   = Mat("Cabinho",   new Color(0.35f, 0.24f, 0.14f));
        mFolhaMaca = Mat("FolhaMaca", new Color(0.25f, 0.62f, 0.25f));
        mGarrafa   = Mat("Garrafa",   new Color(0.45f, 0.78f, 0.95f));
        mTampa     = Mat("Tampa",     new Color(0.10f, 0.35f, 0.70f));
        mRotulo    = Mat("Rotulo",    new Color(0.95f, 0.95f, 0.95f));
        mFrango    = Mat("Frango",    new Color(0.78f, 0.52f, 0.28f));
        mOsso      = Mat("Osso",      new Color(0.95f, 0.92f, 0.85f));
        mAuraBoa   = Mat("AuraBoa",   new Color(0.35f, 0.95f, 0.40f));

        mTreino      = MatNuvem("Treino",      new Color(0.30f, 0.70f, 1.00f, 0.30f));
        mTreinoForte = MatNuvem("TreinoForte", new Color(0.20f, 0.60f, 1.00f, 0.72f));
        mAura      = MatNuvem("Aura",    new Color(1.00f, 0.85f, 0.20f, 0.30f));
        mAuraForte = MatNuvem("AuraForte", new Color(1.00f, 0.78f, 0.10f, 0.72f));
        mCheiro    = MatNuvem("Cheiro",  new Color(0.60f, 0.42f, 0.72f, 0.62f));
        mCheiro2   = MatNuvem("Cheiro2", new Color(0.48f, 0.56f, 0.34f, 0.55f));

        AssetDatabase.SaveAssets();
    }

    static Shader ShaderPadrao()
    {
        Shader s = null;
        if (sombreado)
        {
            s = Shader.Find("Legacy Shaders/VertexLit");
            if (s == null) s = Shader.Find("Mobile/VertexLit");
            if (s == null) s = Shader.Find("Mobile/Diffuse");
        }
        if (s == null) s = Shader.Find("Unlit/Color");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        if (s == null) s = Shader.Find("Standard");
        return s;
    }

    static Material Mat(string nome, Color cor) { return MatComShader(nome, cor, ShaderPadrao()); }

    static Material MatNuvem(string nome, Color cor)
    {
        Shader sh = Shader.Find("Legacy Shaders/Transparent/VertexLit");
        if (sh == null) sh = Shader.Find("Transparent/VertexLit");
        if (sh == null) sh = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        if (sh == null) sh = ShaderPadrao();
        return MatComShader(nome, cor, sh);
    }

    static Material MatComShader(string nome, Color cor, Shader sh)
    {
        if (!AssetDatabase.IsValidFolder(PASTA)) AssetDatabase.CreateFolder("Assets", "NutriRunner");
        if (!AssetDatabase.IsValidFolder(PASTA_MAT)) AssetDatabase.CreateFolder(PASTA, "Materials");

        string caminho = PASTA_MAT + "/" + nome + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(caminho);

        if (m == null)
        {
            m = new Material(sh);
            AssetDatabase.CreateAsset(m, caminho);
        }
        else if (m.shader != sh) m.shader = sh;

        m.color = cor;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", cor);
        if (m.HasProperty("_SpecColor")) m.SetColor("_SpecColor", Color.black);
        EditorUtility.SetDirty(m);
        return m;
    }
}
