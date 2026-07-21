using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject enemyPrefab; // Arrastra tu prefab EnemyCapsule aquí
    public float timeBetweenSpawns = 3f; // Cada cuántos segundos nace un enemigo
    public int maxEnemies = 10; // Máximo de enemigos por nivel

    private float timer = 0f;
    private int spawnedEnemies = 0;

    public void Spawn()
    {
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
    }
}
