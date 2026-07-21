using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles de la UI")]
    public GameObject mainButtonsPanel;
    public GameObject controlsPanel;
    public GameObject optionsPanel;

    [Header("Opciones de Configuración")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    [Header("Audio de Botones")]
    [Tooltip("El sonido que se reproducirá al presionar cualquiera de los botones del menú.")]
    public AudioClip buttonClickSound;
    [Tooltip("El sonido que se reproducirá al pasar el ratón por encima (Hover) de cualquiera de los botones del menú.")]
    public AudioClip buttonHoverSound;
    [Range(0f, 1f)] public float buttonSoundVolume = 1.0f;

    [Header("Música de Fondo")]
    [Tooltip("Clip de música de fondo que sonará continuamente durante el menú y el juego.")]
    public AudioClip backgroundMusicClip;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Nombre de la Escena de Juego")]
    public string gameSceneName = "Juego 1";

    void Start()
    {
        // Asegurar que el tiempo esté corriendo normalmente al entrar al menú
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Mostrar panel principal y ocultar subpaneles por defecto
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Cargar valores guardados o establecer valores por defecto
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (sensitivitySlider != null)
        {
            float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 100f);
            sensitivitySlider.value = savedSens;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        // Asignar automáticamente sonido de Clic y Hover a todos los botones del menú
        Button[] allMenuButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in allMenuButtons)
        {
            btn.onClick.AddListener(PlayButtonClickSound);

            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btn.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { PlayButtonHoverSound(); });
            trigger.triggers.Add(entry);
        }

        // Iniciar la música de fondo que persistirá entre escenas
        if (backgroundMusicClip != null)
        {
            BackgroundMusicManager.PlayMusic(backgroundMusicClip, musicVolume);
        }
    }

    /// <summary>
    /// Inicia la partida cargando la escena del juego.
    /// </summary>
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Abre el panel de controles.
    /// </summary>
    public void OpenControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    /// <summary>
    /// Cierra el panel de controles y vuelve al menú principal.
    /// </summary>
    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    /// <summary>
    /// Abre el panel de opciones.
    /// </summary>
    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    /// <summary>
    /// Cierra el panel de opciones y vuelve al menú principal.
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    /// <summary>
    /// Ajusta el volumen general del juego.
    /// </summary>
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    /// <summary>
    /// Ajusta la sensibilidad del ratón.
    /// </summary>
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }

    /// <summary>
    /// Cierra la aplicación (funciona en builds ejecutables).
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    /// <summary>
    /// Reproduce el sonido de clic del botón.
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            Vector3 spawnPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(buttonClickSound, spawnPos, buttonSoundVolume);
        }
    }

    /// <summary>
    /// Reproduce el sonido cuando el puntero del ratón pasa sobre un botón (Hover).
    /// </summary>
    public void PlayButtonHoverSound()
    {
        if (buttonHoverSound != null)
        {
            Vector3 spawnPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(buttonHoverSound, spawnPos, buttonSoundVolume);
        }
    }
}
