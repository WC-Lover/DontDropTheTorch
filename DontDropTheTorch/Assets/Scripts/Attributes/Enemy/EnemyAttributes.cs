public class EnemyAttributes
{
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float Health { get; set; }
    public float AttackRange { get; set; }
    public float Defence { get; set; }
    public float AttackCooldown { get; set; }
    public float AttackPushMultiplier { get; set; }

    public EnemyAttributes()
    {
        Speed = 3f;
        Damage = 45f;
        Health = 1f;
        AttackRange = 1.5f;
        Defence = 0f;
        AttackCooldown = 1.5f;
        AttackPushMultiplier = 10f;
    }
}