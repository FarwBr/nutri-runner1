using UnityEngine;

/// <summary>
/// Animacao procedural do Alex: braços e pernas balançam conforme a velocidade,
/// mais um leve sobe-desce de passada e uma respiracao quando parado.
/// Nao usa Animator nem clipes: custo praticamente zero.
/// </summary>
public class AnimadorSimples : MonoBehaviour
{
    [Header("Pivos (criados pelo construtor de cena)")]
    public Transform bracoE;
    public Transform bracoD;
    public Transform pernaE;
    public Transform pernaD;
    public Transform corpo;

    [Header("Ajustes")]
    public float amplitudeBraco = 38f;
    public float amplitudePerna = 34f;
    public float velocidadeReferencia = 7f;
    public float alturaPassada = 0.06f;

    CharacterController cc;
    float fase;
    float alturaBaseCorpo;

    void Awake()
    {
        cc = GetComponentInParent<CharacterController>();
        if (corpo != null) alturaBaseCorpo = corpo.localPosition.y;
    }

    void Update()
    {
        float vel = 0f;
        if (cc != null)
        {
            Vector3 plano = cc.velocity;
            plano.y = 0f;
            vel = plano.magnitude;
        }

        float intensidade = Mathf.Clamp01(vel / velocidadeReferencia);

        // A fase anda mais rapido quanto mais rapido o Alex corre.
        fase += Time.deltaTime * (2f + vel * 1.9f);

        float onda = Mathf.Sin(fase);
        float angBraco = onda * amplitudeBraco * intensidade;
        float angPerna = onda * amplitudePerna * intensidade;

        Girar(bracoE, angBraco);
        Girar(bracoD, -angBraco);
        Girar(pernaE, -angPerna);
        Girar(pernaD, angPerna);

        if (corpo != null)
        {
            // sobe-desce a cada passada + respiracao leve quando parado
            float passada = Mathf.Abs(Mathf.Cos(fase)) * alturaPassada * intensidade;
            float respiro = Mathf.Sin(Time.time * 1.8f) * 0.012f * (1f - intensidade);
            Vector3 p = corpo.localPosition;
            p.y = alturaBaseCorpo + passada + respiro;
            corpo.localPosition = p;
        }
    }

    static void Girar(Transform t, float angulo)
    {
        if (t == null) return;
        t.localRotation = Quaternion.Euler(angulo, 0f, 0f);
    }
}
