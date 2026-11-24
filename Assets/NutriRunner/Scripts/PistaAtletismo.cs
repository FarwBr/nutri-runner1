using UnityEngine;

/// <summary>
/// A geometria da pista de atletismo, e a unica fonte de verdade sobre ela.
/// Quem precisa saber "estou na pista?" ou "o quanto eu ja corri?" pergunta aqui.
///
/// A pista NAO e uma elipse: e uma CAPSULA (duas retas paralelas fechadas por dois
/// semicirculos), que e a forma real de uma pista de 400 m. Isso muda tudo para o
/// melhor, porque a capsula tem uma propriedade que a elipse nao tem:
///
///   a distancia de um ponto ate o EIXO da reta central diz, sozinha, se o ponto
///   esta na pista -- e so comparar com o raio interno e o externo.
///
/// Com a elipse era preciso normalizar cada eixo pelo seu raio e ainda assim o
/// resultado era aproximado. Aqui o teste e exato e custa quatro multiplicacoes.
///
/// O progresso tambem fica honesto. O codigo antigo media ANGULO em volta do centro,
/// e angulo tem um furo: quem passa rente ao centro varre 180 graus andando quase
/// nada. Aqui o progresso e o COMPRIMENTO DE ARCO ao longo do circuito, em metros.
/// Nao existe atalho que renda metros que voce nao correu.
/// </summary>
public class PistaAtletismo : MonoBehaviour
{
    [Header("Capsula (medidas em metros, no mundo)")]
    [Tooltip("Meia-reta: metade do comprimento do trecho reto")]
    public float reta = 21.1f;
    [Tooltip("Do eixo da reta ate o meio-fio interno")]
    public float raioInterno = 18.25f;
    [Tooltip("Do eixo da reta ate a borda externa do asfalto")]
    public float raioExterno = 23.13f;
    [Tooltip("Raia de medicao: e nela que a volta tem o comprimento oficial")]
    public float raioMedida = 18.4f;

    [Header("Centro da pista no mundo")]
    public Vector3 centro;

    [Header("Tolerancia do juiz")]
    [Tooltip("Folga em metros para dentro e para fora antes de considerar fora da pista")]
    public float folga = 0.4f;
    [Tooltip("Quanto o corredor pode estar acima ou abaixo do piso e ainda contar")]
    public float folgaAltura = 3.5f;

    /// Comprimento de uma volta na raia de medicao, em metros.
    public float Perimetro
    {
        get { return 4f * reta + 2f * Mathf.PI * raioMedida; }
    }

    /// Comprimento de uma volta a uma distancia qualquer do eixo. Raia de fora e
    /// mais longa -- e por isso que numa prova de verdade a largada e escalonada.
    public float PerimetroDaRaia(float raio)
    {
        return 4f * reta + 2f * Mathf.PI * raio;
    }

    // ---------------- esta na pista? ----------------

    /// Distancia do ponto ate o eixo da reta central, no plano XZ.
    /// Esse unico numero descreve a posicao transversal na pista inteira.
    public float DistanciaDoEixo(Vector3 mundo)
    {
        Vector3 p = mundo - centro;
        float sobra = Mathf.Abs(p.x) - reta;
        if (sobra <= 0f) return Mathf.Abs(p.z);        // ao lado de uma das retas
        return Mathf.Sqrt(sobra * sobra + p.z * p.z);  // na regiao de uma das curvas
    }

    /// True quando o ponto esta sobre a faixa de asfalto.
    public bool NaPista(Vector3 mundo)
    {
        if (Mathf.Abs(mundo.y - centro.y) > folgaAltura) return false;
        float d = DistanciaDoEixo(mundo);
        return d >= raioInterno - folga && d <= raioExterno + folga;
    }

    /// Negativo = cortou para dentro (pelo gramado). Positivo = saiu pela borda de
    /// fora. Zero = esta na pista. Serve para a mensagem dizer qual foi o erro.
    public float QuantoFora(Vector3 mundo)
    {
        float d = DistanciaDoEixo(mundo);
        if (d < raioInterno - folga) return d - (raioInterno - folga);
        if (d > raioExterno + folga) return d - (raioExterno + folga);
        return 0f;
    }

    // ---------------- onde estou na volta? ----------------

