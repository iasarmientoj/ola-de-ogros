using UnityEngine;

public class AmmoPack : MonoBehaviour
{
    [Tooltip("Cantidad de balas que otorga este paquete al recogerlo.")]
    public int ammoAmount = 100;

    [Header("Audio Settings")]
    [Tooltip("Sonido que se reproducirá al recoger las balas.")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el que entró al trigger es el Player
        if (other.CompareTag("Player"))
        {
            // Buscamos el script PlayerShooting en el jugador
            PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();

            if (playerShooting == null)
            {
                playerShooting = other.GetComponentInChildren<PlayerShooting>();
            }

            if (playerShooting != null)
            {
                // Sumamos la munición
                playerShooting.AddAmmo(ammoAmount);
                Debug.Log($"Munición recogida: +{ammoAmount} balas.");

                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
                }

                // Destruimos el paquete de munición
                Destroy(gameObject);
            }
        }
    }
}
