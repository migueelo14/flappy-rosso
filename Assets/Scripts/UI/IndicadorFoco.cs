using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Estado «Seleccionado» de la guia de estilo: borde cian de 7 px + escala 1,08 + flecha ▶.
// Asi el foco del teclado se distingue aunque no se vea el color.
// Tambien hace que el contenido baje 4 px al pulsar y se atenue si el control esta desactivado.
[RequireComponent(typeof(Selectable))]
public class IndicadorFoco : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject marcoFoco;            // ImagenFoco: borde cian de 7 px
    public GameObject flecha;               // ImagenFlechaFoco: ▶ a la izquierda del texto
    public RectTransform contenido;         // GrupoContenido: icono + texto
    public CanvasGroup grupoContenido;      // se atenua cuando el boton esta desactivado
    public bool escalar = true;
    public float escalaFoco = 1.08f;

    private Selectable selectable;
    private Vector2 posicionContenido;
    private bool pulsado;

    void Awake()
    {
        selectable = GetComponent<Selectable>();
        if (contenido != null) posicionContenido = contenido.anchoredPosition;
    }

    void OnEnable()
    {
        bool conFoco = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        Pintar(conFoco);
    }

    void OnDisable()
    {
        pulsado = false;
        Mover();
        Pintar(false);
    }

    void Update()
    {
        if (grupoContenido != null) grupoContenido.alpha = selectable.IsInteractable() ? 1f : 0.45f;
    }

    public void OnSelect(BaseEventData eventData) { Pintar(true); }
    public void OnDeselect(BaseEventData eventData) { Pintar(false); }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!selectable.IsInteractable()) return;
        pulsado = true;
        Mover();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pulsado = false;
        Mover();
    }

    void Pintar(bool conFoco)
    {
        if (marcoFoco != null) marcoFoco.SetActive(conFoco);
        if (flecha != null) flecha.SetActive(conFoco);
        if (escalar) transform.localScale = conFoco ? Vector3.one * escalaFoco : Vector3.one;
    }

    void Mover()
    {
        if (contenido != null) contenido.anchoredPosition = posicionContenido + (pulsado ? Vector2.down * 4f : Vector2.zero);
    }
}
