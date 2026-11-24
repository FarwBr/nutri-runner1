using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// O estadio de atletismo e a corrida de rua: a capsula da pista, o piso vindo do
/// modelo, o podio, a limpeza do terreno e as auras das duas provas.
/// </summary>
public static partial class DemoSceneBuilder
{

    /// O estadio nao e redondo, e um retangulo deitado de ~104 x 61 m. Medir pelo
    /// raio como nos outros marcos deixaria predio em cima das pontas das retas.
    static bool NaReservaDaPista(Vector3 p) { return NaReservaDaPista(p, 0f); }

    static bool NaReservaDaPista(Vector3 p, float raioDaPeca)
    {
        Vector3 d = p - PISTA_CENTRO;
        return Mathf.Abs(d.x) <= PISTA_RESERVA_X + raioDaPeca
            && Mathf.Abs(d.z) <= PISTA_RESERVA_Z + raioDaPeca;
    }

    /// Confere se algum portico caiu em cima de um terreno ocupado.
    /// Um erro desses so aparece jogando, entao vale gritar no Console.
    static void Aferir(Vector3[] trajeto)
    {
        for (int i = 0; i < trajeto.Length; i++)
        {
            Vector3 p = trajeto[i];
            string choque = null;

            // o hospital e retangular: medir pelo raio daria alarme falso
            // num portico que passa de lado, com folga de sobra
            if (Mathf.Abs(p.x - HOSPITAL.x) < 14f && Mathf.Abs(p.z - HOSPITAL.z) < 16f)
                choque = "o hospital";
            else if (Vector3.Distance(p, new Vector3(-ANEL_A, 0f, ANEL_A)) < 20f)
                choque = "a pizzaria";
            else if (NaReservaDaPista(p))
                choque = "o estadio de atletismo";
            else if (new Vector2(p.x, p.z).magnitude < RAIO_PRACA + 4f)
                choque = "a praca";
            else if (new Vector2(p.x, p.z).magnitude > LIMITE - 48f)
                choque = "a fileira de predios";

            if (choque != null)
                Debug.LogWarning("Portico " + (i + 1) + " em " + p.ToString("F0")
                               + " esta em cima de " + choque + ". Mova no trajeto.");
        }
    }

