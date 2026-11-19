using UnityEngine;

// Mueve el objeto hacia la izquierda a la velocidad del GameController.
// Lo usan el suelo, la capa de parallax (mas despacio), las columnas y los coleccionables.
public class ScrollingObject : MonoBehaviour
{
    public float speedMultiplier = 1f;   // 1 = como el suelo; el parallax usa 0.3 (va mas despacio)
    public float menuSpeed = -1f;        // en el menu no hay GameController: el escenario pasa despacio

    private Rigidbody2D rb;
    private GameController controller;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GameController.instance;
        // Dificultad progresiva: cuando el GameController acelera, los que ya se mueven tambien
        if (controller != null) controller.SpeedChanged += ApplySpeed;
        ApplySpeed();
    }

    void OnDestroy()
    {
        if (controller != null) controller.SpeedChanged -= ApplySpeed;
    }

    void ApplySpeed()
    {
        float speed = controller != null ? controller.scrollSpeed : menuSpeed;
        rb.linearVelocity = new Vector2(speed * speedMultiplier, 0);
    }

    void Update()
    {
        // Si se muestra el Game Over paramos el movimiento
        if (controller != null && controller.gameOver)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
