using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float delay = 3f;           // Tiempo de respaldo antes de explotar si no choca
    public float explosionRadius = 5f; // Radio de alcance
    [Tooltip("El daño masivo que le hará a los enemigos (ajustable en el Inspector).")]
    public int damage = 25;            // Daño masivo que hace a los enemigos

    [Header("Effects")]
    [Tooltip("Prefab del efecto visual de explosión a instanciar al detonar.")]
    public GameObject explosionEffect; // Prefab de las partículas de explosión

    private float countdown;
    private bool hasExploded = false;

    void Start()
    {
        countdown = delay;
        AddTrailRenderer();
    }

    void Update()
    {
        countdown -= Time.deltaTime;
        
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    // Explotar de inmediato apenas toque cualquier superficie u objeto
    private void OnCollisionEnter(Collision collision)
    {
        // Evitar explotar inmediatamente en la cara del jugador al ser lanzada
        if (collision.gameObject.CompareTag("Player")) return;

        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // 1. Mostrar efecto visual en el punto de impacto
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // 2. Encontrar todos los objetos dentro del radio de explosión
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // 3. Si encontramos un enemigo, le aplicamos daño puro sin empuje físico
            Enemy enemy = nearbyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else
            {
                RangedEnemy rangedEnemy = nearbyObject.GetComponent<RangedEnemy>();
                if (rangedEnemy != null)
                {
                    rangedEnemy.TakeDamage(damage);
                }
            }
        }

        // 4. Destruir la granada
        Destroy(gameObject);
    }

    private void AddTrailRenderer()
    {
        // Añadir componente TrailRenderer por código para evitar configuración manual
        TrailRenderer tr = gameObject.GetComponent<TrailRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TrailRenderer>();
        }
        
        tr.time = 0.6f;
        tr.startWidth = 0.15f;
        tr.endWidth = 0.0f;
        tr.minVertexDistance = 0.1f;
        
        // Crear un gradiente de fuego (naranja -> amarillo -> transparente)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.0f), 
                new GradientColorKey(new Color(1f, 0.8f, 0.1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0.5f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.85f, 0.0f), 
                new GradientAlphaKey(0.4f, 0.5f),
                new GradientAlphaKey(0.0f, 0.5f) 
            }
        );
        tr.colorGradient = gradient;
        
        // Buscar el shader unlit de URP o estándar para pintar la estela
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        
        if (shader != null)
        {
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f));
            else
                mat.color = new Color(1f, 0.5f, 0f);
            
            tr.sharedMaterial = mat;
        }
    }
}
