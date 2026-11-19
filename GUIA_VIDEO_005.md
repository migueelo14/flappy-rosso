# Guía para grabar el vídeo de la tarea 005 (máx. 4 min)

Nombre del archivo: `005_DDI_Redondo_Orillan_Miguel.mp4`
Graba con Win + Alt + R (Barra de juego) o con OBS. Pon rótulos o habla: cada cosa tiene que decir qué es.

> Antes de grabar: completa en la escena **Menu › CanvasMenu › ZonaSegura › PanelCreditos › … › ContenidoCreditos**
> los textos que tienen `[autor]` y `[licencia]` (personajes, vídeo de fondo, música y efectos).

---

## 1. El editor (≈ 40 s) · 0:00

- Abre **Menu** y despliega la Hierarchy: `CanvasMenu › ZonaSegura › PanelInicio / PanelSeleccion / PanelCreditos`, `PanelOpciones` (prefab), `DialogoConfirmar`, `MenuManager`, `EventSystem`.
- Abre **Flappy** y despliega: `CanvasFlappy › ZonaSegura › PanelHUD`, `PanelPausa`, `PanelOpciones`, `PanelFinPartida`, `DialogoConfirmar`, `Pausa`, `GameController`.
- Selecciona `CanvasMenu` y enseña en el Inspector:
  - **Render Mode: Screen Space – Overlay** → «la interfaz se dibuja encima de todo y se adapta sola a la resolución; no necesito partículas delante de la UI ni UI en el mundo».
  - **Canvas Scaler: Scale With Screen Size, 1920 × 1080, Match Width or Height = 0,5** → «con Constant Pixel Size los botones serían diminutos en 4K; 0,5 reparte el ajuste entre ancho y alto, así en 4:3 nada se sale».

## 2. La colocación (≈ 40 s) · 0:40

- Vista Game en **16:9 (1920×1080)** y en **4:3** (añádela con `+` si no está): Inicio y HUD.
  - En 4:3 el personaje decorativo de Inicio se oculta (script `OcultarSiEstrecho`).
- Selecciona en el HUD `GrupoMonedas` (anclado arriba a la izquierda), `GrupoPuntuacion` (arriba al centro) y `GrupoBotonesHUD` (arriba a la derecha) para que se vea el ancla.
- `ZonaSegura` lleva el script del mismo nombre (Screen.safeArea).
- En `PanelOpciones › ZonaSeguraOpciones › GrupoSonido` enseña el **Vertical Layout Group** (Spacing 20, Padding 40).

## 3. Las propiedades (≈ 25 s) · 1:20

- Selecciona `BotonJugar`:
  - **Image Type = Sliced** (sprite `ui_boton`, 96×96, Border 24).
  - **Button › Transition = Color Tint** con los cinco estados: Normal `#FF2E88`, Highlighted `#FF5FA2`, Pressed `#D91B6E`, Selected `#FF2E88` (+ borde cian, escala 1,08 y flecha ▶ por `IndicadorFoco`), Disabled `#3A2B4A`.
- Selecciona `TextoTituloOpciones` (o `TextoLogo`):
  - **Font Asset: PressStart2P Rosso SDF** (atlas con ñ, tildes, ü, ¿ y ¡).
  - **Material Preset: PressStart2P Rosso SDF Neon** (o *Contorno*).

## 4. Los eventos (≈ 25 s) · 1:45

- `BotonJugar` › **On Click ()** → `MenuManager.Mostrar (GameObject)` con `PanelSeleccion`.
- `SliderMusica` (dentro de `PanelOpciones`) › **On Value Changed (Single)** → `Ajustes.CambiarMusica` **en la sección Dynamic float**.

## 5. El código (≈ 25 s) · 2:10

Abre `Assets/Scripts/GameController.cs` (método `BirdDie`) y explica:
- La **medalla** (extra B5) y los mensajes del diseño: «¡Nuevo récord!», «¡Medalla de bronce!», «¡Casi! Te faltan 3 para el récord».
- La **dificultad** con el extra B2: la velocidad inicial sale de la dificultad elegida (`GameSettings.ScrollSpeed`) y la progresiva sube a partir de ella con un límite proporcional; el récord se guarda en la dificultad con la que se jugó.
- Reintentar se activa a los 0,6 s para que el Espacio del último aleteo no reintente sin querer.

Alternativa: `Assets/Scripts/GameSettings.cs`, método `ApplyToMixer` → el volumen pasa por el **Audio Mixer**
(`Assets/Audio/Mezclador.mixer`, grupos Musica y Efectos con **VolMusica** y **VolEfectos** expuestos).
El deslizador va de 0 a 1 y el mezclador en decibelios: `Log10(v) * 20`; 0,0001 equivale a silencio (−80 dB),
y silenciar pone los dos grupos a −80 dB. Se aplica en `Start` de `MusicPlayer` porque `SetFloat` no funciona en `Awake`.

## 6. El juego exportado (≈ 60 s) · 2:35

Abre `Builds/FlappyRosso/FlappyRosso.exe` y haz, por este orden:
1. Inicio (el foco está en Jugar) → **Opciones** → baja la **Música** con ← → → Volver.
2. **Jugar** → Selección (elige otro personaje con ← →) → **¡A la pista!**
3. Partida con «Pulsa para empezar» → Espacio → pulsa **M** (silencio; el icono del HUD cambia).
4. **Esc** → Pausa (todo congelado, también el sonido) → Esc para volver.
5. Choca → **Fin de partida** con medalla → **Cambiar personaje**.
6. Cierra el juego, ábrelo de nuevo y enseña en Opciones que los ajustes siguen igual.

## 7. Del diseño al juego (≈ 15 s) · 3:35

Frase propuesta: «Respecto a la tarea 004 añadí **Reiniciar** a la pausa (lo pedía la tarea) y quité **Salir** de ahí para no pasar de 4 opciones; los créditos pasan a un **Scroll View** porque con el autor y la licencia de cada recurso ya no caben en seis líneas».

---

## Cambios respecto a la tarea 004 (por si te preguntan)

| Cambio | Por qué |
|---|---|
| Pausa: **Reiniciar** en lugar de **Salir** | La tarea 005 pide Continuar, Opciones, Reiniciar y Menú; así no pasa de 4 opciones. Se sale desde Inicio. |
| Créditos con **Scroll View** | Con autor y licencia de cada recurso ya no caben en seis líneas. |
| Logo de Inicio sobre una **placa** | El vídeo de fondo ya trae el neón «ROSSO by Antique» y sin placa se pisaban. |
| Ignacio se desbloquea con **100 tickets o 15 VIP** | Es lo que dice el texto del diseño (antes el código pedía las dos cosas). |
| «Llamar a Luis» ya no está en el fin de partida | Reintentar reinicia directamente, como en el diseño (Espacio a los 0,6 s). |

## Si cambias algo y quieres rehacer la interfaz

Menú de Unity **Flappy Rosso › Construir interfaz (Menu y Flappy)**: borra los dos Canvas y los vuelve a crear
(¡pierdes lo que hayas retocado a mano en ellos!). **Flappy Rosso › Compilar juego para Windows** genera el .exe.
