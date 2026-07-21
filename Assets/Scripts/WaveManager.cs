using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SpawnerConfig
{
    [Tooltip("El punto/spawner desde donde nacerán los enemigos.")]
    public Transform spawnerTransform;
    [Tooltip("El prefab del enemigo específico para este spawner en esta horda.")]
    public GameObject enemyPrefab;
    [Tooltip("La cantidad de enemigos de este tipo que nacerán de este spawner.")]
    public int amountToSpawn;
}

[System.Serializable]
public class WaveConfig
{
    [Header("Información General")]
    public string waveName = "Nueva Horda";
    [Tooltip("Tiempo de espera en segundos antes de empezar a spawnear.")]
    public float delayBeforeSpawning = 3f;
    [Tooltip("Rango de intervalo de tiempo en segundos entre nacimientos individuales (min/max).")]
    [MinMaxSlider(0f, 10f)]
    public Vector2 spawnIntervalRange = new Vector2(1f, 2f);

    [Header("Multiplicadores de Horda")]
    [Tooltip("Multiplicador de vida de los enemigos creados en esta horda.")]
    public float healthMultiplier = 1.0f;
    [Tooltip("Multiplicador de velocidad de movimiento (NavMeshAgent) de los enemigos.")]
    public float speedMultiplier = 1.0f;
    [Tooltip("Multiplicador de daño de ataque de los enemigos.")]
    public float damageMultiplier = 1.0f;

    [Header("Configuración de Spawners")]
    public SpawnerConfig[] spawnerConfigs;
    
    [Header("Puertas a Desbloquear/Abrir")]
    [Tooltip("Controladores de las puertas que se desbloquearán y abrirán al ganar esta horda.")]
    public DoorController[] doorsToOpen;

    [Header("Acciones Adicionales al Completar")]
    [Tooltip("Objetos que se activarán al ganar la horda (ej. cofres de botiquines, luces extra).")]
    public GameObject[] objectsToActivate;
    [Tooltip("Objetos que se desactivarán al ganar la horda (ej. barreras de energía).")]
    public GameObject[] objectsToDeactivate;

    [Header("Checkpoint del Jugador")]
    [Tooltip("Punto de aparición (spawn) del jugador al reintentar esta horda.")]
    public Transform playerSpawnPoint;
}

public class WaveManager : MonoBehaviour
{
    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI waveText;
    [Tooltip("Texto opcional para mostrar la cantidad de enemigos restantes.")]
    public TextMeshProUGUI enemiesLeftText;
    [Tooltip("El panel/pantalla de victoria (Image - game win) de la UI.")]
    public GameObject gameWinPanel;
    [Tooltip("El panel/pantalla de pausa (Image - game pause) de la UI.")]
    public GameObject gamePausePanel;

    [Header("Configuración General de Hordas")]
    [Tooltip("Tiempo de transición y calma entre que se gana una horda y empieza el conteo de la siguiente.")]
    public float timeBetweenWaves = 5f;
    public WaveConfig[] waves;

    private int currentWaveIndex = 0;
    private int totalEnemiesToSpawnThisWave = 0;
    private int enemiesSpawnedSoFarThisWave = 0;
    private bool isSpawning = false;
    private bool allWavesCompleted = false;
    private bool waveTransitioning = false;

    void Start()
    {
        // Reiniciar contador de enemigos vivos por seguridad al empezar la escena
        Enemy.aliveEnemies = 0;

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(false);
        }

