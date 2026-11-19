using UnityEngine;

// Atajos que valen en todas las pantallas (mapa de acciones de la tarea 004):
//   M   = silenciar / devolver el sonido
//   F11 = pantalla completa (en PC)
// Se crea solo al arrancar el juego y sobrevive a los cambios de escena.
public class AtajosGlobales : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Crear()
    {
        GameSettings.ApplyOnStartup();   // silencio y pantalla completa guardados
        var go = new GameObject("AtajosGlobales");
        DontDestroyOnLoad(go);
        go.AddComponent<AtajosGlobales>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) GameSettings.Muted = !GameSettings.Muted;
        if (Input.GetKeyDown(KeyCode.F11) && !Application.isMobilePlatform) GameSettings.Fullscreen = !GameSettings.Fullscreen;
    }
}
