using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controlador unico de la partida: puntos, coleccionables, fin de partida y reinicio.
public class GameController : MonoBehaviour
{
    public static GameController instance;   // Singleton

    [Header("HUD")]
    public GameObject startHint;              // «Pulsa para empezar» (extra B3)
    public TMP_Text startHintText;            // en movil dice «Toca para empezar»
    public TMP_Text scoreText;                // puntuacion, arriba en el centro
    public TMP_Text bestHudText;              // «RÉCORD 18», debajo de la puntuacion
    public TMP_Text ticketsText;              // monedas normales (extra A4)
    public TMP_Text vipText;                  // monedas especiales (extra A5)
    public TMP_Text speedUpText;              // aviso «¡MÁS RÁPIDO!» de la dificultad progresiva (B2)
    public Image flashImage;                  // destello blanco al chocar

    [Header("Fin de partida")]
    public GameObject gameOverPanel;          // PanelFinPartida
    public RectTransform gameOverBox;         // la caja que entra desde arriba
    public TMP_Text finalScoreText;
    public TMP_Text bestScoreText;            // «RÉCORD: 18»
    public TMP_Text messageText;              // «¡Nuevo récord! · ¡Medalla de bronce!»
    public Image medalImage;
    public Sprite[] medals;                   // bronce, plata, oro, platino (extra B5)
    public Button retryButton;                // Reintentar: se activa 0,6 s despues de chocar

    [Header("Sonido")]
    public AudioClip scoreSound;
    public AudioClip commonSound;
    public AudioClip rareSound;
    public AudioClip dieSound;

    [Header("Partida")]
    public bool started = false;
    public bool gameOver = false;
    public float scrollSpeed = -3f;

    [Header("Dificultad progresiva")]
    public int speedUpEvery = 5;              // cada 5 puntos...
    public float speedUpFactor = 1.08f;       // ...un 8% mas rapido
    public float maxSpeedFactor = 1.6f;       // limite: como mucho un 60% mas que la velocidad de la dificultad elegida
    public event System.Action SpeedChanged;  // avisa a lo que ya se esta moviendo

    private int score = 0;
    private int commons = 0;                  // valor de los tickets (1 cada uno)
    private int rares = 0;                    // chicas VIP recogidas
    private float baseSpeed;
    private int difficulty;                   // la de esta partida (si se cambia en la pausa, vale para la siguiente)
    private bool canRestart = false;
    private AudioSource audioSource;

    // Puntos minimos para cada medalla, como en el original: 10 / 20 / 30 / 40
    private readonly int[] medalScores = { 10, 20, 30, 40 };

    void Awake()
    {
        // Si ya hay otro GameController, este sobra
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        // La dificultad elegida en Seleccion u Opciones marca la velocidad inicial (B2 sube a partir de ella)
        difficulty = GameSettings.Difficulty;
        scrollSpeed = GameSettings.ScrollSpeed;
        baseSpeed = scrollSpeed;
        audioSource = GetComponent<AudioSource>();
    }

    void OnDestroy()
    {
        // Al recargar la escena limpiamos la referencia al controlador destruido
        if (instance == this) instance = null;
    }

