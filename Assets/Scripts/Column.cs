using UnityEngine;

// Par de seguratas (sobre sus altavoces) con un trigger en el hueco: cruzarlo suma un punto.
public class Column : MonoBehaviour
{
    public Transform topColumn;
    public Transform bottomColumn;
    public float columnHeight = 9f;   // alto del sprite en unidades (900 px / 100)

    void Awake()
    {
        SetGap(GameSettings.GapSize);
    }

    // Separa las columnas segun la dificultad y ajusta el trigger al hueco
    public void SetGap(float gap)
    {
        float y = gap / 2f + columnHeight / 2f;
        topColumn.localPosition = new Vector3(0, y, 0);
        bottomColumn.localPosition = new Vector3(0, -y, 0);

        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        trigger.size = new Vector2(trigger.size.x, gap);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameController.instance.BirdScored();
        }
    }
}
