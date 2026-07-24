using UnityEngine;

/// <summary>
/// Colócalo en un GameObject con Collider (marcado como Is Trigger o Collider normal) en la base del parkour.
/// Cuando el jugador cae y toca este collider, se reinicia su posición a donde empezó el parkour.
/// </summary>
public class ParkourFallZone : MonoBehaviour
{
    [Header("--- RESPAWN SETTINGS ---")]
    [Tooltip("Punto de reaparición (Spawn Point). Si se deja vacío, guardará automáticamente la posición inicial del jugador al iniciar la escena.")]
    public Transform respawnPoint;

    [Header("--- AUDIO (OPCIONAL) ---")]
    [Tooltip("Sonido que suena cuando el jugador cae y reaparece.")]
    public AudioClip fallSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    private Vector3 initialSpawnPosition;
    private Quaternion initialSpawnRotation;
    private bool hasCustomInitialPoint = false;

    private void Start()
    {
        // Si no se asignó un respawnPoint manual, buscar al jugador en Start para recordar su posición inicial
        if (respawnPoint == null)
        {
            FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
            if (player != null)
            {
                initialSpawnPosition = player.transform.position;
                initialSpawnRotation = player.transform.rotation;
                hasCustomInitialPoint = true;
                Debug.Log($"[ParkourFallZone] Posición inicial guardada automáticamente: {initialSpawnPosition}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerFall(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerFall(collision.gameObject);
    }

    private void HandlePlayerFall(GameObject target)
    {
        // Detectar si el objeto o sus padres tienen el FirstPersonController o la etiqueta Player
        FirstPersonController player = target.GetComponentInParent<FirstPersonController>();
        
        if (player != null)
        {
            RespawnPlayer(player);
        }
        else if (target.CompareTag("Player") || target.name.ToLower().Contains("player"))
        {
            // Intentar mover el objeto directamente si no se encontró FirstPersonController
            RespawnGenericObject(target);
        }
    }

    public void RespawnPlayer(FirstPersonController player)
    {
        Vector3 targetPos = GetRespawnPosition(player.transform.position);
        Quaternion targetRot = GetRespawnRotation(player.transform.rotation);

        // Reproducir sonido si está asignado
        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(fallSound, targetPos, soundVolume);
        }

        // Teletransportar al jugador de forma segura reseteando su CharacterController y velocidad
        player.TeleportTo(targetPos, targetRot);

        Debug.Log($"[ParkourFallZone] ¡Jugador caído! Reaparecido en {targetPos}");
    }

    private void RespawnGenericObject(GameObject obj)
    {
        CharacterController cc = obj.GetComponentInParent<CharacterController>();
        Vector3 targetPos = GetRespawnPosition(obj.transform.position);
        Quaternion targetRot = GetRespawnRotation(obj.transform.rotation);

        if (cc != null)
        {
            cc.enabled = false;
            cc.transform.position = targetPos;
            cc.transform.rotation = targetRot;
            cc.enabled = true;
        }
        else
        {
            obj.transform.position = targetPos;
            obj.transform.rotation = targetRot;
        }

        Rigidbody rb = obj.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetRespawnPosition(Vector3 defaultPos)
    {
        if (respawnPoint != null) return respawnPoint.position;
        if (hasCustomInitialPoint) return initialSpawnPosition;
        return defaultPos;
    }

    private Quaternion GetRespawnRotation(Quaternion defaultRot)
    {
        if (respawnPoint != null) return respawnPoint.rotation;
        if (hasCustomInitialPoint) return initialSpawnRotation;
        return defaultRot;
    }

    private void OnDrawGizmos()
    {
        // Dibujar en la ventana Scene para visualizar la zona de caída
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
