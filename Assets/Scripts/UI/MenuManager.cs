using UnityEngine;
using UnityEngine.EventSystems;

// Escena Menu: cambia de pantalla activando un panel y desactivando los demas.
// Esc = Volver en cualquier panel; en Inicio (la raiz) abre «¿Salir del juego?».
public class MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelInicio;
    public GameObject panelSeleccion;
    public GameObject panelOpciones;
    public GameObject panelCreditos;

    [Header("Foco al abrir cada panel")]
    public GameObject focoInicio;        // BotonJugar
    public GameObject focoSeleccion;     // BotonALaPista
    public GameObject focoOpciones;      // SliderMusica
    public GameObject focoCreditos;      // BotonVolverCreditos

    [Header("Otros")]
    public Ajustes ajustes;
    public DialogoConfirmar dialogo;

    public static bool abrirSeleccion = false;   // lo pone «Cambiar personaje» del fin de partida

    private GameObject panelActual;
    private GameObject botonOrigen;              // para devolver el foco al boton que abrio el panel

    void Start()
    {
        Time.timeScale = 1f;
        MusicPlayer.ResetPitch();
        ajustes.alVolver = () => Mostrar(panelInicio);   // Opciones vuelve a Inicio

        if (abrirSeleccion)
        {
            abrirSeleccion = false;
            Mostrar(panelSeleccion);
        }
        else
        {
            Mostrar(panelInicio);
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (DialogoConfirmar.Abierto) dialogo.Cancelar();        // Esc = Cancelar
        else if (panelActual == panelInicio) Salir();             // raiz: pregunta si salir
        else Mostrar(panelInicio);                                // Esc = Volver
    }

    // Un solo metodo para todos los botones: el panel llega como parametro desde el Inspector
    public void Mostrar(GameObject panel)
    {
        if (panelActual == panelInicio && panel != panelInicio && EventSystem.current != null)
            botonOrigen = EventSystem.current.currentSelectedGameObject;

        panelInicio.SetActive(panel == panelInicio);
        panelSeleccion.SetActive(panel == panelSeleccion);
        panelOpciones.SetActive(panel == panelOpciones);
        panelCreditos.SetActive(panel == panelCreditos);
        panelActual = panel;

        GameObject foco;
        if (panel == panelInicio)
        {
            // Al volver, el foco regresa al boton desde el que se salio (si no, a Jugar)
            foco = botonOrigen != null && botonOrigen.transform.IsChildOf(panelInicio.transform) ? botonOrigen : focoInicio;
            botonOrigen = null;
        }
        else if (panel == panelSeleccion) foco = focoSeleccion;
        else if (panel == panelOpciones) foco = focoOpciones;
        else foco = focoCreditos;

        Seleccionar(foco);
    }

    // Boton «Salir»: es una accion peligrosa, asi que confirma con el foco en Cancelar
    public void Salir()
    {
        dialogo.Mostrar("¿SALIR DEL JUEGO?", "", "SALIR", ExitGame.Quit);
    }

    static void Seleccionar(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        if (go != null) EventSystem.current.SetSelectedGameObject(go);
    }
}
