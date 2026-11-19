using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de Opciones (prefab PanelOpciones, el mismo en Menu y en Flappy).
// Cada cambio se oye o se ve al momento y se guarda en PlayerPrefs (no hay boton «Aplicar»).
public class Ajustes : MonoBehaviour
{
    [Header("Sonido")]
    public Slider sliderMusica;            // 0..20 (pasos de 5 %)
    public Slider sliderEfectos;
    public TMP_Text textoValorMusica;      // «80 %»
    public TMP_Text textoValorEfectos;
    public Toggle toggleSilencio;

    [Header("Juego")]
    public Toggle togglePantallaCompleta;
    public GameObject filaPantallaCompleta;   // en movil se oculta
    public Toggle[] togglesDificultad;         // Fácil, Normal, Difícil (Toggle Group)

    [Header("Confirmaciones y avisos")]
    public DialogoConfirmar dialogo;
    public GameObject cajaAviso;               // «Récord borrado», «Ajustes restablecidos...»
    public TMP_Text textoAviso;
    public AudioClip sonidoPrueba;             // al mover Efectos suena un aleteo de prueba

    // Volver regresa a la pantalla desde la que se abrio (Inicio o Pausa): lo decide quien abre el panel
    public System.Action alVolver;

    private float ultimaPrueba;

    void Awake()
    {
        if (dialogo == null) dialogo = FindFirstObjectByType<DialogoConfirmar>(FindObjectsInactive.Include);
        if (Application.isMobilePlatform) filaPantallaCompleta.SetActive(false);
    }

    void OnEnable()   // al abrir el panel, muestra lo guardado
    {
        cajaAviso.SetActive(false);
        MostrarValores();
        GameSettings.Changed += MostrarValores;   // por si se silencia con la M con el panel abierto
    }

    void OnDisable()
    {
        GameSettings.Changed -= MostrarValores;
        PlayerPrefs.Save();   // guarda en disco al cerrar el panel
    }

    void MostrarValores()
    {
        sliderMusica.SetValueWithoutNotify(Mathf.Round(GameSettings.MusicVolume * 20f));
        sliderEfectos.SetValueWithoutNotify(Mathf.Round(GameSettings.SfxVolume * 20f));
        textoValorMusica.text = Porcentaje(sliderMusica.value);
        textoValorEfectos.text = Porcentaje(sliderEfectos.value);
        toggleSilencio.SetIsOnWithoutNotify(GameSettings.Muted);
        togglePantallaCompleta.SetIsOnWithoutNotify(GameSettings.Fullscreen);
        // Al abrir el panel el Toggle Group aun no esta activo y no apaga a los demas: se ponen todos a mano
        int d = GameSettings.Difficulty;
        togglesDificultad[d].SetIsOnWithoutNotify(true);
        for (int i = 0; i < togglesDificultad.Length; i++)
            if (i != d) togglesDificultad[i].SetIsOnWithoutNotify(false);
        ActualizarDeslizadores();
    }

    // Con el sonido silenciado los deslizadores se atenuan (desactivados) pero recuerdan su valor
    void ActualizarDeslizadores()
    {
        bool activos = !GameSettings.Muted;
        sliderMusica.interactable = activos;
        sliderEfectos.interactable = activos;
    }

    static string Porcentaje(float pasos) { return Mathf.RoundToInt(pasos * 5f) + " %"; }

    // ---- On Value Changed (Dynamic)
    public void CambiarMusica(float pasos)
    {
        GameSettings.MusicVolume = pasos / 20f;
        textoValorMusica.text = Porcentaje(pasos);
    }

    public void CambiarEfectos(float pasos)
    {
        GameSettings.SfxVolume = pasos / 20f;
        textoValorEfectos.text = Porcentaje(pasos);
        if (Time.unscaledTime - ultimaPrueba > 0.2f)   // aleteo de prueba, sin saturar al arrastrar
        {
            ultimaPrueba = Time.unscaledTime;
            MusicPlayer.PlayOneShot(sonidoPrueba);
        }
    }

    public void CambiarSilencio(bool silencio)
    {
        GameSettings.Muted = silencio;
        ActualizarDeslizadores();
    }

    public void CambiarPantallaCompleta(bool completa)
    {
        GameSettings.Fullscreen = completa;
    }

    public void CambiarDificultad(bool activado)
    {
        if (!activado) return;   // el toggle que se apaga tambien avisa: lo ignoramos
        for (int i = 0; i < togglesDificultad.Length; i++)
            if (togglesDificultad[i].isOn) GameSettings.Difficulty = i;   // vale desde la proxima partida
    }

    // ---- Acciones peligrosas: confirman con el foco en Cancelar
    public void PedirRestablecer()
    {
        dialogo.Mostrar("¿RESTABLECER LOS AJUSTES?", "Tu récord no se toca.", "RESTABLECER", () =>
        {
            GameSettings.ResetSettings();
            MostrarValores();
            Avisar("Ajustes restablecidos. Tu récord sigue");
        });
    }

    public void PedirBorrarRecord()
    {
        dialogo.Mostrar("¿BORRAR TU RÉCORD?", "No se puede deshacer.", "BORRAR", () =>
        {
            GameSettings.DeleteBestScores();
            Avisar("Récord borrado");
        });
    }

    // Boton «Volver» (y Esc, que lo gestiona MenuManager o Pausa)
    public void Volver()
    {
        alVolver?.Invoke();
    }

    void Avisar(string texto)
    {
        StopAllCoroutines();
        StartCoroutine(MostrarAviso(texto));
    }

    IEnumerator MostrarAviso(string texto)
    {
        textoAviso.text = texto;
        cajaAviso.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);   // tiempo real: funciona con la partida en pausa
        cajaAviso.SetActive(false);
    }
}
