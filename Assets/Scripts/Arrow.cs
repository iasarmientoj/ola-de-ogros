using UnityEngine;

public class Arrow : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector3 targetDirection;
    private bool isInitialized = false;

    [Header("Settings")]
    public float lifeTime = 5f; // Tiempo de vida máximo antes de auto-destruirse

    [Header("Visual Offset (Ajuste de Rotación)")]
    public Vector3 rotationOffset = new Vector3(0, -90, 0); // Modifica esto en el Inspector si la flecha sale chueca/de lado

    void Start()
    {
        // Auto-destrucción preventiva en caso de que no choque con nada
        Destroy(gameObject, lifeTime);
    }

    public void Setup(float damageAmount, float arrowSpeed, Vector3 targetPosition)
    {
        damage = damageAmount;
        speed = arrowSpeed;
        
        // Apuntamos al pecho del jugador (aproximadamente 1 unidad por encima de la base)
        Vector3 targetPoint = targetPosition + Vector3.up * 1f; 
        targetDirection = (targetPoint - transform.position).normalized;
        
        // Hacer que la flecha mire hacia donde vuela aplicando el ajuste de rotación
        if (targetDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
        }

        isInitialized = true;
        
        // Si la flecha tiene Rigidbody y NO es kinematic, le aplicamos la velocidad de forma física
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = targetDirection * speed;
            rb.useGravity = false; // Sin gravedad para que vaya recta
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // Si no tiene Rigidbody o si el Rigidbody es Kinematic, la movemos manualmente
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic)
        {
            transform.position += targetDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject otherObj)
    {
        // No colisionar con otros enemigos ni con otras flechas (evitar fuego amigo)
        if (otherObj.GetComponent<Enemy>() != null || otherObj.GetComponent<RangedEnemy>() != null) return;
        if (otherObj.name.Contains("Arrow") || otherObj.name.Contains("flecha")) return;

        // Si choca con el jugador, le hace daño
        if (otherObj.CompareTag("Player"))
        {
            FirstPersonController player = otherObj.GetComponent<FirstPersonController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            // Si choca con cualquier otra cosa (suelo, pared, obstáculos), se destruye
            Destroy(gameObject);
        }
    }
}
