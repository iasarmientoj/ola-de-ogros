using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Settings")]
    public string playerTag = "Player";
    public string openParameterName = "Open";
    
    [Tooltip("If true, the door starts locked and cannot be opened by walking near it until unlocked.")]
    public bool startsLocked = false;

    [Header("NavMesh Blocking")]
    [Tooltip("Obstáculo NavMesh para tapar el camino cuando esté cerrado. Si se deja vacío, buscará uno en los hijos.")]
    public NavMeshObstacle navMeshObstacle;
    [Tooltip("Tiempo en segundos a esperar para desactivar el obstáculo al abrir la puerta (espera a que termine de abrirse).")]
    public float obstacleDisableDelay = 1.2f;
    [Tooltip("Tiempo en segundos a esperar para activar el obstáculo al cerrar la puerta (0 para activarlo al inicio del cierre).")]
    public float obstacleEnableDelay = 0f;

    private int openBoolID;
    private bool isLocked;
    private Coroutine obstacleCoroutine;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        openBoolID = Animator.StringToHash(openParameterName);
        isLocked = startsLocked;

        if (navMeshObstacle == null)
        {
            navMeshObstacle = GetComponentInChildren<NavMeshObstacle>();
        }

        // Al iniciar, si está cerrada, el obstáculo debe estar activo de inmediato para bloquear el navmesh
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = true;
        }
    }

    /// <summary>
    /// Unlocks the door and triggers its open animation.
    /// </summary>
    public void UnlockAndOpen()
    {
        isLocked = false;
        if (animator != null)
        {
            animator.SetBool(openBoolID, true);
        }
        SetObstacleState(true);
        Debug.Log($"DoorController: {gameObject.name} unlocked and opened.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLocked) return;

        if (other.CompareTag(playerTag))
        {
            if (animator != null)
            {
                animator.SetBool(openBoolID, true);
            }
            SetObstacleState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isLocked) return;

        if (other.CompareTag(playerTag))
        {
            if (animator != null)
            {
                animator.SetBool(openBoolID, false);
            }
            SetObstacleState(false);
        }
    }

    private void SetObstacleState(bool isOpen)
    {
        if (obstacleCoroutine != null)
        {
            StopCoroutine(obstacleCoroutine);
        }

        if (navMeshObstacle != null)
        {
            if (isOpen)
            {
                // Al abrir: retrasamos la desactivación del obstáculo (se une al final de la animación)
                obstacleCoroutine = StartCoroutine(DisableObstacleDelayed(obstacleDisableDelay));
            }
            else
            {
                // Al cerrar: retrasamos la activación del obstáculo (0 para separar al inicio de la animación)
                obstacleCoroutine = StartCoroutine(EnableObstacleDelayed(obstacleEnableDelay));
            }
        }
    }

    private System.Collections.IEnumerator DisableObstacleDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = false;
        }
    }

    private System.Collections.IEnumerator EnableObstacleDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (navMeshObstacle != null)
        {
            navMeshObstacle.enabled = true;
        }
    }
}
