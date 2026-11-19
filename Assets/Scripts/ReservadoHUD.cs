using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Contador de coleccionables con forma de reservado de discoteca:
// cada ticket pone una copa en la mesa y cada chica VIP se sienta en el sofa.
// Si hay mas de las que caben, se ven las que caben y el numero sigue contando.
public class ReservadoHUD : MonoBehaviour
{
    public RectTransform drinksArea;   // encima de la mesa
    public RectTransform girlsArea;    // en el sofa (detras de la mesa)
    public Sprite drinkSprite;
    public Sprite girlSprite;
    public TMP_Text drinksText;
    public TMP_Text girlsText;
    public int maxDrinks = 8;
    public int maxGirls = 4;
    public Vector2 drinkSize = new Vector2(28, 37);
    public Vector2 girlSize = new Vector2(50, 82);

    public bool showGirlPoints = true;     // en la partida el texto de las chicas cuenta puntos (valen 5)

    private int drinks;
    private int girls;
    private int girlPoints;
    private readonly List<RectTransform> drinkIcons = new List<RectTransform>();
    private readonly List<RectTransform> girlIcons = new List<RectTransform>();

    public void SetCounts(int drinkCount, int girlCount)
    {
        SetCounts(drinkCount, girlCount, girlCount * 5);
    }

    public void SetCounts(int drinkCount, int girlCount, int girlPointCount)
    {
        drinks = drinkCount;
        girls = girlCount;
        girlPoints = girlPointCount;
        Refresh(false, false);
    }

    // value: lo que vale el coleccionable (ticket 1, chica VIP 5)
    public void Add(bool rare, int value)
    {
        if (rare)
        {
            girls++;
            girlPoints += value;
        }
        else drinks += value;
        Refresh(!rare, rare);
    }

    void Refresh(bool popDrink, bool popGirl)
    {
        Fill(drinkIcons, drinksArea, drinkSprite, drinkSize, Mathf.Min(drinks, maxDrinks), maxDrinks, "Copa ");
        Fill(girlIcons, girlsArea, girlSprite, girlSize, Mathf.Min(girls, maxGirls), maxGirls, "Chica ");
        drinksText.text = "x" + drinks;
        girlsText.text = showGirlPoints ? girlPoints + " pts" : "x" + girls;

        if (popDrink)
        {
            if (drinks <= maxDrinks) StartCoroutine(Pop(drinkIcons[drinks - 1], 1.8f));
            StartCoroutine(Pop(drinksText.rectTransform, 1.4f));
        }
        if (popGirl)
        {
            if (girls <= maxGirls) StartCoroutine(Pop(girlIcons[girls - 1], 1.6f));
            StartCoroutine(Pop(girlsText.rectTransform, 1.4f));
        }
    }

    // Crea los iconos que falten y los reparte a lo ancho de su zona
    void Fill(List<RectTransform> icons, RectTransform area, Sprite sprite, Vector2 size, int count, int max, string label)
    {
        while (icons.Count < count)
        {
            var go = new GameObject(label + (icons.Count + 1), typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(area, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            icons.Add(rt);
        }
        float step = area.rect.width / max;
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].gameObject.SetActive(i < count);
            icons[i].anchoredPosition = new Vector2(step * (i + 0.5f), 0f);
        }
    }

    IEnumerator Pop(RectTransform target, float amount)
    {
        for (float t = 0; t < 0.2f; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(amount, 1f, t / 0.2f);
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
