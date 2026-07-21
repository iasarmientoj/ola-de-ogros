using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private Vector3 impactVelocity; // Velocidad del empujón (Knockback) recibido
    private float xRotation = 0f;

    [Header("Player Stats")]
    public float health = 100f;
    public bool isDead = false;

    [Header("Game Over UI")]
    [Tooltip("El panel/imagen de Game Over (Image - game over) de la UI.")]
    public GameObject gameOverPanel;

    [Header("UI References")]
    public Image healthBar;
    [Tooltip("La imagen de superposición de daño en la pantalla (Image - daño).")]
    public Image damageOverlay;
    [Tooltip("Duración en segundos del efecto de parpadeo de daño.")]
    public float damageFlashDuration = 0.5f;
    [Tooltip("Color inicial de la pantalla de daño (su opacidad define el nivel máximo de destello).")]
    public Color damageColor = new Color(1f, 0f, 0f, 0.5f);
    private float maxHealth;

    [Header("Camera Shake Settings")]
    [Tooltip("Duración en segundos del efecto de sacudida al recibir daño.")]
    public float shakeDuration = 0.15f;
    [Tooltip("Fuerza/intensidad de la sacudida de la cámara.")]
    public float shakeMagnitude = 0.2f;

    [Header("Knockback Settings")]
    [Tooltip("Fuerza con la que el jugador se echa hacia atrás al recibir cualquier golpe (0 = sin empujón).")]
    public float defaultKnockbackForce = 5f;

    private float currentShakeDuration = 0f;
    private Vector3 lastShakeOffset = Vector3.zero;

    [Header("Audio Settings")]
    [Tooltip("El clip de sonido que se reproducirá cuando el jugador reciba daño.")]
    public AudioClip damageSound;
    [Tooltip("Sonido de pisada.")]
    public AudioClip footstepSound;
    [Tooltip("Volumen de las pisadas.")]
    [Range(0f, 1f)] public float footstepVolume = 0.4f;
    [Tooltip("Tiempo en segundos entre pisadas al caminar.")]
    public float walkStepInterval = 0.5f;
    [Tooltip("Tiempo en segundos entre pisadas al correr.")]
    public float runStepInterval = 0.3f;
    [Tooltip("Límite inferior del tono (pitch) aleatorio para pisadas.")]
    [Range(0.5f, 1.5f)] public float minFootstepPitch = 0.9f;
    [Tooltip("Límite superior del tono (pitch) aleatorio para pisadas.")]
    [Range(0.5f, 1.5f)] public float maxFootstepPitch = 1.1f;

    private AudioSource audioSource;
    private float footstepTimer = 0f;

    [Header("Flashlight Settings")]
    public GameObject flashlight; // Arrastra tu linterna de la escena o jerarquía aquí

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        maxHealth = health;
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);

        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
        }

        UpdateHealthUI();

        if (damageOverlay != null)
        {
            damageOverlay.gameObject.SetActive(true);
            Color color = damageOverlay.color;
            color.a = 0f;
            damageOverlay.color = color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDead || Time.timeScale == 0f) return;

        HandleRotation();
        HandleMovement();
        HandleFlashlight();
        UpdateDamageOverlay();
        HandleFootsteps();
    }

    void LateUpdate()
    {
        if (isDead) return;
        HandleCameraShake();
    }

    void HandleCameraShake()
    {
        if (cameraTransform == null) return;

        // Remove the offset from the previous frame to restore the base position
        cameraTransform.localPosition -= lastShakeOffset;

        if (currentShakeDuration > 0)
        {
            // Calculate new shake offset, decaying over time
            float dampFactor = currentShakeDuration / shakeDuration;
            Vector3 randomOffset = Random.insideUnitSphere * (shakeMagnitude * dampFactor);

            // Apply new shake offset
            cameraTransform.localPosition += randomOffset;
            lastShakeOffset = randomOffset;

            currentShakeDuration -= Time.deltaTime;
        }
        else
        {
            lastShakeOffset = Vector3.zero;
        }
    }

    void UpdateDamageOverlay()
    {
        if (damageOverlay != null && damageOverlay.color.a > 0)
        {
            Color color = damageOverlay.color;
            float speed = damageFlashDuration > 0f ? (damageColor.a / damageFlashDuration) : 10f;
            color.a -= speed * Time.deltaTime;
            if (color.a < 0) color.a = 0;
            damageOverlay.color = color;
        }
    }

    void HandleFootsteps()
    {
        if (controller == null || !controller.isGrounded) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Check if there is movement input
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);

        if (isMoving)
        {
            // Determine if running or walking
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            footstepTimer += Time.deltaTime;

            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            // Load the timer so the first step plays instantly when starting to move again
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            footstepTimer = isRunning ? runStepInterval : walkStepInterval;
        }
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepSound != null)
        {
            // Vary pitch slightly for a natural organic feel
            audioSource.pitch = Random.Range(minFootstepPitch, maxFootstepPitch);
            audioSource.PlayOneShot(footstepSound, footstepVolume);
            
            // Restore default pitch so it doesn't affect other sounds
            audioSource.pitch = 1f;
        }
    }

    void HandleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashlight != null)
            {
                flashlight.SetActive(!flashlight.activeSelf);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log("Player Health: " + health);
        UpdateHealthUI();

        if (defaultKnockbackForce > 0f)
        {
            ApplyKnockbackBackwards(defaultKnockbackForce);
        }

        if (damageOverlay != null)
        {
            Color color = damageOverlay.color;
            color.a = damageColor.a;
            damageOverlay.color = color;
        }

        currentShakeDuration = shakeDuration;

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Empuja al jugador hacia atrás (en dirección opuesta a su mirada).
    /// </summary>
    public void ApplyKnockbackBackwards(float force)
    {
        Vector3 pushDirection = -transform.forward;
        pushDirection.y = 0.15f; // Ligerísimo impulso hacia arriba para sensación física
        impactVelocity = pushDirection * force;
    }

    public void HealFull()
    {
        health = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player is DEAD!");
        if (cameraTransform != null && lastShakeOffset != Vector3.zero)
        {
            cameraTransform.localPosition -= lastShakeOffset;
            lastShakeOffset = Vector3.zero;
        }
        
        // Limpiar overlays
        if (damageOverlay != null)
        {
            Color color = damageOverlay.color;
            color.a = 0f;
            damageOverlay.color = color;
        }

        // Mostrar pantalla de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pausar juego y mostrar cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResetPlayerAfterDeath()
    {
        isDead = false;
        health = maxHealth;
        UpdateHealthUI();

        if (damageOverlay != null)
        {
            Color color = damageOverlay.color;
            color.a = 0f;
            damageOverlay.color = color;
        }

        currentShakeDuration = 0f;
        lastShakeOffset = Vector3.zero;
        impactVelocity = Vector3.zero;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Ocultar cursor y pausar físicas a velocidad normal
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical head rotation (X axis)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal body rotation (Y axis)
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Check if grounded
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Get movement input (WASD)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Move relative to the direction player is facing
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Determine current speed (sprint with Shift)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Update animation
        if (animator != null)
        {
            bool isWalking = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
            animator.SetBool("walk", isWalking);
        }

        // Jump logic
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Aplica el empujón (knockback) y lo disipa progresivamente
        if (impactVelocity.magnitude > 0.2f)
        {
            controller.Move(impactVelocity * Time.deltaTime);
            impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 8f * Time.deltaTime);
        }
        else
        {
            impactVelocity = Vector3.zero;
        }
    }
}
