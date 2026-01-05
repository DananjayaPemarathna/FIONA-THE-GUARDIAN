using UnityEngine;

public class ObjectToProtect : Entity
{
    [Header("Protection Target")]
    [SerializeField] private Transform player;

    protected override void Awake()
    {
        base.Awake();

        if (player == null)
        {
            Player playerScript = FindFirstObjectByType<Player>();
            if (playerScript != null)
                player = playerScript.transform;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (UI.instance != null)
            UI.instance.UpdateObjectHealth(GetMaxHealth(), GetMaxHealth());
    }

    protected override void Update()
    {
        if (player == null) return;
        HandleFlip();
    }

    protected override void HandleFlip()
    {
        if (player.position.x > transform.position.x && !facingRight) Flip();
        else if (player.position.x < transform.position.x && facingRight) Flip();
    }

    public override void TakeDamage()
    {
        base.TakeDamage();

        if (UI.instance != null)
            UI.instance.UpdateObjectHealth(GetCurrentHealth(), GetMaxHealth());
    }

    protected override void Die()
    {
        base.Die();

        // ✅ triggers sound + gameover UI
        if (UI.instance != null)
            UI.instance.TriggerGameOver();
    }
}
