using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Musica de fondo que sigue sonando al cambiar de escena (menu -> juego -> menu).
// Tiene una segunda fuente para los clics, que sale por el grupo Efectos del mezclador.
// Tambien aplica al Audio Mixer los volumenes y el silencio guardados en PlayerPrefs.
public class MusicPlayer : MonoBehaviour
{
    public AudioMixer mezclador;            // Mezclador: Master > Musica y Efectos
    public AudioMixerGroup grupoEfectos;    // salida de los clics de la interfaz

    private static MusicPlayer instance;
    private AudioSource audioSource;
    private AudioSource sfxSource;
    private float baseVolume;
    private float duck = 1f;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        audioSource = GetComponent<AudioSource>();
        baseVolume = audioSource.volume;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.outputAudioMixerGroup = grupoEfectos;
        sfxSource.ignoreListenerPause = true;   // los clics de la interfaz suenan aunque la pausa congele el sonido
        GameSettings.Changed += ApplyVolume;
        audioSource.Play();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // mixer.SetFloat no tiene efecto en Awake: los ajustes se aplican en Start
        ApplyVolume();
    }

    void OnDestroy()
    {
        if (instance == this) GameSettings.Changed -= ApplyVolume;
    }

    void ApplyVolume()
    {
        audioSource.volume = baseVolume * duck;
        GameSettings.ApplyToMixer(mezclador);
    }

    // Para sonidos que deben oirse aunque se cambie de escena (p. ej. el clic de "Jugar")
    public static void PlayOneShot(AudioClip clip)
    {
        if (instance != null && clip != null) instance.sfxSource.PlayOneShot(clip);
    }

    // En pausa la musica baja, como si la oyeras desde la barra
    public static void Duck(bool on)
    {
        if (instance == null) return;
        instance.duck = on ? 0.45f : 1f;
        instance.ApplyVolume();
    }

    // Al morir la musica se frena como un disco al que le quitan la corriente
    public static void TapeStop()
    {
        if (instance != null) instance.StartCoroutine(instance.SlowDown());
    }

    public static void ResetPitch()
    {
        if (instance == null) return;
        instance.StopAllCoroutines();
        instance.audioSource.pitch = 1f;
        Duck(false);
        if (!instance.audioSource.isPlaying) instance.audioSource.Play();
    }

    IEnumerator SlowDown()
    {
        for (float t = 0; t < 0.9f; t += Time.unscaledDeltaTime)
        {
            audioSource.pitch = Mathf.Lerp(1f, 0.1f, t / 0.9f);
            yield return null;
        }
        audioSource.Pause();
    }
}
