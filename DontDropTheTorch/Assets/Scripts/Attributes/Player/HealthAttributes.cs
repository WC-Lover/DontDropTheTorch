public class HealthAttributes
{
    public float HealthAmount { get; set; }
    public float HealthRegenerationPercent { get; set; }
    public float HealthRegenerationCooldown { get; set; }
    public float Stress { get; set; }
    public float StressReductionAmount { get; set; }
    public float StressReductionCooldown { get; set; }

    public HealthAttributes()
    {
        HealthAmount = PlayerAttributesSettings.HealthAmount;
        HealthRegenerationPercent = PlayerAttributesSettings.HealthRegenerationPercent;
        HealthRegenerationCooldown = PlayerAttributesSettings.HealthRegenerationCooldown;
        Stress = PlayerAttributesSettings.Stress;
        StressReductionAmount = PlayerAttributesSettings.StressReductionAmount;
        StressReductionCooldown = PlayerAttributesSettings.StressReductionCooldown;
    }
}