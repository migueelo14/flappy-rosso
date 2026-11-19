using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

// Recorrido automatico del editor para grabar el video de la tarea 005 (puntos 1 a 4).
// Se lanza con -executeMethod DemoVideo.Iniciar (o Flappy Rosso › Recorrido para el video).
// Va seleccionando objetos, cambia la vista Game de 16:9 a 4:3 y apunta en Video/pasos_editor.txt
// cuando empieza cada paso, para poner los rotulos al montar. No guarda nada en las escenas.
public static class DemoVideo
{
    const string MENU = "Assets/Scenes/Menu.unity";
    const string FLAPPY = "Assets/Scenes/Flappy.unity";

    struct Paso { public float duracion; public string rotulo; public Action accion; }
    static List<Paso> pasos;
    static int indice;
    static double finPaso;
    static StreamWriter registro;

    [MenuItem("Flappy Rosso/Recorrido para el video")]
    public static void Iniciar()
    {
        Directory.CreateDirectory("Video");
        registro = new StreamWriter("Video/pasos_editor.txt", false);
        pasos = new List<Paso>
        {
            P(5f, "", () => { Abrir(MENU); Game(1920, 1080, "16:9 Principal"); }),
            // ---- 1. El editor
            P(2.2f, "1 · EL EDITOR — Escena Menu: un Canvas con un panel por pantalla", () => Sel("CanvasMenu/ZonaSegura/PanelInicio")),
            P(2.2f, "", () => Sel("CanvasMenu/ZonaSegura/PanelSeleccion")),
            P(2.2f, "", () => Sel("CanvasMenu/ZonaSegura/PanelCreditos")),
            P(2.2f, "", () => Sel("CanvasMenu/PanelOpciones")),
            P(2.2f, "", () => Sel("CanvasMenu/DialogoConfirmar")),
            P(11f, "Canvas: Render Mode Screen Space – Overlay (encima de todo y se adapta sola a la resolución) · Canvas Scaler: Scale With Screen Size 1920×1080, Match 0,5 (en 4:3 nada se sale)",
              () => { Sel("CanvasMenu"); Plegar("RectTransform"); }),
            P(2.2f, "Escena Flappy: HUD dentro de ZonaSegura, Pausa, Opciones (el mismo prefab) y Fin de partida", () => { Abrir(FLAPPY); Sel("CanvasFlappy/ZonaSegura/PanelHUD"); }),
            P(2.2f, "", () => Sel("CanvasFlappy/PanelPausa")),
            P(2.2f, "", () => Sel("CanvasFlappy/PanelOpciones")),
            P(2.2f, "", () => Sel("CanvasFlappy/PanelFinPartida")),
            P(4f, "", () => { Sel("CanvasFlappy"); Plegar("RectTransform"); }),
            // ---- 2. La colocacion
            P(6f, "2 · LA COLOCACIÓN — HUD en 16:9 · monedas ancladas arriba a la izquierda (Anchor Preset top-left)",
              () => { Game(1920, 1080, "16:9 Principal"); Sel("CanvasFlappy/ZonaSegura/PanelHUD/GrupoMonedas"); }),
            P(4f, "Silencio y pausa anclados arriba a la derecha, 120×120, a 40 px del borde",
              () => Sel("CanvasFlappy/ZonaSegura/PanelHUD/GrupoBotonesHUD")),
            P(5f, "HUD en 4:3 (proporción extrema): las esquinas se acercan y nada se sale de la zona segura",
              () => Game(1440, 1080, "4:3 Extrema")),
            P(5f, "Inicio en 16:9 (principal)", () => { Abrir(MENU); Game(1920, 1080, "16:9 Principal"); Sel("CanvasMenu/ZonaSegura/PanelInicio/GrupoBotonesInicio"); }),
            P(5f, "Inicio en 4:3: el personaje decorativo se oculta (script OcultarSiEstrecho) y el orden de los botones no cambia",
              () => { Game(1440, 1080, "4:3 Extrema"); Activo("CanvasMenu/ZonaSegura/PanelInicio/GrupoPersonajeDecorativo", false); }),
            P(7f, "Panel de Opciones: GrupoSonido con Vertical Layout Group (Spacing 20, Padding 40)",
              () =>
              {
                  Game(1920, 1080, "16:9 Principal");
                  Activo("CanvasMenu/ZonaSegura/PanelInicio", false);
                  Activo("CanvasMenu/PanelOpciones", true);
                  Sel("CanvasMenu/PanelOpciones/ZonaSeguraOpciones/GrupoSonido"); Plegar("RectTransform", "Image");
              }),
            // ---- 3. Las propiedades
            P(10f, "3 · PROPIEDADES — BotonJugar: Image Type = Sliced (ui_boton 96×96, Border 24) · Transition Color Tint: Normal, Highlighted, Pressed, Selected y Disabled",
              () =>
              {
                  Activo("CanvasMenu/PanelOpciones", false);
                  Activo("CanvasMenu/ZonaSegura/PanelInicio", true);
                  Sel("CanvasMenu/ZonaSegura/PanelInicio/GrupoBotonesInicio/BotonJugar");
                  Plegar("RectTransform", "LayoutElement", "IndicadorFoco", "SonidoAlPulsar");
              }),
            P(9f, "Texto en TextMeshPro: Font Asset PressStart2P Rosso SDF (ñ y tildes en el atlas) y Material Preset Neon",
              () => { Sel("CanvasMenu/ZonaSegura/PanelSeleccion/TextoTituloSeleccion"); Plegar("RectTransform"); }),
            // ---- 4. Los eventos
            P(8f, "4 · EVENTOS — BotonJugar › On Click () → MenuManager.Mostrar (GameObject) con PanelSeleccion",
              () => { Sel("CanvasMenu/ZonaSegura/PanelInicio/GrupoBotonesInicio/BotonJugar"); Plegar("RectTransform", "LayoutElement", "Image", "IndicadorFoco", "SonidoAlPulsar"); }),
            P(9f, "SliderMusica › On Value Changed (Single) → Ajustes.CambiarMusica, elegida en la sección Dynamic float",
              () =>
              {
                  Activo("CanvasMenu/PanelOpciones", true);
                  Sel("CanvasMenu/PanelOpciones/ZonaSeguraOpciones/GrupoSonido/SliderMusica");
                  Plegar("RectTransform", "LayoutElement", "IndicadorFoco");
              }),
            P(1f, "", () => { }),
        };
        indice = -1;
        finPaso = 0;
        EditorApplication.update += Avanzar;
    }

