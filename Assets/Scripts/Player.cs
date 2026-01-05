using UnityEngine;

public class Player : Entity
{
    protected override void Start()
    {
        base.Start();

        if (UI.instance != null)
            UI.instance.UpdatePlayerHealth(GetMaxHealth(), GetMaxHealth());
    }

    protected override void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToJump();
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(0);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HandleAttack();
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(1);
        }
    }

    public override void TakeDamage()
    {
        base.TakeDamage();

        if (UI.instance != null)
            UI.instance.UpdatePlayerHealth(GetCurrentHealth(), GetMaxHealth());
    }

    protected override void Die()
    {
        base.Die();

        var cam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (cam != null) cam.Follow = null;

        // ✅ THIS is what triggers game over sound + UI
        if (UI.instance != null)
            UI.instance.TriggerGameOver();
    }
}
