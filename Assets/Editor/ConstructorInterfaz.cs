using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

// Construye en uGUI la interfaz de la tarea 005 a partir del plano de la tarea 004:
// dos escenas (Menu y Flappy), un Canvas en cada una (Screen Space - Overlay, Scale With Screen Size
// 1920x1080, Match 0,5) y las pantallas Inicio, Seleccion, Opciones, Creditos, HUD, Pausa y Fin de partida.
// Menu de Unity: Flappy Rosso › Construir interfaz. Se puede repetir: borra el Canvas anterior y lo rehace.
public static class ConstructorInterfaz
{
    // ---------------- Guia de estilo (tarea 004, p. 7) ----------------
    static readonly Color Club = Hex("#120821");          // fondo y velos
    static readonly Color Noche = Hex("#291447");         // paneles, tarjetas
    static readonly Color Nube = Hex("#FFF4F8");          // texto e iconos
    static readonly Color Rosso = Hex("#FF2E88");         // boton principal, elegido, peligro
    static readonly Color Neon = Hex("#00E5FF");          // foco de teclado
    static readonly Color Oro = Hex("#FFC857");           // puntos, record, monedas, medallas
    static readonly Color RossoClaro = Hex("#FF5FA2");    // resaltado
    static readonly Color RossoOscuro = Hex("#D91B6E");   // pulsado
    static readonly Color Desactivado = Hex("#3A2B4A");
    static readonly Color NocheClaro = Hex("#3B1F66");    // botones secundarios
    static readonly Color NocheResalte = Hex("#4C2A80");
    static readonly Color NochePulsado = Hex("#1E0E36");

    const string RUTA_UI = "Assets/Sprites/UI/";
    const string RUTA_FUENTES = "Assets/Fonts/";
    const string ESCENA_MENU = "Assets/Scenes/Menu.unity";
    const string ESCENA_FLAPPY = "Assets/Scenes/Flappy.unity";
    const string PREFAB_OPCIONES = "Assets/Prefabs/PanelOpciones.prefab";

    // Atlas de las fuentes: ASCII + ñ, tildes, ü, ¿ y ¡ (y algun simbolo que usan los textos)
    const string CARACTERES = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
                              "¡¿ÁÉÍÓÚÜÑáéíóúüñ·ºª«»";

    static TMP_FontAsset fTitulo, fTexto;            // Press Start 2P (titulos y cifras) y Pixelify Sans (botones y frases)
    static Material pContorno, pNeon, pTextoContorno; // Material Presets
    static Sprite sBoton, sBotonBorde, sFoco, sDiscontinuo, sPanel, sPanelBorde, sEsquinas, sPastilla, sCirculo, sAsa;
    static Sprite iPausa, iSonido, iSilencio, iReiniciar, iVolver, iOpciones, iAviso, iPapelera, iCandado, iJugar, iCheck, iInfo, iToque, iFlecha;
    static Sprite sTicket, sVip;
    static Sprite[] sMedallas, sQuietos, sAleteo;
    static AudioClip clic, aleteo;

    enum Estilo { Principal, Secundario, Peligro, Icono }

    static readonly Vector2 ArribaIzq = new Vector2(0f, 1f), Arriba = new Vector2(0.5f, 1f), ArribaDer = new Vector2(1f, 1f);
    static readonly Vector2 Izq = new Vector2(0f, 0.5f), Centro = new Vector2(0.5f, 0.5f), Der = new Vector2(1f, 0.5f);
    static readonly Vector2 Abajo = new Vector2(0.5f, 0f), AbajoDer = new Vector2(1f, 0f);

    // =====================================================================================
    [MenuItem("Flappy Rosso/Construir interfaz (Menu y Flappy)")]
    public static void ConstruirTodo()
    {
        PrepararRecursos();
        ConstruirMenu();
        ConstruirFlappy();
        ConfigurarProyecto();
        AssetDatabase.SaveAssets();
        Debug.Log("[ConstructorInterfaz] Interfaz construida en Menu y Flappy");
    }

    // =====================================================================================
    // Recursos: sprites 9-slice, fuentes TextMeshPro y Material Presets
    // =====================================================================================
    static void PrepararRecursos()
    {
        AssetDatabase.Refresh();

        // Border en px (el boton de 96x96 con radio 16 lleva Border = 24 por cada lado)
        Importar("ui_boton", 24); Importar("ui_boton_borde", 24); Importar("ui_foco", 30); Importar("ui_borde_discontinuo", 24);
        Importar("ui_panel", 32); Importar("ui_panel_borde", 32); Importar("ui_marco_esquinas", 36); Importar("ui_pastilla", 31);
        Importar("ui_circulo", 0); Importar("ui_asa", 12);
        string[] iconos = { "pausa", "sonido", "silencio", "reiniciar", "volver", "opciones", "aviso", "papelera", "candado", "jugar", "check", "info", "toque", "flecha" };
        foreach (var n in iconos) Importar("icono_" + n, 0);

        sBoton = S("ui_boton"); sBotonBorde = S("ui_boton_borde"); sFoco = S("ui_foco"); sDiscontinuo = S("ui_borde_discontinuo");
        sPanel = S("ui_panel"); sPanelBorde = S("ui_panel_borde"); sEsquinas = S("ui_marco_esquinas"); sPastilla = S("ui_pastilla");
        sCirculo = S("ui_circulo"); sAsa = S("ui_asa");
        iPausa = S("icono_pausa"); iSonido = S("icono_sonido"); iSilencio = S("icono_silencio"); iReiniciar = S("icono_reiniciar");
        iVolver = S("icono_volver"); iOpciones = S("icono_opciones"); iAviso = S("icono_aviso"); iPapelera = S("icono_papelera");
        iCandado = S("icono_candado"); iJugar = S("icono_jugar"); iCheck = S("icono_check"); iInfo = S("icono_info");
        iToque = S("icono_toque"); iFlecha = S("icono_flecha");

        sTicket = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/coleccionable_ticket.png");
        sVip = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/coleccionable_vip.png");
        sMedallas = new[]
        {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/medalla_bronce.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/medalla_plata.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/medalla_oro.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/medalla_platino.png"),
        };
        sQuietos = new[] { SubSprite("personaje_alex", 0), SubSprite("personaje_jay", 0), SubSprite("personaje_ignacio", 0) };
        sAleteo = new[] { SubSprite("personaje_alex", 1), SubSprite("personaje_jay", 1), SubSprite("personaje_ignacio", 1) };
        clic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/clic.wav");
        aleteo = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/aleteo.wav");

        CrearMezclador();

        fTitulo = CrearFuente("PressStart2P-Regular.ttf", "PressStart2P Rosso SDF");
        fTexto = CrearFuente("PixelifySans.ttf", "PixelifySans Rosso SDF");
        pContorno = CrearPreset(fTitulo, "Contorno", m =>
        {
            m.SetColor("_OutlineColor", Club);
            m.SetFloat("_OutlineWidth", 0.25f);
        });
        pNeon = CrearPreset(fTitulo, "Neon", m =>
        {
            m.SetColor("_OutlineColor", Rosso);
            m.SetFloat("_OutlineWidth", 0.12f);
            m.EnableKeyword("UNDERLAY_ON");
            m.SetColor("_UnderlayColor", A(Rosso, 0.7f));
            m.SetFloat("_UnderlaySoftness", 0.6f);
            m.SetFloat("_UnderlayDilate", 0.4f);
        });
        pTextoContorno = CrearPreset(fTexto, "Contorno", m =>
        {
            m.SetColor("_OutlineColor", Club);
            m.SetFloat("_OutlineWidth", 0.3f);
        });
        AssetDatabase.SaveAssets();
    }

    static void Importar(string nombre, int borde)
    {
        string ruta = RUTA_UI + nombre + ".png";
        var ti = AssetImporter.GetAtPath(ruta) as TextureImporter;
        if (ti == null) { Debug.LogError("[ConstructorInterfaz] Falta " + ruta); return; }
        var ajustes = new TextureImporterSettings();
        ti.ReadTextureSettings(ajustes);
        ajustes.spriteMeshType = SpriteMeshType.FullRect;   // necesario para Sliced y Tiled
        ti.SetTextureSettings(ajustes);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 100;
        ti.spriteBorder = new Vector4(borde, borde, borde, borde);
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.filterMode = FilterMode.Bilinear;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }

    static Sprite S(string nombre) { return AssetDatabase.LoadAssetAtPath<Sprite>(RUTA_UI + nombre + ".png"); }