    /// Etapa 3: o estadio de atletismo.
    ///
    /// O piso vem do modelo "pista_atletismo", uma pista oficial de 400 m (duas
    /// retas de 84,39 m fechadas por semicirculos de 36,50 m, 8 raias de 1,22 m),
    /// reduzida pela metade para caber na cidade: a volta fica com 200 m exatos.
    ///
    /// Nada de mureta. Quem decide se o Alex esta na pista e o componente
    /// PistaAtletismo, por conta: a distancia dele ate o eixo da reta central tem
    /// que estar entre o raio interno e o externo. Cortar pelo gramado e possivel -
    /// e inutil, porque o CircuitoOlimpico congela o progresso enquanto ele estiver
    /// fora e desclassifica quem insiste.
    static GameObject CriarPista(Transform pai)
    {
        GameObject g = new GameObject("PistaAtletismo");
        g.transform.SetParent(pai);
        g.transform.position = PISTA_CENTRO;

        // ---- a autoridade sobre a geometria ----
        PistaAtletismo geo = g.AddComponent<PistaAtletismo>();
        geo.centro = PISTA_CENTRO;
        geo.reta = PISTA_RETA;
        geo.raioInterno = PISTA_R_INTERNO;
        geo.raioMedida = PISTA_R_MEDIDA;
        geo.raioExterno = PISTA_R_EXTERNO;

        // ---- o piso ----
        if (!PisoDoModelo(g.transform)) PisoDeCodigo(g.transform);

        // onde fica a entrada: no meio da reta norte, virada para a cidade
        float zEntrada = PISTA_R_EXTERNO;
        float meioDaPista = (PISTA_R_INTERNO + PISTA_R_EXTERNO) * 0.5f;

        // ---- linha de largada e chegada, na entrada ----
        Malha(g.transform, "LinhaLargada", PrimitiveType.Cube,
              new Vector3(0f, PISTA_Y + 0.02f, meioDaPista),
              new Vector3(0.25f, 0.02f, PISTA_R_EXTERNO - PISTA_R_INTERNO), mFaixaRua);

        // quadriculado de largada, uma casa por raia
        int raias = Mathf.Max(1, Mathf.RoundToInt(
            (PISTA_R_EXTERNO - PISTA_R_INTERNO) / PISTA_RAIA));
        for (int c = 0; c < raias; c++)
            Malha(g.transform, "Quadro" + c, PrimitiveType.Cube,
                  new Vector3(-0.75f, PISTA_Y + 0.02f,
                              PISTA_R_INTERNO + PISTA_RAIA * (c + 0.5f)),
                  new Vector3(1.1f, 0.02f, PISTA_RAIA),
                  (c % 2 == 0) ? mFaixaRua : mBorracha);

        // ---- portico sobre a entrada ----
        Solida(g.transform, "PilarE", PrimitiveType.Cube,
               new Vector3(-6.2f, 3.6f, zEntrada + 2.5f),
               new Vector3(1.2f, 7.2f, 1.2f), mMetal);
        Solida(g.transform, "PilarD", PrimitiveType.Cube,
               new Vector3(6.2f, 3.6f, zEntrada + 2.5f),
               new Vector3(1.2f, 7.2f, 1.2f), mMetal);
        Malha(g.transform, "Viga", PrimitiveType.Cube,
              new Vector3(0f, 7.7f, zEntrada + 2.5f),
              new Vector3(14f, 1.2f, 1.2f), mPizzaria);
        Malha(g.transform, "Letreiro", PrimitiveType.Cube,
              new Vector3(0f, 8.9f, zEntrada + 2.5f),
              new Vector3(11f, 1.5f, 0.3f), mPlaca);

        Material[] aneis = { mRival4, mPlaca, mBorracha, mRival1, mRival2 };
        for (int i = 0; i < 5; i++)
            MalhaRot(g.transform, "Anel" + i, PrimitiveType.Cylinder,
                     new Vector3(-3.2f + i * 1.6f, 8.9f, zEntrada + 2.28f),
                     new Vector3(90f, 0f, 0f),
                     new Vector3(1.0f, 0.06f, 1.0f), aneis[i]);

        // ---- arquibancada, dos dois lados da entrada ----
        // Fica ao NORTE, de frente para a reta de chegada, porque e desse lado que
        // o publico chega da cidade. Ao sul nao caberia: com a pista no lugar, o
        // muro do mapa passa a pouco mais de 3 m da borda do asfalto.
        float meiaLargura = (PISTA_RETA - 9f) * 0.5f;
        float xBloco = 9f + meiaLargura;
        for (int d = 0; d < 3; d++)
        {
            float zDeg = PISTA_R_APRON + 2f + d * 1.5f;
            float yDeg = 0.7f + d * 1.0f;
            Malha(g.transform, "DegrauO" + d, PrimitiveType.Cube,
                  new Vector3(-xBloco, yDeg, zDeg),
                  new Vector3(meiaLargura * 2f, 1.1f, 1.5f), mCalcada);
            Malha(g.transform, "DegrauL" + d, PrimitiveType.Cube,
                  new Vector3(xBloco, yDeg, zDeg),
                  new Vector3(meiaLargura * 2f, 1.1f, 1.5f), mCalcada);
        }

        // ---- podio, no gramado em frente a chegada ----
        GameObject podio = new GameObject("Podio");
        podio.transform.SetParent(g.transform);
        podio.transform.localPosition = new Vector3(0f, 0f, PISTA_R_INTERNO - 5f);

        // Alturas de podio de verdade. Na primeira versao eu havia posto 1,5 / 2,1 /
        // 1,2 m, o que daria um bloco mais alto que o proprio Alex.
        float[] alturas = { 0.50f, 0.75f, 0.35f };  // 2o, 1o, 3o
        float[] xs = { -1.8f, 0f, 1.8f };
        Material[] lugares = { mMetal, mOuro, mCaixote };
        for (int i = 0; i < 3; i++)
        {
            Solida(podio.transform, "Degrau" + i, PrimitiveType.Cube,
                   new Vector3(xs[i], alturas[i] * 0.5f, 0f),
                   new Vector3(1.7f, alturas[i], 1.7f), mCalcada);
            Malha(podio.transform, "Topo" + i, PrimitiveType.Cube,
                  new Vector3(xs[i], alturas[i] + 0.04f, 0f),
                  new Vector3(1.8f, 0.1f, 1.8f), lugares[i]);
        }
        Malha(podio.transform, "Faixa", PrimitiveType.Cube,
              new Vector3(0f, 2.4f, -1.6f), new Vector3(8f, 1.1f, 0.2f), mPlaca);

        // Marcador onde o Alex vai parar na vitoria, virado para a arquibancada
        // (que fica ao norte, no +Z). O podio e construido antes do Alex, entao
        // guardamos a referencia para ligar na Comemoracao depois.
        GameObject alvoPodio = new GameObject("AlvoDoPodio");
        alvoPodio.transform.SetParent(podio.transform, false);
        alvoPodio.transform.localPosition = new Vector3(0f, alturas[1] + 0.06f, 0f);
        alvoPodio.transform.localRotation = Quaternion.identity;
        degrauDoOuro = alvoPodio.transform;

        // ---- rivais, um por raia ----
        // Raias 2 a 5: ficam perto do Alex, que corre livre pela faixa inteira.
        // O rival de raia mais externa roda mais metros, entao os ritmos crescem
        // junto - senao o de fora pareceria mais lento sem ter culpa.
        Material[] cores = { mRival1, mRival2, mRival3, mRival4 };
        string[] nomes = { "Bruno", "Carla", "Diego", "Elisa" };
        // Subidos de 5,8-7,0 para 6,4-7,9: com os rivais lentos, quem fechava a
        // dieta perfeita ganhava de qualquer jeito e o treino de rua nao mudava
        // nada para ele. Agora a prova e decidida pelas DUAS etapas anteriores.
        float[] ritmos = { 6.4f, 6.9f, 7.4f, 7.9f };

        Corredor[] rivais = new Corredor[cores.Length];
        for (int i = 0; i < cores.Length; i++)
        {
            GameObject c = CriarCorredor(g.transform, nomes[i], cores[i]);
            Corredor r = c.GetComponent<Corredor>();
            r.nome = nomes[i];
            r.velocidade = ritmos[i];
            r.pista = geo;
            r.raia = PISTA_R_INTERNO + PISTA_RAIA * (i + 1.5f);
            rivais[i] = r;
        }

        // ---- treinador e nutricionista esperam na entrada ----
        CriarPessoa(g.transform, "Treinador", new Vector3(-4.2f, 0f, zEntrada + 5f),
                    200f, mRival3, false, ESTADO_ACENANDO);
        CriarPessoa(g.transform, "Nutricionista", new Vector3(4.2f, 0f, zEntrada + 5f),
                    160f, mJaleco, true, ESTADO_ACENANDO);

        // ---- gatilho: toda a reta de chegada ----
        // Como nao existe mais mureta externa, o Alex pode pisar na pista em
        // qualquer ponto. O gatilho cobre a reta norte INTEIRA para a prova comecar
        // de qualquer jeito que ele entre pelo lado da cidade, e nao so pelo portico.
        BoxCollider zona = g.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.center = new Vector3(0f, 2f, meioDaPista);
        zona.size = new Vector3(PISTA_RETA * 2f, 5f,
                                PISTA_R_EXTERNO - PISTA_R_INTERNO + 2f);

        CircuitoOlimpico co = g.AddComponent<CircuitoOlimpico>();
        co.pista = geo;
        co.voltas = 2;
        co.rivais = rivais;

        Debug.Log("Pista: volta de " + geo.Perimetro.ToString("F1") + " m, prova de "
                + (2f * geo.Perimetro).ToString("F0") + " m. Faixa de corrida entre "
                + PISTA_R_INTERNO.ToString("F2") + " e " + PISTA_R_EXTERNO.ToString("F2")
                + " m do eixo (" + raias + " raias de "
                + PISTA_RAIA.ToString("F2") + " m).");

        return g;
    }

