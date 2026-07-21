using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Fire VFX References")]
    [Tooltip("El primer efecto visual de fuego que se activa al primer disparo (ej. VFX_Fire_Floor_02).")]
    public GameObject fireVFX1;
    [Tooltip("El segundo efecto de fuego que se activa al segundo disparo (ej. VFX_Fire_Floor_02 (1)).")]
    public GameObject fireVFX2;

    [Header("Explosion Settings")]
    [Tooltip("El prefab del efecto de explosión masiva (partículas/humo/fuego).")]
    public GameObject explosionVFXPrefab;
    [Tooltip("El radio de alcance de la explosión.")]
    public float explosionRadius = 6f;
    [Tooltip("El daño masivo que le hará a los enemigos dentro del radio.")]
    public int explosionDamage = 50;

    [Header("Explosion Audio")]
    [Tooltip("El clip de sonido que sonará al explotar el barril.")]
    public AudioClip explosionSound;
    [Range(0f, 1f)] public float explosionVolume = 1.0f;

    private int currentHits = 0;
    private bool hasExploded = false;

    void Start()
    {
        // Aseguramos que los fuegos estén apagados al iniciar la partida
        if (fireVFX1 != null) fireVFX1.SetActive(false);
        if (fireVFX2 != null) fireVFX2.SetActive(false);
    }

    /// <summary>
    /// Recibe un disparo y maneja las fases de daño (fuego 1, fuego 2, explosión).
    /// </summary>
    public void GetShot()
    {
        if (hasExploded) return;

        currentHits++;

        if (currentHits == 1)
        {
            if (fireVFX1 != null)
            {
                fireVFX1.SetActive(true);
            }
            Debug.Log($"Barril {gameObject.name}: ¡Primer disparo! Fuego 1 activado.");
        }
        else if (currentHits == 2)
        {
            if (fireVFX2 != null)
            {
                fireVFX2.SetActive(true);
            }
            Debug.Log($"Barril {gameObject.name}: ¡Segundo disparo! Fuego 2 activado.");
        }
        else if (currentHits >= 3)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.Log($"Barril {gameObject.name}: ¡Tercer disparo! EXPLOSIÓN.");

        // 1. Instanciar el prefab de la explosión visual
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, transform.rotation);
        }

        // Reproducir sonido de explosión en 3D en su posición
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // 2. Encontrar todos los enemigos cercanos y aplicarles daño de área
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Enemy enemy = nearbyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
            }
            else
            {
                RangedEnemy rangedEnemy = nearbyObject.GetComponent<RangedEnemy>();
                if (rangedEnemy != null)
                {
                    rangedEnemy.TakeDamage(explosionDamage);
                }
                else
                {
                    BossEnemy boss = nearbyObject.GetComponentInParent<BossEnemy>();
                    if (boss != null)
                    {
                        boss.TakeDamage(explosionDamage);
                    }
                }
            }
        }

        // 3. Eliminar el barril de la escena
        Destroy(gameObject);
    }

    // Dibujar el radio de la explosión en la escena al seleccionarlo para facilitar el diseño del nivel
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
