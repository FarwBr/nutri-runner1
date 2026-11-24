using UnityEngine;

/// <summary>
/// A pizzaria: origem do cheiro. Nao e mais objetivo do jogo, so o lugar de onde
/// as nuvens nascem. O GameManager pede uma nuvem extra a cada X alimentos.
/// </summary>
public class Pizzaria : MonoBehaviour
{
    public GameObject modeloNuvemPequena;
    public Transform[] pontosNuvem;

    [Header("Nuvens extras da fase 1")]
    public float velocidadeExtraInicial = 3.6f;
    public float velocidadeExtraFinal = 6.4f;
    [Range(0.5f, 1.1f)] public float tetoExtra = 0.95f;

    int proximoPonto;

    /// Chamado pelo GameManager a cada X alimentos: mais um perseguidor na rua.
    public void SoltarNuvemExtra()
    {
        NuvemCheiro nc = Nascer();
        if (nc == null) return;

        nc.velocidadeInicial = velocidadeExtraInicial;
        nc.velocidadeFinal = velocidadeExtraFinal;
        nc.tetoDaCorrida = tetoExtra;   // quase te acompanha: so da para escapar manobrando
        nc.raioDano = 2.3f;
        nc.intensidade = 0.7f;
        nc.antecipacao = 0.7f;
    }

    NuvemCheiro Nascer()
    {
        if (modeloNuvemPequena == null || pontosNuvem == null || pontosNuvem.Length == 0)
            return null;

        Transform ponto = pontosNuvem[proximoPonto % pontosNuvem.Length];
        proximoPonto++;
        if (ponto == null) return null;

        GameObject n = Instantiate(modeloNuvemPequena, ponto.position,
                                   Quaternion.identity, transform.parent);
        n.SetActive(true);

        NuvemCheiro nc = n.GetComponent<NuvemCheiro>();
        if (nc != null) nc.principal = false;
        return nc;
    }

}