    /// Varre um grupo da cena e apaga as pecas que ficaram em cima do estadio.
    ///
    /// Os testes de reserva vivem em cada funcao que coloca peca, e e facil
    /// esquecer um - foi o que aconteceu com os arbustos e com o mobiliario de
    /// rua, que nem chegavam a consultar a reserva. Esta varredura nao substitui
    /// aqueles testes: ela GARANTE a regra, medindo a caixa envolvente do que de
    /// fato ficou na cena, em vez de confiar na minha aritmetica de layout.
    ///
    /// Varre so o grupo "Quadras" (predios, arbustos, mobiliario, postes,
    /// canteiros), de proposito. O grupo "Cenario" tem o chao do mapa inteiro e as
    /// avenidas, que sao planos rentes ao solo e podem passar por baixo do estadio
    /// sem atrapalhar - alem de que apagar o chao deixaria o jogador caindo no vazio.
    static int LimparPista(Transform grupo, PistaAtletismo geo)
    {
        if (geo == null || grupo == null) return 0;

        float alcance = PISTA_R_APRON + 1.5f;
        List<Transform> condenados = new List<Transform>();
        List<string> nomes = new List<string>();

        // so filhos DIRETOS: cada um e uma peca inteira (um predio, um arbusto).
        // Subir ate a raiz levaria o grupo todo junto, ou seja, a cidade inteira.
        for (int i = 0; i < grupo.childCount; i++)
        {
            Transform peca = grupo.GetChild(i);

            Renderer[] rs = peca.GetComponentsInChildren<Renderer>(true);
            if (rs.Length == 0) continue;

            Bounds b = rs[0].bounds;
            for (int j = 1; j < rs.Length; j++) b.Encapsulate(rs[j].bounds);

            if (!EncostaNaPista(b, geo, alcance)) continue;

            condenados.Add(peca);
            nomes.Add(peca.name + " " + peca.position.ToString("F0"));
        }

        for (int i = 0; i < condenados.Count; i++)
            Object.DestroyImmediate(condenados[i].gameObject);

        if (condenados.Count > 0)
            Debug.Log("Limpeza do estadio: " + condenados.Count + " peca(s) removida(s) de"
                    + " cima do terreno da pista -> " + string.Join(", ", nomes.ToArray()));
        else
            Debug.Log("Limpeza do estadio: nada de cenario em cima da pista.");

        return condenados.Count;
    }

