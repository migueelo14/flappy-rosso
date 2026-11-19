using UnityEngine;
using UnityEngine.Video;

// Pone el video de la discoteca de fondo, cubriendo toda la camara sea cual sea el tamaño de la ventana.
// Mientras el video se prepara se ve una imagen fija (el primer fotograma) para que no salga negro.
[RequireComponent(typeof(VideoPlayer))]
public class VideoBackground : MonoBehaviour
{
    public float videoAspect = 16f / 9f;

    private VideoPlayer player;
    private Material material;
    private RenderTexture renderTexture;

    void Start()
    {
        FitToCamera();
        material = GetComponent<MeshRenderer>().material;
        renderTexture = new RenderTexture(1280, 720, 0);

        player = GetComponent<VideoPlayer>();
        player.renderMode = VideoRenderMode.RenderTexture;
        player.targetTexture = renderTexture;
        player.audioOutputMode = VideoAudioOutputMode.None;   // la musica va aparte, en MusicPlayer
        player.isLooping = true;
        player.prepareCompleted += OnPrepared;
        player.Prepare();
    }

    void OnPrepared(VideoPlayer p)
    {
        material.SetTexture("_BaseMap", renderTexture);
        material.SetTexture("_MainTex", renderTexture);
        p.Play();
    }

    void LateUpdate()
    {
        FitToCamera();
    }

    void FitToCamera()
    {
        Camera cam = Camera.main;
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        if (w / h > videoAspect) h = w / videoAspect;
        else w = h * videoAspect;
        transform.localScale = new Vector3(w, h, 1f);
    }

    void OnDestroy()
    {
        if (renderTexture != null) renderTexture.Release();
    }
}
