using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Clic corto al pulsar un control (con raton, toque, Intro o Espacio): da sensacion de respuesta.
// Suena por MusicPlayer, que sigue vivo al cambiar de escena y usa el volumen de efectos.
public class SonidoAlPulsar : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    public AudioClip clip;

    private Selectable selectable;

    void Awake() { selectable = GetComponent<Selectable>(); }

    bool Activo() { return selectable == null || selectable.IsInteractable(); }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && Activo()) MusicPlayer.PlayOneShot(clip);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (Activo()) MusicPlayer.PlayOneShot(clip);
    }
}