    static Paso P(float d, string r, Action a) { return new Paso { duracion = d, rotulo = r, accion = a }; }

    static void Avanzar()
    {
        if (EditorApplication.timeSinceStartup < finPaso) return;
        indice++;
        if (indice >= pasos.Count)
        {
            EditorApplication.update -= Avanzar;
            Apuntar("FIN");
            registro.Close();
            EditorSceneManager.OpenScene(FLAPPY, OpenSceneMode.Single);   // descarta los cambios de Menu
            if (Environment.GetCommandLineArgs().Length > 0 && Array.IndexOf(Environment.GetCommandLineArgs(), "-salirAlTerminar") >= 0)
                EditorApplication.Exit(0);
            return;
        }
        var paso = pasos[indice];
        try { paso.accion(); }
        catch (Exception e) { Debug.LogWarning("[DemoVideo] " + e.Message); }
        Apuntar(paso.rotulo);
        finPaso = EditorApplication.timeSinceStartup + paso.duracion;
    }

    static void Apuntar(string rotulo)
    {
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        registro.WriteLine(ms + "\t" + rotulo);
        registro.Flush();
    }

    static void Abrir(string escena)
    {
        EditorSceneManager.OpenScene(escena, OpenSceneMode.Single);
    }

    static GameObject Buscar(string ruta)
    {
        string[] partes = ruta.Split('/');
        foreach (var raiz in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (raiz.name != partes[0]) continue;
            if (partes.Length == 1) return raiz;
            var t = raiz.transform.Find(string.Join("/", partes, 1, partes.Length - 1));
            if (t != null) return t.gameObject;
        }
        throw new Exception("No encuentro " + ruta);
    }

    static void Sel(string ruta)
    {
        var go = Buscar(ruta);
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
        foreach (var c in go.GetComponents<Component>()) InternalEditorUtility.SetIsInspectorExpanded(c, true);
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    static void Plegar(params string[] tipos)
    {
        var go = Selection.activeGameObject;
        foreach (var c in go.GetComponents<Component>())
            if (c != null && Array.IndexOf(tipos, c.GetType().Name) >= 0) InternalEditorUtility.SetIsInspectorExpanded(c, false);
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    static void Activo(string ruta, bool activo) { Buscar(ruta).SetActive(activo); }

    // Pone la vista Game en una resolucion fija (la crea si no existe)
    static void Game(int ancho, int alto, string nombre)
    {
        var asm = typeof(Editor).Assembly;
        var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
        var tipoSizes = asm.GetType("UnityEditor.GameViewSizes");
        var singleton = typeof(ScriptableSingleton<>).MakeGenericType(tipoSizes);
        var sizes = singleton.GetProperty("instance").GetValue(null);
        var grupo = tipoSizes.GetMethod("GetGroup").Invoke(sizes, new object[] { GameViewSizeGroupType.Standalone });
        var tipoGrupo = grupo.GetType();
        int total = (int)tipoGrupo.GetMethod("GetTotalCount").Invoke(grupo, null);
        int encontrado = -1;
        for (int i = 0; i < total; i++)
        {
            var s = tipoGrupo.GetMethod("GetGameViewSize").Invoke(grupo, new object[] { i });
            var tipoSize = s.GetType();
            if ((int)tipoSize.GetProperty("width").GetValue(s) == ancho && (int)tipoSize.GetProperty("height").GetValue(s) == alto &&
                tipoSize.GetProperty("sizeType").GetValue(s).ToString() == "FixedResolution") { encontrado = i; break; }
        }
        if (encontrado < 0)
        {
            var tipoSize = asm.GetType("UnityEditor.GameViewSize");
            var tipoTipo = asm.GetType("UnityEditor.GameViewSizeType");
            var nuevo = tipoSize.GetConstructor(new[] { tipoTipo, typeof(int), typeof(int), typeof(string) })
                                .Invoke(new object[] { Enum.Parse(tipoTipo, "FixedResolution"), ancho, alto, nombre });
            tipoGrupo.GetMethod("AddCustomSize").Invoke(grupo, new[] { nuevo });
            encontrado = (int)tipoGrupo.GetMethod("GetTotalCount").Invoke(grupo, null) - 1;
        }
        var tipoGame = asm.GetType("UnityEditor.GameView");
        var ventana = EditorWindow.GetWindow(tipoGame);
        var metodo = tipoGame.GetMethod("SizeSelectionCallback", flags);
        if (metodo != null) metodo.Invoke(ventana, new object[] { encontrado, null });
        else tipoGame.GetProperty("selectedSizeIndex", flags).SetValue(ventana, encontrado);
        ventana.Repaint();
    }
}