    static Sprite SubSprite(string textura, int i)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/" + textura + ".png"))
            if (o is Sprite s && s.name == textura + "_" + i) return s;
        Debug.LogWarning("[ConstructorInterfaz] No encuentro el sprite " + textura + "_" + i);
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + textura + ".png");
    }

    // Font Asset estatico con todos los caracteres en el atlas (ñ, tildes, ü, ¿ y ¡ incluidos)
    static TMP_FontAsset CrearFuente(string ttf, string nombre)
    {
        string ruta = RUTA_FUENTES + nombre + ".asset";
        var existente = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ruta);
        if (existente != null) return existente;

        var fuente = AssetDatabase.LoadAssetAtPath<Font>(RUTA_FUENTES + ttf);
        var fa = TMP_FontAsset.CreateFontAsset(fuente, 72, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        fa.name = nombre;
        fa.TryAddCharacters(CARACTERES, out string faltan);
        if (!string.IsNullOrEmpty(faltan)) Debug.Log("[ConstructorInterfaz] " + nombre + " no tiene: " + faltan);
        fa.atlasPopulationMode = AtlasPopulationMode.Static;

        AssetDatabase.CreateAsset(fa, ruta);
        var atlas = fa.atlasTexture;
        atlas.name = nombre + " Atlas";
        AssetDatabase.AddObjectToAsset(atlas, fa);
        fa.material.name = nombre + " Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
        EditorUtility.SetDirty(fa);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(ruta);
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ruta);
    }

    // Material Preset: copia del material de la fuente con otro estilo (el original no se toca)
    static Material CrearPreset(TMP_FontAsset fa, string sufijo, System.Action<Material> configurar)
    {
        string ruta = RUTA_FUENTES + fa.name + " " + sufijo + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(fa.material);
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.shader = fa.material.shader;
        m.CopyPropertiesFromMaterial(fa.material);
        m.shaderKeywords = fa.material.shaderKeywords;
        configurar(m);
        ShaderUtilities.GetShaderPropertyIDs();
        ShaderUtilities.UpdateShaderRatios(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    // =====================================================================================
    // Escena Menu: Inicio, Seleccion, Opciones y Creditos
    // =====================================================================================
    static void ConstruirMenu()
    {
        var escena = EditorSceneManager.OpenScene(ESCENA_MENU, OpenSceneMode.Single);
        Borrar("Canvas", "CanvasMenu", "EventSystem", "MenuController", "MenuManager", "SalirJuego");
        RenombrarEscenario();

        var mm = new GameObject("MenuManager").AddComponent<MenuManager>();
        var eventSystem = CrearEventSystem();
        var canvas = CrearCanvas("CanvasMenu");

        var velo = Nuevo("ImagenVelo", canvas.transform); Estirar(velo);
        Imagen(velo, null, A(Club, 0.7f));
        var zona = Nuevo("ZonaSegura", canvas.transform); Estirar(zona);
        zona.AddComponent<ZonaSegura>();

        // ---------------- INICIO ----------------
        var inicio = Nuevo("PanelInicio", zona.transform); Estirar(inicio);
        // Logo sobre una placa: el video de fondo ya lleva el neon «ROSSO by Antique» y sin placa se pisan
        var grupoLogo = Nuevo("GrupoLogo", inicio.transform);
        Anclar(grupoLogo, Arriba, Arriba, new Vector2(0, -30), new Vector2(1580, 236));
        Imagen(grupoLogo, sPanel, A(Club, 0.92f), Image.Type.Sliced);
        Borde(grupoLogo, Rosso);
        var logo = NuevoTexto("TextoLogo", grupoLogo.transform, "FLAPPY <color=#FF2E88>ROSSO</color>", fTitulo, 96, Nube, TextAlignmentOptions.Center, pContorno);
        Anclar(logo, Arriba, Arriba, new Vector2(0, -30), new Vector2(1500, 120));
        var sub = NuevoTexto("TextoSubtitulo", grupoLogo.transform, "by Antique", fTexto, 36, Oro, TextAlignmentOptions.Center);
        Anclar(sub, Arriba, Arriba, new Vector2(0, -154), new Vector2(600, 50));

        var grupoBotones = Nuevo("GrupoBotonesInicio", inicio.transform);
        Anclar(grupoBotones, Centro, Centro, new Vector2(0, 10), new Vector2(480, 408));
        Vertical(grupoBotones, 24, 0, TextAnchor.UpperCenter);
        var bJugar = Boton("BotonJugar", grupoBotones.transform, "JUGAR", Estilo.Principal, null, 480, 120);
        var bOpciones = Boton("BotonOpciones", grupoBotones.transform, "OPCIONES", Estilo.Secundario, iOpciones, 480, 120);
        var bCreditos = Boton("BotonCreditos", grupoBotones.transform, "CRÉDITOS", Estilo.Secundario, iInfo, 480, 120);

        // «Salir» va la ultima y separada 72 px (proximidad aparte)
        var grupoSalir = Nuevo("GrupoSalir", inicio.transform);
        Anclar(grupoSalir, Centro, Centro, new Vector2(0, 10 - 204 - 72 - 60), new Vector2(480, 120));
        var bSalir = Boton("BotonSalir", grupoSalir.transform, "SALIR", Estilo.Peligro, iAviso, 480, 120);

        // Personaje decorativo a la derecha (se oculta en 4:3)
        var deco = Nuevo("GrupoPersonajeDecorativo", inicio.transform);
        Anclar(deco, Der, Der, new Vector2(-80, 10), new Vector2(380, 460));
        Imagen(deco, sPanel, A(Noche, 0.8f), Image.Type.Sliced);
        Borde(deco, A(Nube, 0.25f));
        var decoImagen = Nuevo("ImagenPersonajeDecorativo", deco.transform);
        Anclar(decoImagen, Arriba, Arriba, new Vector2(0, -30), new Vector2(300, 300));
        var decoImg = Imagen(decoImagen, sQuietos[0], Color.white);
        var decoEtiqueta = NuevoTexto("TextoEtiquetaPersonaje", deco.transform, "PERSONAJE ELEGIDO", fTexto, 32, Oro, TextAlignmentOptions.Center);
        Anclar(decoEtiqueta, Abajo, Abajo, new Vector2(0, 92), new Vector2(360, 44));
        var decoNombre = NuevoTexto("TextoPersonajeDecorativo", deco.transform, "ALEX", fTitulo, 36, Nube, TextAlignmentOptions.Center);
        Anclar(decoNombre, Abajo, Abajo, new Vector2(0, 34), new Vector2(360, 50));
        var personajeDeco = deco.AddComponent<PersonajeDecorativo>();
        personajeDeco.imagen = decoImg; personajeDeco.textoNombre = decoNombre;
        personajeDeco.spritesQuietos = sQuietos; personajeDeco.spritesAleteo = sAleteo;
        var ocultar = inicio.AddComponent<OcultarSiEstrecho>();
        ocultar.objetivo = deco;

        var version = NuevoTexto("TextoVersion", inicio.transform, "v1.0 · Miguel Redondo Orillán", fTexto, 32, A(Nube, 0.85f), TextAlignmentOptions.MidlineRight, pTextoContorno);
        Anclar(version, AbajoDer, AbajoDer, new Vector2(-40, 40), new Vector2(800, 44));

        // ---------------- SELECCION DE PERSONAJE ----------------
        var seleccion = Nuevo("PanelSeleccion", zona.transform); Estirar(seleccion);
        var selector = seleccion.AddComponent<Selector>();
        var bVolverSel = BotonVolver("BotonVolverSeleccion", seleccion.transform);
        Titulo("TextoTituloSeleccion", seleccion.transform, "ELIGE PERSONAJE");
        var textoRecord = NuevoTexto("TextoRecordSeleccion", seleccion.transform, "Récord en Normal: 0", fTexto, 36, Oro, TextAlignmentOptions.Center);
        Anclar(textoRecord, Arriba, Arriba, new Vector2(0, -160), new Vector2(1000, 50));

        var grupoTarjetas = Nuevo("GrupoTarjetas", seleccion.transform);
        Anclar(grupoTarjetas, Centro, Centro, new Vector2(0, 90), new Vector2(1116, 420));
        Horizontal(grupoTarjetas, 48, 0, TextAnchor.MiddleCenter);

        string[] ids = { "Alex", "Jay", "Ignacio" };
        var tarjetas = new Button[3];
        var marcas = new GameObject[3];
        var dibujos = new Image[3];
        var estados = new TMP_Text[3];
        var candados = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var tarjeta = Nuevo("Tarjeta" + ids[i], grupoTarjetas.transform);
            RT(tarjeta).sizeDelta = new Vector2(340, 420);
            Tam(tarjeta, 340, 420);
            var fondo = Imagen(tarjeta, sPanel, Color.white, Image.Type.Sliced, true);
            var b = tarjeta.AddComponent<Button>();
            b.targetGraphic = fondo;
            b.colors = Bloque(Noche, NocheClaro, NochePulsado, Noche, Desactivado);

            // Elegido: marco Rosso + insignia con check + texto «elegido» (no solo color)
            var marco = Nuevo("MarcoElegido" + ids[i], tarjeta.transform); Estirar(marco, -2);
            Imagen(marco, sFoco, Rosso, Image.Type.Sliced);
            var insignia = Nuevo("ImagenInsigniaElegido" + ids[i], marco.transform);
            Anclar(insignia, ArribaDer, ArribaDer, new Vector2(-14, -14), new Vector2(64, 64));
            Imagen(insignia, sCirculo, Rosso);
            var check = Nuevo("IconoElegido" + ids[i], insignia.transform); Estirar(check, 12);
            Imagen(check, iCheck, Club);

            var foco = Nuevo("ImagenFoco" + ids[i], tarjeta.transform); Estirar(foco, -16);
            Imagen(foco, sFoco, Neon, Image.Type.Sliced);
            foco.SetActive(false);

            var dibujo = Nuevo("ImagenPersonaje" + ids[i], tarjeta.transform);
            Anclar(dibujo, Arriba, Arriba, new Vector2(0, -24), new Vector2(220, 220));
            dibujos[i] = Imagen(dibujo, sQuietos[i], Color.white);

            if (i == GameSettings.LockedCharacter)
            {
                var candado = Nuevo("Candado" + ids[i], tarjeta.transform); Estirar(candado);
                Imagen(candado, sPanel, A(Club, 0.55f), Image.Type.Sliced);
                var icono = Nuevo("IconoCandado" + ids[i], candado.transform);
                Anclar(icono, Arriba, Arriba, new Vector2(0, -80), new Vector2(110, 110));
                Imagen(icono, iCandado, Nube);
                candados[i] = candado;
            }

            var nombre = NuevoTexto("TextoNombre" + ids[i], tarjeta.transform, ids[i].ToUpper(), fTitulo, 32, Nube, TextAlignmentOptions.Center);
            Anclar(nombre, Abajo, Abajo, new Vector2(0, 112), new Vector2(320, 44));
            var estado = NuevoTexto("TextoEstado" + ids[i], tarjeta.transform, i == 0 ? "elegido" : "", fTexto, 32, Oro, TextAlignmentOptions.Center);
            Anclar(estado, Abajo, Abajo, new Vector2(0, 14), new Vector2(316, 90));
            estado.textWrappingMode = TextWrappingModes.Normal;

            var indicador = tarjeta.AddComponent<IndicadorFoco>();
            indicador.marcoFoco = foco;
            var elegir = tarjeta.AddComponent<ElegirAlEnfocar>();
            elegir.grupo = "personaje";
            elegir.marcaElegido = marco;
            tarjeta.AddComponent<SonidoAlPulsar>().clip = clic;
            UnityEventTools.AddIntPersistentListener(b.onClick, selector.Marcar, i);

            marco.SetActive(i == 0);
            tarjetas[i] = b; marcas[i] = marco; estados[i] = estado;
        }

        var etiquetaDif = NuevoTexto("TextoEtiquetaDificultad", seleccion.transform, "DIFICULTAD", fTexto, 32, Nube, TextAlignmentOptions.Center);
        Anclar(etiquetaDif, Centro, Centro, new Vector2(0, -162), new Vector2(600, 44));
        var difSeleccion = FilaDificultad("FilaDificultadSeleccion", seleccion.transform, 720, "dificultadSeleccion");
        Anclar(difSeleccion[0].transform.parent.gameObject, Centro, Centro, new Vector2(0, -240), new Vector2(720, 100));
        var bPista = Boton("BotonALaPista", seleccion.transform, "¡A LA PISTA!", Estilo.Principal, null, 480, 120);
        Anclar(bPista, Abajo, Abajo, new Vector2(0, 40), new Vector2(480, 120));

        selector.tarjetas = tarjetas; selector.marcasElegido = marcas; selector.dibujos = dibujos;
        selector.textosEstado = estados; selector.candados = candados;
        selector.spritesQuietos = sQuietos; selector.spritesAleteo = sAleteo;
        selector.togglesDificultad = difSeleccion; selector.textoRecord = textoRecord;
        foreach (var t in difSeleccion) AlCambiar(t, selector.CambiarDificultad);
        AlPulsar(bPista, selector.Jugar);

        // ---------------- CREDITOS ----------------
        var creditos = Nuevo("PanelCreditos", zona.transform); Estirar(creditos);
        var bVolverCred = BotonVolver("BotonVolverCreditos", creditos.transform);
        Titulo("TextoTituloCreditos", creditos.transform, "CRÉDITOS");
        var marcoCred = Nuevo("MarcoCreditos", creditos.transform);
        Anclar(marcoCred, Centro, Centro, new Vector2(0, -40), new Vector2(1320, 760));
        Imagen(marcoCred, sPanel, A(Noche, 0.85f), Image.Type.Sliced);
        var esquinas = Nuevo("ImagenEsquinasCreditos", marcoCred.transform); Estirar(esquinas, -10);
        Imagen(esquinas, sEsquinas, Oro, Image.Type.Sliced).fillCenter = false;
        var scroll = ScrollCreditos(marcoCred.transform);
        var desplazar = creditos.AddComponent<DesplazarConTeclado>();
        desplazar.scroll = scroll;

        // ---------------- OPCIONES (prefab, el mismo en las dos escenas) ----------------
        var opciones = ConstruirPanelOpciones(canvas.transform);
        PrefabUtility.SaveAsPrefabAssetAndConnect(opciones, PREFAB_OPCIONES, InteractionMode.AutomatedAction);
        var ajustes = opciones.GetComponent<Ajustes>();

        // ---------------- DIALOGO ----------------
        var dialogo = ConstruirDialogo(canvas.transform);
        ajustes.dialogo = dialogo;
        PrefabUtility.RecordPrefabInstancePropertyModifications(ajustes);

        // ---------------- Eventos On Click ----------------
        AlPulsarObjeto(bJugar, mm.Mostrar, seleccion);
        AlPulsarObjeto(bOpciones, mm.Mostrar, opciones);
        AlPulsarObjeto(bCreditos, mm.Mostrar, creditos);
        AlPulsar(bSalir, mm.Salir);
        AlPulsarObjeto(bVolverSel, mm.Mostrar, inicio);
        AlPulsarObjeto(bVolverCred, mm.Mostrar, inicio);

        mm.panelInicio = inicio; mm.panelSeleccion = seleccion; mm.panelOpciones = opciones; mm.panelCreditos = creditos;
        mm.focoInicio = bJugar.gameObject; mm.focoSeleccion = bPista.gameObject;
        mm.focoOpciones = ajustes.sliderMusica.gameObject; mm.focoCreditos = bVolverCred.gameObject;
        mm.ajustes = ajustes; mm.dialogo = dialogo;
        eventSystem.firstSelectedGameObject = bJugar.gameObject;   // el foco empieza en Jugar
        EnrutarAudio();

        seleccion.SetActive(false);
        creditos.SetActive(false);
        opciones.SetActive(false);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    static ScrollRect ScrollCreditos(Transform padre)
    {
        var go = Nuevo("ScrollCreditos", padre); Estirar(go, 40);
        var sr = go.AddComponent<ScrollRect>();

        var viewport = Nuevo("ViewportCreditos", go.transform); Estirar(viewport);
        viewport.AddComponent<RectMask2D>();
        Imagen(viewport, null, new Color(1, 1, 1, 0), Image.Type.Simple, true);   // recibe la rueda y el arrastre

        var contenido = Nuevo("ContenidoCreditos", viewport.transform);
        var rc = RT(contenido);
        rc.anchorMin = new Vector2(0, 1); rc.anchorMax = new Vector2(1, 1); rc.pivot = new Vector2(0.5f, 1);
        rc.sizeDelta = Vector2.zero; rc.anchoredPosition = Vector2.zero;
        var v = Vertical(contenido, 14, 0, TextAnchor.UpperCenter);
        v.padding = new RectOffset(16, 40, 8, 24);
        v.childForceExpandWidth = true;
        var fitter = contenido.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Autor y licencia de cada grafico, sonido y fuente.
        var lineas = new List<(string nombre, string texto, bool cabecera)>
        {
            ("TextoCreditoTitulo", "FLAPPY ROSSO · 2º DAM", true),
            ("TextoCreditoAutor", "Diseño, código e interfaz: Miguel Redondo Orillán", false),
            ("TextoCreditoCabeceraFuentes", "FUENTES", true),
            ("TextoCreditoPressStart", "Press Start 2P · CodeMan38 (zone38.net) · SIL Open Font License 1.1", false),
            ("TextoCreditoPixelify", "Pixelify Sans · Stefie Justprince · SIL Open Font License 1.1", false),
            ("TextoCreditoCabeceraGraficos", "GRÁFICOS", true),
            ("TextoCreditoPersonajes", "Personajes, coleccionables, medallas y escenario · Uiverse (uiverse.io) · Licencia MIT", false),
            ("TextoCreditoVideo", "Vídeo de fondo de la discoteca Rosso · Uiverse (uiverse.io) · Licencia MIT", false),
            ("TextoCreditoIconos", "Iconos de la interfaz · trazos de Lucide (lucide.dev) · Licencia ISC", false),
            ("TextoCreditoCabeceraSonido", "SONIDO", true),
            ("TextoCreditoMusica", "Música · Uiverse (uiverse.io) · Licencia MIT", false),
            ("TextoCreditoEfectos", "Efectos (aleteo, punto, ticket, VIP, choque y clic) · Uiverse (uiverse.io) · Licencia MIT", false),
            ("TextoCreditoCabeceraMotor", "MOTOR", true),
            ("TextoCreditoUnity", "Unity 6 · Unity Technologies", false),
        };
        foreach (var l in lineas)
        {
            var t = NuevoTexto(l.nombre, contenido.transform, l.texto, l.cabecera ? fTitulo : fTexto, l.cabecera ? 32 : 34,
                               l.cabecera ? Oro : Nube, TextAlignmentOptions.Center);
            t.textWrappingMode = TextWrappingModes.Normal;
            if (l.cabecera && contenido.transform.childCount > 1) t.margin = new Vector4(0, 20, 0, 0);   // aire antes de cada bloque
        }

        var barra = Nuevo("ScrollbarCreditos", go.transform);
        var rb = RT(barra);
        rb.anchorMin = new Vector2(1, 0); rb.anchorMax = new Vector2(1, 1); rb.pivot = new Vector2(1, 0.5f);
        rb.sizeDelta = new Vector2(20, 0); rb.anchoredPosition = Vector2.zero;
        Imagen(barra, sPastilla, NocheClaro, Image.Type.Sliced, true);
        var zonaBarra = Nuevo("GrupoDeslizamientoCreditos", barra.transform); Estirar(zonaBarra);
        var asa = Nuevo("ImagenAsaCreditos", zonaBarra.transform); Estirar(asa);
        var asaImg = Imagen(asa, sPastilla, Rosso, Image.Type.Sliced, true);
        var sb = barra.AddComponent<Scrollbar>();
        sb.handleRect = RT(asa); sb.targetGraphic = asaImg; sb.direction = Scrollbar.Direction.BottomToTop;
        sb.navigation = new Navigation { mode = Navigation.Mode.None };

        sr.content = rc; sr.viewport = RT(viewport);
        sr.horizontal = false; sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 40;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        sr.verticalScrollbarSpacing = 12;
        return sr;
    }

    // =====================================================================================
    // Panel de Opciones (se guarda como prefab)
    // =====================================================================================
    static GameObject ConstruirPanelOpciones(Transform padre)
    {
        var raiz = Nuevo("PanelOpciones", padre); Estirar(raiz);
        Imagen(raiz, null, A(Club, 0.8f), Image.Type.Simple, true);   // velo hasta el borde
        var ajustes = raiz.AddComponent<Ajustes>();
        var zona = Nuevo("ZonaSeguraOpciones", raiz.transform); Estirar(zona);
        zona.AddComponent<ZonaSegura>();

        var bVolver = BotonVolver("BotonVolverOpciones", zona.transform);
        Titulo("TextoTituloOpciones", zona.transform, "OPCIONES");

        // Region comun SONIDO
        var sonido = Nuevo("GrupoSonido", zona.transform);
        Anclar(sonido, Centro, Centro, new Vector2(-398, 50), new Vector2(740, 540));
        Imagen(sonido, sPanel, Noche, Image.Type.Sliced);
        Borde(sonido, Rosso);
        Vertical(sonido, 20, 40, TextAnchor.UpperLeft).childForceExpandWidth = true;
        var etiquetaSonido = NuevoTexto("TextoSonido", sonido.transform, "SONIDO", fTitulo, 32, Oro, TextAlignmentOptions.MidlineLeft);
        Tam(etiquetaSonido.gameObject, -1, 44);
        var sMusica = FilaDeslizador("Musica", sonido.transform, "MÚSICA", 660, out var valorMusica);
        var sEfectos = FilaDeslizador("Efectos", sonido.transform, "EFECTOS", 660, out var valorEfectos);
        var tSilencio = FilaInterruptor("Silencio", sonido.transform, "SILENCIAR TODO", 660);

        // Region comun JUEGO
        var juego = Nuevo("GrupoJuego", zona.transform);
        Anclar(juego, Centro, Centro, new Vector2(398, 50), new Vector2(740, 540));
        Imagen(juego, sPanel, Noche, Image.Type.Sliced);
        Borde(juego, Rosso);
        Vertical(juego, 20, 40, TextAnchor.UpperLeft).childForceExpandWidth = true;
        var etiquetaJuego = NuevoTexto("TextoJuego", juego.transform, "JUEGO", fTitulo, 32, Oro, TextAlignmentOptions.MidlineLeft);
        Tam(etiquetaJuego.gameObject, -1, 44);
        var tPantalla = FilaInterruptor("PantallaCompleta", juego.transform, "PANTALLA COMPLETA", 660);
        var etiquetaDif = NuevoTexto("TextoDificultad", juego.transform, "DIFICULTAD", fTexto, 32, Nube, TextAlignmentOptions.MidlineLeft);
        Tam(etiquetaDif.gameObject, -1, 44);
        var dificultad = FilaDificultad("FilaDificultadOpciones", juego.transform, 660, "dificultadOpciones");
        var nota = NuevoTexto("TextoNotaDificultad", juego.transform, "Se aplica en la próxima partida", fTexto, 32, A(Nube, 0.75f), TextAlignmentOptions.MidlineLeft);
        Tam(nota.gameObject, -1, 44);
        // Los rotulos sueltos se alinean con el texto de las filas (que tienen 20 px de aire por dentro)
        foreach (var rotulo in new[] { etiquetaSonido, etiquetaJuego, etiquetaDif, nota }) rotulo.margin = new Vector4(20, 0, 0, 0);

        // Pie: las dos acciones peligrosas, separadas 90 px de los ajustes
        var pie = Nuevo("GrupoPie", zona.transform);
        Anclar(pie, Centro, Centro, new Vector2(0, -370), new Vector2(1040, 120));
        Horizontal(pie, 80, 0, TextAnchor.MiddleCenter);
        var bRestablecer = Boton("BotonRestablecer", pie.transform, "RESTABLECER", Estilo.Secundario, iReiniciar, 480, 120, 36);
        var bBorrar = Boton("BotonBorrarRecord", pie.transform, "BORRAR RÉCORD", Estilo.Peligro, iPapelera, 480, 120, 36);

        // Aviso breve («Récord borrado», «Ajustes restablecidos. Tu récord sigue»)
        var aviso = Nuevo("CajaAvisoOpciones", zona.transform);
        Anclar(aviso, Arriba, Arriba, new Vector2(0, -150), new Vector2(900, 64));
        Imagen(aviso, sPanel, Oro, Image.Type.Sliced);
        var textoAviso = NuevoTexto("TextoAvisoOpciones", aviso.transform, "Récord borrado", fTexto, 32, Club, TextAlignmentOptions.Center);
        Estirar(textoAviso.gameObject);
        aviso.SetActive(false);

        // On Value Changed con la funcion en Dynamic (recibe el valor real del control)
        AlCambiar(sMusica, ajustes.CambiarMusica);
        AlCambiar(sEfectos, ajustes.CambiarEfectos);
        AlCambiar(tSilencio, ajustes.CambiarSilencio);
        AlCambiar(tPantalla, ajustes.CambiarPantallaCompleta);
        foreach (var t in dificultad) AlCambiar(t, ajustes.CambiarDificultad);
        AlPulsar(bRestablecer, ajustes.PedirRestablecer);
        AlPulsar(bBorrar, ajustes.PedirBorrarRecord);
        AlPulsar(bVolver, ajustes.Volver);

        ajustes.sliderMusica = sMusica; ajustes.sliderEfectos = sEfectos;
        ajustes.textoValorMusica = valorMusica; ajustes.textoValorEfectos = valorEfectos;
        ajustes.toggleSilencio = tSilencio; ajustes.togglePantallaCompleta = tPantalla;
        ajustes.filaPantallaCompleta = tPantalla.gameObject; ajustes.togglesDificultad = dificultad;
        ajustes.cajaAviso = aviso; ajustes.textoAviso = textoAviso; ajustes.sonidoPrueba = aleteo;
        return raiz;
    }

    static DialogoConfirmar ConstruirDialogo(Transform padre)
    {
        var raiz = Nuevo("DialogoConfirmar", padre); Estirar(raiz);
        Imagen(raiz, null, A(Club, 0.75f), Image.Type.Simple, true);   // bloquea los clics a lo de detras
        var d = raiz.AddComponent<DialogoConfirmar>();

        var caja = Nuevo("CajaDialogo", raiz.transform);
        Anclar(caja, Centro, Centro, Vector2.zero, new Vector2(980, 440));
        Imagen(caja, sPanel, Noche, Image.Type.Sliced);
        Borde(caja, Rosso);
        Vertical(caja, 24, 56, TextAnchor.MiddleCenter).childForceExpandWidth = true;
        caja.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var titulo = NuevoTexto("TextoTituloDialogo", caja.transform, "¿BORRAR TU RÉCORD?", fTitulo, 40, Nube, TextAlignmentOptions.Center);
        titulo.textWrappingMode = TextWrappingModes.Normal;
        var mensaje = NuevoTexto("TextoMensajeDialogo", caja.transform, "No se puede deshacer.", fTexto, 36, Nube, TextAlignmentOptions.Center);
        mensaje.textWrappingMode = TextWrappingModes.Normal;
        var fila = Nuevo("FilaBotonesDialogo", caja.transform);
        Horizontal(fila, 32, 0, TextAnchor.MiddleCenter);
        Tam(fila, -1, 120);
        var bCancelar = Boton("BotonCancelar", fila.transform, "CANCELAR", Estilo.Principal, null, 380, 120);
        var bConfirmar = Boton("BotonConfirmar", fila.transform, "BORRAR", Estilo.Peligro, iAviso, 380, 120);

        // Navegacion explicita: con las flechas no se sale del dialogo
        bCancelar.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = bConfirmar };
        bConfirmar.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = bCancelar };

        d.textoTitulo = titulo; d.textoMensaje = mensaje;
        d.textoConfirmar = bConfirmar.GetComponentInChildren<TextMeshProUGUI>(true);
        d.botonCancelar = bCancelar; d.botonConfirmar = bConfirmar;
        AlPulsar(bCancelar, d.Cancelar);
        AlPulsar(bConfirmar, d.Confirmar);
        raiz.SetActive(false);
        return d;
    }

    // =====================================================================================
    // Escena Flappy: HUD, Pausa, Opciones y Fin de partida
    // =====================================================================================
    static void ConstruirFlappy()
    {
        var escena = EditorSceneManager.OpenScene(ESCENA_FLAPPY, OpenSceneMode.Single);
        Borrar("Canvas", "CanvasFlappy", "EventSystem", "PauseMenu", "Pausa");
        RenombrarEscenario();

        var gc = Object.FindFirstObjectByType<GameController>();
        var pausa = new GameObject("Pausa").AddComponent<Pausa>();
        var eventSystem = CrearEventSystem();
        var canvas = CrearCanvas("CanvasFlappy");

        var destello = Nuevo("ImagenDestello", canvas.transform); Estirar(destello);
        var destelloImg = Imagen(destello, null, new Color(1, 1, 1, 0));
        var zona = Nuevo("ZonaSegura", canvas.transform); Estirar(zona);
        zona.AddComponent<ZonaSegura>();

        // ---------------- HUD ----------------
        var hud = Nuevo("PanelHUD", zona.transform); Estirar(hud);

        // Monedas arriba a la izquierda: icono y numero pegados (proximidad), dos contadores con iconos distintos
        var monedas = Nuevo("GrupoMonedas", hud.transform);
        Anclar(monedas, ArribaIzq, ArribaIzq, new Vector2(40, -40), new Vector2(560, 120));
        Horizontal(monedas, 40, 0, TextAnchor.MiddleLeft);
        var grupoTickets = Nuevo("GrupoTickets", monedas.transform);
        Horizontal(grupoTickets, 12, 0, TextAnchor.MiddleLeft);
        var iconoTicket = Nuevo("IconoTicket", grupoTickets.transform);
        Imagen(iconoTicket, sTicket, Color.white); Tam(iconoTicket, 112, 96);
        var textoTickets = NuevoTexto("TextoTickets", grupoTickets.transform, "0", fTitulo, 48, Oro, TextAlignmentOptions.MidlineLeft, pContorno);
        Tam(textoTickets.gameObject, 130, 96);
        var grupoVip = Nuevo("GrupoVip", monedas.transform);
        Horizontal(grupoVip, 12, 0, TextAnchor.MiddleLeft);
        var iconoVip = Nuevo("IconoVip", grupoVip.transform);
        Imagen(iconoVip, sVip, Color.white); Tam(iconoVip, 80, 104);
        var textoVip = NuevoTexto("TextoVip", grupoVip.transform, "0", fTitulo, 48, Oro, TextAlignmentOptions.MidlineLeft, pContorno);
        Tam(textoVip.gameObject, 130, 96);

        // Puntuacion arriba en el centro
        var puntuacion = Nuevo("GrupoPuntuacion", hud.transform);
        Anclar(puntuacion, Arriba, Arriba, new Vector2(0, -40), new Vector2(600, 170));
        Vertical(puntuacion, 4, 0, TextAnchor.UpperCenter).childForceExpandWidth = true;
        var textoPuntos = NuevoTexto("TextoPuntuacion", puntuacion.transform, "0", fTitulo, 96, Nube, TextAlignmentOptions.Center, pContorno);
        Tam(textoPuntos.gameObject, -1, 112);
        var textoRecordHud = NuevoTexto("TextoRecordHUD", puntuacion.transform, "RÉCORD 0", fTexto, 36, Oro, TextAlignmentOptions.Center, pTextoContorno);
        Tam(textoRecordHud.gameObject, -1, 48);

        // Silencio y pausa arriba a la derecha, 120x120, lejos del pulgar que aletea
        var botonesHud = Nuevo("GrupoBotonesHUD", hud.transform);
        Anclar(botonesHud, ArribaDer, ArribaDer, new Vector2(-40, -40), new Vector2(264, 120));
        Horizontal(botonesHud, 24, 0, TextAnchor.MiddleRight);
        var bSilencio = Boton("BotonSilencio", botonesHud.transform, "", Estilo.Icono, iSonido, 120, 120);
        var bPausa = Boton("BotonPausa", botonesHud.transform, "", Estilo.Icono, iPausa, 120, 120);
        bSilencio.navigation = new Navigation { mode = Navigation.Mode.None };   // no se quedan con el foco:
        bPausa.navigation = new Navigation { mode = Navigation.Mode.None };      // el Espacio no los repite
        var botonSilencio = bSilencio.gameObject.AddComponent<BotonSilencio>();
        botonSilencio.icono = bSilencio.transform.Find("GrupoContenidoSilencio/IconoSilencio").GetComponent<Image>();
        botonSilencio.spriteSonido = iSonido; botonSilencio.spriteSilencio = iSilencio;
        AlPulsar(bSilencio, botonSilencio.Alternar);
        AlPulsar(bPausa, pausa.Pausar);

        // «Pulsa para empezar» en el centro (extra B3)
        var empezar = Nuevo("PanelEmpezar", hud.transform);
        Anclar(empezar, Centro, Centro, new Vector2(0, -40), new Vector2(1000, 300));
        Imagen(empezar, sPanel, A(Club, 0.82f), Image.Type.Sliced);
        Vertical(empezar, 12, 32, TextAnchor.MiddleCenter).childForceExpandWidth = true;
        var iconoToque = Nuevo("IconoToque", empezar.transform);
        Imagen(iconoToque, iToque, Nube); Tam(iconoToque, -1, 80);
        var textoEmpezar = NuevoTexto("TextoPulsaEmpezar", empezar.transform, "PULSA PARA EMPEZAR", fTitulo, 48, Nube, TextAlignmentOptions.Center, pNeon);
        Tam(textoEmpezar.gameObject, -1, 64);
        var textoAyuda = NuevoTexto("TextoAyudaEmpezar", empezar.transform, "Espacio o clic para aletear · M silencia · Esc pausa", fTexto, 32, Nube, TextAlignmentOptions.Center);
        Tam(textoAyuda.gameObject, -1, 44);

        var masRapido = NuevoTexto("TextoMasRapido", hud.transform, "¡MÁS RÁPIDO!", fTitulo, 48, Rosso, TextAlignmentOptions.Center, pContorno);
        Anclar(masRapido, Arriba, Arriba, new Vector2(0, -230), new Vector2(900, 70));
        masRapido.gameObject.SetActive(false);

        // ---------------- PAUSA ----------------
        var panelPausa = Nuevo("PanelPausa", canvas.transform); Estirar(panelPausa);
        Imagen(panelPausa, null, A(Club, 0.8f), Image.Type.Simple, true);   // velo hasta el borde (stretch)
        var zonaPausa = Nuevo("ZonaSeguraPausa", panelPausa.transform); Estirar(zonaPausa);
        zonaPausa.AddComponent<ZonaSegura>();
        var bVolverPausa = BotonVolver("BotonVolverPausa", zonaPausa.transform);
        var cajaPausa = Nuevo("CajaPausa", zonaPausa.transform);
        Anclar(cajaPausa, Centro, Centro, Vector2.zero, new Vector2(640, 880));
        Imagen(cajaPausa, sPanel, Noche, Image.Type.Sliced);
        Borde(cajaPausa, Rosso);
        Vertical(cajaPausa, 24, 48, TextAnchor.UpperCenter);
        cajaPausa.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var tituloPausa = NuevoTexto("TextoTituloPausa", cajaPausa.transform, "PAUSA", fTitulo, 64, Nube, TextAlignmentOptions.Center, pNeon);
        Tam(tituloPausa.gameObject, -1, 80);
        var ayudaPausa = NuevoTexto("TextoAyudaPausa", cajaPausa.transform, "Pausa · Esc para seguir", fTexto, 32, Oro, TextAlignmentOptions.Center);
        Tam(ayudaPausa.gameObject, -1, 44);
        var bContinuar = Boton("BotonContinuar", cajaPausa.transform, "CONTINUAR", Estilo.Principal, iJugar, 480, 120);
        var bOpcionesPausa = Boton("BotonOpcionesPausa", cajaPausa.transform, "OPCIONES", Estilo.Secundario, iOpciones, 480, 120);
        var separador = Nuevo("GrupoSeparadorPausa", cajaPausa.transform);
        Tam(separador, 10, 40);   // 24 + 40 + 24 = 88 px entre lo seguro y lo peligroso
        var bReiniciar = Boton("BotonReiniciar", cajaPausa.transform, "REINICIAR", Estilo.Peligro, iReiniciar, 480, 120);
        var bMenuPausa = Boton("BotonMenuPausa", cajaPausa.transform, "MENÚ", Estilo.Peligro, iAviso, 480, 120);
        AlPulsar(bVolverPausa, pausa.Continuar);
        AlPulsar(bContinuar, pausa.Continuar);
        AlPulsar(bOpcionesPausa, pausa.AbrirOpciones);
        AlPulsar(bReiniciar, pausa.PedirReiniciar);
        AlPulsar(bMenuPausa, pausa.PedirMenu);

        // ---------------- OPCIONES (el mismo prefab que en Menu) ----------------
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_OPCIONES);
        var opciones = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
        var ajustes = opciones.GetComponent<Ajustes>();

        // ---------------- FIN DE PARTIDA ----------------
        var panelFin = Nuevo("PanelFinPartida", canvas.transform); Estirar(panelFin);
        Imagen(panelFin, null, A(Club, 0.75f), Image.Type.Simple, true);
        var zonaFin = Nuevo("ZonaSeguraFin", panelFin.transform); Estirar(zonaFin);
        zonaFin.AddComponent<ZonaSegura>();
        var bVolverFin = BotonVolver("BotonVolverFin", zonaFin.transform);
        var cajaFin = Nuevo("CajaFinPartida", zonaFin.transform);
        Anclar(cajaFin, Centro, Centro, Vector2.zero, new Vector2(1040, 800));
        Imagen(cajaFin, sPanel, Noche, Image.Type.Sliced);
        Borde(cajaFin, Rosso);
        Vertical(cajaFin, 24, 48, TextAnchor.UpperCenter);
        cajaFin.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var tituloFin = NuevoTexto("TextoTituloFin", cajaFin.transform, "FIN DE PARTIDA", fTitulo, 64, Nube, TextAlignmentOptions.Center, pNeon);
        Tam(tituloFin.gameObject, -1, 80);

        // Primero el resultado (medalla, puntos y record), despues las acciones
        var resultado = Nuevo("GrupoResultado", cajaFin.transform);
        Horizontal(resultado, 40, 0, TextAnchor.MiddleCenter);
        Tam(resultado, -1, 220);
        var hueco = Nuevo("GrupoMedalla", resultado.transform);
        Tam(hueco, 200, 200);
        var medalla = Nuevo("ImagenMedalla", hueco.transform); Estirar(medalla);
        var medallaImg = Imagen(medalla, sMedallas[0], Color.white);
        var columna = Nuevo("GrupoPuntos", resultado.transform);
        Vertical(columna, 4, 0, TextAnchor.MiddleLeft).childForceExpandWidth = true;
        Tam(columna, 420, 220);
        var puntosFin = NuevoTexto("TextoPuntosFin", columna.transform, "18", fTitulo, 96, Oro, TextAlignmentOptions.MidlineLeft, pContorno);
        Tam(puntosFin.gameObject, -1, 110);
        var etiquetaPuntos = NuevoTexto("TextoEtiquetaPuntos", columna.transform, "PUNTOS", fTexto, 32, Nube, TextAlignmentOptions.MidlineLeft);
        Tam(etiquetaPuntos.gameObject, -1, 40);
        var recordFin = NuevoTexto("TextoRecordFin", columna.transform, "RÉCORD: 18", fTexto, 36, Oro, TextAlignmentOptions.MidlineLeft);
        Tam(recordFin.gameObject, -1, 48);
        var mensajeFin = NuevoTexto("TextoMensajeFin", cajaFin.transform, "¡Nuevo récord! · ¡Medalla de bronce!", fTexto, 36, Nube, TextAlignmentOptions.Center);
        mensajeFin.textWrappingMode = TextWrappingModes.Normal;
        Tam(mensajeFin.gameObject, 900, 48);

        var bReintentar = Boton("BotonReintentar", cajaFin.transform, "REINTENTAR", Estilo.Principal, iReiniciar, 480, 120);
        var filaFin = Nuevo("FilaBotonesFin", cajaFin.transform);
        Horizontal(filaFin, 24, 0, TextAnchor.MiddleCenter);
        Tam(filaFin, -1, 120);
        var bCambiar = Boton("BotonCambiarPersonaje", filaFin.transform, "CAMBIAR PERSONAJE", Estilo.Secundario, null, 440, 120, 32);
        var bMenuFin = Boton("BotonMenuFin", filaFin.transform, "MENÚ", Estilo.Secundario, null, 440, 120, 32);
        AlPulsar(bVolverFin, gc.GoToMenu);
        AlPulsar(bReintentar, gc.Retry);
        AlPulsar(bCambiar, gc.ChangeCharacter);
        AlPulsar(bMenuFin, gc.GoToMenu);

        // ---------------- DIALOGO ----------------
        var dialogo = ConstruirDialogo(canvas.transform);

        // ---------------- Referencias ----------------
        gc.startHint = empezar; gc.startHintText = textoEmpezar;
        gc.scoreText = textoPuntos; gc.bestHudText = textoRecordHud;
        gc.ticketsText = textoTickets; gc.vipText = textoVip;
        gc.speedUpText = masRapido; gc.flashImage = destelloImg;
        gc.gameOverPanel = panelFin; gc.gameOverBox = RT(cajaFin);
        gc.finalScoreText = puntosFin; gc.bestScoreText = recordFin; gc.messageText = mensajeFin;
        gc.medalImage = medallaImg; gc.medals = sMedallas; gc.retryButton = bReintentar;
        EditorUtility.SetDirty(gc);

        pausa.panelPausa = panelPausa; pausa.panelOpciones = opciones; pausa.ajustes = ajustes; pausa.dialogo = dialogo;
        pausa.focoPausa = bContinuar.gameObject; pausa.botonOpciones = bOpcionesPausa.gameObject;
        pausa.focoOpciones = ajustes.sliderMusica.gameObject;
        ajustes.dialogo = dialogo;
        PrefabUtility.RecordPrefabInstancePropertyModifications(ajustes);
        eventSystem.firstSelectedGameObject = null;   // en la partida no hay foco: el Espacio aletea
        EnrutarAudio();

        panelPausa.SetActive(false);
        opciones.SetActive(false);
        panelFin.SetActive(false);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
    }

    // =====================================================================================
    // Audio Mixer: Master > Musica y Efectos, con VolMusica y VolEfectos expuestos
    // (Unity no tiene API publica para crear un mezclador: se usa la misma que el menu Create › Audio Mixer)
    // =====================================================================================
    const string RUTA_MEZCLADOR = "Assets/Audio/Mezclador.mixer";
    static AudioMixer mezclador;
    static AudioMixerGroup grupoMusica, grupoEfectos;

    static void CrearMezclador()
    {
        mezclador = AssetDatabase.LoadAssetAtPath<AudioMixer>(RUTA_MEZCLADOR);
        if (mezclador == null)
        {
            var tipo = TipoEditor("UnityEditor.Audio.AudioMixerController");
            var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;
            var controlador = tipo.GetMethod("CreateMixerControllerAtPath", flags).Invoke(null, new object[] { RUTA_MEZCLADOR });
            var master = tipo.GetProperty("masterGroup", flags).GetValue(controlador);
            var tipoGrupo = master.GetType();
            var tipoRuta = TipoEditor("UnityEditor.Audio.AudioGroupParameterPath");

            var nombres = new[] { "Musica", "Efectos" };
            var expuestos = new[] { "VolMusica", "VolEfectos" };
            for (int i = 0; i < 2; i++)
            {
                var grupo = tipo.GetMethod("CreateNewGroup", flags).Invoke(controlador, new object[] { nombres[i], false });
                tipo.GetMethod("AddChildToParent", flags).Invoke(controlador, new[] { grupo, master });
                var guidVolumen = tipoGrupo.GetMethod("GetGUIDForVolume", flags).Invoke(grupo, null);
                var ruta = System.Activator.CreateInstance(tipoRuta, grupo, guidVolumen);
                tipo.GetMethod("AddExposedParameter", flags).Invoke(controlador, new[] { ruta });

                // Renombra el parametro recien expuesto (Unity lo llama «MyExposedParam»)
                var propExpuestos = tipo.GetProperty("exposedParameters", flags);
                var lista = (System.Array)propExpuestos.GetValue(controlador);
                for (int k = 0; k < lista.Length; k++)
                {
                    var p = lista.GetValue(k);
                    var campoGuid = p.GetType().GetField("guid");
                    if (!campoGuid.GetValue(p).Equals(guidVolumen)) continue;
                    p.GetType().GetField("name").SetValue(p, expuestos[i]);
                    lista.SetValue(p, k);
                }
                propExpuestos.SetValue(controlador, lista);
            }
            // Vista «View» con los tres grupos, para que se vean en la ventana Audio Mixer
            try
            {
                var tipoVista = TipoEditor("UnityEditor.Audio.MixerGroupView");
                var ids = new List<GUID>();
                foreach (var g in (System.Collections.IEnumerable)tipo.GetMethod("GetAllAudioGroupsSlow", flags).Invoke(controlador, null))
                    ids.Add((GUID)tipoGrupo.GetProperty("groupID", flags).GetValue(g));
                var guids = ids.ToArray();
                var vista = System.Activator.CreateInstance(tipoVista);
                tipoVista.GetField("guids").SetValue(vista, guids);
                tipoVista.GetField("name").SetValue(vista, "View");
                var vistas = System.Array.CreateInstance(tipoVista, 1);
                vistas.SetValue(vista, 0);
                tipo.GetProperty("views", flags).SetValue(controlador, vistas);
                tipo.GetProperty("currentViewIndex", flags).SetValue(controlador, 0);
            }
            catch (System.Exception e) { Debug.LogWarning("[ConstructorInterfaz] Vista del mezclador sin crear: " + e.Message); }

            EditorUtility.SetDirty((Object)controlador);
            AssetDatabase.SaveAssets();
            mezclador = AssetDatabase.LoadAssetAtPath<AudioMixer>(RUTA_MEZCLADOR);
        }
        grupoMusica = mezclador.FindMatchingGroups("Musica")[0];
        grupoEfectos = mezclador.FindMatchingGroups("Efectos")[0];
    }

    static System.Type TipoEditor(string nombre)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(nombre);
            if (t != null) return t;
        }
        throw new System.Exception("[ConstructorInterfaz] No encuentro el tipo " + nombre);
    }

    // Cada AudioSource sale por su grupo: la musica por Musica y el resto por Efectos
    static void EnrutarAudio()
    {
        foreach (var mp in Object.FindObjectsByType<MusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            mp.GetComponent<AudioSource>().outputAudioMixerGroup = grupoMusica;
            mp.mezclador = mezclador;
            mp.grupoEfectos = grupoEfectos;
            EditorUtility.SetDirty(mp);
            EditorUtility.SetDirty(mp.GetComponent<AudioSource>());
        }
        foreach (var mb in new MonoBehaviour[] { Object.FindFirstObjectByType<GameController>(), Object.FindFirstObjectByType<Bird>() })
        {
            if (mb == null) continue;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(mb.gameObject);   // el antiguo SfxVolume
            var fuente = mb.GetComponent<AudioSource>();
            fuente.outputAudioMixerGroup = grupoEfectos;
            EditorUtility.SetDirty(fuente);
        }
    }

    static void ConfigurarProyecto()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ESCENA_MENU, true),
            new EditorBuildSettingsScene(ESCENA_FLAPPY, true),
        };
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.resizableWindow = true;
    }

    // =====================================================================================
    // Piezas de la interfaz
    // =====================================================================================
    static Button Boton(string nombre, Transform padre, string texto, Estilo estilo, Sprite icono, float ancho, float alto, float tamTexto = 40f)
    {
        string suf = nombre.StartsWith("Boton") ? nombre.Substring(5) : nombre;
        var go = Nuevo(nombre, padre);
        RT(go).sizeDelta = new Vector2(ancho, alto);
        Tam(go, ancho, alto);
        var fondo = Imagen(go, sBoton, Color.white, Image.Type.Sliced, true);   // boton.png 96x96 con Border 24
        var b = go.AddComponent<Button>();
        b.targetGraphic = fondo;
        b.transition = Selectable.Transition.ColorTint;
        b.colors = Colores(estilo);

        if (estilo == Estilo.Peligro)
        {
            // El peligro no tiene color propio: se marca con borde discontinuo + icono de aviso
            var borde = Nuevo("ImagenBorde" + suf, go.transform); Estirar(borde);
            Imagen(borde, sDiscontinuo, Rosso, Image.Type.Tiled);
        }
        else if (estilo != Estilo.Principal)
        {
            var borde = Nuevo("ImagenBorde" + suf, go.transform); Estirar(borde);
            Imagen(borde, sBotonBorde, A(Nube, 0.35f), Image.Type.Sliced);
        }

        var foco = Nuevo("ImagenFoco" + suf, go.transform); Estirar(foco, -12);
        Imagen(foco, sFoco, Neon, Image.Type.Sliced);
        foco.SetActive(false);

        Color colorTexto = estilo == Estilo.Principal ? Club : estilo == Estilo.Peligro ? Rosso : Nube;
        var contenido = Nuevo("GrupoContenido" + suf, go.transform); Estirar(contenido);
        Horizontal(contenido, 16, 0, TextAnchor.MiddleCenter);
        var grupo = contenido.AddComponent<CanvasGroup>();
        grupo.blocksRaycasts = false;
        if (icono != null)
        {
            var ic = Nuevo("Icono" + suf, contenido.transform);
            Imagen(ic, icono, colorTexto);
            float t = estilo == Estilo.Icono ? 64 : 44;
            Tam(ic, t, t);
        }
        if (!string.IsNullOrEmpty(texto))
        {
            var tx = NuevoTexto("Texto" + suf, contenido.transform, texto, fTexto, tamTexto, colorTexto, TextAlignmentOptions.Center);
            tx.fontStyle = FontStyles.Bold;
        }

        GameObject flecha = null;
        if (estilo != Estilo.Icono)
        {
            flecha = Nuevo("ImagenFlechaFoco" + suf, go.transform);
            Anclar(flecha, Izq, Izq, new Vector2(20, 0), new Vector2(28, 28));
            Imagen(flecha, iFlecha, colorTexto);
            flecha.SetActive(false);
        }

        var indicador = go.AddComponent<IndicadorFoco>();
        indicador.marcoFoco = foco; indicador.flecha = flecha;
        indicador.contenido = RT(contenido); indicador.grupoContenido = grupo;
        go.AddComponent<SonidoAlPulsar>().clip = clic;
        return b;
    }

    static Button BotonVolver(string nombre, Transform padre)
    {
        var b = Boton(nombre, padre, "VOLVER", Estilo.Secundario, iVolver, 240, 120, 36);
        Anclar(b, ArribaIzq, ArribaIzq, new Vector2(40, -40), new Vector2(240, 120));
        return b;
    }

    static TextMeshProUGUI Titulo(string nombre, Transform padre, string texto)
    {
        var t = NuevoTexto(nombre, padre, texto, fTitulo, 64, Nube, TextAlignmentOptions.Center, pNeon);
        Anclar(t, Arriba, Arriba, new Vector2(0, -55), new Vector2(1200, 90));
        return t;
    }

    static ColorBlock Colores(Estilo e)
    {
        switch (e)
        {
            case Estilo.Principal: return Bloque(Rosso, RossoClaro, RossoOscuro, Rosso, Desactivado);
            case Estilo.Peligro: return Bloque(Noche, NocheClaro, NochePulsado, Noche, Desactivado);
            case Estilo.Icono: return Bloque(A(Noche, 0.85f), NocheResalte, NochePulsado, A(Noche, 0.85f), Desactivado);
            default: return Bloque(NocheClaro, NocheResalte, NochePulsado, NocheClaro, Desactivado);
        }
    }

    static ColorBlock Bloque(Color normal, Color resaltado, Color pulsado, Color seleccionado, Color desactivado)
    {
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = normal; cb.highlightedColor = resaltado; cb.pressedColor = pulsado;
        cb.selectedColor = seleccionado; cb.disabledColor = desactivado;
        cb.colorMultiplier = 1f; cb.fadeDuration = 0.08f;
        return cb;
    }

    // Filas transparentes que solo se iluminan al pasar el raton o pulsar
    static ColorBlock BloqueFila()
    {
        return Bloque(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.08f), new Color(1, 1, 1, 0.16f), new Color(1, 1, 1, 0.05f), new Color(1, 1, 1, 0));
    }

    static Slider FilaDeslizador(string suf, Transform padre, string etiqueta, float ancho, out TextMeshProUGUI valor)
    {
        var go = Nuevo("Slider" + suf, padre);
        RT(go).sizeDelta = new Vector2(ancho, 120);
        Tam(go, ancho, 120);

        var foco = Nuevo("ImagenFoco" + suf, go.transform); Estirar(foco, -6);
        Imagen(foco, sFoco, Neon, Image.Type.Sliced);
        foco.SetActive(false);

        var texto = NuevoTexto("Texto" + suf, go.transform, etiqueta, fTexto, 32, Nube, TextAlignmentOptions.MidlineLeft);
        Anclar(texto, ArribaIzq, ArribaIzq, new Vector2(20, -10), new Vector2(ancho - 220, 48));
        valor = NuevoTexto("TextoValor" + suf, go.transform, "80 %", fTexto, 32, Oro, TextAlignmentOptions.MidlineRight);
        Anclar(valor, ArribaDer, ArribaDer, new Vector2(-20, -10), new Vector2(180, 48));

        var zona = Nuevo("GrupoZona" + suf, go.transform);
        var rz = RT(zona);
        rz.anchorMin = new Vector2(0, 0); rz.anchorMax = new Vector2(1, 0); rz.pivot = new Vector2(0.5f, 0);
        rz.offsetMin = new Vector2(20, 12); rz.offsetMax = new Vector2(-20, 60);
        var grupo = zona.AddComponent<CanvasGroup>();   // se atenua con el sonido silenciado

        var fondo = Nuevo("ImagenFondo" + suf, zona.transform);
        var rf = RT(fondo);
        rf.anchorMin = new Vector2(0, 0.5f); rf.anchorMax = new Vector2(1, 0.5f); rf.sizeDelta = new Vector2(0, 16);
        Imagen(fondo, sPastilla, Desactivado, Image.Type.Sliced, true);

        var areaRelleno = Nuevo("GrupoRelleno" + suf, zona.transform);
        var rar = RT(areaRelleno);
        rar.anchorMin = new Vector2(0, 0.5f); rar.anchorMax = new Vector2(1, 0.5f); rar.sizeDelta = new Vector2(0, 16);
        var relleno = Nuevo("ImagenRelleno" + suf, areaRelleno.transform);
        var rr = RT(relleno);
        rr.anchorMin = Vector2.zero; rr.anchorMax = new Vector2(0.8f, 1); rr.sizeDelta = Vector2.zero;
        Imagen(relleno, sPastilla, Rosso, Image.Type.Sliced);

        var areaAsa = Nuevo("GrupoAsa" + suf, zona.transform); Estirar(areaAsa);
        RT(areaAsa).offsetMin = new Vector2(16, 0); RT(areaAsa).offsetMax = new Vector2(-16, 0);
        var asa = Nuevo("ImagenAsa" + suf, areaAsa.transform);
        var ra = RT(asa);
        ra.anchorMin = new Vector2(0.8f, 0); ra.anchorMax = new Vector2(0.8f, 1); ra.sizeDelta = new Vector2(32, 0);
        var asaImg = Imagen(asa, sAsa, Color.white, Image.Type.Sliced, true);

        var s = go.AddComponent<Slider>();
        s.fillRect = rr; s.handleRect = ra; s.targetGraphic = asaImg;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0; s.maxValue = 20; s.wholeNumbers = true;   // 20 pasos de 5 %
        s.value = 16;                                             // 80 % por defecto
        s.colors = Bloque(Nube, Color.white, RossoClaro, Nube, Desactivado);

        var indicador = go.AddComponent<IndicadorFoco>();
        indicador.marcoFoco = foco; indicador.escalar = false; indicador.grupoContenido = grupo;
        return s;
    }

    static Toggle FilaInterruptor(string suf, Transform padre, string etiqueta, float ancho)
    {
        var go = Nuevo("Toggle" + suf, padre);
        RT(go).sizeDelta = new Vector2(ancho, 100);
        Tam(go, ancho, 100);
        var fondo = Imagen(go, sBoton, Color.white, Image.Type.Sliced, true);   // toda la fila se pulsa
        var t = go.AddComponent<Toggle>();
        t.targetGraphic = fondo; t.colors = BloqueFila();
        t.toggleTransition = Toggle.ToggleTransition.None;
        t.isOn = false;

        var foco = Nuevo("ImagenFoco" + suf, go.transform); Estirar(foco, -6);
        Imagen(foco, sFoco, Neon, Image.Type.Sliced);
        foco.SetActive(false);

        var texto = NuevoTexto("Texto" + suf, go.transform, etiqueta, fTexto, 32, Nube, TextAlignmentOptions.MidlineLeft);
        Anclar(texto, Izq, Izq, new Vector2(20, 0), new Vector2(ancho - 260, 60));
        var estado = NuevoTexto("TextoEstado" + suf, go.transform, "NO", fTexto, 32, Oro, TextAlignmentOptions.MidlineRight);
        Anclar(estado, Der, Der, new Vector2(-160, 0), new Vector2(80, 60));
        var pista = Nuevo("ImagenPista" + suf, go.transform);
        Anclar(pista, Der, Der, new Vector2(-20, 0), new Vector2(120, 64));
        var pistaImg = Imagen(pista, sPastilla, Desactivado, Image.Type.Sliced);
        var perilla = Nuevo("ImagenPerilla" + suf, pista.transform);
        Anclar(perilla, Centro, Centro, new Vector2(-28, 0), new Vector2(48, 48));
        Imagen(perilla, sCirculo, Nube);

        var interruptor = go.AddComponent<Interruptor>();
        interruptor.pista = pistaImg; interruptor.perilla = RT(perilla); interruptor.textoEstado = estado;
        interruptor.colorEncendido = Rosso; interruptor.colorApagado = Desactivado; interruptor.recorrido = 28;
        var indicador = go.AddComponent<IndicadorFoco>();
        indicador.marcoFoco = foco; indicador.escalar = false;
        go.AddComponent<SonidoAlPulsar>().clip = clic;
        return t;
    }

    // Dificultad: botones segmentados con Toggle Group (uno de varios, Allow Switch Off desmarcado)
    static Toggle[] FilaDificultad(string nombre, Transform padre, float ancho, string grupo)
    {
        var fila = Nuevo(nombre, padre);
        RT(fila).sizeDelta = new Vector2(ancho, 100);
        Tam(fila, ancho, 100);
        Imagen(fila, sBoton, NochePulsado, Image.Type.Sliced);
        Borde(fila, A(Nube, 0.35f), sBotonBorde);
        var toggleGroup = fila.AddComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = false;
        var h = Horizontal(fila, 8, 8, TextAnchor.MiddleCenter);
        h.childForceExpandWidth = true; h.childForceExpandHeight = true;

        string[] ids = { "Facil", "Normal", "Dificil" };
        string[] textos = { "FÁCIL", "NORMAL", "DIFÍCIL" };
        var res = new Toggle[3];
        for (int i = 0; i < 3; i++)
        {
            var go = Nuevo("Toggle" + ids[i], fila.transform);
            var fondo = Imagen(go, sBoton, Color.white, Image.Type.Sliced, true);
            var t = go.AddComponent<Toggle>();
            t.targetGraphic = fondo; t.colors = BloqueFila();
            t.toggleTransition = Toggle.ToggleTransition.None;
            var elegida = Nuevo("ImagenElegida" + ids[i], go.transform); Estirar(elegida);
            t.graphic = Imagen(elegida, sBoton, Rosso, Image.Type.Sliced);
            var foco = Nuevo("ImagenFoco" + ids[i], go.transform); Estirar(foco, -6);
            Imagen(foco, sFoco, Neon, Image.Type.Sliced);
            foco.SetActive(false);
            var contenido = Nuevo("GrupoContenido" + ids[i], go.transform); Estirar(contenido);
            Horizontal(contenido, 8, 0, TextAnchor.MiddleCenter);
            var check = Nuevo("IconoCheck" + ids[i], contenido.transform);
            Imagen(check, iCheck, Club); Tam(check, 32, 32);
            var texto = NuevoTexto("Texto" + ids[i], contenido.transform, textos[i], fTexto, 32, Nube, TextAlignmentOptions.Center);
            texto.fontStyle = FontStyles.Bold;

            var segmento = go.AddComponent<Segmento>();
            segmento.texto = texto; segmento.check = check;
            segmento.colorTextoElegido = Club; segmento.colorTextoNormal = Nube;
            t.group = toggleGroup;
            t.isOn = i == 1;   // Normal por defecto
            check.SetActive(i == 1);
            texto.color = i == 1 ? Club : Nube;

            var indicador = go.AddComponent<IndicadorFoco>();
            indicador.marcoFoco = foco; indicador.escalar = false; indicador.contenido = RT(contenido);
            go.AddComponent<ElegirAlEnfocar>().grupo = grupo;
            go.AddComponent<SonidoAlPulsar>().clip = clic;
            res[i] = t;
        }
        return res;
    }

    static void Borde(GameObject caja, Color color, Sprite sprite = null)
    {
        string suf = caja.name;
        foreach (var p in new[] { "Caja", "Grupo", "Marco", "Panel", "Fila" })
            if (suf.StartsWith(p)) { suf = suf.Substring(p.Length); break; }
        var b = Nuevo("ImagenBorde" + suf, caja.transform); Estirar(b);
        Imagen(b, sprite != null ? sprite : sPanelBorde, color, Image.Type.Sliced);
        b.AddComponent<LayoutElement>().ignoreLayout = true;
        b.transform.SetAsFirstSibling();
    }

    // =====================================================================================
    // Utilidades
    // =====================================================================================
    static GameObject CrearCanvas(string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = LayerMask.NameToLayer("UI");
        var c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;   // encima de todo y se adapta sola a la resolucion
        c.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
        var s = go.GetComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920, 1080);
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight = 0.5f;
        s.referencePixelsPerUnit = 100;
        return go;
    }

    static EventSystem CrearEventSystem()
    {
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return go.GetComponent<EventSystem>();
    }

    static void Borrar(params string[] nombres)
    {
        foreach (var raiz in SceneManager.GetActiveScene().GetRootGameObjects())
            if (System.Array.IndexOf(nombres, raiz.name) >= 0) Object.DestroyImmediate(raiz);
    }

    // Nombres claros tambien en el escenario (nada de «Ground 2»)
    static void RenombrarEscenario()
    {
        var mapa = new Dictionary<string, string>
        {
            { "Scenery", "Escenario" }, { "Ground", "Suelo1" }, { "Ground 2", "Suelo2" },
            { "Parallax", "Parallax1" }, { "Parallax 2", "Parallax2" }, { "Global Light 2D", "LuzGlobal2D" },
        };
        foreach (var raiz in SceneManager.GetActiveScene().GetRootGameObjects()) Renombrar(raiz.transform, mapa);
    }

    static void Renombrar(Transform t, Dictionary<string, string> mapa)
    {
        if (mapa.TryGetValue(t.name, out var nuevo)) t.name = nuevo;
        foreach (Transform hijo in t) Renombrar(hijo, mapa);
    }

    static GameObject Nuevo(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(padre, false);
        return go;
    }

    static RectTransform RT(GameObject go) { return go.GetComponent<RectTransform>(); }

    static void Anclar(Component c, Vector2 ancla, Vector2 pivote, Vector2 pos, Vector2 tam) { Anclar(c.gameObject, ancla, pivote, pos, tam); }

    static void Anclar(GameObject go, Vector2 ancla, Vector2 pivote, Vector2 pos, Vector2 tam)
    {
        var rt = RT(go);
        rt.anchorMin = ancla; rt.anchorMax = ancla; rt.pivot = pivote;
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
    }

    static void Estirar(GameObject go, float margen = 0f)
    {
        var rt = RT(go);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(margen, margen); rt.offsetMax = new Vector2(-margen, -margen);
    }

    static LayoutElement Tam(GameObject go, float ancho, float alto)
    {
        var le = go.GetComponent<LayoutElement>();
        if (le == null) le = go.AddComponent<LayoutElement>();
        if (ancho >= 0) { le.preferredWidth = ancho; le.minWidth = ancho; }
        if (alto >= 0) { le.preferredHeight = alto; le.minHeight = alto; }
        return le;
    }

    static Image Imagen(GameObject go, Sprite s, Color c, Image.Type tipo = Image.Type.Simple, bool raycast = false)
    {
        var img = go.AddComponent<Image>();
        img.sprite = s; img.color = c; img.type = tipo;
        img.raycastTarget = raycast;   // lo decorativo no roba clics
        if (tipo == Image.Type.Simple && s != null) img.preserveAspect = true;
        return img;
    }

    static TextMeshProUGUI NuevoTexto(string nombre, Transform padre, string texto, TMP_FontAsset fuente, float tam, Color color,
                                      TextAlignmentOptions alineacion, Material preset = null)
    {
        var go = Nuevo(nombre, padre);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = fuente;
        if (preset != null) t.fontSharedMaterial = preset;
        t.text = texto; t.fontSize = tam; t.color = color; t.alignment = alineacion;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        return t;
    }

    static VerticalLayoutGroup Vertical(GameObject go, float espacio, int relleno, TextAnchor alineacion)
    {
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.spacing = espacio; v.padding = new RectOffset(relleno, relleno, relleno, relleno); v.childAlignment = alineacion;
        v.childControlWidth = true; v.childControlHeight = true;
        v.childForceExpandWidth = false; v.childForceExpandHeight = false;
        return v;
    }

    static HorizontalLayoutGroup Horizontal(GameObject go, float espacio, int relleno, TextAnchor alineacion)
    {
        var h = go.AddComponent<HorizontalLayoutGroup>();
        h.spacing = espacio; h.padding = new RectOffset(relleno, relleno, relleno, relleno); h.childAlignment = alineacion;
        h.childControlWidth = true; h.childControlHeight = true;
        h.childForceExpandWidth = false; h.childForceExpandHeight = false;
        return h;
    }

    static void AlPulsar(Button b, UnityAction accion) { UnityEventTools.AddPersistentListener(b.onClick, accion); }
    static void AlPulsarObjeto(Button b, UnityAction<GameObject> accion, GameObject arg) { UnityEventTools.AddObjectPersistentListener(b.onClick, accion, arg); }
    static void AlCambiar(Slider s, UnityAction<float> accion) { UnityEventTools.AddPersistentListener<float>(s.onValueChanged, accion); }
    static void AlCambiar(Toggle t, UnityAction<bool> accion) { UnityEventTools.AddPersistentListener<bool>(t.onValueChanged, accion); }

    static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out Color c); return c; }
    static Color A(Color c, float a) { c.a = a; return c; }

    // =====================================================================================
    // Compilar el .exe
    // =====================================================================================
    [MenuItem("Flappy Rosso/Compilar juego para Windows")]
    public static void CompilarWindows() { Compilar("Builds/FlappyRosso/FlappyRosso.exe"); }

    // Linea de comandos: construye las dos escenas y compila el .exe en un solo paso
    public static void ConstruirYCompilar() { ConstruirTodo(); CompilarWindows(); }


    static void Compilar(string ruta)
    {
        var opciones = new BuildPlayerOptions
        {
            scenes = new[] { ESCENA_MENU, ESCENA_FLAPPY },
            locationPathName = ruta,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var informe = BuildPipeline.BuildPlayer(opciones);
        Debug.Log("[ConstructorInterfaz] Compilacion: " + informe.summary.result + " · " + (informe.summary.totalSize / (1024 * 1024)) + " MB");
        if (Application.isBatchMode && informe.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