        if (gamePausePanel != null)
        {
            gamePausePanel.SetActive(false);
        }
        
        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("WaveManager: ¡No has configurado ninguna horda en el Inspector!");
            if (waveText != null) waveText.text = "¡Sin Hordas configuradas!";
            return;
        }
        
        waveTransitioning = true;
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        waveTransitioning = false;
        WaveConfig currentWave = waves[currentWaveIndex];
        enemiesSpawnedSoFarThisWave = 0;
        
        // Calcular el total de enemigos requeridos para esta horda
        totalEnemiesToSpawnThisWave = 0;
        foreach (var config in currentWave.spawnerConfigs)
        {
            totalEnemiesToSpawnThisWave += config.amountToSpawn;
        }

        // Mostrar UI de inicio de horda
        if (waveText != null)
        {
            waveText.gameObject.SetActive(true);
            waveText.text = $"Horda {currentWaveIndex + 1}\n{currentWave.waveName}";
            StartCoroutine(HideWaveTextAfterDelay(5f));
        }

        // Esperar el retraso configurado antes de empezar a spawnear
        yield return new WaitForSeconds(currentWave.delayBeforeSpawning);

        isSpawning = true;
        StartCoroutine(SpawnEnemiesForWave(currentWave));
    }

    IEnumerator HideWaveTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (waveText != null) waveText.gameObject.SetActive(false);
    }

    IEnumerator SpawnEnemiesForWave(WaveConfig wave)
    {
        int spawnersFinished = 0;
        int totalSpawners = wave.spawnerConfigs.Length;

        if (totalSpawners == 0)
        {
            isSpawning = false;
            yield break;
        }

        foreach (var config in wave.spawnerConfigs)
        {
            StartCoroutine(SpawnIndividualSpawner(config, wave, () => {
                spawnersFinished++;
            }));
        }

        while (spawnersFinished < totalSpawners)
        {
            yield return null;
        }
        
        isSpawning = false;
    }

    IEnumerator SpawnIndividualSpawner(SpawnerConfig config, WaveConfig wave, System.Action onComplete)
    {
        int remaining = config.amountToSpawn;

        while (remaining > 0)
        {
            if (config.spawnerTransform != null && config.enemyPrefab != null)
            {
                // Instanciar el enemigo en el spawner
                GameObject spawnedEnemy = Instantiate(config.enemyPrefab, config.spawnerTransform.position, config.spawnerTransform.rotation);
                
                // Aplicar multiplicadores de horda
                Enemy meleeEnemy = spawnedEnemy.GetComponent<Enemy>();
                if (meleeEnemy != null)
                {
                    meleeEnemy.ScaleStats(wave.healthMultiplier, wave.speedMultiplier, wave.damageMultiplier);
                }
                else
                {
                    RangedEnemy rangedEnemy = spawnedEnemy.GetComponent<RangedEnemy>();
                    if (rangedEnemy != null)
                    {
                        rangedEnemy.ScaleStats(wave.healthMultiplier, wave.speedMultiplier, wave.damageMultiplier);
                    }
                }

                enemiesSpawnedSoFarThisWave++;
                remaining--;
            }

            if (remaining > 0)
            {
                // Elegir un intervalo aleatorio entre el rango definido para la horda
                float randomInterval = Random.Range(wave.spawnIntervalRange.x, wave.spawnIntervalRange.y);
                yield return new WaitForSeconds(randomInterval);
            }
        }

        onComplete?.Invoke();
    }

    [HideInInspector] public bool isPaused = false;

    void Update()
    {
        // Detectar presionar Escape o P para pausar/reanudar
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (isPaused || allWavesCompleted) return;

        bool isLastWave = (currentWaveIndex == waves.Length - 1);
        BossEnemy bossInScene = FindFirstObjectByType<BossEnemy>();

        // Actualizar el UI de enemigos restantes si está asignado
        if (enemiesLeftText != null)
        {
            if (isLastWave && bossInScene != null && !bossInScene.IsDead)
            {
                enemiesLeftText.text = "¡DERROTA AL JEFE FINAL!";
                enemiesLeftText.gameObject.SetActive(true);
            }
            else
            {
                int remainingToSpawn = totalEnemiesToSpawnThisWave - enemiesSpawnedSoFarThisWave;
                int totalEnemiesRemaining = remainingToSpawn + Enemy.aliveEnemies;

                if (totalEnemiesRemaining > 0)
                {
                    enemiesLeftText.text = $"Enemigos restantes: {totalEnemiesRemaining}";
                    enemiesLeftText.gameObject.SetActive(true);
                }
                else
                {
                    enemiesLeftText.gameObject.SetActive(false);
                }
            }
        }

        // Detectar si la horda ha terminado
        if (!waveTransitioning)
        {
            if (isLastWave)
            {
                // En la última horda, el jugador gana la partida al derrotar al Boss (independiente de los minions)
                if (bossInScene != null && bossInScene.IsDead)
                {
                    waveTransitioning = true;
                    OnWaveCompleted();
                }
            }
            else
            {
                // En hordas normales, termina cuando ya spawnearon todos y no queda ninguno vivo
                if (!isSpawning && enemiesSpawnedSoFarThisWave >= totalEnemiesToSpawnThisWave && Enemy.aliveEnemies <= 0)
                {
                    waveTransitioning = true;
                    OnWaveCompleted();
                }
            }
        }
    }

    void OnWaveCompleted()
    {
        WaveConfig completedWave = waves[currentWaveIndex];
        Debug.Log($"WaveManager: ¡Horda {currentWaveIndex + 1} completada!");

        // Desbloquear y abrir las puertas asociadas
        if (completedWave.doorsToOpen != null)
        {
            foreach (var door in completedWave.doorsToOpen)
            {
                if (door != null)
                {
                    door.UnlockAndOpen();
                }
            }
        }

        // Activar objetos de recompensa/escenario
        if (completedWave.objectsToActivate != null)
        {
            foreach (var obj in completedWave.objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        // Desactivar objetos de bloqueo/escenario
        if (completedWave.objectsToDeactivate != null)
        {
            foreach (var obj in completedWave.objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // Avanzar a la siguiente horda
        currentWaveIndex++;
        if (currentWaveIndex < waves.Length)
        {
            StartCoroutine(WaitAndStartNextWave());
        }
        else
        {
            allWavesCompleted = true;
            if (waveText != null)
            {
                waveText.gameObject.SetActive(true);
                waveText.text = "¡VICTORIA TOTAL!\nTodos los sectores han sido defendidos.";
            }
            if (enemiesLeftText != null) enemiesLeftText.gameObject.SetActive(false);

            if (gameWinPanel != null)
            {
                gameWinPanel.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Debug.Log("WaveManager: ¡Todas las hordas completadas! Fin de partida.");
        }
    }

    IEnumerator WaitAndStartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(StartNextWave());
    }

    /// <summary>
    /// Reinicia la horda actual desde el checkpoint (llamado por el botón Reintentar).
    /// </summary>
    public void RetryCurrentWave()
    {
        // 1. Despausar el juego
        Time.timeScale = 1f;

        // 2. Buscar y restablecer al jugador
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.ResetPlayerAfterDeath();

            // Mover al jugador al checkpoint de esta horda
            WaveConfig currentWave = waves[currentWaveIndex];
            if (currentWave.playerSpawnPoint != null)
            {
                CharacterController cc = playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false; // Desactivar temporalmente para que permita cambiar la posición
                
                playerController.transform.position = currentWave.playerSpawnPoint.position;
                playerController.transform.rotation = currentWave.playerSpawnPoint.rotation;
                
                if (cc != null) cc.enabled = true;
            }
        }

        // 3. Eliminar todos los enemigos y proyectiles activos de la escena
        Enemy[] meleeEnemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in meleeEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        RangedEnemy[] rangedEnemies = FindObjectsOfType<RangedEnemy>();
        foreach (var enemy in rangedEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        Arrow[] arrows = FindObjectsOfType<Arrow>();
        foreach (var arrow in arrows)
        {
            if (arrow != null) Destroy(arrow.gameObject);
        }

        // Si hay un Boss en la escena, restablecer su salud y estado
        BossEnemy boss = FindFirstObjectByType<BossEnemy>(FindObjectsInactive.Include);
        if (boss != null)
        {
            boss.ResetBoss();
        }

        // Restablecer contadores
        Enemy.aliveEnemies = 0;
        enemiesSpawnedSoFarThisWave = 0;

        // 4. Detener procesos activos de spawning y reiniciar la horda
        StopAllCoroutines();
        isSpawning = false;
        waveTransitioning = true;

        StartCoroutine(StartNextWave());
    }

    /// <summary>
    /// Carga la escena del menú (llamado por el botón Volver al Menú).
    /// </summary>
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Carga la primera escena en el Build Settings (usualmente el menú)
    }

    /// <summary>
    /// Conmuta entre pausar y reanudar el juego.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    /// <summary>
    /// Pausa el juego y muestra el panel de pausa.
    /// </summary>
    public void PauseGame()
    {
        // No pausar si el jugador está muerto
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null && player.isDead) return;

        isPaused = true;
        if (gamePausePanel != null) gamePausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Reanuda el juego y oculta el panel de pausa (llamado por el botón CONTINUAR).
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        if (gamePausePanel != null) gamePausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
