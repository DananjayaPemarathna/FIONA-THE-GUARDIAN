using UnityEngine;

/// <summary>
/// Spawns enemies repeatedly at random respawn points.
/// Difficulty progression is handled by decreasing the spawn cooldown over time
/// (down to a minimum cap).
/// 
/// Notes:
/// - Spawning stops automatically if the Player is missing or destroyed.
/// - Newly spawned enemies can be flipped to immediately face the player.
/// </summary>
public class Enemy_Respawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] respawnPoints;
    [SerializeField] private float cooldown = 2f;

    [Header("Difficulty Progression")]
    [Space]
    [Tooltip("How much the cooldown decreases after each spawn.")]
    [SerializeField] private float cooldownDecreaseRate = 0.05f;

    [Tooltip("Minimum allowed cooldown (prevents infinite spawning speed).")]
    [SerializeField] private float cooldownCap = 0.7f;

    private float timer;
    private Transform playerTransform;

    private void Awake()
    {
        // Cache Player transform once at startup (avoids searching every frame).
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player not found in scene!");
        }
    }

    private void Update()
    {
        // If Player is missing/dead, stop spawning.
        if (playerTransform == null) return;

        // Countdown timer for spawning.
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            // Reset timer before spawning.
            timer = cooldown;

            // Spawn enemy.
            CreateNewEnemy();

            // Increase difficulty: reduce cooldown each spawn, but never below cap.
            cooldown = Mathf.Max(cooldown - cooldownDecreaseRate, cooldownCap);
        }
    }

    /// <summary>
    /// Instantiates a new enemy at a random respawn point and flips it if needed
    /// so that it faces toward the player immediately.
    /// </summary>
    private void CreateNewEnemy()
    {
        // Basic safety checks.
        if (enemyPrefab == null || respawnPoints.Length == 0) return;

        // 1) Pick a random spawn point.
        int respawnPointIndex = Random.Range(0, respawnPoints.Length);
        Vector3 spawnPoint = respawnPoints[respawnPointIndex].position;

        // 2) Instantiate the enemy.
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);

        // 3) Make the enemy face the player immediately on spawn.
        // Current logic: if the enemy spawns to the right of the player, flip it.
        bool createdOnTheRight = newEnemy.transform.position.x > playerTransform.position.x;

        if (createdOnTheRight)
        {
            Enemy enemyScript = newEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.Flip();
            }
        }
    }
}
