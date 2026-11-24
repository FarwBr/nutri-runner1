using UnityEngine;

/// <summary>
/// Coloca um NPC parado num estado do Animator logo no inicio.
///
/// Usar Play() com o nome do estado evita criar parametro so para isso: o
/// controlador dos NPCs ja tem os estados prontos, e cada personagem escolhe
/// o seu na hora de montar a cena.
/// </summary>
public class AnimadorNpc : MonoBehaviour
{
    public Animator animator;

    [Tooltip("Nome do estado no controlador. Vazio = fica no idle padrao.")]
    public string estado = "";

    [Tooltip("Defasagem para dois NPCs lado a lado nao respirarem em sincronia")]
    public float defasagem;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null) return;

        if (!string.IsNullOrEmpty(estado)) animator.Play(estado, 0, defasagem);
        else animator.Play("Locomocao", 0, defasagem);
    }
}
