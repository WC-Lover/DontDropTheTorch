public class HealthAttributes
{
    public float HealthAmount { get; set; }
    public float HealthRegenerationAmount { get; set; }
    public float HealthRegenerationCooldown { get; set; }
    public float FearAmount { get; set; }
    public float FearIncrease { get; set; }
    public float Calmness { get; set; }
    public float CalmnessRegenerationAmount { get; set; }
    public float CalmnessRegenerationCooldown { get; set; }

    public HealthAttributes()
    {
        HealthAmount = 100;
        HealthRegenerationAmount = 0;
        HealthRegenerationCooldown = 10;
        FearAmount = 0;
        FearIncrease = 1f;
        Calmness = 100f;
        CalmnessRegenerationAmount = 1f;
        CalmnessRegenerationCooldown = 5f;
    }
}