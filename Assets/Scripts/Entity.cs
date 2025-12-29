using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for any living entity in the game (Player, Enemy, etc.).
/// Handles core features like movement, ground checks, attacking, taking damage, and death.
/// 
/// Child classes are expected to override input/behaviour methods such as:
/// - HandleInput()
/// - TryToJump()
/// - HandleAttack()
/// - TakeDamage()
/// </summary>

// Ensures the GameObject always contains the required physics components.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Entity : MonoBehaviour
{
    #region Components
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;
    #endregion

    [Header("Entity Stats")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int currentHealth;

    [Tooltip("Material used to flash the sprite when taking damage.")]
    [SerializeField] protected Material damageMaterial;

    [Tooltip("How long the damage flash effect lasts.")]
    [SerializeField] private float damageFeedbackDuration = 0.1f;

    private Coroutine damageFeedbackCoroutine;

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float jumpForce = 7f;

    [Header("Attack Settings")]
    [SerializeField] protected float attackRadius = 1.0f;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected LayerMask whatIsEnemy;

    [Header("Collision Detection")]
    [Tooltip("Distance used for the downward raycast to detect ground.")]
    [SerializeField] private float groundCheckDistance = 0.5f;

    [SerializeField] private LayerMask whatIsGround;
    protected bool isGrounded;

    // State Variables
    protected int facingDir = 1;         // 1 = right, -1 = left
    protected bool canMove = true;
    protected bool facingRight = true;
    protected bool canJump = true;

    // Input variable set by child classes (Player/Enemy AI)
    protected float xInput;

    protected virtual void Awake()
    {
        // Cache required component references.
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Animator/SpriteRenderer are searched in children to allow separate visuals objects.
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        // Initialize health at spawn.
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // Note:
        // UI health update logic was removed from base class because
        // Player and ObjectToProtect handle UI updates separately via overrides.
    }

    protected virtual void Update()
    {
        // Main update flow is intentionally structured for clarity:
        // 1) read input / AI decision
        // 2) detect environment collisions
        // 3) apply movement
        // 4) update animation parameters
        // 5) flip sprite orientation
        HandleInput();
        HandleCollisions();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    /// <summary>
    /// Intended to be overridden by child classes.
    /// Example: Player reads keyboard input. Enemy uses AI decision-making.
    /// </summary>
    protected virtual void HandleInput()
    {
        // Base method intentionally left empty.
    }

    /// <summary>
    /// Handles horizontal movement using Rigidbody2D velocity.
    /// </summary>
    protected virtual void HandleMovement()
    {
        if (canMove)
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    /// <summary>
    /// Checks if the entity is grounded via a downward raycast.
    /// </summary>
    protected virtual void HandleCollisions()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    /// <summary>
    /// Flips entity based on movement direction.
    /// </summary>
    protected virtual void HandleFlip()
    {
        if (xInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (xInput < 0 && facingRight)
        {
            Flip();
        }
    }

    /// <summary>
    /// Rotates the transform 180 degrees on Y axis to face the opposite direction.
    /// Updates facing state variables accordingly.
    /// </summary>
    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }

    /// <summary>
    /// Sends movement and state values to the Animator Controller.
    /// </summary>
    protected virtual void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    /// <summary>
    /// Performs a jump if grounded and jumping is allowed.
    /// </summary>
    protected virtual void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jump");
        }
    }

    /// <summary>
    /// Triggers attack animation.
    /// Damage application is handled by DamageTargets() which should be called
    /// via animation event at the correct frame.
    /// </summary>
    protected virtual void HandleAttack()
    {
        anim.SetTrigger("attack");
    }

    /// <summary>
    /// Applies damage to targets within attack radius.
    /// This is typically called via an Animation Event.
    /// </summary>
    public void DamageTargets()
    {
        if (attackPoint == null) return;

        Collider2D[] enemyColliders =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnemy);

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            Entity enemyEntity = enemyCollider.GetComponent<Entity>();

            // Only damage objects that use the Entity base class.
            if (enemyEntity != null)
            {
                enemyEntity.TakeDamage();
            }
        }
    }

    /// <summary>
    /// Reduces health by 1 and triggers visual feedback.
    /// Child classes may override this method to update UI or apply extra logic.
    /// </summary>
    public virtual void TakeDamage()
    {
        currentHealth--;

        // Note:
        // UI.instance.UpdateHealth(...) was removed from base class.
        // Player.cs and ObjectToProtect.cs handle UI updates in their overrides.

        PlayDamageFeedback();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles entity death logic: disable collision, disable animator,
    /// apply a dramatic "launch" effect, then destroy after delay.
    /// </summary>
    protected virtual void Die()
    {
        // Play Death SFX (Index 2 - Explode)
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(2);
        }

        // Disable interactions (prevents further animation/physics collisions).
        anim.enabled = false;
        col.enabled = false;

        // Apply a "knockback / launch" effect for visual feedback.
        rb.gravityScale = 12;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);

        // Cleanup after a short delay.
        Destroy(gameObject, 3);
    }

    /// <summary>
    /// Ensures damage feedback coroutine doesn't overlap.
    /// </summary>
    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);

        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCo());
    }

    /// <summary>
    /// Temporarily swaps the sprite material to create a hit-flash effect.
    /// </summary>
    private IEnumerator DamageFeedbackCo()
    {
        // Cache current material and restore it after flash.
        Material originalMat = sr.material;

        sr.material = damageMaterial;
        yield return new WaitForSeconds(damageFeedbackDuration);

        sr.material = originalMat;
    }

    /// <summary>
    /// Enables or disables movement/jumping.
    /// Useful for stun, cutscenes, hit reactions, etc.
    /// </summary>
    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
        canJump = enable;
    }

    /// <summary>
    /// Accessors for child classes or UI systems.
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void OnDrawGizmos()
    {
        // Debug visualization in Scene view.
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
