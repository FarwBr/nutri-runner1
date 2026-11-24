using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Camada unica de entrada. Funciona tanto com o Input Manager antigo quanto com o
/// Input System novo, sem precisar mexer em Project Settings.
/// O bloco certo e escolhido na compilacao pelos simbolos que o Unity define.
/// </summary>
public static class Entrada
{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

    // ---------- Input System novo ----------
    const float ESCALA_MOUSE = 0.05f; // aproxima o delta em pixels do antigo GetAxis

    static Keyboard Tec { get { return Keyboard.current; } }
    static Mouse Rato { get { return Mouse.current; } }

    static float Eixo(bool positivo, bool negativo)
    {
        return (positivo ? 1f : 0f) - (negativo ? 1f : 0f);
    }

    public static float Horizontal
    {
        get
        {
            if (Tec == null) return 0f;
            return Eixo(Tec.dKey.isPressed || Tec.rightArrowKey.isPressed,
                        Tec.aKey.isPressed || Tec.leftArrowKey.isPressed);
        }
    }

    public static float Vertical
    {
        get
        {
            if (Tec == null) return 0f;
            return Eixo(Tec.wKey.isPressed || Tec.upArrowKey.isPressed,
                        Tec.sKey.isPressed || Tec.downArrowKey.isPressed);
        }
    }

    public static bool Correndo
    {
        get { return Tec != null && (Tec.leftShiftKey.isPressed || Tec.rightShiftKey.isPressed); }
    }

    public static bool PulouAgora
    {
        get { return Tec != null && Tec.spaceKey.wasPressedThisFrame; }
    }

    public static bool EscAgora
    {
        get { return Tec != null && Tec.escapeKey.wasPressedThisFrame; }
    }

    public static bool ReiniciarAgora
    {
        get { return Tec != null && Tec.rKey.wasPressedThisFrame; }
    }

    public static bool CliqueAgora
    {
        get { return Rato != null && Rato.leftButton.wasPressedThisFrame; }
    }

    public static bool Interagindo
    {
        get { return Tec != null && Tec.eKey.isPressed; }
    }

    public static float MouseX
    {
        get { return Rato == null ? 0f : Rato.delta.ReadValue().x * ESCALA_MOUSE; }
    }

    public static float MouseY
    {
        get { return Rato == null ? 0f : Rato.delta.ReadValue().y * ESCALA_MOUSE; }
    }

    public static float Scroll
    {
        get { return Rato == null ? 0f : Rato.scroll.ReadValue().y / 120f; }
    }

#else

    // ---------- Input Manager antigo ----------
    public static float Horizontal { get { return Input.GetAxisRaw("Horizontal"); } }
    public static float Vertical { get { return Input.GetAxisRaw("Vertical"); } }

    public static bool Correndo
    {
        get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }
    }

    public static bool PulouAgora { get { return Input.GetKeyDown(KeyCode.Space); } }
    public static bool EscAgora { get { return Input.GetKeyDown(KeyCode.Escape); } }
    public static bool ReiniciarAgora { get { return Input.GetKeyDown(KeyCode.R); } }
    public static bool CliqueAgora { get { return Input.GetMouseButtonDown(0); } }

    public static bool Interagindo { get { return Input.GetKey(KeyCode.E); } }

    public static float MouseX { get { return Input.GetAxis("Mouse X"); } }
    public static float MouseY { get { return Input.GetAxis("Mouse Y"); } }
    public static float Scroll { get { return Input.GetAxis("Mouse ScrollWheel"); } }

#endif
}
