public class HealthAttributes
{
    public float HealthAmount { get; set; }
    public float HealthRegenerationPercent { get; set; }
    public float HealthRegenerationCooldown { get; set; }
    public float FearAmount { get; set; }
    public float FearIncrease { get; set; }
    public float Calmness { get; set; }
    public float CalmnessRegenerationAmount { get; set; }
    public float CalmnessRegenerationCooldown { get; set; }

    public HealthAttributes()
    {
        HealthAmount = PlayerAttributesSettings.HealthAmount;
        HealthRegenerationPercent = PlayerAttributesSettings.HealthRegenerationPercent;
        HealthRegenerationCooldown = PlayerAttributesSettings.HealthRegenerationCooldown;
        FearAmount = PlayerAttributesSettings.FearAmount;
        FearIncrease = PlayerAttributesSettings.FearIncrease;
        Calmness = PlayerAttributesSettings.Calmness;
        CalmnessRegenerationAmount = PlayerAttributesSettings.CalmnessRegenerationAmount;
        CalmnessRegenerationCooldown = PlayerAttributesSettings.CalmnessRegenerationCooldown;
    }
}