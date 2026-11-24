using UnityEngine;

/// <summary>
/// Comida ou bebida no cenario. Gira e flutua (so a malha filha;
/// o collider fica parado para a coleta ser confiavel).
/// </summary>
public class Collectible : MonoBehaviour
{
    public TipoItem tipo = TipoItem.Fruta;
    public float giroPorSegundo = 110f;
    public float alturaFlutuacao = 0.12f;

    Transform malha;
    Vector3 posBase;
    float faseInicial;

    void Awake()
    {
        malha = transform.Find("Malha");
        if (malha == null) malha = transform;
        posBase = malha.localPosition;
        faseInicial = Random.Range(0f, 6.28f);
    }

    void Update()
    {
        malha.Rotate(0f, giroPorSegundo * Time.deltaTime, 0f, Space.Self);
        float y = Mathf.Sin(Time.time * 2.5f + faseInicial) * alturaFlutuacao;
        malha.localPosition = posBase + new Vector3(0f, y, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThirdPersonController>() == null) return;
        Coletar();
    }

    public void Coletar()
    {
        if (GameManager.Instance != null) GameManager.Instance.Coletar(tipo);

        bool ruim = (tipo == TipoItem.Doce || tipo == TipoItem.Fritura);
        AudioDirector.Tocar(ruim);

        gameObject.SetActive(false);
    }
}
