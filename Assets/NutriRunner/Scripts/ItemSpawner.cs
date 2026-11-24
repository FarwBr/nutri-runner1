using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Espalha os alimentos pelo bairro.
/// Os SAUDAVEIS nascem uma unica vez e nao voltam: sao a meta da fase.
/// Nesta versao so existe comida saudavel no mapa: o unico vilao e a nuvem.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Modelos")]
    public GameObject[] modelosBons;

    [Header("Quantidade")]
    public int quantidadeBons = 10;

    [Header("Area")]
    public float raioArea = 38f;
    public float altura = 1.2f;
    public float espacoLivre = 0.9f;
    public float distanciaMinimaDoJogador = 14f;

    readonly List<GameObject> bons = new List<GameObject>();
    Transform jogador;

    void Start()
    {
        ThirdPersonController j = Object.FindObjectOfType<ThirdPersonController>();
        if (j != null) jogador = j.transform;

        for (int i = 0; i < quantidadeBons; i++) Nascer(modelosBons, bons, true);

        if (GameManager.Instance != null)
            GameManager.Instance.DefinirTotalItens(bons.Count);
    }

    /// Item saudavel ativo mais proximo, para o HUD apontar o caminho.
    public Transform MaisProximo(Vector3 de)
    {
        Transform melhor = null;
        float menor = float.MaxValue;

        for (int i = 0; i < bons.Count; i++)
        {
            GameObject go = bons[i];
            if (go == null || !go.activeSelf) continue;

            float d = (go.transform.position - de).sqrMagnitude;
            if (d < menor) { menor = d; melhor = go.transform; }
        }
        return melhor;
    }

    void Nascer(GameObject[] modelos, List<GameObject> lista, bool longeDoJogador)
    {
        if (modelos == null || modelos.Length == 0) return;

        GameObject modelo = modelos[Random.Range(0, modelos.Length)];
        if (modelo == null) return;

        Vector3 p;
        if (!Posicionar(out p, longeDoJogador))
            p = new Vector3(Random.Range(-20f, 20f), altura, Random.Range(-20f, 20f));

        GameObject go = Instantiate(modelo, p, modelo.transform.rotation, transform);
        go.SetActive(true);
        lista.Add(go);
    }

    bool Posicionar(out Vector3 posicao, bool longeDoJogador)
    {
        for (int tentativa = 0; tentativa < 30; tentativa++)
        {
            Vector3 p = new Vector3(Random.Range(-raioArea, raioArea),
                                    altura,
                                    Random.Range(-raioArea, raioArea));

            if (Physics.CheckSphere(p, espacoLivre, ~0, QueryTriggerInteraction.Ignore))
                continue;

            if (longeDoJogador && jogador != null &&
                Vector3.Distance(p, jogador.position) < distanciaMinimaDoJogador)
                continue;

            posicao = p;
            return true;
        }

        posicao = Vector3.zero;
        return false;
    }
}