    /// A caixa envolvente encosta no terreno do estadio?
    ///
    /// O terreno e uma capsula, entao o teste e: existe algum ponto do EIXO da
    /// reta central a menos de "alcance" da caixa? Percorremos o eixo em passos
    /// de meio metro e medimos a distancia ate a caixa em cada ponto. Exato o
    /// bastante para pecas de varios metros, e roda so na construcao.
    static bool EncostaNaPista(Bounds b, PistaAtletismo geo, float alcance)
    {
        // de fora da altura do estadio nao interessa (caixa d'agua no alto, por ex.)
        if (b.min.y > geo.centro.y + 14f) return false;

        float L = geo.reta;
        int passos = Mathf.CeilToInt((L * 2f) / 0.5f);

        for (int i = 0; i <= passos; i++)
        {
            float x = -L + (L * 2f) * i / passos;
            Vector3 noEixo = geo.centro + new Vector3(x, 0f, 0f);

            float dx = Mathf.Max(0f, Mathf.Max(b.min.x - noEixo.x, noEixo.x - b.max.x));
            float dz = Mathf.Max(0f, Mathf.Max(b.min.z - noEixo.z, noEixo.z - b.max.z));

            if (dx * dx + dz * dz <= alcance * alcance) return true;
        }
        return false;
    }

    /// Instancia o modelo da pista. Diferente dos outros modelos do projeto, este
    /// e escalado so em X e Z: as malhas sao planos empilhados com 1 cm entre eles
    /// (asfalto, linhas, meio-fio) e reduzir o Y junto colocaria as camadas a 5 mm
    /// uma da outra, o que da briga de profundidade na tela.
    ///
    /// Aceita .obj ou .fbx. O arquivo entregue e .obj porque a Unity le OBJ
    /// nativamente, enquanto .glb exige um pacote a mais no projeto.
    static bool PisoDoModelo(Transform pai)
    {
        GameObject fonte = null;
        string[] extensoes = { ".obj", ".fbx" };
        string achado = null;

        for (int i = 0; i < extensoes.Length && fonte == null; i++)
        {
            string caminho = CaminhoDoModelo(MODELO_PISTA, extensoes[i]);
            if (caminho == null) continue;
            fonte = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
            if (fonte != null) achado = caminho;
        }

        if (fonte == null)
        {
            Debug.LogWarning("Modelo da pista nao encontrado em " + PASTA_MODELOS
                           + "/" + MODELO_PISTA + ".obj - desenhando a pista por codigo. "
                           + "A geometria e a mesma; muda so o acabamento.");
            return false;
        }

        GameObject inst = Object.Instantiate(fonte);
        inst.name = "Piso_" + MODELO_PISTA;
        inst.transform.SetParent(pai, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        Collider[] cols = inst.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) Object.DestroyImmediate(cols[i]);

        Renderer[] rends = inst.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0)
        {
            Debug.LogWarning(achado + " nao tem malha visivel. Desenhando por codigo.");
            Object.DestroyImmediate(inst);
            return false;
        }

