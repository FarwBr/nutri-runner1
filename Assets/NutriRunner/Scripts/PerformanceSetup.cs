using UnityEngine;

/// <summary>
/// Desliga tudo que custa GPU. Roda em Awake, entao vale tambem na build.
/// Requisito da disciplina: maquinas sem GPU dedicada.
/// </summary>
public class PerformanceSetup : MonoBehaviour
{
    public int fpsAlvo = 60;

    void Awake()
    {
        Application.targetFrameRate = fpsAlvo;
        QualitySettings.vSyncCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.billboardsFaceCameraPosition = false;
    }
}
