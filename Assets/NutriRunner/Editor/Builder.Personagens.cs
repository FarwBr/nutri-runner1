using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Tudo que tem corpo: o Avatar Humanoid compartilhado, os controladores de
/// Animator, o Alex, os NPCs, os rivais e a taca.
/// </summary>
public static partial class DemoSceneBuilder
{

    /// Avatar do Mannequin, preparado uma unica vez e reaproveitado por todo mundo.
    static Avatar avatarHumano;
    /// Onde o Alex para na vitoria. O podio nasce com a pista, antes do Alex.
    static Transform degrauDoOuro;
    static AnimatorController controladorNpc;


    /// Corredor rival: boneco simples de primitivas, com um no "Visual"
    /// para o Corredor poder inclinar o corpo na passada.
    static GameObject CriarCorredor(Transform pai, string nome, Material cor)
    {
        GameObject raiz = new GameObject("Corredor_" + nome);
        raiz.transform.SetParent(pai);

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(raiz.transform);
        visual.transform.localPosition = Vector3.zero;

        Corredor c = raiz.AddComponent<Corredor>();

        // o rival usa o MESMO modelo do Alex, so com outra cor de camisa
        Animator an = MontarHumanoide(visual.transform, cor, ALTURA_NPC);
        if (an != null)
        {
            c.animator = an;
            return raiz;
        }

        // reserva de primitivas, caso o modelo nao esteja no projeto
        Boneco(visual.transform, cor);
        return raiz;
    }

    /// Boneco de primitivas: a reserva de quando o modelo humanoide nao esta no
    /// projeto. Vive numa funcao so para o rival e o treinador nao divergirem.
    static void Boneco(Transform visual, Material cor)
    {
        Malha(visual, "Perna", PrimitiveType.Capsule,
              new Vector3(0f, 0.45f, 0f), new Vector3(0.30f, 0.45f, 0.30f), mCalca);
        Malha(visual, "Tronco", PrimitiveType.Cube,
              new Vector3(0f, 1.18f, 0f), new Vector3(0.52f, 0.60f, 0.30f), cor);
        Malha(visual, "BracoE", PrimitiveType.Capsule,
              new Vector3(-0.36f, 1.15f, 0f), new Vector3(0.17f, 0.24f, 0.17f), mPele);
        Malha(visual, "BracoD", PrimitiveType.Capsule,
              new Vector3(0.36f, 1.15f, 0f), new Vector3(0.17f, 0.24f, 0.17f), mPele);
        Malha(visual, "Cabeca", PrimitiveType.Sphere,
              new Vector3(0f, 1.68f, 0f), new Vector3(0.36f, 0.40f, 0.36f), mPele);
        Malha(visual, "Cabelo", PrimitiveType.Sphere,
              new Vector3(0f, 1.74f, -0.02f), new Vector3(0.38f, 0.30f, 0.38f), mCabelo);
    }

    /// Personagem parado da cena (medico, nutricionista, treinador).
    /// Usa o MESMO modelo do Alex: muda a roupa e o estado de animacao.
    /// Sao ancoras visuais - quem conduz a conversa e a caixa de dialogo.
    static void CriarPessoa(Transform pai, string nome, Vector3 posLocal,
                            float giro, Material roupa, bool cabeloLongo,
                            string estadoAnim)
    {
        GameObject g = new GameObject(nome);
        g.transform.SetParent(pai);
        g.transform.localPosition = posLocal;
        g.transform.localRotation = Quaternion.Euler(0f, giro, 0f);

        CapsuleCollider col = g.AddComponent<CapsuleCollider>();
        col.height = 2f;
        col.radius = 0.4f;
        col.center = new Vector3(0f, 1f, 0f);

        Animator an = MontarHumanoide(g.transform, roupa, ALTURA_NPC);
        if (an != null)
        {
            AnimadorNpc anim = g.AddComponent<AnimadorNpc>();
            anim.animator = an;
            anim.estado = estadoAnim;
            // defasagem para dois NPCs lado a lado nao respirarem juntos
            anim.defasagem = Random.value;
            return;
        }

        // reserva de primitivas
        Malha(g.transform, "Perna", PrimitiveType.Capsule,
              new Vector3(0f, 0.45f, 0f), new Vector3(0.32f, 0.45f, 0.32f), mCalca);
        Malha(g.transform, "Tronco", PrimitiveType.Cube,
              new Vector3(0f, 1.15f, 0f), new Vector3(0.56f, 0.72f, 0.32f), roupa);
        Malha(g.transform, "BracoE", PrimitiveType.Capsule,
              new Vector3(-0.38f, 1.15f, 0f), new Vector3(0.17f, 0.26f, 0.17f), roupa);
        Malha(g.transform, "BracoD", PrimitiveType.Capsule,
              new Vector3(0.38f, 1.15f, 0f), new Vector3(0.17f, 0.26f, 0.17f), roupa);
        Malha(g.transform, "Cabeca", PrimitiveType.Sphere,
              new Vector3(0f, 1.70f, 0f), new Vector3(0.36f, 0.40f, 0.36f), mPele);
        Malha(g.transform, "Cabelo", PrimitiveType.Sphere,
              new Vector3(0f, 1.78f, -0.03f),
              cabeloLongo ? new Vector3(0.40f, 0.34f, 0.42f)
                          : new Vector3(0.38f, 0.26f, 0.38f), mCabelo);
        Malha(g.transform, "Prancheta", PrimitiveType.Cube,
              new Vector3(0.30f, 1.10f, 0.26f), new Vector3(0.32f, 0.42f, 0.04f), mPlaca);
    }

