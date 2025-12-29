using UnityEngine;

/// <summary>
/// Basic enemy implementation built on top of the shared Entity base class.
/// Behaviour:
/// - Moves constantly in its facing direction
/// - Detects the player within attack radius
/// - Triggers attack animation when player is detected
/// - Increments kill count on death
/// 
/// Note: Damage application should happen via Entity.DamageTargets() using an Animation Event.
/// </summary>
public class Enemy : Entity
{
    [Header("Enemy Detection")]
    [SerializeField] private LayerMask whatIsPlayer;

    // Stores the detected player collider when within range.
    private Collider2D playerDetected;

    /// <summary>
    /// Uses the base Entity update loop, then runs enemy-specific attack checks.
    /// </summary>
    protected override void Update()
    {
        base.Update();

        // Enemy attempts to attack whenever the player is detected.
        // ⚠️ If attack is triggered every frame, the animation may restart continuously.
        HandleAttack();
    }

    /// <summary>
    /// Triggers attack animation if the player is within detection range.
    /// Actual damage should be applied at the correct animation frame via Animation Event.
    /// </summary>
    protected override void HandleAttack()
    {
        // Only attack if a player collider is detected in the attack radius.
        if (playerDetected)
        {
            anim.SetTrigger("attack");
        }
    }

    /// <summary>
    /// Enemy movement is automatic: it continuously moves in the current facing direction.
    /// </summary>
    protected override void HandleMovement()
    {
        if (canMove)
        {
            // Move forward in facing direction (facingDir is +1 or -1).
            rb.linearVelocity = new Vector2(facingDir * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Stop horizontal movement while preserving vertical velocity.
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Runs base ground detection and additionally checks whether the player is within attack range.
    /// </summary>
    protected override void HandleCollisions()
    {
        base.HandleCollisions();

        // Detect player within attack radius centered at attackPoint.
        // (This collider reference is used in HandleAttack())
        if (attackPoint != null)
        {
            playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsPlayer);
        }
        else
        {
            playerDetected = null;
        }
    }

    /// <summary>
    /// Enemy death behaviour:
    /// - Executes base Entity death logic
    /// - Updates UI kill count
    /// </summary>
    protected override void Die()
    {
        base.Die();

        // Increment kill count in the UI.
        if (UI.instance != null)
        {
            UI.instance.AddKillCount();
        }
    }
}