        // ---- afere a escala medindo o arquivo, em vez de confiar no importador ----
        Bounds b = BoundsDe(inst);
        float k = (b.size.x > 0.01f)
                ? PISTA_ESCALA * (ARQ_COMPRIMENTO / b.size.x)
                : PISTA_ESCALA;

        inst.transform.localScale = new Vector3(k, 1f, k);

        // recentraliza em XZ e levanta do chao
        Bounds b2 = BoundsDe(inst);
        Vector3 desvio = b2.center - inst.transform.position;
        inst.transform.localPosition = new Vector3(-desvio.x, PISTA_Y, -desvio.z);

        // ---- repinta com a paleta do projeto ----
        for (int i = 0; i < rends.Length; i++)
        {
            Renderer r = rends[i];
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

            Material[] mats = r.sharedMaterials;
            Material[] novos = new Material[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                // Tenta o nome do material e, se ele nao disser nada (caso o .mtl
                // nao tenha sido importado e tudo tenha virado "Default-Material"),
                // cai no nome do objeto, que a Unity tira do grupo do OBJ.
                novos[j] = MaterialDaPista(mats[j] != null ? mats[j].name : null)
                        ?? MaterialDaPista(r.name)
                        ?? MaterialDaPista(r.transform.parent != null
                                           ? r.transform.parent.name : null)
                        ?? mPista;
            }
            r.sharedMaterials = novos;
        }

