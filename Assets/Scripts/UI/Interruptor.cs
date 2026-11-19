using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Aspecto de interruptor (switch) para un Toggle: la perilla cambia de lado, la pista de color
// y el texto dice SÍ o NO, asi el estado se ve sin depender solo del color.
[RequireComponent(typeof(Toggle))]
public class Interruptor : MonoBehaviour
{
    public Image pista;
    public RectTransform perilla;
    public TMP_Text textoEstado;
    public Color colorEncendido = new Color(1f, 0.18f, 0.53f);
    public Color colorApagado = new Color(0.23f, 0.17f, 0.29f);
    public float recorrido = 28f;

    private Toggle toggle;
    private int ultimo = -1;

    void Awake() { toggle = GetComponent<Toggle>(); }
    void OnEnable() { ultimo = -1; }

    void LateUpdate()
    {
        int actual = toggle.isOn ? 1 : 0;
        if (actual == ultimo) return;
        ultimo = actual;
        pista.color = toggle.isOn ? colorEncendido : colorApagado;
        perilla.anchoredPosition = new Vector2(toggle.isOn ? recorrido : -recorrido, 0f);
        if (textoEstado != null) textoEstado.text = toggle.isOn ? "SÍ" : "NO";
    }
}
