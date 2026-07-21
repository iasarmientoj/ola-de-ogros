using UnityEngine;
using UnityEngine.AI; // Necesario para el NavMesh

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator anim;

    [Header("Enemy Stats")]
    public int health = 5;
    public float attackRange = 2.5f;
    public float attackDamage = 20f;
    public float attackCooldown = 1.5f;
    public float attackDelay = 1.0f; // Tiempo para sincronizar con la animación (ej. 1 seg)
    public float stunDuration = 1.0f; // Tiempo que se queda quieto al recibir daño

    [Header("Audio Settings")]
    [Tooltip("Sonido ambiental/gruñido genérico del ogro.")]
    public AudioClip ambientSound;
    [Tooltip("Volumen del sonido ambiental.")]
    [Range(0f, 1f)] public float soundVolume = 0.5f;
    [Tooltip("Tiempo mínimo en segundos entre sonidos.")]
    public float minSoundDelay = 4f;
    [Tooltip("Tiempo máximo en segundos entre sonidos.")]
    public float maxSoundDelay = 8f;
    [Tooltip("Límite inferior del tono (pitch) aleatorio para el gruñido.")]
    [Range(0.5f, 1.5f)] public float minPitch = 0.85f;
    [Tooltip("Límite superior del tono (pitch) aleatorio para el gruñido.")]
    [Range(0.5f, 1.5f)] public float maxPitch = 1.15f;

    private float lastAttackTime = 0f;
    private bool isDead = false;
    private bool isHit = false;
    private AudioSource audioSource;

    public static int aliveEnemies = 0;

    void Start()
    {
        aliveEnemies++;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); // El Animator está en el modelo hijo
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.spatialBlend = 1.0f; // Sonido 3D para escuchar la dirección del enemigo
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 20f;
        }

        StartCoroutine(PlayAmbientSoundLoop());
    }

    private System.Collections.IEnumerator PlayAmbientSoundLoop()
    {
        while (!isDead)
        {
            // Esperamos un tiempo aleatorio antes del siguiente rugido/gruñido
            float waitTime = Random.Range(minSoundDelay, maxSoundDelay);
            yield return new WaitForSeconds(waitTime);

            if (isDead) yield break;

            if (audioSource != null && ambientSound != null)
            {
                // Variamos el pitch para darle naturalidad al sonido del ogro
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(ambientSound, soundVolume);
                audioSource.pitch = 1f;
            }
        }
    }

    void Update()
    {
        if (isDead) return;
        
        // Si acaba de recibir un impacto, se detiene
        if (isHit)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (player != null && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                // El enemigo deja de caminar para atacar
                agent.isStopped = true;
                
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Seguir corriendo hacia el jugador
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("attack");

        // Iniciamos la corrutina para aplicar el daño con un retraso
        StartCoroutine(DealDamageAfterDelay(attackDelay));
    }

    private System.Collections.IEnumerator DealDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Si el enemigo murió o fue aturdido mientras atacaba, no hace daño
        if (isDead || isHit) yield break;

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Verificamos si el jugador sigue cerca al momento del impacto
            if (distanceToPlayer <= attackRange)
            {
                FirstPersonController playerScript = player.GetComponent<FirstPersonController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;

        health--;
        
        if (health > 0)
        {
            // Reacción al impacto
            anim.SetTrigger("hit");
            Debug.Log("Ogre health: " + health);
            
            // Iniciar parálisis momentánea
            StartCoroutine(Stun(stunDuration)); 
        }
        else
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        
        if (health > 0)
        {
            if (anim != null) anim.SetTrigger("hit");
            StartCoroutine(Stun(stunDuration)); 
        }
        else
        {
            Die();
        }
    }

    private System.Collections.IEnumerator Stun(float duration)
    {
        isHit = true;
        yield return new WaitForSeconds(duration);
        isHit = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        aliveEnemies--;

        agent.isStopped = true;
        anim.SetTrigger("die");
        
        Debug.Log("Ogre DIED!");

        // Desactivamos collider para que no bloquee balas estando muerto
        GetComponent<Collider>().enabled = false;

        // Desaparece después de 5 segundos para limpiar la escena
        Destroy(gameObject, 5f);
    }
    public void TakeExplosionDamage(int damage, Vector3 explosionPoint, float explosionForce, float explosionRadius)
    {
        if (isDead) return;

        health -= damage;
        
        if (health > 0)
        {
            // Reacción al impacto
            anim.SetTrigger("hit");
            StartCoroutine(Stun(stunDuration)); 
        }
        else
        {
            DieFromExplosion(explosionPoint, explosionForce, explosionRadius);
        }
    }

    void DieFromExplosion(Vector3 explosionPoint, float explosionForce, float explosionRadius)
    {
        if (isDead) return;
        isDead = true;
        aliveEnemies--;

        // Desactivamos el NavMeshAgent para que las físicas puedan empujarlo
        if (agent != null) 
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Detenemos el animador para que el enemigo salga volando como "muñeco de trapo" rígido
        if (anim != null) anim.enabled = false;

        // Añadimos Rigidbody si no lo tiene, para que pueda salir volando
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configuramos el Rigidbody para que reaccione a la explosión
        rb.isKinematic = false;
        rb.useGravity = true;
        
        // Aplicamos la fuerza explosiva (El 3f final hace que salgan disparados ligeramente hacia arriba)
        rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, 3f, ForceMode.Impulse);

        Debug.Log("Ogre DIED from Explosion!");

        // Desactivamos collider principal (opcional, o le cambiamos la capa para que no estorbe)
        // GetComponent<Collider>().enabled = false; // Mejor dejarlo activado para que caiga al piso y ruede

        // Desaparece después de 5 segundos
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// Escala las estadísticas del enemigo según los multiplicadores de la horda.
    /// </summary>
    public void ScaleStats(float healthMult, float speedMult, float damageMult)
    {
        health = Mathf.RoundToInt(health * healthMult);
        attackDamage *= damageMult;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.speed *= speedMult;
        }
    }
}
