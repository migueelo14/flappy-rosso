using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Brillo de la pantalla: cambia la exposicion (Post Exposure) del Volume global de postprocesado de URP.
// La camara tiene que tener activado "Post Processing".
[RequireComponent(typeof(Volume))]
public class BrightnessController : MonoBehaviour
{
    private ColorAdjustments colorAdjustments;

    void Awake()
    {
        // volume.profile crea una copia en tiempo de ejecucion: no se toca el asset del proyecto
        GetComponent<Volume>().profile.TryGet(out colorAdjustments);
    }

    void OnEnable()
    {
        GameSettings.Changed += Apply;
        Apply();
    }

    void OnDisable()
    {
        GameSettings.Changed -= Apply;
    }

    void Apply()
    {
        if (colorAdjustments == null) return;
        colorAdjustments.postExposure.overrideState = true;
        colorAdjustments.postExposure.value = Mathf.Log(GameSettings.Brightness, 2f);   // en pasos de diafragma (EV)
    }
}
