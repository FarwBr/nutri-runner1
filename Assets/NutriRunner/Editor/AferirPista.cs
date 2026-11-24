using UnityEngine;
using UnityEditor;

/// <summary>
/// Testes da matematica da pista, rodando dentro da Unity.
///
/// Por que isto existe: a geometria da pista e o unico pedaco do projeto onde um
/// erro sutil e INVISIVEL. Nada quebra, nada fica rosa, nenhuma excecao aparece no
/// Console - o jogador so sente que a prova esta estranha, e nao sabe dizer por que.
/// Foi exatamente o que aconteceu com a versao antiga, que media angulo: o furo
/// ficou ali por semanas porque nada o media.
///
/// Rode por Tools -> Nutri Runner -> Aferir Matematica da Pista. Nao precisa estar
/// jogando, e nao mexe na cena.
/// </summary>
public static class AferirPista
{
    static int passou, falhou;

    [MenuItem("Tools/Nutri Runner/Aferir Matematica da Pista", false, 40)]
    public static void Rodar()
    {
        passou = 0;
        falhou = 0;

        PistaAtletismo pista = Object.FindObjectOfType<PistaAtletismo>();
        GameObject temporario = null;

        if (pista == null)
        {
            // Sem cena construida, testa com os valores padrao do componente.
            temporario = new GameObject("PistaTemporaria");
            pista = temporario.AddComponent<PistaAtletismo>();
            Debug.Log("Nenhuma pista na cena: testando com os valores padrao do componente.");
        }

        Debug.Log("=== Aferindo a pista: reta de " + (pista.reta * 2f).ToString("F3")
                + " m, faixa de " + pista.raioInterno.ToString("F2") + " a "
                + pista.raioExterno.ToString("F2") + " m do eixo ===");

        Perimetro(pista);
        IdaEVolta(pista);
        ComprimentoDeArco(pista);
        Monotonia(pista);
        Costuras(pista);
        Deteccao(pista);
        Tangentes(pista);
        Atalho(pista);

        if (temporario != null) Object.DestroyImmediate(temporario);

        string resumo = "=== " + passou + " teste(s) passaram, " + falhou + " falharam ===";
        if (falhou == 0) Debug.Log(resumo + "  A pista esta coerente.");
        else Debug.LogError(resumo + "  A prova vai medir errado. Nao apresente assim.");
    }

    // ---------------- os testes ----------------

    static void Perimetro(PistaAtletismo p)
    {
        float esperado = 4f * p.reta + 2f * Mathf.PI * p.raioMedida;
        Conferir("Perimetro bate com a formula", p.Perimetro, esperado, 0.001f);

        // a raia de fora e mais longa que a de dentro, sempre
        Afirmar("Raia externa e mais longa que a interna",
                p.PerimetroDaRaia(p.raioExterno) > p.PerimetroDaRaia(p.raioInterno));

        Debug.Log("  volta na raia de medicao: " + p.Perimetro.ToString("F3") + " m"
                + "   |   raia 1: " + p.PerimetroDaRaia(p.raioInterno + 0.3f).ToString("F2")
                + " m   |   borda externa: "
                + p.PerimetroDaRaia(p.raioExterno).ToString("F2") + " m");
    }

    /// Ponto(Parametro(p)) tem que devolver o mesmo p. Se nao devolver, uma das
    /// duas contas esta errada e o progresso nao vai bater com a posicao.
    static void IdaEVolta(PistaAtletismo p)
    {
        float[] raios = { p.raioInterno, p.raioMedida, p.raioExterno,
                          (p.raioInterno + p.raioExterno) * 0.5f };
        float pior = 0f;

        for (int i = 0; i < 1200; i++)
        {
            float t = i / 1200f;
            for (int r = 0; r < raios.Length; r++)
            {
                Vector3 mundo = p.Ponto(t, raios[r]);
                float volta = p.Parametro(mundo);
                float d = Mathf.Abs(PistaAtletismo.Delta(t, volta)) * p.Perimetro;
                if (d > pior) pior = d;
            }
        }

        Afirmar("Ida e volta fecha (erro de " + pior.ToString("F4") + " m)", pior < 0.02f);
    }

