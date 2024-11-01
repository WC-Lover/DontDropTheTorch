using UnityEngine;

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
    public float LeapSpeedMultiplier { get; set; }
    public float LeapEffectTime { get; set; }
    public float LeapCooldown { get; set; }

    public EnemyAttributes()
    {
        Speed = 2f;
        Damage = 20f;
        Health = 100f;
        AttackRange = 1.5f;
        LeapRange = 4f;
        Defence = 1f;
        AttackCooldown = 1.5f;
        AttackPushMultiplier = 3f;
        LeapSpeedMultiplier = 3f;
        LeapEffectTime = 0.5f;
        LeapCooldown = 2f;
    }
}