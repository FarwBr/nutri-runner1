using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Gera o executavel em um clique, com as configuracoes certas ja aplicadas.
///
/// Existe porque fechar uma build na mao sao dez passos em tres telas diferentes,
/// e errar um deles na vespera de uma apresentacao e facil demais: a cena nao
/// entra na lista, a janela abre em fullscreen e trava o projetor, a resolucao
/// sai num tamanho que a sala nao mostra. Aqui isso tudo vira um item de menu.
///
/// Tools -> Nutri Runner -> Gerar Executavel (Windows)
/// </summary>
public static class Publicador
{
    const string NOME_DO_JOGO = "NutriRunner";

    [MenuItem("Tools/Nutri Runner/Gerar Executavel (Windows)", false, 60)]
    public static void Gerar()
    {
        // ---- 1. a cena precisa estar salva em disco ----
        var cena = EditorSceneManager.GetActiveScene();

        if (string.IsNullOrEmpty(cena.path))
        {
            EditorUtility.DisplayDialog(
                "Salve a cena antes",
                "A cena aberta nunca foi salva, entao nao tem caminho para entrar na build.\n\n"
              + "Ctrl+S, escolha um nome, e rode este menu de novo.", "Entendi");
            return;
        }

        if (cena.isDirty)
        {
            bool salvar = EditorUtility.DisplayDialog(
                "A cena tem mudancas nao salvas",
                "A build usa o que esta EM DISCO, nao o que esta na tela.\n\n"
              + "Salvar agora?", "Salvar e continuar", "Cancelar");
            if (!salvar) return;
            EditorSceneManager.SaveScene(cena);
        }

        // ---- 2. conferir que a cena tem o jogo dentro ----
        if (GameObject.Find("NutriRunner") == null)
        {
            bool seguir = EditorUtility.DisplayDialog(
                "Esta cena parece vazia",
                "Nao achei o objeto 'NutriRunner' na cena. Voce rodou "
              + "Construir Cena Demo?\n\nGerar assim mesmo?", "Gerar", "Cancelar");
            if (!seguir) return;
        }

        // ---- 3. as configuracoes que importam numa sala de aula ----
        AjustarPlayerSettings();

        // ---- 4. onde salvar ----
        string pasta = EditorUtility.SaveFolderPanel(
            "Pasta para o executavel (escolha uma VAZIA)", "", NOME_DO_JOGO + "-Build");
        if (string.IsNullOrEmpty(pasta)) return;

        // ---- 5. build ----
        BuildPlayerOptions opcoes = new BuildPlayerOptions();
        opcoes.scenes = new string[] { cena.path };
        opcoes.locationPathName = pasta + "/" + NOME_DO_JOGO + ".exe";
        opcoes.target = BuildTarget.StandaloneWindows64;
        opcoes.targetGroup = BuildTargetGroup.Standalone;
        opcoes.options = BuildOptions.None;

        Debug.Log("Gerando executavel em " + pasta + " ... isso leva alguns minutos.");
        BuildReport relatorio = BuildPipeline.BuildPlayer(opcoes);
        BuildSummary resumo = relatorio.summary;

        if (resumo.result == BuildResult.Succeeded)
        {
            float mb = resumo.totalSize / (1024f * 1024f);
            Debug.Log("BUILD PRONTA: " + opcoes.locationPathName
                    + "  (" + mb.ToString("F0") + " MB, "
                    + resumo.totalTime.TotalSeconds.ToString("F0") + " s)\n"
                    + "Para entregar, zipe a PASTA INTEIRA: o .exe nao roda sem a "
                    + "pasta _Data do lado.");
            EditorPrefs.SetString("NutriRunner.UltimaBuild", opcoes.locationPathName);
            EditorUtility.RevealInFinder(opcoes.locationPathName);
        }
        else
        {
            Debug.LogError("A build falhou (" + resumo.result + ") com "
                         + resumo.totalErrors + " erro(s). Veja o Console acima: "
                         + "quase sempre e erro de compilacao em algum script.");
        }
    }

    /// As quatro configuracoes que ja deram problema em apresentacao.
    static void AjustarPlayerSettings()
    {
        PlayerSettings.productName = "Nutri Runner";
        if (string.IsNullOrEmpty(PlayerSettings.companyName)
            || PlayerSettings.companyName == "DefaultCompany")
            PlayerSettings.companyName = "Univates";

        // Janela, nao fullscreen: trocar de tela com o projetor em fullscreen
        // costuma congelar a imagem ou mandar o jogo para o monitor errado.
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;

        // Sem isso o jogo pausa quando voce clica fora para mexer em outra coisa.
        PlayerSettings.runInBackground = true;

        Debug.Log("Player Settings: janela 1280x720, redimensionavel, roda em segundo plano.");

        // Gamma e mais leve que Linear, mas trocar REIMPORTA todos os assets e
        // pode levar minutos. Entao pergunta em vez de fazer por conta.
        if (PlayerSettings.colorSpace != ColorSpace.Gamma)
        {
            bool trocar = EditorUtility.DisplayDialog(
                "Color Space esta em Linear",
                "Gamma e mais leve e combina com o visual cartoon do projeto.\n\n"
              + "ATENCAO: trocar reimporta TODOS os assets e pode levar varios "
              + "minutos. Nao faca isso em cima da hora.\n\nTrocar para Gamma?",
                "Trocar (demora)", "Deixar como esta");
            if (trocar) PlayerSettings.colorSpace = ColorSpace.Gamma;
        }
    }

    [MenuItem("Tools/Nutri Runner/Abrir a pasta da ultima build", false, 61)]
    public static void AbrirPasta()
    {
        string caminho = EditorPrefs.GetString("NutriRunner.UltimaBuild", "");
        if (string.IsNullOrEmpty(caminho))
        {
            Debug.Log("Nenhuma build gerada por este menu ainda.");
            return;
        }
        EditorUtility.RevealInFinder(caminho);
    }
}
