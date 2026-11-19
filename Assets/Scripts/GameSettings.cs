using UnityEngine;
using UnityEngine.Audio;

// Ajustes del juego, records y coleccionables guardados en PlayerPrefs.
// Es estatica: se puede leer desde cualquier escena sin buscar ningun objeto.
// Claves (tarea 004, p. 6): volumen_musica, volumen_efectos, muted, fullscreen, dificultad,
// personaje y record_0/1/2 (un record por dificultad, que «Restablecer» no toca).
public static class GameSettings
{
    public static readonly string[] CharacterNames = { "Alex Madafoking Gardel", "Jay Woper", "Ignacio Granuja" };
    public static readonly string[] ShortNames = { "Alex", "Jay", "Ignacio" };
    public static readonly string[] DifficultyNames = { "Fácil", "Normal", "Difícil" };
    public static readonly string[] MedalNames = { "bronce", "plata", "oro", "platino" };

    // Ignacio Granuja se desbloquea juntando coleccionables entre todas las partidas
    public const int LockedCharacter = 2;
    public const int UnlockCommon = 100;   // tickets
    public const int UnlockRare = 15;      // chicas VIP

    public const float DefaultVolume = 0.8f;   // 80 % por defecto, como en la tabla de opciones

    // Por dificultad: velocidad del escenario, segundos entre columnas y tamaño del hueco
    static readonly float[] scrollSpeeds = { -2.6f, -3.2f, -4.0f };
    static readonly float[] spawnRates = { 2.6f, 2.1f, 1.65f };
    static readonly float[] gapSizes = { 4.4f, 3.9f, 3.5f };

    // Avisa a quien tenga que aplicar un ajuste (musica, efectos, icono de silencio...)
    public static event System.Action Changed;

    // ---------------- Personaje y dificultad ----------------
    public static int Character
    {
        get
        {
            int c = Mathf.Clamp(PlayerPrefs.GetInt("personaje", 0), 0, CharacterNames.Length - 1);
            return IsUnlocked(c) ? c : 0;
        }
        set { PlayerPrefs.SetInt("personaje", value); PlayerPrefs.Save(); }
    }

    public static int Difficulty
    {
        get { return Mathf.Clamp(PlayerPrefs.GetInt("dificultad", 1), 0, DifficultyNames.Length - 1); }
        set { PlayerPrefs.SetInt("dificultad", value); PlayerPrefs.Save(); Changed?.Invoke(); }
    }

    public static float ScrollSpeed { get { return scrollSpeeds[Difficulty]; } }
    public static float SpawnRate { get { return spawnRates[Difficulty]; } }
    public static float GapSize { get { return gapSizes[Difficulty]; } }

    // «Consigue 100 tickets o 15 VIP»
    public static bool IsUnlocked(int character)
    {
        if (character != LockedCharacter) return true;
        return TotalCommon >= UnlockCommon || TotalRare >= UnlockRare;
    }

    // ---------------- Records ----------------
    public static int GetBestScore(int difficulty)
    {
        return PlayerPrefs.GetInt("record_" + difficulty, 0);
    }

    // Devuelve true si es un record nuevo. Se guarda en la dificultad con la que se jugo la partida
    public static bool SaveScore(int score, int difficulty)
    {
        if (score <= GetBestScore(difficulty)) return false;
        PlayerPrefs.SetInt("record_" + difficulty, score);
        PlayerPrefs.Save();
        return true;
    }

    // «Borrar récord» en Opciones: borra el de las tres dificultades
    public static void DeleteBestScores()
    {
        for (int i = 0; i < DifficultyNames.Length; i++) PlayerPrefs.DeleteKey("record_" + i);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }

    // Totales de coleccionables de todas las partidas
    public static int TotalCommon
    {
        get { return PlayerPrefs.GetInt("tickets_total", 0); }
        set { PlayerPrefs.SetInt("tickets_total", value); PlayerPrefs.Save(); }
    }

    public static int TotalRare
    {
        get { return PlayerPrefs.GetInt("vip_total", 0); }
        set { PlayerPrefs.SetInt("vip_total", value); PlayerPrefs.Save(); }
    }

    // ---------------- Ajustes de la pantalla de Opciones ----------------
    public static float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("volumen_musica", DefaultVolume); }
        set { PlayerPrefs.SetFloat("volumen_musica", Mathf.Clamp01(value)); Changed?.Invoke(); }
    }

    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat("volumen_efectos", DefaultVolume); }
        set { PlayerPrefs.SetFloat("volumen_efectos", Mathf.Clamp01(value)); Changed?.Invoke(); }
    }

    // Silenciar no mueve los deslizadores: baja el volumen general y recuerda los valores
    public static bool Muted
    {
        get { return PlayerPrefs.GetInt("muted", 0) == 1; }
        set
        {
            PlayerPrefs.SetInt("muted", value ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();   // MusicPlayer lo aplica al mezclador
        }
    }

    // Pantalla completa: Si por defecto en PC
    public static bool Fullscreen
    {
        get { return PlayerPrefs.GetInt("fullscreen", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("fullscreen", value ? 1 : 0);
            PlayerPrefs.Save();
            ApplyFullscreen(value);
            Changed?.Invoke();
        }
    }

    static void ApplyFullscreen(bool on)
    {
        if (Application.isMobilePlatform || Application.isEditor) return;   // en el editor no hace nada visible
        if (on) Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        else Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }

    // «Restablecer valores»: vuelve a los valores por defecto sin tocar el record
    public static void ResetSettings()
    {
        PlayerPrefs.DeleteKey("volumen_musica");
        PlayerPrefs.DeleteKey("volumen_efectos");
        PlayerPrefs.DeleteKey("muted");
        PlayerPrefs.DeleteKey("fullscreen");
        PlayerPrefs.DeleteKey("dificultad");
        PlayerPrefs.Save();
        ApplyFullscreen(Fullscreen);
        Changed?.Invoke();
    }

    // Al arrancar el juego se aplica lo guardado (lo llama AtajosGlobales)
    public static void ApplyOnStartup()
    {
        if (Screen.fullScreen != Fullscreen) ApplyFullscreen(Fullscreen);
    }

    // Volumen en el Audio Mixer: el deslizador va de 0 a 1 y el mezclador en decibelios.
    // log10(0) no existe: 0,0001 equivale a silencio (-80 dB). Silenciar pone los dos grupos a -80 dB.
    public static void ApplyToMixer(AudioMixer mixer)
    {
        if (mixer == null) return;
        bool mudo = Muted;
        bool ok = mixer.SetFloat("VolMusica", mudo ? -80f : ToDecibels(MusicVolume));
        ok &= mixer.SetFloat("VolEfectos", mudo ? -80f : ToDecibels(SfxVolume));
        if (!ok) Debug.LogWarning("El mezclador no tiene expuestos VolMusica y VolEfectos");
    }

    static float ToDecibels(float v) { return Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f; }

    // ---- Brillo (extra de PMDM, lo usa BrightnessController)
    public static float Brightness   // 1 = normal; menos oscurece y mas aclara
    {
        get { return PlayerPrefs.GetFloat("brillo", 1f); }
        set { PlayerPrefs.SetFloat("brillo", Mathf.Clamp(value, 0.4f, 1.6f)); Changed?.Invoke(); }
    }
}
