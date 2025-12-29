using UnityEngine;

/// <summary>
/// Bridge script between Animator events and the Entity gameplay logic.
/// This component is typically placed on the same GameObject that has the Animator,
/// or on a child object under an Entity.
/// 
/// Animation Events can call these methods to:
/// - Apply attack damage at the correct animation frame
/// - Temporarily disable movement/jumping during attack animations
/// </summary>
public class Entity_AnimationEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        // Cache the reference to the parent Entity component.
        // This allows the Animator (usually on a child) to control the main Entity logic.
        entity = GetComponentInParent<Entity>();
    }

    /// <summary>
    /// Called via an Animation Event during the attack animation.
    /// Applies damage to valid targets inside the Entity attack radius.
    /// </summary>
    public void DamageTargets()
    {
        // Safety check: prevents errors if Entity is missing.
        if (entity == null) return;

        entity.DamageTargets();
    }

    /// <summary>
    /// Called via an Animation Event to lock movement and jumping.
    /// Useful during attack wind-up or heavy animations.
    /// </summary>
    private void DisableMovementAndJump()
    {
        if (entity == null) return;
        entity.EnableMovement(false);
    }

    /// <summary>
    /// Called via an Animation Event to re-enable movement and jumping.
    /// </summary>
    private void EnableMovementAndJump()
    {
        if (entity == null) return;
        entity.EnableMovement(true);
    }
}