    /// Andar pela curva que o Ponto descreve tem que dar o mesmo numero que a
    /// formula do perimetro. E isso que garante que "metro" significa metro.
    static void ComprimentoDeArco(PistaAtletismo p)
    {
        float[] raios = { p.raioMedida, p.raioInterno, p.raioExterno };
        const int N = 4000;

        for (int r = 0; r < raios.Length; r++)
        {
            float raio = raios[r];
            float andado = 0f;
            Vector3 ant = p.Ponto(0f, raio);
            for (int i = 1; i <= N; i++)
            {
                Vector3 agora = p.Ponto(i / (float)N, raio);
                andado += Vector3.Distance(ant, agora);
                ant = agora;
            }
            Conferir("Arco medido = formula, raio " + raio.ToString("F2"),
                     andado, p.PerimetroDaRaia(raio), 0.05f);
        }
    }

    /// O progresso nunca pode voltar atras nem dar pulo. Um pulo aqui seria um
    /// atalho de graca; um passo negativo faria o jogador perder volta correndo certo.
    static void Monotonia(PistaAtletismo p)
    {
        const int N = 20000;
        int negativos = 0;
        float maiorPasso = 0f;
        // soma em double de proposito: 20 mil parcelas de 5e-5 em float acumulam
        // erro da ordem de 2e-3, o que reprovaria o teste sem haver bug nenhum
        double soma = 0.0;
        float ant = p.Parametro(p.Ponto(0f, p.raioMedida));

        for (int i = 1; i <= N; i++)
        {
            float t = p.Parametro(p.Ponto(i / (float)N, p.raioMedida));
            float d = PistaAtletismo.Delta(ant, t);
            if (d < 0f) negativos++;
            soma += d;
            maiorPasso = Mathf.Max(maiorPasso, Mathf.Abs(d) * p.Perimetro);
            ant = t;
        }

        Afirmar("Progresso nunca anda para tras (" + negativos + " passos negativos)",
                negativos == 0);
        Conferir("Uma volta completa soma 1", (float)soma, 1f, 0.001f);

        float esperado = p.Perimetro / N;
        Afirmar("Nenhum pulo de progresso (maior passo " + maiorPasso.ToString("F4")
                + " m, esperado " + esperado.ToString("F4") + " m)",
                maiorPasso < esperado * 3f);
    }

    /// A pista tem quatro emendas: onde cada reta encontra cada curva. A conta do
    /// parametro muda de ramo ali, e um erro de sinal daria um salto invisivel.
    static void Costuras(PistaAtletismo p)
    {
        float L = p.reta, R = p.raioMedida;
        const float e = 0.002f;

        Vector3[] pontos = {
            p.centro + new Vector3(-L, 0f, -R), p.centro + new Vector3(L, 0f, -R),
            p.centro + new Vector3(L, 0f,  R),  p.centro + new Vector3(-L, 0f,  R)
        };
        string[] nomes = { "oeste/sul", "sul/leste", "leste/norte", "norte/oeste" };

        for (int i = 0; i < 4; i++)
        {
            float t0 = p.Parametro(pontos[i] + Vector3.left * e);
            float t1 = p.Parametro(pontos[i] + Vector3.right * e);
            float salto = Mathf.Abs(PistaAtletismo.Delta(t0, t1)) * p.Perimetro;
            Afirmar("Costura " + nomes[i] + " e continua (" + salto.ToString("F4") + " m)",
                    salto < 0.05f);
        }
    }