    /// Posicao ao longo do circuito, normalizada em [0,1).
    ///
    /// O percurso e dividido em quatro trechos, no sentido da prova: reta sul (para
    /// +X), curva leste, reta norte (para -X), curva oeste. A conta e continua nas
    /// quatro costuras -- conferido nos quatro pontos de emenda.
    ///
    /// Vale para qualquer ponto, inclusive fora da pista: o valor existe, so nao e
    /// contabilizado enquanto o corredor estiver fora.
    public float Parametro(Vector3 mundo)
    {
        Vector3 p = mundo - centro;
        float L = reta, R = raioMedida;
        float arco = Mathf.PI * R;
        float s;

        if (p.x > L)
        {
            // curva leste: angulo de -90 (sul) a +90 (norte), passando pelo leste
            float ang = Mathf.Atan2(p.z, p.x - L);
            s = 2f * L + R * (ang + Mathf.PI * 0.5f);
        }
        else if (p.x < -L)
        {
            // curva oeste: 90 -> 270 graus. O atan2 devolve negativo na metade sul,
            // entao somamos uma volta para a contagem seguir crescendo.
            float ang = Mathf.Atan2(p.z, p.x + L);
            if (ang < 0f) ang += Mathf.PI * 2f;
            s = 4f * L + arco + R * (ang - Mathf.PI * 0.5f);
        }
        else if (p.z < 0f) s = p.x + L;                       // reta sul
        else s = 2f * L + arco + (L - p.x);                   // reta norte

        return Mathf.Repeat(s / Perimetro, 1f);
    }

    /// Volta atras: do parametro para um ponto do mundo, a uma distancia "raio"
    /// do eixo. E assim que cada rival e colocado na sua raia.
    public Vector3 Ponto(float t, float raio)
    {
        float L = reta, R = raioMedida;
        float arco = Mathf.PI * R;
        float s = Mathf.Repeat(t, 1f) * Perimetro;
        float x, z;

        if (s <= 2f * L) { x = s - L; z = -raio; }
        else if (s <= 2f * L + arco)
        {
            float ang = (s - 2f * L) / R - Mathf.PI * 0.5f;
            x = L + Mathf.Cos(ang) * raio;
            z = Mathf.Sin(ang) * raio;
        }
        else if (s <= 4f * L + arco) { x = L - (s - 2f * L - arco); z = raio; }
        else
        {
            float ang = (s - 4f * L - arco) / R + Mathf.PI * 0.5f;
            x = -L + Mathf.Cos(ang) * raio;
            z = Mathf.Sin(ang) * raio;
        }

        return centro + new Vector3(x, 0f, z);
    }

    /// Para onde o percurso segue nesse ponto. Ja normalizada.
    public Vector3 Tangente(float t)
    {
        float L = reta, R = raioMedida;
        float arco = Mathf.PI * R;
        float s = Mathf.Repeat(t, 1f) * Perimetro;

        if (s <= 2f * L) return Vector3.right;
        if (s <= 4f * L + arco && s > 2f * L + arco) return Vector3.left;

        // nas duas curvas a tangente e perpendicular ao raio
        float ang = (s <= 2f * L + arco)
                  ? (s - 2f * L) / R - Mathf.PI * 0.5f
                  : (s - 4f * L - arco) / R + Mathf.PI * 0.5f;
        return new Vector3(-Mathf.Sin(ang), 0f, Mathf.Cos(ang));
    }

    /// Diferenca entre dois parametros tratando a virada de 1 para 0, como o
    /// DeltaAngle faz com graus. Positivo = andou no sentido da prova.
    public static float Delta(float de, float para)
    {
        float d = para - de;
        if (d > 0.5f) d -= 1f;
        else if (d < -0.5f) d += 1f;
        return d;
    }

    /// Desenha a pista no Editor, para dar para conferir a geometria sem jogar.
    void OnDrawGizmosSelected()
    {
        Contorno(raioInterno, Color.cyan);
        Contorno(raioExterno, Color.cyan);
        Contorno(raioMedida, Color.yellow);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(centro + Vector3.left * reta, centro + Vector3.right * reta);
    }

    void Contorno(float raio, Color cor)
    {
        Gizmos.color = cor;
        const int N = 96;
        Vector3 ant = Ponto(0f, raio);
        for (int i = 1; i <= N; i++)
        {
            Vector3 p = Ponto(i / (float)N, raio);
            Gizmos.DrawLine(ant, p);
            ant = p;
        }
    }
}
