using System.Collections;
using TMPro;
using UnityEngine;

// Coleccionable: ticket de copa (comun, vale 1) o chica VIP (especial, vale 5 y sale menos).
// Lleva el dibujo en un hijo ("Visual") con un Rotator para girar mientras el padre se desplaza.
public class Collectible : MonoBehaviour
{
    public Sprite commonSprite;
    public Sprite rareSprite;
    public int commonValue = 1;
    public int rareValue = 5;
    public Transform visual;
    public Rotator rotator;
    public TextMeshPro valueText;           // "+1" / "+5" que sube al recogerlo

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool rare;
    private bool taken = true;
    private float phase;

    public int Value { get { return rare ? rareValue : commonValue; } }

    void Awake()
    {
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        col.enabled = false;
        spriteRenderer.enabled = false;
        valueText.gameObject.SetActive(false);
    }

    public void Setup(bool isRare, Vector2 position)
    {
        StopAllCoroutines();
        rare = isRare;
        taken = false;
        transform.position = position;
        spriteRenderer.sprite = rare ? rareSprite : commonSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.enabled = true;
        visual.localScale = Vector3.one;
        visual.localRotation = Quaternion.identity;
        rotator.enabled = true;
        rotator.rotation = new Vector3(0f, rare ? 120f : 180f, 0f);   // la especial gira mas despacio
        valueText.gameObject.SetActive(false);
        col.enabled = true;
        phase = Random.value * 6f;
    }

    void Update()
    {
        if (taken) return;

        // Flota arriba y abajo; la especial ademas late para que se note
        float t = Time.time * 3f + phase;
        visual.localPosition = new Vector3(0, Mathf.Sin(t) * 0.12f, 0);
        if (rare)
        {
            float s = 1f + Mathf.Sin(t * 2.2f) * 0.08f;
            visual.localScale = new Vector3(s, s, 1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag("Player") || GameController.instance.gameOver) return;

        taken = true;
        col.enabled = false;
        GameController.instance.CollectItem(rare, Value);
        StartCoroutine(Pop());
    }

    // Crece y se desvanece al recogerlo, y sale lo que vale
    IEnumerator Pop()
    {
        rotator.enabled = false;
        valueText.text = "+" + Value;
        valueText.color = rare ? new Color(1f, 0.82f, 0.25f) : Color.white;
        valueText.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        valueText.gameObject.SetActive(true);
        for (float t = 0; t < 0.7f; t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / 0.25f);
            visual.localScale = Vector3.one * (1f + k * 0.8f);
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - k);
            valueText.transform.localPosition = new Vector3(0f, 0.5f + t * 1.4f, 0f);
            valueText.alpha = 1f - Mathf.Clamp01((t - 0.35f) / 0.35f);
            yield return null;
        }
        spriteRenderer.enabled = false;
        valueText.gameObject.SetActive(false);
    }
}
