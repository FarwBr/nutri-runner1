using UnityEngine;

/// <summary>
/// Liga o movimento real do CharacterController ao Animator do personagem.
/// Substitui o AnimadorSimples quando existe um modelo com esqueleto na cena.
///
/// Parametros esperados no controlador:
///   Velocidade (float) - metros por segundo no plano
///   NoChao     (bool)  - se os pes estao no chao
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AnimadorPersonagem : MonoBehaviour
{
    public Animator animator;

    [Tooltip("Suaviza a troca entre parado, andando e correndo")]
    public float suavidade = 9f;

    [Header("Passos")]
    [Tooltip("Metros entre uma pisada e a seguinte")]
    public float passada = 1.75f;

    CharacterController cc;
    float velocidade;
    int idVelocidade, idNoChao;
    float desdeOUltimoPasso;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        idVelocidade = Animator.StringToHash("Velocidade");
        idNoChao = Animator.StringToHash("NoChao");
    }

    void Update()
    {
        if (animator == null) return;

        Vector3 plano = cc.velocity;
        plano.y = 0f;

        velocidade = Mathf.Lerp(velocidade, plano.magnitude, Time.deltaTime * suavidade);

        animator.SetFloat(idVelocidade, velocidade);
        animator.SetBool(idNoChao, cc.isGrounded);

        Passos(plano.magnitude);
    }

    /// O som de passo sai por DISTANCIA andada, nao por tempo: assim ele
    /// acompanha sozinho a troca de andar para correr, sem evento de animacao.
    void Passos(float vel)
    {
        if (!cc.isGrounded || vel < 0.4f)
        {
            desdeOUltimoPasso = passada * 0.6f;   // o proximo passo sai logo
            return;
        }

        desdeOUltimoPasso += vel * Time.deltaTime;
        if (desdeOUltimoPasso < passada) return;

        desdeOUltimoPasso = 0f;
        AudioDirector.Passo(Mathf.InverseLerp(2f, 8f, vel));
    }
}
