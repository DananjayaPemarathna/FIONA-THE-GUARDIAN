using UnityEngine;

/// <summary>
/// Player controller built on top of the shared Entity base class.
/// Responsible for:
/// - Reading player input (movement, jump, attack)
/// - Updating the Player Health UI
/// - Triggering Game Over flow on death
/// </summary>
public class Player : Entity
{
    /// <summary>
    /// Initializes the Player health UI when the game starts.
    /// This is placed in Start() to ensure UI systems are ready.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        if (UI.instance != null)
        {
            // Fill player health bar at game start (current = max).
            UI.instance.UpdatePlayerHealth(GetMaxHealth(), GetMaxHealth());
        }
    }

    /// <summary>
    /// Reads player input every frame.
    /// Movement uses horizontal axis, jump uses Space, and attack uses Left Mouse Button.
    /// </summary>
    protected override void HandleInput()
    {
        // Horizontal movement input (-1, 0, +1)
        xInput = Input.GetAxisRaw("Horizontal");

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToJump();

            // Play Jump SFX (index 0)
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(0);
        }

        // Attack
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HandleAttack();

            // Play Attack SFX (index 1)
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(1);
        }
    }

    /// <summary>
    /// Applies damage using the base Entity implementation,
    /// then updates the Player health UI.
    /// </summary>
    public override void TakeDamage()
    {
        // Base damage logic: reduce health, play feedback, die if needed.
        base.TakeDamage();

        // Update player health bar after taking damage.
        if (UI.instance != null)
        {
            UI.instance.UpdatePlayerHealth(GetCurrentHealth(), GetMaxHealth());
        }
    }

    /// <summary>
    /// Handles Player death.
    /// Calls base death behavior first, then triggers Game Over UI and detaches camera follow.
    /// </summary>
    protected override void Die()
    {
        base.Die();

        // Detach Cinemachine follow target to prevent camera tracking a destroyed object.
        var cam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (cam != null)
        {
            cam.Follow = null;
        }

        // Enable Game Over UI.
        if (UI.instance != null)
        {
            UI.instance.EnableGameOverUI();
        }
    }
}
