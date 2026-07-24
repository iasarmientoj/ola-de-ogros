using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controlador del Minijuego de Barriles por Hordas / Niveles.
/// Cada base representa un nivel de dificultad progresivo (más lejano).
/// </summary>
public class BarrelMinigameManager : MonoBehaviour
{
    [Header("--- PREFABS Y BASES ---")]
    [Tooltip("El prefab del barril (ej. barril.prefab).")]
    public GameObject barrelPrefab;

    [Tooltip("Objeto padre principal de los targets (ej. 'targets'). Si se asigna, obtiene automáticamente todas las bases en orden.")]
    public Transform targetsParent;

    [Tooltip("Lista de bases. Si se asigna 'targetsParent', esta lista se llena automáticamente en el Start.")]
    public List<Transform> baseParents = new List<Transform>();

    [Header("--- CONFIGURACIÓN DEL MINIJUEGO ---")]
    [Tooltip("Tiempo límite en segundos (ej. 60 para 1 minuto).")]
    public float gameDuration = 60f;

    [Tooltip("Cantidad de barriles que deben destruirse para pasar al siguiente nivel.")]
    public int barrelsPerLevel = 5;

    [Header("--- REFERENCIAS DE UI ---")]
    [Tooltip("Texto para mostrar el título de la horda/nivel. Vincular a: Text (TMP) - titulos hordas")]
    public TextMeshProUGUI hordeTitleText;

    [Tooltip("Texto para mostrar el temporizador. Vincular a: Text (TMP) - enemigos")]
    public TextMeshProUGUI timerText;

    [Tooltip("Panel emergente de Game Over. Vincular a: Image - game over")]
    public GameObject gameOverPanel;

    [Tooltip("Texto de resultados en Game Over. Vincular a: Text (TMP) - titulos hordas (1)")]
    public TextMeshProUGUI gameOverStatsText;

    [Tooltip("Botón para reintentar la horda. Vincular a: Button - reintentar")]
    public Button retryButton;

    // --- ESTADO INTERNO ---
    private int currentLevelIndex = 0; // Index 0 = Nivel 1 (Base 0)
    private int barrelsDestroyedInCurrentLevel = 0;
    private int totalBarrelsDestroyed = 0;
    private float timeRemaining;
    private bool isGameActive = false;

    private GameObject currentActiveBarrel;
    private Transform lastSpawnTarget;

    private const string BEST_LEVEL_KEY = "BarrelMinigame_BestLevel";

    private void Start()
    {
        // Si hay botón de reintentar asignado, vincular su evento al hacer clic
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(Reintentar);
        }

        // Suscribir al evento estático cuando se destruye/explota un barril
        ExplosiveBarrel.OnBarrelDestroyed += OnBarrelDestroyed;

