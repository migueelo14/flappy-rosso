using UnityEngine;
using UnityEngine.EventSystems;

// El personaje (Alex Madafoking Gardel, Jay Woper o Ignacio Granuja). Solo sube y baja: el mundo es el que se mueve.
public class Bird : MonoBehaviour
{
    public float upForce = 350f;                      // fuerza del aleteo
    public float gravity = 1.6f;                      // gravityScale cuando empieza la partida
    public RuntimeAnimatorController[] characters;    // 0 Alex, 1 Jay, 2 Ignacio (desbloqueable)
    public AudioClip flapSound;

    private bool isDead = false;
    private bool started = false;
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Personaje elegido en la pantalla de Seleccion (PlayerPrefs «personaje»)
        int index = GameSettings.Character;
        if (characters != null && index < characters.Length && characters[index] != null)
            anim.runtimeAnimatorController = characters[index];
    }

    void Start()
    {
        startPosition = transform.position;
        rb.simulated = false;   // flota esperando el primer clic («Pulsa para empezar»)
    }

    void Update()
    {
        if (Pausa.IsPaused) return;   // en pausa no se aletea

        if (isDead)
        {
            // Cae en picado
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, -75f), 4f * Time.deltaTime);
            return;
        }

        if (!started)
        {
            // Antes de empezar flota arriba y abajo
            transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * 4f) * 0.18f;
        }

        // Al pulsar un boton el pajaro no aletea:
        //  - el clic sobre la interfaz (pausa, silencio...) es para la interfaz
        //  - el Espacio o el clic que acaba de cerrar la pausa tampoco cuenta
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool justResumed = Time.frameCount - Pausa.ResumedFrame <= 1;
        bool click = Input.GetMouseButtonDown(0) && !overUI;
        if (!justResumed && (click || Input.GetKeyDown(KeyCode.Space)))
        {
            Flap();
        }

        // Se inclina segun la velocidad vertical: hacia arriba al subir, hacia abajo al caer
        if (started)
        {
            float angle = Mathf.Clamp(rb.linearVelocity.y * 5f, -60f, 25f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 10f * Time.deltaTime);
        }
    }

    public void Flap()
    {
        if (isDead) return;

        if (!started)
        {
            started = true;
            rb.simulated = true;
            rb.gravityScale = gravity;
            GameController.instance.StartGame();
        }

        // Se para la caida y se añade una fuerza hacia arriba
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * upForce);
        anim.SetTrigger("Flap");
        if (flapSound != null) audioSource.PlayOneShot(flapSound, 0.8f);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (isDead) return;

        rb.linearVelocity = Vector2.zero;
        isDead = true;
        anim.SetTrigger("Die");
        GameController.instance.BirdDie();
    }
}
