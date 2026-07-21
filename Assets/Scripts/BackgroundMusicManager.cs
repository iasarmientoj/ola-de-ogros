using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Patron Singleton: Si ya existe un reproductor de música activo, destruye este duplicado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Mantiene la música sonando al cambiar entre escenas

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
    }

    /// <summary>
    /// Inicia la música de fondo o mantiene la actual sin reiniciarla si es la misma.
    /// </summary>
    public static void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        if (clip == null) return;

        if (Instance == null)
        {
            GameObject musicObj = new GameObject("BackgroundMusicManager");
            Instance = musicObj.AddComponent<BackgroundMusicManager>();
        }

        if (Instance.audioSource == null)
        {
            Instance.audioSource = Instance.gameObject.AddComponent<AudioSource>();
            Instance.audioSource.loop = true;
        }

        // Si la misma canción ya está sonando, ajusta el volumen sin reiniciarla
        if (Instance.audioSource.clip == clip && Instance.audioSource.isPlaying)
        {
            Instance.audioSource.volume = volume;
            return;
        }

        Instance.audioSource.clip = clip;
        Instance.audioSource.volume = volume;
        Instance.audioSource.Play();
    }

    /// <summary>
    /// Modifica el volumen de la música de fondo.
    /// </summary>
    public static void SetVolume(float volume)
    {
        if (Instance != null && Instance.audioSource != null)
        {
            Instance.audioSource.volume = volume;
        }
    }
}
