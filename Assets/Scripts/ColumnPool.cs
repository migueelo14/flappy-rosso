using UnityEngine;

// Pool de columnas: se crean 5 al empezar y se reciclan en vez de crear y destruir.
// Entre cada par de columnas puede salir, al azar, un coleccionable (ticket de copa o chica VIP).
public class ColumnPool : MonoBehaviour
{
    public GameObject columnPrefab;
    public GameObject collectiblePrefab;
    public int columnPoolSize = 5;
    public float spawnRate = 2.1f;
    public float columnMin = -1f;
    public float columnMax = 2.2f;

    [Range(0f, 1f)] public float commonChance = 0.6f;   // probabilidad de ticket (comun)
    [Range(0f, 1f)] public float rareChance = 0.12f;    // probabilidad de chica VIP (especial, sale menos)

    private GameObject[] columns;
    private Collectible[] collectibles;
    private int currentColumn = 0;
    private int currentCollectible = 0;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float spawnXPosition = 12f;
    private float timeSinceLastSpawned;
    private float lastY;
    private bool firstColumn = true;
    private float baseSpawnRate;
    private float baseSpeed;

    void Start()
    {
        spawnRate = GameSettings.SpawnRate;
        baseSpawnRate = spawnRate;
        baseSpeed = GameController.instance.scrollSpeed;
        timeSinceLastSpawned = spawnRate - 0.8f;   // la primera columna sale enseguida
        GameController.instance.SpeedChanged += OnSpeedChanged;

        // Que aparezcan justo fuera de la pantalla, sea cual sea su ancho
        Camera cam = Camera.main;
        spawnXPosition = cam.orthographicSize * cam.aspect + 1.5f;

        columns = new GameObject[columnPoolSize];
        collectibles = new Collectible[columnPoolSize];
        for (int i = 0; i < columnPoolSize; i++)
        {
            columns[i] = Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity);
            columns[i].name = "Columna " + (i + 1);
            collectibles[i] = Instantiate(collectiblePrefab, objectPoolPosition, Quaternion.identity).GetComponent<Collectible>();
            collectibles[i].name = "Coleccionable " + (i + 1);
        }
    }

    void OnDestroy()
    {
        if (GameController.instance != null) GameController.instance.SpeedChanged -= OnSpeedChanged;
    }

    // Dificultad progresiva: si el escenario va mas rapido, las columnas salen antes
    // para que sigan igual de separadas
    void OnSpeedChanged()
    {
        spawnRate = baseSpawnRate * baseSpeed / GameController.instance.scrollSpeed;
    }

    void Update()
    {
        if (!GameController.instance.started || GameController.instance.gameOver) return;

        timeSinceLastSpawned += Time.deltaTime;

        if (timeSinceLastSpawned >= spawnRate)
        {
            timeSinceLastSpawned = 0f;
            float spawnYPosition = Random.Range(columnMin, columnMax);
            columns[currentColumn].transform.position = new Vector2(spawnXPosition, spawnYPosition);

            if (!firstColumn) SpawnCollectible(spawnYPosition);
            firstColumn = false;
            lastY = spawnYPosition;

            currentColumn++;
            if (currentColumn >= columnPoolSize)
            {
                currentColumn = 0;
            }
        }
    }

    // Entre la columna anterior y la nueva: decide al azar si sale (Random.value), cual sale y a que altura (Random.Range)
    void SpawnCollectible(float newY)
    {
        float roll = Random.value;
        if (roll > commonChance + rareChance) return;

        bool rare = roll < rareChance;
        float distance = Mathf.Abs(GameController.instance.scrollSpeed) * spawnRate;
        float low = Mathf.Min(newY, lastY) - 0.6f;
        float high = Mathf.Max(newY, lastY) + 0.6f;
        float y = Mathf.Clamp(Random.Range(low, high), -2.4f, 3.6f);
        Vector2 position = new Vector2(spawnXPosition - distance / 2f, y);

        // Al recogerlo se desactiva: al reciclarlo, Setup lo vuelve a activar
        collectibles[currentCollectible].Setup(rare, position);
        currentCollectible = (currentCollectible + 1) % collectibles.Length;
    }
}