    // ================= Alex, camera, luz =================

    static GameObject CriarAlex(Transform pai, Vector3 pos)
    {
        GameObject alex = new GameObject("Alex");
        alex.transform.SetParent(pai);
        alex.transform.position = pos;

        CharacterController cc = alex.AddComponent<CharacterController>();
        cc.height = 1.75f;
        cc.radius = 0.34f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.slopeLimit = 50f;
        cc.stepOffset = 0.45f;

        alex.AddComponent<ThirdPersonController>();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(alex.transform);
        visual.transform.localPosition = Vector3.zero;

        if (MontarAlexAnimado(alex, visual.transform)) return alex;

        GameObject corpo = new GameObject("Corpo");
        corpo.transform.SetParent(visual.transform);
        corpo.transform.localPosition = Vector3.zero;

        Malha(corpo.transform, "Quadril", PrimitiveType.Cube,
              new Vector3(0f, 0.90f, 0f), new Vector3(0.44f, 0.24f, 0.28f), mCalca);
        Malha(corpo.transform, "Tronco", PrimitiveType.Cube,
              new Vector3(0f, 1.26f, 0f), new Vector3(0.54f, 0.58f, 0.31f), mCamiseta);
        Malha(corpo.transform, "OmbroE", PrimitiveType.Sphere,
              new Vector3(-0.30f, 1.47f, 0f), new Vector3(0.24f, 0.24f, 0.28f), mCamiseta);
        Malha(corpo.transform, "OmbroD", PrimitiveType.Sphere,
              new Vector3(0.30f, 1.47f, 0f), new Vector3(0.24f, 0.24f, 0.28f), mCamiseta);
        Malha(corpo.transform, "Pescoco", PrimitiveType.Cylinder,
              new Vector3(0f, 1.59f, 0f), new Vector3(0.13f, 0.06f, 0.13f), mPele);
        Malha(corpo.transform, "Cabeca", PrimitiveType.Sphere,
              new Vector3(0f, 1.74f, 0f), new Vector3(0.38f, 0.42f, 0.38f), mPele);
        Malha(corpo.transform, "Cabelo", PrimitiveType.Sphere,
              new Vector3(0f, 1.80f, -0.02f), new Vector3(0.40f, 0.32f, 0.40f), mCabelo);
        Malha(corpo.transform, "OlhoE", PrimitiveType.Sphere,
              new Vector3(-0.09f, 1.76f, 0.16f), new Vector3(0.07f, 0.09f, 0.05f), mOlho);
        Malha(corpo.transform, "OlhoD", PrimitiveType.Sphere,
              new Vector3(0.09f, 1.76f, 0.16f), new Vector3(0.07f, 0.09f, 0.05f), mOlho);
        Malha(corpo.transform, "Nariz", PrimitiveType.Cube,
              new Vector3(0f, 1.70f, 0.18f), new Vector3(0.08f, 0.08f, 0.08f), mPele);

        Transform bracoE = Membro(visual.transform, "BracoE", new Vector3(-0.36f, 1.46f, 0f), false);
        Transform bracoD = Membro(visual.transform, "BracoD", new Vector3(0.36f, 1.46f, 0f), false);
        Transform pernaE = Membro(visual.transform, "PernaE", new Vector3(-0.14f, 0.82f, 0f), true);
        Transform pernaD = Membro(visual.transform, "PernaD", new Vector3(0.14f, 0.82f, 0f), true);

        AnimadorSimples anim = alex.AddComponent<AnimadorSimples>();
        anim.bracoE = bracoE; anim.bracoD = bracoD;
        anim.pernaE = pernaE; anim.pernaD = pernaD;
        anim.corpo = corpo.transform;

        // sem esqueleto a taca nao tem onde se pendurar, mas ainda aparece
        Comemoracao com = alex.AddComponent<Comemoracao>();
        com.taca = CriarTaca(alex.transform);
        com.podio = degrauDoOuro;

        return alex;
    }

