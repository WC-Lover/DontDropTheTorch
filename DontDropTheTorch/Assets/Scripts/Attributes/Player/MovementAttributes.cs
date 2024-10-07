using UnityEngine;

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
        Stamina = PlayerAttributesSettings.Stamina;
        StaminaRegenerationAmount = PlayerAttributesSettings.StaminaRegenerationAmount;
        StaminaRegenerationCooldown = PlayerAttributesSettings.StaminaRegenerationCooldown;
        MoveSpeed = PlayerAttributesSettings.MoveSpeed;
        BoostSpeedMultiplier = PlayerAttributesSettings.BoostSpeedMultiplier;
        BoostStaminaCost = PlayerAttributesSettings.BoostStaminaCost;
        DashSpeedMultiplier = PlayerAttributesSettings.DashSpeedMultiplier;
        DashStaminaCost = PlayerAttributesSettings.DashStaminaCost;
        DashCooldown = PlayerAttributesSettings.DashCooldown;
        DashDuration = PlayerAttributesSettings.DashDuration;
    }
}