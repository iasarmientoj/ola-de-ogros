using UnityEngine;
using UnityEngine.AI; // Necesario para el NavMesh

public class RangedEnemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    private Animator anim;

    [Header("Enemy Stats")]
    public int health = 5;
    public float attackRange = 10f; // Distancia desde la que empezará a disparar
    public float attackDamage = 20f;
    public float attackCooldown = 2f;
    public float attackDelay = 1.0f; // Retraso para sincronizar con la animación de disparo
    public float stunDuration = 1.0f; // Tiempo que se queda quieto al recibir daño

    [Header("Ranged Attack Settings")]
    public GameObject arrowPrefab;        // Prefab de la flecha que va a disparar
    public Transform firePoint;           // Punto de origen del disparo (ej. la punta de la ballesta)
    public float arrowSpeed = 20f;        // Velocidad de la flecha

    private float lastAttackTime = 0f;
    private bool isDead = false;
    private bool isHit = false;

    void Start()
    {
        // Sumamos a la caja global de enemigos vivos compartida con el enemigo normal
        Enemy.aliveEnemies++;
        
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>(); // El Animator está en el modelo hijo
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;
        
        // Si acaba de recibir un impacto, se detiene
        if (isHit)
        {
            if (agent != null) agent.isStopped = true;
            if (anim != null) anim.SetBool("attack", false); // Detiene la animación de ataque
            return;
        }

        if (player != null && agent != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange)
            {
                // El enemigo deja de caminar para disparar
                agent.isStopped = true;
                agent.updateRotation = false; // Evitamos conflictos entre nuestra rotación manual y la del NavMesh
                
                // Mantenemos la animación de ataque activa en bucle
                if (anim != null) anim.SetBool("attack", true);
                
                // Rotar suavemente hacia el jugador para apuntar con precisión
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // Evitamos rotaciones extrañas hacia arriba/abajo
                if (direction.sqrMagnitude > 0.1f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
            }
            else
            {
                // Seguir corriendo hacia el jugador si está lejos
                agent.isStopped = false;
                agent.updateRotation = true; // Dejamos que el NavMesh controle la rotación al correr
                agent.SetDestination(player.position);

                // Apagamos la animación de ataque para volver a correr
                if (anim != null) anim.SetBool("attack", false);
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        // No hace falta disparar ningún Trigger aquí, ya que 'attack' es un Bool
        // que se mantiene en true y mantiene al enemigo en el estado 'atacar'.

        // Iniciamos la corrutina para disparar la flecha con el retraso de la animación
        StartCoroutine(ShootArrowAfterDelay(attackDelay));
    }

    private System.Collections.IEnumerator ShootArrowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Si el enemigo murió o fue aturdido mientras atacaba, no dispara
        if (isDead || isHit) yield break;

        if (player != null && arrowPrefab != null && firePoint != null)
        {
            // Creamos la flecha físicamente en la escena
            GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            
            // Si la flecha tiene el script Arrow, la configuramos
            Arrow arrowScript = arrowObj.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.Setup(attackDamage, arrowSpeed, player.position);
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
            if (anim != null) anim.SetTrigger("hit");
            
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
        
        // Restamos de la caja global de enemigos vivos
        Enemy.aliveEnemies--;

        if (agent != null) agent.isStopped = true;
        if (anim != null) anim.SetTrigger("die");
        
        // Desactivamos collider para que no bloquee balas estando muerto
        GetComponent<Collider>().enabled = false;

        // Desaparece después de 5 segundos
        Destroy(gameObject, 5f);
    }

    public void TakeExplosionDamage(int damage, Vector3 explosionPoint, float explosionForce, float explosionRadius)
    {
        if (isDead) return;

        health -= damage;
        
        if (health > 0)
        {
            if (anim != null) anim.SetTrigger("hit");
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
        
        Enemy.aliveEnemies--;

        // Desactivamos el NavMeshAgent para que las físicas puedan empujarlo
        if (agent != null) 
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (anim != null) anim.enabled = false;

        // Añadimos Rigidbody si no lo tiene
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = false;
        rb.useGravity = true;
        
        // Aplicamos la fuerza de la explosión
        rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, 3f, ForceMode.Impulse);

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
