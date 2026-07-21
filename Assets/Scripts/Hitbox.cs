using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Arrastra aquí el script Enemy (de la raíz de este enemigo).")]
    public Enemy meleeEnemy;
    [Tooltip("Arrastra aquí el script RangedEnemy (de la raíz de este enemigo, si aplica).")]
    public RangedEnemy rangedEnemy;
    [Tooltip("Arrastra aquí el script BossEnemy (de la raíz del Boss, si aplica).")]
    public BossEnemy bossEnemy;

    [Header("Damage Settings")]
    [Tooltip("Multiplicador de daño para este colisionador específico (ej: 2.5 para la cabeza, 1.0 para el cuerpo).")]
    public float damageMultiplier = 1.0f;

    /// <summary>
    /// Recibe el impacto de la bala, aplica el multiplicador y pasa el daño al enemigo correspondiente.
    /// </summary>
    public void ReceiveHit(int baseDamage)
    {
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

        if (damageMultiplier > 1.0f)
        {
            Debug.Log($"¡TIRO CRÍTICO/HEADSHOT! Daño multiplicado ({damageMultiplier}x) a: {finalDamage}");
            HeadshotUI.ShowHeadshotText("¡HEADSHOT!");
        }

        if (meleeEnemy != null)
        {
            meleeEnemy.TakeDamage(finalDamage);
        }
        else if (rangedEnemy != null)
        {
            rangedEnemy.TakeDamage(finalDamage);
        }
        else if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(finalDamage);
        }
    }
}
