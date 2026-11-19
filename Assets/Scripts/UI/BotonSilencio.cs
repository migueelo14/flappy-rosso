using UnityEngine;
using UnityEngine.UI;

// Boton de altavoz del HUD: silencia o devuelve el sonido (igual que la tecla M y «Silenciar todo»).
// El icono cambia entre altavoz y altavoz tachado.
[RequireComponent(typeof(Button))]
public class BotonSilencio : MonoBehaviour
{
    public Image icono;
    public Sprite spriteSonido;
    public Sprite spriteSilencio;

    void OnEnable()
    {
        GameSettings.Changed += Pintar;
        Pintar();
    }

    void OnDisable()
    {
        GameSettings.Changed -= Pintar;
    }

    // On Click del boton
    public void Alternar()
    {
        GameSettings.Muted = !GameSettings.Muted;
    }

    void Pintar()
    {
        icono.sprite = GameSettings.Muted ? spriteSilencio : spriteSonido;
    }
}
