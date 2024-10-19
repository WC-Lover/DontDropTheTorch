public class EnemyAttributes
{
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float Health { get; set; }
    public float AttackRange { get; set; }
    public float LeapRange { get; set; }
    public float Defence { get; set; }
    public float AttackCooldown { get; set; }
    public float AttackPushMultiplier { get; set; }

    public EnemyAttributes()
    {
        Speed = 2f;
        Damage = 5f;
        Health = 5f;
        AttackRange = 0.25f;
        LeapRange = 1.5f;
        Defence = 0f;
        AttackCooldown = 1.5f;
        AttackPushMultiplier = 5f;
    }
}