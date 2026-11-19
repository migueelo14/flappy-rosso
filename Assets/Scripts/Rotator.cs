using UnityEngine;

// Hace girar el coleccionable, como el Rotator de Roll a Ball.
// Girando sobre el eje Y parece que el ticket se da la vuelta como una moneda.
public class Rotator : MonoBehaviour
{
    public Vector3 rotation = new Vector3(0f, 180f, 0f);   // grados por segundo en cada eje

    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