    /// O teste que o jogo usa para dizer "voce esta fora da pista".
    static void Deteccao(PistaAtletismo p)
    {
        float meio = (p.raioInterno + p.raioExterno) * 0.5f;
        float L = p.reta;

        Caso(p, "centro do gramado", new Vector3(0f, 0f, 0f), false);
        Caso(p, "meio do gramado", new Vector3(0f, 0f, p.raioInterno * 0.5f), false);
        Caso(p, "sobre o meio-fio", new Vector3(0f, 0f, p.raioInterno), true);
        Caso(p, "meio da pista", new Vector3(0f, 0f, meio), true);
        Caso(p, "borda externa", new Vector3(0f, 0f, p.raioExterno), true);
        Caso(p, "no apron, fora", new Vector3(0f, 0f, p.raioExterno + 2f), false);
        Caso(p, "curva leste, na pista", new Vector3(L + meio, 0f, 0f), true);
        Caso(p, "curva leste, no gramado",
             new Vector3(L + p.raioInterno * 0.5f, 0f, 0f), false);
        Caso(p, "diagonal pelo miolo", new Vector3(L * 0.5f, 0f, meio * 0.3f), false);
        Caso(p, "voando acima da pista",
             new Vector3(0f, p.folgaAltura + 2f, meio), false);

        // o sinal de QuantoFora tem que dizer de que lado ele saiu
        Afirmar("QuantoFora negativo para dentro",
                p.QuantoFora(p.centro) < 0f);
        Afirmar("QuantoFora positivo para fora",
                p.QuantoFora(p.centro + new Vector3(0f, 0f, p.raioExterno + 5f)) > 0f);
        Afirmar("QuantoFora zero na pista",
                Mathf.Approximately(0f, p.QuantoFora(p.centro + new Vector3(0f, 0f, meio))));
    }

    static void Caso(PistaAtletismo p, string nome, Vector3 local, bool esperado)
    {
        bool obtido = p.NaPista(p.centro + local);
        Afirmar("NaPista: " + nome + " = " + (esperado ? "sim" : "nao"), obtido == esperado);
    }

    /// A tangente orienta os rivais. Se ela nao for a derivada do Ponto, eles
    /// correm de lado ou de costas nas curvas.
    static void Tangentes(PistaAtletismo p)
    {
        float pior = 0f;
        const float h = 0.0002f;

        for (int i = 0; i < 1200; i++)
        {
            float t = i / 1200f;
            Vector3 a = p.Ponto(t, p.raioMedida);
            Vector3 b = p.Ponto(t + h, p.raioMedida);
            Vector3 numerica = (b - a);
            if (numerica.sqrMagnitude < 1e-10f) continue;
            numerica.Normalize();

            pior = Mathf.Max(pior, Vector3.Distance(numerica, p.Tangente(t).normalized));
        }

        Afirmar("Tangente bate com a derivada (erro " + pior.ToString("F4") + ")",
                pior < 0.02f);
    }

    /// Nao e um teste: e o relatorio de POR QUE o juiz de pista existe. Mostra
    /// quanto progresso um corte pelo miolo renderia se o progresso nao congelasse.
    static void Atalho(PistaAtletismo p)
    {
        Vector3 sul = p.centro + new Vector3(0f, 0f, -p.raioMedida);
        Vector3 norte = p.centro + new Vector3(0f, 0f, p.raioMedida);

        float ganho = Mathf.Abs(PistaAtletismo.Delta(p.Parametro(sul), p.Parametro(norte)))
                    * p.Perimetro;
        float andado = Vector3.Distance(sul, norte);

        Debug.Log("  Relatorio do atalho: cortar reto de uma reta a outra atravessa "
                + andado.ToString("F1") + " m e cruzaria " + ganho.ToString("F1")
                + " m de parametro. E por isso que o progresso CONGELA fora da pista:"
                + " sem isso, esse corte valeria " + (ganho / andado).ToString("F1")
                + "x o que foi corrido.");

        Afirmar("O atalho esta fora da pista nos dois pontos de corte",
                !p.NaPista(p.centro) && !p.NaPista(p.centro + new Vector3(0f, 0f, p.raioMedida * 0.4f)));
    }

    // ---------------- utilidades ----------------

    static void Afirmar(string nome, bool condicao)
    {
        if (condicao) { passou++; Debug.Log("  ok   " + nome); }
        else { falhou++; Debug.LogError("  FALHOU   " + nome); }
    }

    static void Conferir(string nome, float obtido, float esperado, float tolerancia)
    {
        bool ok = Mathf.Abs(obtido - esperado) <= tolerancia;
        if (ok) { passou++; Debug.Log("  ok   " + nome + " (" + obtido.ToString("F4") + ")"); }
        else
        {
            falhou++;
            Debug.LogError("  FALHOU   " + nome + ": obtido " + obtido.ToString("F4")
                         + ", esperado " + esperado.ToString("F4"));
        }
    }
}
