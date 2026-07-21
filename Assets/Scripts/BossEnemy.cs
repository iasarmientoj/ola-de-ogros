using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator anim;

    [Header("Boss Health & Stats")]
    public int maxHealth = 1000;
    public int currentHealth;
    public float attackRange = 4.0f; // Distancia a la que inicia el ataque
    public float attackDamage = 35f;
    public float attackCooldown = 3.5f;
    public float attackDelay = 1.2f; // Momento de la animación en que cae el garrote

    [Header("Club Attack Settings (Garrote)")]
    [Tooltip("El Transform del garrote (ej: garrotetex) para medir la distancia real al golpear.")]
    public Transform clubTransform;
    [Tooltip("Radio de alcance del impacto del garrote para dañar al jugador.")]
    public float clubDamageRadius = 2.5f;

    [Header("Audio Settings")]
    [Tooltip("Sonido que se reproducirá cuando el garrote del Boss golpee al jugador.")]
    public AudioClip attackHitSound;
    [Tooltip("Volumen del sonido del impacto del golpe.")]
    [Range(0f, 1f)] public float hitSoundVolume = 1.0f;

    [Header("Rotation Alignment")]
    [Tooltip("Ángulo de ajuste de orientación en grados (eje Y). Úsalo si el modelo ataca o camina chueco (ej: 90, -90, 45).")]
    public float rotationOffsetAngle = 0f;

    [Header("Visual Feedback / Damage Color")]
    [Tooltip("El Renderer principal de la malla del Boss (ej: Mesh_0 o boss-base).")]
    public Renderer bossRenderer;
    [Tooltip("Color del destello al recibir daño.")]
    public Color hitFlashColor = Color.red;
    public float flashDuration = 0.15f;
    private Color originalColor = Color.white;

    [Header("Minion Spawners Settings")]
    [Tooltip("Los prefabs de los otros 5 enemigos que pueden nacer.")]
    public GameObject[] minionPrefabs;
    [Tooltip("Puntos de aparición / spawners alrededor de la arena.")]
    public Transform[] minionSpawnPoints;

    private float lastAttackTime = 0f;
    private float accumulatedDamageForHitAnim = 0f;
    private bool hasSpawned50Percent = false;
    private bool hasSpawned10Percent = false;
    private bool isDead = false;

    public bool IsDead => isDead;

    void Start()
    {
        // Se suma al contador global de enemigos vivos para integrarse con el WaveManager
        Enemy.aliveEnemies++;

        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (bossRenderer == null)
        {
            bossRenderer = GetComponentInChildren<Renderer>();
        }

        if (bossRenderer != null && bossRenderer.material != null)
        {
            if (bossRenderer.material.HasProperty("_Color"))
                originalColor = bossRenderer.material.color;
            else if (bossRenderer.material.HasProperty("_BaseColor"))
                originalColor = bossRenderer.material.GetColor("_BaseColor");
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        SetupClubHitbox();
    }

    void Update()
    {
        if (isDead) return;

        if (player != null && agent != null && agent.enabled)
        {
            LookAtPlayer();

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                agent.isStopped = true;

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, rotationOffsetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    void SetupClubHitbox()
    {
        if (clubTransform != null)
        {
            Collider col = clubTransform.GetComponent<Collider>();
            if (col == null)
            {
                col = clubTransform.gameObject.AddComponent<BoxCollider>();
            }
            col.isTrigger = true;

            BossClubHitbox hitbox = clubTransform.GetComponent<BossClubHitbox>();
            if (hitbox == null)
            {
                hitbox = clubTransform.gameObject.AddComponent<BossClubHitbox>();
            }
            hitbox.damage = attackDamage;
            hitbox.hitSound = attackHitSound;
            hitbox.soundVolume = hitSoundVolume;
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("attack");
        }
    }

    /// <summary>
    /// Recibe daño de proyectiles o colisión de Hitbox.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        accumulatedDamageForHitAnim += damage;

        // Feedback visual: parpadeo rojo y tinte progresivo
        StartCoroutine(FlashDamageColor());

        // Cada 10% de daño total recibido (ej. 100 de 1000 HP), ejecuta la animación de recibir impacto
        float tenPercentHP = maxHealth * 0.10f;
        if (accumulatedDamageForHitAnim >= tenPercentHP)
        {
            accumulatedDamageForHitAnim = 0f;
            if (anim != null)
            {
                anim.SetTrigger("hit");
            }
        }

        // Al llegar a la mitad de vida (50%), invoca enemigos aleatorios en cada spawner
        if (!hasSpawned50Percent && currentHealth <= maxHealth * 0.5f)
        {
            hasSpawned50Percent = true;
            SpawnMinionsAtSpawners();
        }

        // Al llegar al 10% de vida restante, invoca otra oleada de refuerzos
        if (!hasSpawned10Percent && currentHealth <= maxHealth * 0.10f)
        {
            hasSpawned10Percent = true;
            SpawnMinionsAtSpawners();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamageColor()
    {
        if (bossRenderer == null || bossRenderer.material == null) yield break;

        // Destello rojo al recibir impacto
        SetMaterialColor(hitFlashColor);
        yield return new WaitForSeconds(flashDuration);

        // Retorna a un color que se va tiñendo paulatinamente más rojo según la vida restante
        float healthPercent = (float)Mathf.Max(0, currentHealth) / maxHealth;
        Color targetColor = Color.Lerp(Color.red, originalColor, healthPercent);
        SetMaterialColor(targetColor);
    }

    private void SetMaterialColor(Color color)
    {
        if (bossRenderer == null || bossRenderer.material == null) return;

        if (bossRenderer.material.HasProperty("_BaseColor"))
            bossRenderer.material.SetColor("_BaseColor", color);
        else if (bossRenderer.material.HasProperty("_Color"))
            bossRenderer.material.color = color;
    }

    private void SpawnMinionsAtSpawners()
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0) return;
        if (minionSpawnPoints == null || minionSpawnPoints.Length == 0) return;

        Debug.Log("¡EL BOSS INVOCA REFUERZOS ALEATORIOS EN LOS SPAWNERS!");

        foreach (Transform spawnPoint in minionSpawnPoints)
        {
            if (spawnPoint != null)
            {
                // Elegir aleatoriamente uno de los prefabs de los otros 5 enemigos
                int randomIndex = Random.Range(0, minionPrefabs.Length);
                GameObject chosenPrefab = minionPrefabs[randomIndex];

                if (chosenPrefab != null)
                {
                    Instantiate(chosenPrefab, spawnPoint.position, spawnPoint.rotation);
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Enemy.aliveEnemies--;

        if (agent != null) agent.isStopped = true;
        if (anim != null) anim.SetTrigger("die");

        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 6f);
    }

    /// <summary>
    /// Restablece la salud y estado del Boss al reintentar la horda.
    /// </summary>
    public void ResetBoss()
    {
        isDead = false;
        currentHealth = maxHealth;
        hasSpawned50Percent = false;
        hasSpawned10Percent = false;
        accumulatedDamageForHitAnim = 0f;

        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = true;
        }

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        SetMaterialColor(originalColor);
        gameObject.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar en la ventana Scene el radio de golpe del garrote
        if (clubTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clubTransform.position, clubDamageRadius);
        }
    }
}
