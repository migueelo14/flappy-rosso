using UnityEngine;

// Hace que este contenedor ocupe solo Screen.safeArea (muescas y esquinas redondeadas de los moviles).
// Los fondos y velos se quedan fuera para llegar hasta el borde. En PC la zona segura es toda la pantalla.
[RequireComponent(typeof(RectTransform))]
public class ZonaSegura : MonoBehaviour
{
    private Rect ultima;
    private Vector2Int ultimaPantalla;

    void Update()
    {
        var pantalla = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea != ultima || pantalla != ultimaPantalla)
            Aplicar(pantalla);
    }

    void Aplicar(Vector2Int pantalla)
    {
        ultima = Screen.safeArea;
        ultimaPantalla = pantalla;
        if (pantalla.x <= 0 || pantalla.y <= 0) return;

        Vector2 min = ultima.position;
        Vector2 max = ultima.position + ultima.size;
        min.x /= pantalla.x; min.y /= pantalla.y;
        max.x /= pantalla.x; max.y /= pantalla.y;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
