using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Sonido que se reproducirá al recoger el botiquín.")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el que entró al trigger es el Player
        if (other.CompareTag("Player"))
        {
            // Buscamos el script FirstPersonController en el objeto que entró
            FirstPersonController player = other.GetComponent<FirstPersonController>();

            if (player != null)
            {
                player.HealFull();
                Debug.Log("Vida restaurada al máximo");

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
                }
                
                // Destruimos el botiquín de forma segura (evita destruir carpetas de entorno)
                if (transform.parent != null && transform.parent.name.ToLower().Contains("botiquin"))
                {
                    Destroy(transform.parent.gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
