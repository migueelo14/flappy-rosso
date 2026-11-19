using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Dialogo modal para las acciones peligrosas (Salir, Borrar récord, Restablecer, Menú, Reiniciar).
// El foco empieza en «Cancelar» y el velo oscuro bloquea los clics a lo que hay detras.
public class DialogoConfirmar : MonoBehaviour
{
    public TMP_Text textoTitulo;
    public TMP_Text textoMensaje;
    public TMP_Text textoConfirmar;
    public Button botonCancelar;
    public Button botonConfirmar;

    public static bool Abierto { get; private set; }

    private System.Action alConfirmar;
    private GameObject focoAnterior;

    public void Mostrar(string titulo, string mensaje, string confirmar, System.Action accion)
    {
        textoTitulo.text = titulo;
        textoMensaje.text = mensaje;
        textoMensaje.gameObject.SetActive(!string.IsNullOrEmpty(mensaje));
        textoConfirmar.text = confirmar;
        alConfirmar = accion;
        focoAnterior = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();   // por encima de todo
        Abierto = true;
        Seleccionar(botonCancelar.gameObject);
    }

    // On Click de «Cancelar» (y Esc)
    public void Cancelar()
    {
        Cerrar();
        Seleccionar(focoAnterior);
    }

    // On Click del boton de peligro
    public void Confirmar()
    {
        var accion = alConfirmar;
        Cerrar();
        Seleccionar(focoAnterior);
        accion?.Invoke();
    }

    void Cerrar()
    {
        gameObject.SetActive(false);
        Abierto = false;
        alConfirmar = null;
    }

    void OnDisable()
    {
        Abierto = false;
    }

    static void Seleccionar(GameObject go)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        if (go != null && go.activeInHierarchy) EventSystem.current.SetSelectedGameObject(go);
    }
}
