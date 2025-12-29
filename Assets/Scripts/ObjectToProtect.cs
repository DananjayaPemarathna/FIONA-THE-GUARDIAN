using UnityEngine;

/// <summary>
/// A stationary objective that the player must defend.
/// Inherits from Entity to reuse:
/// - Health system (TakeDamage / Die)
/// - Facing / flip logic
/// 
/// Differences from typical entities:
/// - Does NOT move or jump
/// - Does NOT run base.Update() to avoid movement/physics/animation logic
/// - Only rotates to face the player for visual feedback
/// - Updates a dedicated object health UI bar
/// </summary>
public class ObjectToProtect : Entity
{
    [Header("Protection Target")]
    [SerializeField] private Transform player;

    /// <summary>
    /// Initializes component references via base class.
    /// Auto-finds Player transform if not assigned in the Inspector.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // Auto-assign player reference if not set from Inspector.
        if (player == null)
        {
            Player playerScript = FindFirstObjectByType<Player>();
            if (playerScript != null)
                player = playerScript.transform;
        }
    }

    /// <summary>
    /// Initializes object health UI at the beginning of the game.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        if (UI.instance != null)
        {
            // Fill object health bar at game start (current = max).
            UI.instance.UpdateObjectHealth(GetMaxHealth(), GetMaxHealth());
        }
    }

    /// <summary>
    /// Custom update loop:
    /// We intentionally skip base.Update() to prevent unwanted behaviour
    /// such as movement, collisions, and animation updates.
    /// This object is meant to remain static.
    /// </summary>
    protected override void Update()
    {
        if (player == null) return;

        // Only facing/flip logic is required for this object.
        HandleFlip();
    }

    /// <summary>
    /// Ensures the object always faces the player (purely visual).
    /// </summary>
    protected override void HandleFlip()
    {
        if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    /// <summary>
    /// Applies damage using the base Entity implementation,
    /// then updates the Object health UI.
    /// </summary>
    public override void TakeDamage()
    {
        // Base damage logic: reduce health, play feedback, die if needed.
        base.TakeDamage();

        // Update objective health bar.
        if (UI.instance != null)
        {
            UI.instance.UpdateObjectHealth(GetCurrentHealth(), GetMaxHealth());
        }
    }

    /// <summary>
    /// On death, triggers Game Over UI.
    /// </summary>
    protected override void Die()
    {
        base.Die();

        // Show Game Over screen when the protected object is destroyed.
        if (UI.instance != null)
        {
            UI.instance.EnableGameOverUI();
        }
    }
}