        Debug.Log("Modelo da pista: " + achado + ", medido " + b.size.ToString("F2")
                + " -> escala XZ " + k.ToString("F4") + " (vao de "
                + (ARQ_COMPRIMENTO * PISTA_ESCALA).ToString("F1") + " m).");
        return true;
    }

    /// Casa a parte do modelo com um material do projeto pelo NOME, como e feito
    /// com os alimentos. Funciona mesmo que a ordem dos submeshes mude na importacao.
    /// Devolve null quando o nome nao diz nada, para quem chamou tentar outro nome.
    static Material MaterialDaPista(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return null;
        string n = nome.ToLower();

        if (n.Contains("gramado") || n.Contains("grass")) return mGrama;
        if (n.Contains("meio_fio") || n.Contains("meiofio") || n.Contains("curb"))
            return mFaixaRua;
        if (n.Contains("linha") || n.Contains("line") || n.Contains("branc")) return mFaixaRua;
        if (n.Contains("chegada") || n.Contains("finish")) return mBorracha;
        if (n.Contains("externa") || n.Contains("apron") || n.Contains("outer")) return mCalcada;
        if (n.Contains("pista") || n.Contains("track")) return mPista;

        return null;
    }

    /// Reserva: a mesma capsula desenhada com primitivas, para o jogo nao depender
    /// do arquivo do modelo. A GEOMETRIA USADA PELAS REGRAS E A MESMA nos dois
    /// casos - quem manda e o componente PistaAtletismo, nao o visual.
    static void PisoDeCodigo(Transform pai)
    {
        const int SEG = 64;

        // gramado do miolo: um retangulo mais dois semicirculos
        Malha(pai, "GramadoMiolo", PrimitiveType.Cube,
              new Vector3(0f, PISTA_Y, 0f),
              new Vector3(PISTA_RETA * 2f, 0.02f, PISTA_R_INTERNO * 2f), mGrama);
        Malha(pai, "GramadoL", PrimitiveType.Cylinder,
              new Vector3(PISTA_RETA, PISTA_Y, 0f),
              new Vector3(PISTA_R_INTERNO * 2f, 0.01f, PISTA_R_INTERNO * 2f), mGrama);
        Malha(pai, "GramadoO", PrimitiveType.Cylinder,
              new Vector3(-PISTA_RETA, PISTA_Y, 0f),
              new Vector3(PISTA_R_INTERNO * 2f, 0.01f, PISTA_R_INTERNO * 2f), mGrama);

        float largura = PISTA_R_EXTERNO - PISTA_R_INTERNO;
        float meio = (PISTA_R_INTERNO + PISTA_R_EXTERNO) * 0.5f;
        float perimetro = 4f * PISTA_RETA + 2f * Mathf.PI * meio;
        float passo = perimetro / SEG * 1.25f;   // folga para nao abrir fresta

        for (int i = 0; i < SEG; i++)
        {
            float t = i / (float)SEG;
            Vector3 p = PontoDaCapsula(t, meio);
            Vector3 tg = TangenteDaCapsula(t);
            float giro = Mathf.Atan2(tg.x, tg.z) * Mathf.Rad2Deg;

            GameObject piso = Malha(pai, "Piso" + i, PrimitiveType.Cube,
                                    new Vector3(p.x, PISTA_Y, p.z),
                                    new Vector3(largura, 0.02f, passo), mPista);
            piso.transform.localRotation = Quaternion.Euler(0f, giro, 0f);

            // meio-fio interno, so visual
            Vector3 pin = PontoDaCapsula(t, PISTA_R_INTERNO);
            GameObject fio = Malha(pai, "MeioFio" + i, PrimitiveType.Cube,
                                   new Vector3(pin.x, PISTA_Y + 0.02f, pin.z),
                                   new Vector3(0.3f, 0.06f, passo), mFaixaRua);
            fio.transform.localRotation = Quaternion.Euler(0f, giro, 0f);

            // riscos das raias
            for (int raia = 1; raia < 8; raia++)
            {
                Vector3 pr = PontoDaCapsula(t, PISTA_R_INTERNO + PISTA_RAIA * raia);
                GameObject risco = Malha(pai, "Raia" + raia + "_" + i, PrimitiveType.Cube,
                                         new Vector3(pr.x, PISTA_Y + 0.01f, pr.z),
                                         new Vector3(0.08f, 0.01f, passo), mFaixaRua);
                risco.transform.localRotation = Quaternion.Euler(0f, giro, 0f);
            }
        }
    }

    /// As mesmas contas do PistaAtletismo, em coordenada LOCAL, para o construtor
    /// poder posicionar pecas sem depender do componente ja estar na cena.
    static Vector3 PontoDaCapsula(float t, float raio)
    {
        float L = PISTA_RETA, R = PISTA_R_MEDIDA;
        float arco = Mathf.PI * R;
        float perimetro = 4f * L + 2f * arco;
        float s = Mathf.Repeat(t, 1f) * perimetro;

        if (s <= 2f * L) return new Vector3(s - L, 0f, -raio);
        if (s <= 2f * L + arco)
        {
            float a = (s - 2f * L) / R - Mathf.PI * 0.5f;
            return new Vector3(L + Mathf.Cos(a) * raio, 0f, Mathf.Sin(a) * raio);
        }
        if (s <= 4f * L + arco) return new Vector3(L - (s - 2f * L - arco), 0f, raio);

        float b = (s - 4f * L - arco) / R + Mathf.PI * 0.5f;
        return new Vector3(-L + Mathf.Cos(b) * raio, 0f, Mathf.Sin(b) * raio);
    }

    static Vector3 TangenteDaCapsula(float t)
    {
        float L = PISTA_RETA, R = PISTA_R_MEDIDA;
        float arco = Mathf.PI * R;
        float perimetro = 4f * L + 2f * arco;
        float s = Mathf.Repeat(t, 1f) * perimetro;

        if (s <= 2f * L) return Vector3.right;
        if (s > 2f * L + arco && s <= 4f * L + arco) return Vector3.left;

        float a = (s <= 2f * L + arco)
                ? (s - 2f * L) / R - Mathf.PI * 0.5f
                : (s - 4f * L - arco) / R + Mathf.PI * 0.5f;
        return new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
    }

    /// Porticos da prova, em posicoes fixas    /// Porticos da prova, em posicoes fixas: o trajeto e sempre o mesmo, o que
    /// ajuda na hora de treinar a apresentacao.
    static Corrida CriarCorrida(Transform pai, GameObject largada)
    {
        GameObject raizCp = new GameObject("Porticos");
        raizCp.transform.SetParent(pai);

        // O trajeto tem que caber na faixa livre entre a avenida em anel (raio 30)
        // e a primeira fileira de predios (raio ~51), desviando dos terrenos
        // ocupados: hospital a oeste, pizzaria a noroeste, largada a nordeste e
        // a pista de atletismo ao sul. O Aferir() abaixo reclama se algum
        // portico encostar em alguma dessas coisas.
        Vector3[] trajeto = {
            new Vector3(0f, 0f, 40f),           // norte, na avenida
            new Vector3(40f, 0f, 8f),           // leste
            new Vector3(30f, 0f, -30f),         // sudeste
            new Vector3(-30f, 0f, -30f),        // sudoeste, acima do estadio
            new Vector3(-40f, 0f, -20f),        // oeste, abaixo do hospital
            largada.transform.position * 0.78f  // chegada, logo a frente do arco
        };

        Aferir(trajeto);

        Corrida corrida = largada.AddComponent<Corrida>();
        Checkpoint[] lista = new Checkpoint[trajeto.Length];

        for (int i = 0; i < trajeto.Length; i++)
        {
            GameObject cp = new GameObject("Portico" + (i + 1));
            cp.transform.SetParent(raizCp.transform);
            cp.transform.position = trajeto[i];
            // vira de frente para o centro do mapa
            Vector3 olhar = -trajeto[i];
            olhar.y = 0f;
            if (olhar.sqrMagnitude > 0.1f)
                cp.transform.rotation = Quaternion.LookRotation(olhar.normalized);

            // ---- a aura ----
            // Nada de portico: so uma coluna de luz amarela para atravessar. Sao
            // tres cilindros transparentes, um dentro do outro, mais um anel no
            // chao. Custa quatro malhas e nenhuma particula, nenhuma luz extra.
            GameObject aura = new GameObject("Aura");
            aura.transform.SetParent(cp.transform);
            aura.transform.localPosition = Vector3.zero;

            // O cilindro padrao da Unity tem 2 unidades de altura, entao a escala
            // em Y e METADE da altura desejada.
            Malha(aura.transform, "ColunaExterna", PrimitiveType.Cylinder,
                  new Vector3(0f, AURA_ALTURA * 0.5f, 0f),
                  new Vector3(AURA_RAIO * 2f, AURA_ALTURA * 0.5f, AURA_RAIO * 2f), mAura);
            Malha(aura.transform, "ColunaMedia", PrimitiveType.Cylinder,
                  new Vector3(0f, AURA_ALTURA * 0.42f, 0f),
                  new Vector3(AURA_RAIO * 1.3f, AURA_ALTURA * 0.42f, AURA_RAIO * 1.3f), mAura);
            Malha(aura.transform, "Nucleo", PrimitiveType.Cylinder,
                  new Vector3(0f, AURA_ALTURA * 0.34f, 0f),
                  new Vector3(AURA_RAIO * 0.5f, AURA_ALTURA * 0.34f, AURA_RAIO * 0.5f),
                  mAuraForte);

            // Anel no chao: marca onde pisar mesmo visto de cima. Fica FORA do
            // grupo que respira, senao a marca do chao inflaria junto e sairia
            // do lugar onde o jogador precisa pisar.
            Malha(cp.transform, "AnelDoChao", PrimitiveType.Cylinder,
                  new Vector3(0f, 0.06f, 0f),
                  new Vector3(AURA_RAIO * 2.3f, 0.05f, AURA_RAIO * 2.3f), mAuraForte);

            // Gatilho cilindrico: combina com o que se ve, em vez de uma caixa
            // invisivel maior que a luz.
            CapsuleCollider cc2 = cp.AddComponent<CapsuleCollider>();
            cc2.isTrigger = true;
            cc2.direction = 1;                       // eixo Y
            cc2.radius = AURA_RAIO;
            cc2.height = AURA_ALTURA;
            cc2.center = new Vector3(0f, AURA_ALTURA * 0.5f, 0f);

            Checkpoint c = cp.AddComponent<Checkpoint>();
            c.corrida = corrida;
            c.numero = i + 1;
            c.aura = aura.transform;
            lista[i] = c;
        }

        corrida.checkpoints = lista;
        corrida.treinos = AurasDeTreino(raizCp.transform, trajeto);

        TreinadorPacer pacer = CriarPacer(pai, largada.transform.position);
        pacer.pontos = trajeto;
        corrida.pacer = pacer;

        Transform parado = largada.transform.Find("Treinador");
        if (parado != null) corrida.treinadorParado = parado.gameObject;
        return corrida;
    }

    /// Auras AZUIS: condicionamento opcional, plantado fora da linha mais curta.
    ///
    /// Uma por trecho do percurso, deslocada perpendicularmente ao trecho. Tenta o
    /// lado de DENTRO primeiro (fica visivel do meio do mapa, que e aberto) e vai
    /// afrouxando o desvio ate achar um ponto que nao caia em terreno ocupado nem
    /// dentro da praca, onde tem fonte, postes e canteiros.
    static AuraDeTreino[] AurasDeTreino(Transform pai, Vector3[] trajeto)
    {
        float[] desvios = { 11f, 12f, 10f, 13f, 9f, 14f, 8f };
        List<AuraDeTreino> saida = new List<AuraDeTreino>();

        for (int i = 0; i < trajeto.Length - 1; i++)
        {
            Vector3 a = trajeto[i], b = trajeto[i + 1];
            Vector3 dir = (b - a);
            dir.y = 0f;
            if (dir.sqrMagnitude < 1f) continue;
            dir.Normalize();

            Vector3 meio = (a + b) * 0.5f;
            Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

            bool achou = false;
            Vector3 onde = Vector3.zero;

            for (int d = 0; d < desvios.Length && !achou; d++)
            {
                Vector3 dentro = meio + perp * desvios[d];
                Vector3 fora = meio - perp * desvios[d];
                // o "dentro" e o que fica mais perto do centro do mapa
                if (fora.sqrMagnitude < dentro.sqrMagnitude)
                { Vector3 t = dentro; dentro = fora; fora = t; }

                if (LugarDeAura(dentro)) { onde = dentro; achou = true; }
                else if (LugarDeAura(fora)) { onde = fora; achou = true; }
            }

            if (!achou)
            {
                Debug.LogWarning("Trecho " + i + " do treino ficou sem aura azul: nao achei"
                               + " lugar livre perto dele.");
                continue;
            }

            saida.Add(UmaAuraDeTreino(pai, onde, saida.Count));
        }

        Debug.Log("Treino de rua: " + saida.Count + " auras azuis de condicionamento.");
        return saida.ToArray();
    }

    /// Serve para aura azul? Fora de terreno ocupado, fora do miolo da praca
    /// (fonte e canteiros) e longe o bastante dos predios.
    static bool LugarDeAura(Vector3 p)
    {
        float r = new Vector2(p.x, p.z).magnitude;
        if (r < 20f || r > 50f) return false;
        return !Reservado(p, 3f);
    }

    static AuraDeTreino UmaAuraDeTreino(Transform pai, Vector3 pos, int indice)
    {
        GameObject g = new GameObject("AuraTreino" + (indice + 1));
        g.transform.SetParent(pai);
        g.transform.position = pos;

        GameObject aura = new GameObject("Aura");
        aura.transform.SetParent(g.transform);
        aura.transform.localPosition = Vector3.zero;

        float raio = AURA_RAIO * 0.85f;        // um pouco mais fina que a amarela
        float alt = AURA_ALTURA * 0.8f;

        Malha(aura.transform, "ColunaExterna", PrimitiveType.Cylinder,
              new Vector3(0f, alt * 0.5f, 0f),
              new Vector3(raio * 2f, alt * 0.5f, raio * 2f), mTreino);
        Malha(aura.transform, "Nucleo", PrimitiveType.Cylinder,
              new Vector3(0f, alt * 0.34f, 0f),
              new Vector3(raio * 0.5f, alt * 0.34f, raio * 0.5f), mTreinoForte);
        Malha(g.transform, "AnelDoChao", PrimitiveType.Cylinder,
              new Vector3(0f, 0.06f, 0f),
              new Vector3(raio * 2.3f, 0.05f, raio * 2.3f), mTreinoForte);

        CapsuleCollider cc = g.AddComponent<CapsuleCollider>();
        cc.isTrigger = true;
        cc.direction = 1;
        cc.radius = raio;
        cc.height = alt;
        cc.center = new Vector3(0f, alt * 0.5f, 0f);

        AuraDeTreino at = g.AddComponent<AuraDeTreino>();
        at.aura = aura.transform;
        at.fase = indice * 1.3f;
        return at;
    }

    /// O treinador que corre junto. Nasce desligado; a Corrida acende quando o
    /// cronometro solta.
    static TreinadorPacer CriarPacer(Transform pai, Vector3 ondeNasce)
    {
        GameObject g = new GameObject("TreinadorPacer");
        g.transform.SetParent(pai);
        g.transform.position = ondeNasce;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(g.transform);
        visual.transform.localPosition = Vector3.zero;

        Animator an = MontarHumanoide(visual.transform, mRival3, ALTURA_NPC);
        if (an == null) Boneco(visual.transform, mRival3);

        TreinadorPacer tp = g.AddComponent<TreinadorPacer>();
        tp.animator = an;
        return tp;
    }
}
