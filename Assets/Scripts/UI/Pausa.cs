using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Pausa de la partida: congela el juego (Time.timeScale = 0) y el sonido (AudioListener.pause).
// Esc o P pausan y vuelven; tambien se pausa sola si el juego pierde el foco.
// Desde Opciones la partida sigue congelada y «Volver» regresa a la Pausa.
public class Pausa : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static int ResumedFrame { get; private set; } = -10;   // el pajaro ignora ese fotograma

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelOpciones;
    public Ajustes ajustes;
    public DialogoConfirmar dialogo;

    [Header("Foco")]
    public GameObject focoPausa;           // BotonContinuar
    public GameObject botonOpciones;       // al volver de Opciones el foco vuelve aqui
    public GameObject focoOpciones;        // SliderMusica

    void Start()
    {
        IsPaused = false;
        panelPausa.SetActive(false);
        panelOpciones.SetActive(false);
        ajustes.alVolver = CerrarOpciones;
    }

    void Update()
    {
        if (!(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))) return;

        if (DialogoConfirmar.Abierto) dialogo.Cancelar();         // Esc = Cancelar
        else if (panelOpciones.activeSelf) CerrarOpciones();       // Esc = Volver
        else if (IsPaused) Continuar();
        else Pausar();
    }

    void OnApplicationFocus(bool tieneFoco)
    {
        var gc = GameController.instance;
        if (!tieneFoco && gc != null && gc.started && !gc.gameOver) Pausar();
    }

    // Boton de pausa del HUD, Esc o P
    public void Pausar()
    {
        var gc = GameController.instance;
        if (IsPaused || gc == null || gc.gameOver) return;
        IsPaused = true;
        Time.timeScale = 0f;            // se congela la fisica, las columnas y los coleccionables
        AudioListener.pause = true;     // y el sonido
        panelPausa.SetActive(true);
        Seleccionar(focoPausa);
    }

    // Boton «Continuar», «Volver» de la pausa, Esc o P
    public void Continuar()
    {
        if (!IsPaused) return;
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        panelPausa.SetActive(false);
        panelOpciones.SetActive(false);
        ResumedFrame = Time.frameCount;   // el Espacio o el clic que ha cerrado la pausa no aletea
        Seleccionar(null);                // si un boton se quedara con el foco, el Espacio lo volveria a pulsar
    }

    public void AbrirOpciones()
    {
        panelPausa.SetActive(false);
        panelOpciones.SetActive(true);
        Seleccionar(focoOpciones);
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
        panelPausa.SetActive(true);
        Seleccionar(botonOpciones);
    }

    // «Reiniciar» y «Menú» pierden la partida: confirman con el foco en Cancelar
    public void PedirReiniciar()
    {
        dialogo.Mostrar("¿REINICIAR LA PARTIDA?", "Perderás la partida.", "REINICIAR", Reiniciar);
    }

    public void PedirMenu()
    {
        dialogo.Mostrar("¿VOLVER AL MENÚ?", "Perderás la partida.", "MENÚ", IrAlMenu);
    }

    void Reiniciar()
    {
        Descongelar();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void IrAlMenu()
    {
        Descongelar();
        SceneManager.LoadScene("Menu");
    }

    static void Descongelar()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnDestroy()
    {
        // Al cambiar de escena, nunca dejes el juego congelado
        Descongelar();
    }

    static void Seleccionar(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        if (go != null) EventSystem.current.SetSelectedGameObject(go);
    }
}