        // Iniciar la partida
        StartGame();
    }

    private void OnDestroy()
    {
        // Cancelar suscripción al destruir la instancia
        ExplosiveBarrel.OnBarrelDestroyed -= OnBarrelDestroyed;
    }

    private void Update()
    {
        if (!isGameActive) return;

        // Cuenta regresiva del temporizador
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerUI();
            GameOver();
            return;
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// Carga automáticamente las bases hijas desde el objeto padre 'targetsParent' o buscando el objeto 'targets'.
    /// </summary>
    private void LoadBasesFromParent()
    {
        if (targetsParent != null)
        {
            baseParents.Clear();
            foreach (Transform childBase in targetsParent)
            {
                baseParents.Add(childBase);
            }
        }
        else if (baseParents == null || baseParents.Count == 0)
        {
            GameObject autoFoundTargets = GameObject.Find("targets");
            if (autoFoundTargets != null)
            {
                targetsParent = autoFoundTargets.transform;
                baseParents.Clear();
                foreach (Transform childBase in targetsParent)
                {
                    baseParents.Add(childBase);
                }
            }
        }
    }

    /// <summary>
    /// Inicia o reinicia el minijuego desde el nivel 1.
    /// </summary>
    public void StartGame()
    {
        LoadBasesFromParent();

        currentLevelIndex = 0;
        barrelsDestroyedInCurrentLevel = 0;
        totalBarrelsDestroyed = 0;
        timeRemaining = gameDuration;
        isGameActive = true;

        // Desbloquear tiempo y ocultar/bloquear cursor para primera persona
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateHordeTitleUI($"HORDA 1 - NIVEL 1");

        // Destruir barril activo anterior si existía
        ClearActiveBarrel();

        // Generar el primer barril
        SpawnNextBarrel();
    }

    /// <summary>
    /// Método público para vincular a botones de reintentar en el inspector.
    /// </summary>
    public void Reintentar()
    {
        StartGame();
    }

    /// <summary>
    /// Aparece un barril aleatorio en uno de los targets de la base del nivel actual.
    /// </summary>
    void SpawnNextBarrel()
    {
        if (!isGameActive) return;

        if (baseParents == null || baseParents.Count == 0)
        {
            Debug.LogWarning("[BarrelMinigameManager] ¡No hay bases asignadas en el Inspector!");
            return;
        }

        // Determinar qué base usar según el nivel actual (se usa la última base si el nivel supera la cantidad de bases)
        int effectiveBaseIndex = Mathf.Clamp(currentLevelIndex, 0, baseParents.Count - 1);
        Transform currentBaseTransform = baseParents[effectiveBaseIndex];

        if (currentBaseTransform == null) return;

        // Obtener todos los targets (hijos) válidos dentro de la base actual
        List<Transform> targets = new List<Transform>();
        foreach (Transform child in currentBaseTransform)
        {
            if (child != null)
            {
                targets.Add(child);
            }
        }

        // Fallback por si la base no tiene hijos targets
        if (targets.Count == 0)
        {
            targets.Add(currentBaseTransform);
        }

        // Seleccionar un target aleatorio evitando repetir inmediatamente si hay varios
        Transform selectedTarget = targets[Random.Range(0, targets.Count)];
        if (targets.Count > 1 && selectedTarget == lastSpawnTarget)
        {
            targets.Remove(selectedTarget);
            selectedTarget = targets[Random.Range(0, targets.Count)];
        }
        lastSpawnTarget = selectedTarget;

        // Instanciar el barril en la posición del target seleccionado
        if (barrelPrefab != null && selectedTarget != null)
        {
            currentActiveBarrel = Instantiate(barrelPrefab, selectedTarget.position, selectedTarget.rotation);
        }
    }

    /// <summary>
    /// Callback ejecutado cuando se destruye un barril en la escena.
    /// </summary>
    private void OnBarrelDestroyed(ExplosiveBarrel barrel)
    {
        if (!isGameActive) return;

        // Verificar que el barril destruido corresponda al barril activo del minijuego
        if (barrel != null && currentActiveBarrel != null)
        {
            if (barrel.gameObject != currentActiveBarrel && barrel.transform.root != currentActiveBarrel.transform.root)
            {
                return;
            }
        }

        totalBarrelsDestroyed++;
        barrelsDestroyedInCurrentLevel++;

        // Verificar si completó los 5 barriles del nivel actual
        if (barrelsDestroyedInCurrentLevel >= barrelsPerLevel)
        {
            barrelsDestroyedInCurrentLevel = 0;
            currentLevelIndex++;

            int levelNumber = currentLevelIndex + 1;
            UpdateHordeTitleUI($"¡NIVEL {levelNumber} ALCANZADO!");

            // Actualizar récord de nivel alcanzado
            int bestLevel = PlayerPrefs.GetInt(BEST_LEVEL_KEY, 1);
            if (levelNumber > bestLevel)
            {
                PlayerPrefs.SetInt(BEST_LEVEL_KEY, levelNumber);
                PlayerPrefs.Save();
            }
        }

        currentActiveBarrel = null;

        // Hacer aparecer el siguiente barril (uno a uno)
        SpawnNextBarrel();
    }

    /// <summary>
    /// Finaliza la partida al agotarse el tiempo.
    /// </summary>
    void GameOver()
    {
        isGameActive = false;
        ClearActiveBarrel();

        int reachedLevel = currentLevelIndex + 1;
        int bestLevel = PlayerPrefs.GetInt(BEST_LEVEL_KEY, 1);

        if (reachedLevel > bestLevel)
        {
            bestLevel = reachedLevel;
            PlayerPrefs.SetInt(BEST_LEVEL_KEY, bestLevel);
            PlayerPrefs.Save();
        }

        // Pausar el juego y liberar el cursor para poder presionar los botones
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Mostrar pantalla de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Mostrar estadísticas en el texto de Game Over
        if (gameOverStatsText != null)
        {
            gameOverStatsText.text = $"Barriles eliminados: {totalBarrelsDestroyed}\n" +
                                     $"Nivel alcanzado: {reachedLevel}\n" +
                                     $"Mejor nivel alcanzado: {bestLevel}";
        }
    }

    /// <summary>
    /// Limpia el barril que esté actualmente en la escena.
    /// </summary>
    void ClearActiveBarrel()
    {
        if (currentActiveBarrel != null)
        {
            Destroy(currentActiveBarrel);
            currentActiveBarrel = null;
        }
    }

    /// <summary>
    /// Actualiza la interfaz del temporizador en pantalla.
    /// </summary>
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("Tiempo: {0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Actualiza el título del nivel / horda en pantalla.
    /// </summary>
    void UpdateHordeTitleUI(string text)
    {
        if (hordeTitleText != null)
        {
            hordeTitleText.text = text;
        }
    }
}
