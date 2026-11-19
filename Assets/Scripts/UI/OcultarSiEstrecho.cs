using UnityEngine;

// En 4:3 no cabe todo: el personaje decorativo de Inicio se oculta si ancho/alto < 1,5
// (tarea 004, «Inicio · qué cambia en 4:3»). Va en el panel, no en el objeto que oculta.
public class OcultarSiEstrecho : MonoBehaviour
{
    public GameObject objetivo;
    public float proporcionMinima = 1.5f;

    void Update()
    {
        bool ver = (float)Screen.width / Mathf.Max(1, Screen.height) >= proporcionMinima;
        if (objetivo.activeSelf != ver) objetivo.SetActive(ver);
    }
}
