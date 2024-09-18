public class MovementAttributes
{
    public float Stamina { get; set; }
    public float StaminaRegenerationAmount { get; set; }
    public float StaminaRegenerationCooldown { get; set; }
    public float MoveSpeed { get; set; }
    public float BoostSpeedMultiplier { get; set; }
    public float BoostStaminaCost { get; set; }
    public float DashSpeedMultiplier { get; set; }
    public float DashStaminaCost { get; set; }
    public float DashCooldown { get; set; }
    public float DashDuration { get; set; }

    public MovementAttributes()
    {
        Stamina = 100f;
        StaminaRegenerationAmount = 2f;
        StaminaRegenerationCooldown = 5f;
        MoveSpeed = 3f;
        BoostSpeedMultiplier = 1.75f;
        BoostStaminaCost = 1.5f;
        DashSpeedMultiplier = 5f;
        DashStaminaCost = 30f;
        DashCooldown = 2.5f;
        DashDuration = 0.25f;
    }
}