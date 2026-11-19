using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de Seleccion de personaje (extra TOP de PMDM, mejorado):
//  - recuerda el ultimo personaje y la ultima dificultad (PlayerPrefs)
//  - el elegido se marca con marco, insignia con check y el texto «elegido», no solo con color
//  - nada se aplica hasta pulsar «¡A la pista!», asi que se puede cambiar de idea
public class Selector : MonoBehaviour
{
    [Header("Tarjetas (Alex, Jay, Ignacio)")]
    public Button[] tarjetas;
    public GameObject[] marcasElegido;     // MarcoElegido de cada tarjeta
    public Image[] dibujos;
    public TMP_Text[] textosEstado;        // «elegido» o lo que falta para desbloquear
    public GameObject[] candados;          // solo Ignacio tiene candado
    public Sprite[] spritesQuietos;
    public Sprite[] spritesAleteo;

    [Header("Dificultad")]
    public Toggle[] togglesDificultad;     // Fácil, Normal, Difícil (Toggle Group)

    [Header("Textos")]
    public TMP_Text textoRecord;           // «Récord en Normal: 18»

    private int personaje;
    private int dificultad;
    private float avisoHasta;

    void OnEnable()   // cada vez que se abre el panel: lo guardado
    {
        personaje = GameSettings.Character;
        dificultad = GameSettings.Difficulty;
        // Al abrir el panel el Toggle Group aun no esta activo y no apaga a los demas: se ponen todos a mano
        togglesDificultad[dificultad].SetIsOnWithoutNotify(true);
        for (int i = 0; i < togglesDificultad.Length; i++)
            if (i != dificultad) togglesDificultad[i].SetIsOnWithoutNotify(false);
        avisoHasta = 0f;
        Pintar();
    }

    void Update()
    {
        // El personaje elegido aletea en su tarjeta
        float t = Time.unscaledTime;
        bool aleteo = (t % 0.5f) < 0.18f;
        for (int i = 0; i < dibujos.Length; i++)
        {
            bool elegido = i == personaje;
            dibujos[i].sprite = elegido && aleteo ? spritesAleteo[i] : spritesQuietos[i];
            float y = elegido ? Mathf.Sin(t * 4f) * 6f : 0f;
            dibujos[i].rectTransform.anchoredPosition = new Vector2(0f, -24f + y);
        }

        // Atajos 1 2 3 para la dificultad
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) togglesDificultad[0].isOn = true;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) togglesDificultad[1].isOn = true;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) togglesDificultad[2].isOn = true;

        if (avisoHasta > 0f && t > avisoHasta)
        {
            avisoHasta = 0f;
            PintarRecord();
        }
    }

    // On Click de cada tarjeta, con el numero del personaje (0, 1, 2)
    public void Marcar(int i)
    {
        if (!GameSettings.IsUnlocked(i))
        {
            // «Consigue 100 tickets o 15 VIP»: dice como desbloquearlo
            textoRecord.text = "Consigue " + GameSettings.UnlockCommon + " tickets o " + GameSettings.UnlockRare + " VIP";
            avisoHasta = Time.unscaledTime + 2f;
            StartCoroutine(Temblar(candados[i] != null ? candados[i].transform : dibujos[i].transform));
            return;
        }
        personaje = i;
        Pintar();
    }

    // On Value Changed (Dynamic) de los tres toggles de dificultad
    public void CambiarDificultad(bool activado)
    {
        if (!activado) return;   // el toggle que se apaga tambien avisa: lo ignoramos
        for (int i = 0; i < togglesDificultad.Length; i++)
            if (togglesDificultad[i].isOn) dificultad = i;
        PintarRecord();
    }

    // Boton «¡A la pista!»: ahora si se guarda la eleccion
    public void Jugar()
    {
        GameSettings.Character = personaje;
        GameSettings.Difficulty = dificultad;
        SceneManager.LoadScene("Flappy");
    }

    void Pintar()
    {
        for (int i = 0; i < tarjetas.Length; i++)
        {
            bool libre = GameSettings.IsUnlocked(i);
            bool elegido = i == personaje;
            marcasElegido[i].SetActive(elegido);
            if (candados[i] != null) candados[i].SetActive(!libre);
            dibujos[i].color = libre ? Color.white : new Color(0f, 0f, 0f, 0.85f);   // silueta
            if (elegido) textosEstado[i].text = "elegido";
            else if (!libre)
                textosEstado[i].text = Mathf.Min(GameSettings.TotalCommon, GameSettings.UnlockCommon) + "/" + GameSettings.UnlockCommon +
                                       " tickets o " + Mathf.Min(GameSettings.TotalRare, GameSettings.UnlockRare) + "/" + GameSettings.UnlockRare + " VIP";
            else textosEstado[i].text = "";
        }
        PintarRecord();
    }

    void PintarRecord()
    {
        if (avisoHasta > 0f) return;
        textoRecord.text = "Récord en " + GameSettings.DifficultyNames[dificultad] + ": " + GameSettings.GetBestScore(dificultad);
    }

    // La tarjeta bloqueada tiembla al intentar elegirla
    IEnumerator Temblar(Transform objetivo)
    {
        Vector3 inicio = objetivo.localPosition;
        for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            objetivo.localPosition = inicio + Vector3.right * Mathf.Sin(t * 60f) * 10f * (1f - t / 0.3f);
            yield return null;
        }
        objetivo.localPosition = inicio;
    }
}
