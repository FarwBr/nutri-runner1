using UnityEngine;

/// <summary>
/// Fase 2: o treino de rua amador. O gatilho fica no portico de largada. Cruzar
/// a largada com a dieta fechada dispersa o cheiro e comeca a corrida contra o relogio.
///
/// A ligacao com a fase 1 e o PESO: o Alex corre mais rapido quanto mais leve chegou,
/// entao quem coletou bem passa folgado e quem raspou sofre. Nao existe numero
/// escondido premiando a fase 1 - a recompensa e a propria velocidade.
///
/// A ligacao com a fase 3 e o CONDICIONAMENTO: as auras azuis coletadas e o tempo
/// que sobrou no relogio viram folego na prova. Sem isso o treino de rua nao
/// deixava marca nenhuma - voce passava ou nao passava, e acabava ali.
/// </summary>
public class Corrida : MonoBehaviour
{
    [Header("Percurso")]
    public Checkpoint[] checkpoints;
    [Tooltip("Auras azuis de condicionamento, fora do caminho mais curto")]
    public AuraDeTreino[] treinos;

    [Header("Companhia")]
    [Tooltip("O treinador corre junto como pacer. Vazio = o Alex corre sozinho.")]
    public TreinadorPacer pacer;
    [Tooltip("O treinador que fica parado na largada. Some quando o pacer entra,"
           + " para nao existirem dois ao mesmo tempo.")]
    public GameObject treinadorParado;

    [Header("Cronometro")]
    public float tempoLimite = 75f;
    public float bonusPorCheckpoint = 7f;

    int atual = -1;
    bool correndo;
    bool dentro;

    void Start()
    {
        for (int i = 0; i < checkpoints.Length; i++)
            if (checkpoints[i] != null) checkpoints[i].gameObject.SetActive(false);

        // as azuis so aparecem durante o treino
        for (int i = 0; i < treinos.Length; i++)
            if (treinos[i] != null) treinos[i].gameObject.SetActive(false);
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
        if (gm == null || gm.Estado != EstadoJogo.Jogando) return;
        if (correndo || !dentro) return;

        if (gm.Fase == FaseJogo.Dieta)
        {
            gm.Notificar("Feche a dieta antes de comecar o treino de rua");
            return;
        }
        if (gm.Fase == FaseJogo.IrParaLargada) Largar(gm);
    }

    void Largar(GameManager gm)
    {
        correndo = true;

        // O treinador conversa antes de soltar o cronometro. Se nao houver
        // caixa de dialogo na cena, a corrida comeca direto.
        if (Dialogo.Instance != null)
        {
            Dialogo.Instance.Abrir(Dialogo.Treinador(), ComecarDeFato);
            return;
        }
        ComecarDeFato();
    }

    void ComecarDeFato()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        DispersarCheiro();
        gm.IniciarCorrida(checkpoints.Length, tempoLimite);

        // acende as azuis e diz ao GameManager quantas existem
        int quantas = 0;
        for (int i = 0; i < treinos.Length; i++)
        {
            if (treinos[i] == null) continue;
            treinos[i].gameObject.SetActive(true);
            quantas++;
        }
        gm.TreinosTotal = quantas;
        gm.TreinosColetados = 0;

        if (pacer != null)
        {
            // o que estava parado vira o que corre: some um, entra o outro no
            // mesmo lugar, e le como a mesma pessoa comecando a correr
            Vector3 onde = transform.position;
            if (treinadorParado != null)
            {
                onde = treinadorParado.transform.position;
                treinadorParado.SetActive(false);
            }
            pacer.Largar(onde);
        }

        gm.Notificar(quantas > 0
            ? "TREINO DE RUA! Amarelas sao obrigatorias; as " + quantas
              + " azuis valem folego na prova"
            : "TREINO DE RUA! Cruze as auras antes do tempo acabar");

        atual = -1;
        Avancar(gm);
    }

    /// Na fase 2 o adversario e o cronometro, nao o cheiro.
    void DispersarCheiro()
    {
        NuvemCheiro[] nuvens = Object.FindObjectsOfType<NuvemCheiro>();
        for (int i = 0; i < nuvens.Length; i++)
            nuvens[i].gameObject.SetActive(false);
    }

    void Avancar(GameManager gm)
    {
        if (atual >= 0 && atual < checkpoints.Length && checkpoints[atual] != null)
            checkpoints[atual].gameObject.SetActive(false);

        atual++;

        if (atual >= checkpoints.Length)
        {
            if (pacer != null) pacer.Parar();
            for (int i = 0; i < treinos.Length; i++)
                if (treinos[i] != null) treinos[i].gameObject.SetActive(false);

            gm.TerminarCorrida();
            return;
        }

        Checkpoint c = checkpoints[atual];
        if (c == null) { Avancar(gm); return; }

        c.gameObject.SetActive(true);
        gm.DefinirAlvoCorrida(c.transform, atual + 1);
    }

    public void Passou(Checkpoint c)
    {
        GameManager gm = GameManager.Instance;
        if (!correndo || gm == null) return;
        if (atual < 0 || atual >= checkpoints.Length || checkpoints[atual] != c) return;

        gm.PassarCheckpoint(bonusPorCheckpoint);
        AudioDirector.Tocar(false);
        Avancar(gm);
    }
}
