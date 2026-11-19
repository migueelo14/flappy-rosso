using UnityEngine;
using UnityEngine.UI;

// Creditos: con ↑ ↓ se desplaza el Scroll View sin tener que usar el raton.
public class DesplazarConTeclado : MonoBehaviour
{
    public ScrollRect scroll;
    public float velocidad = 700f;   // pixeles por segundo

    void OnEnable()
    {
        scroll.verticalNormalizedPosition = 1f;   // siempre empieza arriba
    }

    void Update()
    {
        float direccion = 0f;
        if (Input.GetKey(KeyCode.DownArrow)) direccion -= 1f;
        if (Input.GetKey(KeyCode.UpArrow)) direccion += 1f;
        if (direccion == 0f) return;

        float sobrante = scroll.content.rect.height - scroll.viewport.rect.height;
        if (sobrante <= 0f) return;
        scroll.verticalNormalizedPosition = Mathf.Clamp01(
            scroll.verticalNormalizedPosition + direccion * velocidad * Time.unscaledDeltaTime / sobrante);
    }
}