    void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        startHint.SetActive(true);
        startHintText.text = Application.isMobilePlatform ? "TOCA PARA EMPEZAR" : "PULSA PARA EMPEZAR";
        scoreText.text = "0";
        ticketsText.text = "0";
        vipText.text = "0";
        speedUpText.gameObject.SetActive(false);
        flashImage.color = Color.clear;
        bestHudText.text = "RÉCORD " + GameSettings.GetBestScore(difficulty);
        MusicPlayer.ResetPitch();
    }

    void Update()
    {
        if (Pausa.IsPaused || !gameOver || !canRestart) return;

        // Esc = Volver, que en el fin de partida es «Menú»
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMenu();
            return;
        }

        // Espacio reintenta aunque se haya quitado el foco con el raton
        // (si un boton tiene el foco, Espacio ya lo pulsa el EventSystem)
        bool sinFoco = EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null;
        if (sinFoco && Input.GetKeyDown(KeyCode.Space)) Retry();
    }

    public void StartGame()
    {
        started = true;
        startHint.SetActive(false);
    }

    public void BirdScored()
    {
        if (gameOver) return;
        score++;
        scoreText.text = score.ToString();
        audioSource.PlayOneShot(scoreSound, 0.7f);
        StartCoroutine(Punch(scoreText.transform, 1.25f));

        if (score % speedUpEvery == 0) SpeedUp();
    }

    // Dificultad progresiva: cada 5 puntos el escenario va un poco mas rapido, con un limite
    // que depende de la velocidad de la dificultad elegida
    void SpeedUp()
    {
        float newSpeed = Mathf.Max(scrollSpeed * speedUpFactor, baseSpeed * maxSpeedFactor);   // velocidades negativas
        if (Mathf.Approximately(newSpeed, scrollSpeed)) return;
        scrollSpeed = newSpeed;
        SpeedChanged?.Invoke();
        StartCoroutine(ShowSpeedUp());
    }

    IEnumerator ShowSpeedUp()
    {
        speedUpText.gameObject.SetActive(true);
        for (float t = 0; t < 1.2f; t += Time.deltaTime)
        {
            float s = 1f + Mathf.Sin(t * 12f) * 0.08f;
            speedUpText.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        speedUpText.gameObject.SetActive(false);
    }

    // Comun: ticket de copa (vale 1). Especial: chica VIP (vale 5), con su propio contador
    public void CollectItem(bool rare, int value)
    {
        if (gameOver) return;
        if (rare)
        {
            rares++;
            vipText.text = rares.ToString();
            audioSource.PlayOneShot(rareSound, 0.9f);
            StartCoroutine(Punch(vipText.transform, 1.4f));
        }
        else
        {
            commons += value;
            ticketsText.text = commons.ToString();
            audioSource.PlayOneShot(commonSound, 0.7f);
            StartCoroutine(Punch(ticketsText.transform, 1.4f));
        }
    }

    public void BirdDie()
    {
        if (gameOver) return;
        gameOver = true;
        audioSource.PlayOneShot(dieSound);
        MusicPlayer.TapeStop();   // la musica se frena como un disco

        int oldBest = GameSettings.GetBestScore(difficulty);
        bool newRecord = GameSettings.SaveScore(score, difficulty) && score > 0;
        bool wasLocked = !GameSettings.IsUnlocked(GameSettings.LockedCharacter);
        GameSettings.TotalCommon += commons;
        GameSettings.TotalRare += rares;
        bool unlocked = wasLocked && GameSettings.IsUnlocked(GameSettings.LockedCharacter);

        finalScoreText.text = score.ToString();
        bestScoreText.text = "RÉCORD: " + GameSettings.GetBestScore(difficulty);

        // Medalla (extra B5): 10 bronce, 20 plata, 30 oro, 40 platino; oculta si no llega a 10
        int medal = -1;
        for (int i = 0; i < medalScores.Length; i++)
            if (score >= medalScores[i]) medal = i;
        medalImage.gameObject.SetActive(medal >= 0);
        if (medal >= 0) medalImage.sprite = medals[medal];

        // Mensajes de la tarea 004 (p. 8): siempre de tu, en positivo y cortos
        var parts = new List<string>();
        if (newRecord) parts.Add("¡Nuevo récord!");
        if (medal >= 0) parts.Add("¡Medalla de " + GameSettings.MedalNames[medal] + "!");
        int missing = oldBest + 1 - score;   // lo que falta para superar el record
        if (!newRecord && oldBest > 0 && missing <= 5)
            parts.Add("¡Casi! Te " + (missing == 1 ? "falta 1" : "faltan " + missing) + " para el récord");
        if (unlocked) parts.Add("¡Ignacio desbloqueado!");
        messageText.text = string.Join(" · ", parts);
        messageText.gameObject.SetActive(parts.Count > 0);

        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        retryButton.interactable = false;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // Destello y pequeño temblor de camara (0,2 s)
        Transform cam = Camera.main.transform;
        Vector3 camPos = cam.position;
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            float k = 1f - t / 0.2f;
            flashImage.color = new Color(1f, 1f, 1f, 0.6f * k);
            cam.position = camPos + (Vector3)Random.insideUnitCircle * 0.15f * k;
            yield return null;
        }
        flashImage.color = Color.clear;
        cam.position = camPos;

        // El panel entra desde arriba en 0,4 s: a los 0,6 s del choque ya se puede reintentar
        gameOverPanel.SetActive(true);
        Vector2 target = gameOverBox.anchoredPosition;
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / 0.4f, 3f);
            gameOverBox.anchoredPosition = target + Vector2.up * 900f * (1f - k);
            yield return null;
        }
        gameOverBox.anchoredPosition = target;

        // Reintentar se activa ahora y recibe el foco: el Espacio que estaba pulsado al chocar no cuenta,
        // solo una pulsacion nueva
        retryButton.interactable = true;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        canRestart = true;
    }

    IEnumerator Punch(Transform target, float amount)
    {
        for (float t = 0; t < 0.15f; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(amount, 1f, t / 0.15f);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    // ---- Botones del fin de partida
    public void Retry()
    {
        if (!canRestart) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ChangeCharacter()
    {
        MenuManager.abrirSeleccion = true;   // el menu se abrira directamente en la seleccion
        SceneManager.LoadScene("Menu");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
