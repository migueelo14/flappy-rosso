using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Segmento de la dificultad (Fácil / Normal / Difícil): el elegido lleva fondo Rosso,
// texto oscuro y un check, asi no depende solo del color.
[RequireComponent(typeof(Toggle))]
public class Segmento : MonoBehaviour
{
    public TMP_Text texto;
    public GameObject check;
    public Color colorTextoElegido = new Color(0.07f, 0.03f, 0.13f);
    public Color colorTextoNormal = new Color(1f, 0.96f, 0.97f);

    private Toggle toggle;
    private int ultimo = -1;

    void Awake() { toggle = GetComponent<Toggle>(); }
    void OnEnable() { ultimo = -1; }

    void LateUpdate()
    {
        int actual = toggle.isOn ? 1 : 0;
        if (actual == ultimo) return;
        ultimo = actual;
        texto.color = toggle.isOn ? colorTextoElegido : colorTextoNormal;
        if (check != null) check.SetActive(toggle.isOn);
    }
}
