using UnityEngine;
using TMPro;

public class HeadshotUI : MonoBehaviour
{
    public static HeadshotUI Instance;
    private TextMeshProUGUI textMesh;
    private float displayTimer = 0f;

    void Awake()
    {
        Instance = this;
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = "";
        }
    }

    void OnEnable()
    {
        Instance = this;
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// Muestra un texto en pantalla al realizar un tiro crítico / headshot.
    /// </summary>
    public static void ShowHeadshotText(string message = "¡HEADSHOT!")
    {
        // Buscar el componente en la escena si no se ha registrado aún
        if (Instance == null)
        {
            Instance = FindFirstObjectByType<HeadshotUI>(FindObjectsInactive.Include);
        }

        if (Instance != null)
        {
            if (Instance.textMesh == null)
            {
                Instance.textMesh = Instance.GetComponent<TextMeshProUGUI>();
            }

            if (Instance.textMesh != null)
            {
                Instance.gameObject.SetActive(true);
                Instance.textMesh.enabled = true;
                Instance.textMesh.text = message;
                Instance.displayTimer = 1.2f; // Se muestra durante 1.2 segundos
            }
        }
        else
        {
            Debug.LogWarning("¡Atención! No se ha asignado el componente 'HeadshotUI' a ningún texto TextMeshPro en el Canvas.");
        }
    }

    void Update()
    {
        if (displayTimer > 0)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0 && textMesh != null)
            {
                textMesh.text = "";
            }
        }
    }
}
