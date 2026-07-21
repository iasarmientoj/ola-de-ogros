using UnityEngine;

public class BossClubHitbox : MonoBehaviour
{
    [HideInInspector] public float damage = 35f;
    [HideInInspector] public AudioClip hitSound;
    [HideInInspector] public float soundVolume = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        // Solo inflige daño cuando el garrote entra en contacto físico con el Jugador
        if (other.CompareTag("Player"))
        {
            FirstPersonController player = other.GetComponent<FirstPersonController>();
            if (player == null)
            {
                player = other.GetComponentInParent<FirstPersonController>();
            }

            if (player != null)
            {
                player.TakeDamage(damage);

                // Reproducir el sonido de golpe en la posición 3D del contacto
                if (hitSound != null)
                {
                    AudioSource.PlayClipAtPoint(hitSound, transform.position, soundVolume);
                }

                Debug.Log("¡El garrote del Boss tocó físicamente al jugador e infligió daño con sonido!");
            }
        }
    }
}