    /// Encaixa o modelo com esqueleto, prepara os rigs como Humanoid, monta o
    /// AnimatorController por codigo e liga tudo. Devolve false se faltar arquivo,
    /// e ai o construtor cai no boneco de primitivas de sempre.
    static bool MontarAlexAnimado(GameObject alex, Transform visual)
    {
        if (!TemModelo(MODELO_ALEX)) return false;

        Avatar avatar = AvatarHumano();
        if (avatar == null)
        {
            Debug.LogWarning("Nao consegui gerar o Avatar Humanoid de " + MODELO_ALEX
                           + ". Abra o FBX, aba Rig, escolha Humanoid e reconstrua.");
            return false;
        }

        AnimationClip parado = Clipe(RIG_GERAL, CLIPE_PARADO);
        AnimationClip andando = Clipe(RIG_MOVIMENTO, CLIPE_ANDANDO);
        AnimationClip correndo = Clipe(RIG_MOVIMENTO, CLIPE_CORRENDO);
        AnimationClip pulando = Clipe(RIG_MOVIMENTO, CLIPE_PULANDO);

        if (parado == null || andando == null || correndo == null)
        {
            Debug.LogWarning("Faltam clipes de animacao (" + CLIPE_PARADO + "/"
                           + CLIPE_ANDANDO + "/" + CLIPE_CORRENDO + "). Usando o boneco simples.");
            return false;
        }

        GameObject fonte = CarregarModelo(MODELO_ALEX, ".fbx");
        GameObject inst = Object.Instantiate(fonte);
        inst.name = "Alex_Modelo";
        inst.transform.SetParent(visual, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        // reescala para bater com a altura do CharacterController
        Bounds b = BoundsDe(inst);
        if (b.size.y > 0.01f)
        {
            float k = ALTURA_ALEX / b.size.y;
            inst.transform.localScale = Vector3.one * k;
        }

        PintarPersonagem(inst, mCamiseta);

        Animator an = inst.GetComponent<Animator>();
        if (an == null) an = inst.AddComponent<Animator>();
        an.avatar = avatar;
        an.applyRootMotion = false;   // quem move o Alex e o ThirdPersonController
        an.updateMode = AnimatorUpdateMode.Normal;
        an.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        an.runtimeAnimatorController = CriarControlador(parado, andando, correndo, pulando);

        AnimadorPersonagem ap = alex.AddComponent<AnimadorPersonagem>();
        ap.animator = an;

        Comemoracao com = alex.AddComponent<Comemoracao>();
        com.animator = an;
        com.taca = CriarTaca(alex.transform);
        com.podio = degrauDoOuro;

        Debug.Log("Alex animado: modelo + Animator prontos.");
        return true;
    }

    /// Prepara o Mannequin como Humanoid uma vez e guarda o Avatar. Todos os
    /// personagens da cena saem deste mesmo molde: muda so a cor da roupa.
    /// Prepara o Avatar Humanoid compartilhado. Cada saida tem mensagem propria:
    /// antes isto devolvia null em silencio, e um personagem quadrado na tela nao
    /// dizia QUAL dos quatro motivos tinha acontecido.
    static Avatar AvatarHumano()
    {
        if (avatarHumano != null) return avatarHumano;

        if (!TemModelo(MODELO_ALEX))
        {
            Debug.LogError("PERSONAGENS QUADRADOS: falta " + MODELO_ALEX + ".fbx em "
                         + PASTA_MODELOS + ". Alex, medicos, treinador, nutricionista e"
                         + " os quatro rivais caem para bonecos de primitivas.");
            return null;
        }

        avatarHumano = PrepararRig(MODELO_ALEX, null);
        if (avatarHumano == null)
        {
            Debug.LogError("PERSONAGENS QUADRADOS: " + MODELO_ALEX + ".fbx existe, mas a"
                         + " Unity nao gerou o Avatar Humanoid dele. Abra o arquivo, aba"
                         + " Rig, Animation Type = Humanoid, Apply, e reconstrua a cena.");
            return null;
        }

        if (!TemModelo(RIG_GERAL))
            Debug.LogError("Falta " + RIG_GERAL + ".fbx: ninguem anda nem corre.");
        else PrepararRig(RIG_GERAL, avatarHumano);

        if (!TemModelo(RIG_MOVIMENTO))
            Debug.LogWarning("Falta " + RIG_MOVIMENTO + ".fbx: sem o pulo animado.");
        else PrepararRig(RIG_MOVIMENTO, avatarHumano);

        if (TemModelo(RIG_SIMULACAO)) PrepararRig(RIG_SIMULACAO, avatarHumano);
        else Debug.LogWarning("Falta " + RIG_SIMULACAO + ".fbx: sem o '" + CLIPE_COMEMORA
                            + "' do final da taca e sem o '" + CLIPE_ACENA + "' dos NPCs.");

        return avatarHumano;
    }

    /// Instancia o modelo humano sob "pai", na altura pedida e com a roupa dada,
    /// ja com Animator e o controlador dos NPCs. Devolve null se faltar arquivo.
    static Animator MontarHumanoide(Transform pai, Material roupa, float altura)
    {
        Avatar av = AvatarHumano();
        if (av == null) return null;

        AnimatorController ctrl = ControladorNpc();
        if (ctrl == null)
        {
            Debug.LogError("PERSONAGENS QUADRADOS: nao consegui montar o Npc.controller."
                         + " Confira se " + RIG_GERAL + ".fbx tem os clipes de locomocao.");
            return null;
        }

        GameObject fonte = CarregarModelo(MODELO_ALEX, ".fbx");
        if (fonte == null) return null;

        GameObject inst = Object.Instantiate(fonte);
        inst.name = "Modelo";
        inst.transform.SetParent(pai, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        Bounds b = BoundsDe(inst);
        if (b.size.y > 0.01f) inst.transform.localScale = Vector3.one * (altura / b.size.y);

        PintarPersonagem(inst, roupa);

        Animator an = inst.GetComponent<Animator>();
        if (an == null) an = inst.AddComponent<Animator>();
        an.avatar = av;
        an.applyRootMotion = false;
        an.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        an.runtimeAnimatorController = ctrl;
        return an;
    }

    /// Controlador dos NPCs e dos rivais: um blend tree de locomocao, mais os
    /// estados soltos de acenar e comemorar, chamados por Play().
    static AnimatorController ControladorNpc()
    {
        if (controladorNpc != null) return controladorNpc;

        AnimationClip parado = Clipe(RIG_GERAL, CLIPE_PARADO);
        AnimationClip andando = Clipe(RIG_MOVIMENTO, CLIPE_ANDANDO);
        AnimationClip correndo = Clipe(RIG_MOVIMENTO, CLIPE_CORRENDO);
        if (parado == null || andando == null || correndo == null) return null;

        string caminho = PASTA + "/Npc.controller";
        AssetDatabase.DeleteAsset(caminho);

        AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(caminho);
        ac.AddParameter("Velocidade", AnimatorControllerParameterType.Float);

        BlendTree arvore;
        AnimatorState loco = ac.CreateBlendTreeInController(ESTADO_LOCOMOCAO, out arvore, 0);
        arvore.blendType = BlendTreeType.Simple1D;
        arvore.blendParameter = "Velocidade";
        arvore.useAutomaticThresholds = false;
        arvore.AddChild(parado, 0f);
        arvore.AddChild(andando, 4.4f);
        arvore.AddChild(correndo, 7.8f);
        ac.layers[0].stateMachine.defaultState = loco;

        AnimationClip acena = Clipe(RIG_SIMULACAO, CLIPE_ACENA);
        if (acena != null)
            ac.layers[0].stateMachine.AddState(ESTADO_ACENANDO).motion = acena;

        AnimationClip comemora = Clipe(RIG_SIMULACAO, CLIPE_COMEMORA);
        if (comemora != null)
            ac.layers[0].stateMachine.AddState(ESTADO_COMEMORANDO).motion = comemora;

        AssetDatabase.SaveAssets();
        controladorNpc = ac;
        return ac;
    }

    /// Configura o importador do FBX: rig Humanoid e loop nos clipes de locomocao.
    static Avatar PrepararRig(string arquivo, Avatar copiarDe)
    {
        string caminho = CaminhoDoModelo(arquivo, ".fbx");
        if (caminho == null) return null;
        ModelImporter mi = AssetImporter.GetAtPath(caminho) as ModelImporter;
        if (mi == null) return null;

        bool mudou = false;

        if (mi.animationType != ModelImporterAnimationType.Human)
        {
            mi.animationType = ModelImporterAnimationType.Human;
            mudou = true;
        }

        ModelImporterAvatarSetup alvo = (copiarDe == null)
            ? ModelImporterAvatarSetup.CreateFromThisModel
            : ModelImporterAvatarSetup.CopyFromOther;

        if (mi.avatarSetup != alvo)
        {
            mi.avatarSetup = alvo;
            mudou = true;
        }
        if (copiarDe != null && mi.sourceAvatar != copiarDe)
        {
            mi.sourceAvatar = copiarDe;
            mudou = true;
        }

        // locomocao precisa repetir; pulo e uma pose sustentada, tambem repete
        string[] emLoop = { CLIPE_PARADO, CLIPE_ANDANDO, CLIPE_CORRENDO, CLIPE_PULANDO,
                            CLIPE_COMEMORA, CLIPE_ACENA };
        ModelImporterClipAnimation[] clipes = mi.defaultClipAnimations;
        bool mexeuClipe = false;
        for (int i = 0; i < clipes.Length; i++)
        {
            for (int k = 0; k < emLoop.Length; k++)
            {
                if (clipes[i].name == emLoop[k] && !clipes[i].loopTime)
                {
                    clipes[i].loopTime = true;
                    mexeuClipe = true;
                }
            }
        }
        if (mexeuClipe) { mi.clipAnimations = clipes; mudou = true; }

        if (mudou) mi.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Avatar>(caminho);
    }

    static AnimationClip Clipe(string arquivo, string nome)
    {
        string caminho = CaminhoDoModelo(arquivo, ".fbx");
        if (caminho == null) return null;
        Object[] todos = AssetDatabase.LoadAllAssetsAtPath(caminho);
        for (int i = 0; i < todos.Length; i++)
        {
            AnimationClip c = todos[i] as AnimationClip;
            if (c != null && c.name == nome) return c;
        }
        return null;
    }

    /// Blend tree de locomocao por velocidade, mais um estado de pulo.
    static AnimatorController CriarControlador(AnimationClip parado, AnimationClip andando,
                                               AnimationClip correndo, AnimationClip pulando)
    {
        string caminho = PASTA + "/Alex.controller";
        AssetDatabase.DeleteAsset(caminho);

        AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(caminho);
        ac.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
        ac.AddParameter("NoChao", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = ac.layers[0].stateMachine;

        BlendTree arvore;
        AnimatorState locomocao = ac.CreateBlendTreeInController("Locomocao", out arvore, 0);
        arvore.blendType = BlendTreeType.Simple1D;
        arvore.blendParameter = "Velocidade";
        arvore.useAutomaticThresholds = false;
        arvore.AddChild(parado, 0f);
        arvore.AddChild(andando, 4.4f);   // velCaminhada do ThirdPersonController
        arvore.AddChild(correndo, 7.8f);  // velCorrida
        sm.defaultState = locomocao;

        if (pulando != null)
        {
            AnimatorState pulo = sm.AddState("Pulo");
            pulo.motion = pulando;

            AnimatorStateTransition sobe = locomocao.AddTransition(pulo);
            sobe.AddCondition(AnimatorConditionMode.IfNot, 0f, "NoChao");
            sobe.hasExitTime = false;
            sobe.duration = 0.08f;

            AnimatorStateTransition desce = pulo.AddTransition(locomocao);
            desce.AddCondition(AnimatorConditionMode.If, 0f, "NoChao");
            desce.hasExitTime = false;
            desce.duration = 0.12f;
        }

        // o final: entra por Play() quando a prova termina no podio
        AnimationClip comemora = Clipe(RIG_SIMULACAO, CLIPE_COMEMORA);
        if (comemora != null) sm.AddState(ESTADO_COMEMORANDO).motion = comemora;
        else Debug.LogWarning("Sem o clipe " + CLIPE_COMEMORA + ": a comemoracao "
                            + "final nao vai animar. Falta " + RIG_SIMULACAO + ".fbx?");

        AssetDatabase.SaveAssets();
        return ac;
    }

    /// A taca do campeonato, montada com primitivas. Nasce desligada sob o Alex;
    /// o script Comemoracao pendura ela no osso da mao quando ele vence.
    static GameObject CriarTaca(Transform pai)
    {
        GameObject t = new GameObject("Taca");
        t.transform.SetParent(pai);
        t.transform.localPosition = Vector3.zero;

        Malha(t.transform, "Base", PrimitiveType.Cylinder,
              new Vector3(0f, 0.015f, 0f), new Vector3(0.17f, 0.015f, 0.17f), mOuro);
        Malha(t.transform, "Pedestal", PrimitiveType.Cylinder,
              new Vector3(0f, 0.05f, 0f), new Vector3(0.11f, 0.03f, 0.11f), mOuro);
        Malha(t.transform, "Haste", PrimitiveType.Cylinder,
              new Vector3(0f, 0.11f, 0f), new Vector3(0.035f, 0.04f, 0.035f), mOuro);
        Malha(t.transform, "Copo", PrimitiveType.Cylinder,
              new Vector3(0f, 0.20f, 0f), new Vector3(0.15f, 0.06f, 0.15f), mOuro);
        Malha(t.transform, "Boca", PrimitiveType.Cylinder,
              new Vector3(0f, 0.265f, 0f), new Vector3(0.17f, 0.012f, 0.17f), mOuro);

        // as duas alcas
        for (int i = 0; i < 2; i++)
        {
            float x = (i == 0) ? -0.105f : 0.105f;
            MalhaRot(t.transform, "Alca" + i, PrimitiveType.Cylinder,
                     new Vector3(x, 0.215f, 0f), new Vector3(90f, 0f, 0f),
                     new Vector3(0.022f, 0.055f, 0.022f), mOuro);
            Malha(t.transform, "AlcaTopo" + i, PrimitiveType.Sphere,
                  new Vector3(x, 0.265f, 0f), new Vector3(0.05f, 0.05f, 0.05f), mOuro);
            Malha(t.transform, "AlcaBase" + i, PrimitiveType.Sphere,
                  new Vector3(x, 0.165f, 0f), new Vector3(0.05f, 0.05f, 0.05f), mOuro);
        }

        t.SetActive(false);
        return t;
    }

    /// Veste o boneco com a paleta do projeto, por nome de parte.
    /// "roupa" e a cor do tronco: e so o que distingue um personagem do outro,
    /// ja que todos usam o mesmo modelo.
    static void PintarPersonagem(GameObject raiz, Material roupa)
    {
        Renderer[] rs = raiz.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; i++)
        {
            string n = rs[i].gameObject.name.ToLower();
            Material m = mPele;
            if (n.Contains("body") || n.Contains("torso")) m = roupa;
            else if (n.Contains("leg") || n.Contains("foot")) m = mCalca;

            Material[] slots = new Material[rs[i].sharedMaterials.Length];
            for (int k = 0; k < slots.Length; k++) slots[k] = m;
            rs[i].sharedMaterials = slots;

            rs[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rs[i].receiveShadows = false;
            rs[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            rs[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }

    static Camera CriarCamera(Transform pai, Transform alvo)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindObjectOfType<Camera>();
        if (cam == null) cam = new GameObject("Main Camera").AddComponent<Camera>();

        cam.gameObject.name = "Main Camera";
        cam.gameObject.tag = "MainCamera";
        cam.enabled = true;
        if (Object.FindObjectOfType<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        cam.transform.SetParent(pai);
        cam.transform.localPosition = new Vector3(0f, 3f, -18f);
        cam.transform.localRotation = Quaternion.identity;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.58f, 0.78f, 0.93f);
        cam.fieldOfView = 65f;
        cam.nearClipPlane = 0.2f;
        cam.farClipPlane = 220f;
        cam.allowHDR = false;
        cam.allowMSAA = false;

        OrbitCamera orb = cam.GetComponent<OrbitCamera>();
        if (orb == null) orb = cam.gameObject.AddComponent<OrbitCamera>();
        orb.alvo = alvo;
        return cam;
    }
}
