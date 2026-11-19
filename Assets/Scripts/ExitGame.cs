using UnityEngine;

// Salir del juego. Lo llama el boton «Salir» de Inicio despues de confirmar «¿Salir del juego?».
// (Antes salia con Escape; ahora Escape significa siempre «atrás».)
public class ExitGame : MonoBehaviour
{
    public static void Quit()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
