using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Personaje elegido que aletea a la derecha de la pantalla de Inicio (solo decoracion).
public class PersonajeDecorativo : MonoBehaviour
{
    public Image imagen;
    public TMP_Text textoNombre;
    public Sprite[] spritesQuietos;
    public Sprite[] spritesAleteo;

    private float yBase;

    void Awake() { yBase = imagen.rectTransform.anchoredPosition.y; }

    void Update()
    {
        int c = GameSettings.Character;
        float t = Time.unscaledTime;
        imagen.sprite = (t % 0.5f) < 0.18f ? spritesAleteo[c] : spritesQuietos[c];
        imagen.rectTransform.anchoredPosition = new Vector2(0f, yBase + Mathf.Sin(t * 4f) * 10f);
        textoNombre.text = GameSettings.ShortNames[c].ToUpper();
    }
}
