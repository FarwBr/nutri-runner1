using UnityEngine;

/// <summary>
/// Aura AZUL do treino de rua: condicionamento opcional, plantado FORA do caminho
/// mais curto.
///
/// Existe para dar ao treino a coisa que faltava: uma decisao. O percurso em si
/// tem uma estrategia so - correr na linha mais curta na velocidade maxima - e
/// isso e pressao sem tensao. Com as azuis, o relogio deixa de ser so ameaca e
/// vira moeda: desviar custa uns quatro segundos e compra folego para a prova.
///
/// Diferente das amarelas, esta nao e sequencial: todas ficam ligadas o treino
/// inteiro, e cada uma vale uma vez so.
/// </summary>
public class AuraDeTreino : MonoBehaviour
{
    [Header("Aura")]
    public Transform aura;
    [Tooltip("Quanto a coluna engorda e afina, em fracao do tamanho")]
    public float respiracao = 0.11f;
    [Tooltip("Ciclos por segundo")]
    public float ritmo = 0.5f;
    [Tooltip("Defasagem para duas vizinhas nao pulsarem juntas")]
    public float fase;

    bool colhida;

    void Awake()
    {
        if (aura == null) aura = transform.Find("Aura");
    }

    void Update()
    {
        if (colhida || aura == null) return;

        float onda = Mathf.Sin(Time.time * ritmo * 2f * Mathf.PI + fase);
        float s = 1f + onda * respiracao;
        aura.localScale = new Vector3(s, 1f + onda * 0.06f, s);
    }

    void OnTriggerEnter(Collider other)
    {
        if (colhida) return;
        if (other.GetComponent<ThirdPersonController>() == null) return;

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.Fase != FaseJogo.CorridaRua) return;

        colhida = true;
        gm.RegistrarTreino();
        AudioDirector.Tocar(true);
        gameObject.SetActive(false);
    }
}
