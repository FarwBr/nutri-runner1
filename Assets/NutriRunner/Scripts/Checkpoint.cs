using UnityEngine;

/// <summary>
/// Ponto de passagem da corrida de rua: uma coluna de luz amarela para atravessar.
///
/// So UM fica ativo por vez. Como objeto desativado nao dispara trigger, isso ja
/// impede o jogador de cortar caminho pulando pontos - a regra sai de graca do
/// jeito que a cena e montada, sem nenhuma verificacao extra.
///
/// A aura respira e gira devagar. Nao e enfeite: uma coluna de luz parada se
/// confunde com cenario, e o jogador precisa reconhecer de longe o que e o
/// proximo alvo no meio de uma cidade cheia de postes e placas.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    public Corrida corrida;
    public int numero;

    [Header("Aura")]
    public Transform aura;
    [Tooltip("Quanto a coluna engorda e afina, em fracao do tamanho")]
    public float respiracao = 0.09f;
    [Tooltip("Quanto ela sobe e desce de altura, em fracao do tamanho")]
    public float pulsoDeAltura = 0.05f;
    [Tooltip("Ciclos por segundo")]
    public float ritmo = 0.35f;

    float fase;

    void Awake()
    {
        if (aura == null) aura = transform.Find("Aura");

        // Fase propria: dois pontos visiveis ao mesmo tempo pulsando em sincronia
        // parecem uma animacao so, e o olho para de ler como dois alvos.
        fase = numero * 1.7f;
    }

    void Update()
    {
        if (aura == null) return;

        // Girar nao adianta: um cilindro e simetrico no eixo Y e a rotacao nao
        // aparece. O que se ve e a coluna engordando e esticando.
        float onda = Mathf.Sin((Time.time * ritmo * 2f * Mathf.PI) + fase);
        aura.localScale = new Vector3(1f + onda * respiracao,
                                      1f + onda * pulsoDeAltura,
                                      1f + onda * respiracao);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() == null) return;
        if (corrida != null) corrida.Passou(this);
    }
}
