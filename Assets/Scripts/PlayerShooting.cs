using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public Camera cam;

    [Header("Settings")]
    public float shootingRange = 100f;
    [Tooltip("El daño que inflige cada disparo al impactar a un enemigo.")]
    public int damagePerShot = 1;

    [Header("Zoom Settings")]
    public GameObject zoomImage;
    public GameObject hands;
    public float zoomFOV = 20f;
    private float defaultFOV;
    private bool isZoomed = false;

    [Header("Grenade Settings")]
    public GameObject grenadePrefab; // Arrastra tu prefab de la granada aquí
    public Transform throwPoint;     // Arrastra aquí desde donde se lanza (ej. la mano o la cámara)
    [Tooltip("Fuerza de empuje horizontal/hacia adelante.")]
    public float throwForce = 12f;   // Fuerza hacia adelante
    [Tooltip("Fuerza de empuje vertical para arquear y elevar la trayectoria.")]
    public float upwardForce = 8f;   // Fuerza hacia arriba

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioSource audioSource;

    [Header("Ammo Settings")]
    public TextMeshProUGUI ammoText;
    public int currentAmmo = 300;
    public int totalAmmo = 999;
    public int ammoPerShot = 10;

    private Animator animator;

    void Start()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (cam != null) defaultFOV = cam.fieldOfView;
        
        // Initialize state
        if (zoomImage != null) zoomImage.SetActive(false);
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo >= ammoPerShot)
            {
                Shoot();
            }
            else
            {
                Debug.Log("Out of ammo!");
            }
        }

        // El sistema de recarga con R ha sido desactivado para usar recolección en el mapa
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
        */

        // Lanzamiento instantáneo de granada al presionar la tecla Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowGrenadeInstant();
        }

        HandleZoom();
    }

    void ThrowGrenadeInstant()
    {
        if (animator != null)
        {
            animator.SetTrigger("granade");
        }
        else
        {
            // Fallback si no hay animator
            ThrowGrenade();
        }
    }

    void Reload()
    {
        if (currentAmmo == totalAmmo) return;

        currentAmmo = totalAmmo;
        UpdateAmmoUI();

        if (animator != null)
        {
            animator.SetTrigger("reload");
        }
    }

    /// <summary>
    /// Añade una cantidad de balas al cargador del jugador, sin superar el límite de totalAmmo.
    /// </summary>
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > totalAmmo)
        {
            currentAmmo = totalAmmo;
        }
        UpdateAmmoUI();
    }

    void HandleZoom()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            if (animator != null) animator.Play("zoom", 1);
        }
        else if (Input.GetMouseButtonUp(1)) 
        {
            if (animator != null) animator.Play("zoom-out", 1);
        }
    }

    public void SetZoomTrue()
    {
        isZoomed = true;
        if (zoomImage != null) zoomImage.SetActive(true);
        if (hands != null) hands.SetActive(false);
    }

    public void SetZoomFalse()
    {
        isZoomed = false;
        if (zoomImage != null) zoomImage.SetActive(false);
        if (hands != null) hands.SetActive(true);
    }

    void Shoot()
    {
        currentAmmo -= ammoPerShot;
        UpdateAmmoUI();

        if (animator != null)
        {
            animator.SetTrigger("shoot");
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (cam != null)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, shootingRange))
            {
                Debug.Log("Hit: " + hit.collider.name);
                
                // 1. Intentar golpear mediante el componente modular Hitbox (el mejor método pedagógico)
                Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
                if (hitbox != null)
                {
                    hitbox.ReceiveHit(damagePerShot);
                }
                else
                {
                    // 2. Intentar golpear un barril explosivo (en el objeto impactado o en sus padres)
                    ExplosiveBarrel barrel = hit.collider.GetComponentInParent<ExplosiveBarrel>();
                    if (barrel != null)
                    {
                        barrel.GetShot();
                    }
                    else
                    {
                        // 3. Método alternativo de fallback por si disparan al colisionador principal de la raíz sin script Hitbox
                        Enemy enemy = hit.collider.GetComponent<Enemy>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage(damagePerShot);
                        }
                        else
                        {
                            RangedEnemy rangedEnemy = hit.collider.GetComponent<RangedEnemy>();
                            if (rangedEnemy != null)
                            {
                                rangedEnemy.TakeDamage(damagePerShot);
                            }
                            else
                            {
                                BossEnemy boss = hit.collider.GetComponentInParent<BossEnemy>();
                                if (boss != null)
                                {
                                    boss.TakeDamage(damagePerShot);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString("D3") + "/" + totalAmmo.ToString();
        }
    }

    // Método que llama el evento de animación para instanciar y lanzar la granada
    public void ThrowGrenade()
    {
        if (grenadePrefab != null && throwPoint != null)
        {
            GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
            
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            if (rb != null && cam != null)
            {
                // Aplicar la fuerza de empuje hacia adelante y hacia arriba con el arco configurado
                Vector3 throwVector = cam.transform.forward * throwForce + Vector3.up * upwardForce;
                rb.AddForce(throwVector, ForceMode.Impulse);
            }
        }
    }
}
