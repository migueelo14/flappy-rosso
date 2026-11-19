using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Tarjetas de personaje y segmentos de dificultad: con ← → se elige el que recibe el foco
// (mapa de acciones de la tarea 004). Al entrar al grupo desde fuera con ↑ ↓, el foco salta
// al que ya esta elegido, para no cambiarlo sin querer. Con el raton, manda el clic.
[RequireComponent(typeof(Selectable))]
public class ElegirAlEnfocar : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public string grupo;                    // «personaje», «dificultadSeleccion», «dificultadOpciones»
    public GameObject marcaElegido;         // en las tarjetas: su MarcoElegido (en un Toggle se usa isOn)

    static readonly List<ElegirAlEnfocar> activos = new List<ElegirAlEnfocar>();
    static ElegirAlEnfocar ultimoSinFoco;
    static int frameUltimoSinFoco = -1;

    void OnEnable() { activos.Add(this); }
    void OnDisable() { activos.Remove(this); }

    bool EstaElegido()
    {
        var toggle = GetComponent<Toggle>();
        if (toggle != null) return toggle.isOn;
        return marcaElegido != null && marcaElegido.activeSelf;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ultimoSinFoco = this;
        frameUltimoSinFoco = Time.frameCount;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (eventData is PointerEventData) return;   // raton o toque: ya lo hace el clic

        bool desdeElGrupo = ultimoSinFoco != null && ultimoSinFoco != this &&
                            ultimoSinFoco.grupo == grupo && frameUltimoSinFoco == Time.frameCount;
        if (desdeElGrupo)
        {
            Elegir();
        }
        else if (!EstaElegido())
        {
            foreach (var otro in activos)
            {
                if (otro.grupo == grupo && otro.EstaElegido())
                {
                    StartCoroutine(SeleccionarLuego(otro.gameObject));
                    break;
                }
            }
        }
    }

    void Elegir()
    {
        var toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            if (toggle.IsInteractable()) toggle.isOn = true;
            return;
        }
        var boton = GetComponent<Button>();
        if (boton != null && boton.IsInteractable()) boton.onClick.Invoke();
    }

    IEnumerator SeleccionarLuego(GameObject go)
    {
        yield return null;   // no se puede cambiar la seleccion dentro de OnSelect
        if (EventSystem.current != null && go != null && go.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(go);
    }
}
