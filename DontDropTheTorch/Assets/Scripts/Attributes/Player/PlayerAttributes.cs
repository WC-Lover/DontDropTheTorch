public class PlayerAttributes
{
    public HealthAttributes HealthAttributes;
    public WeaponAttributes WeaponAttributes;
    public MovementAttributes MovementAttributes;
    public EnemySpawnAttributes EnemySpawnAttributes;
    public TradingAttributes TradingAttributes;

    public PlayerAttributes()
    {
        HealthAttributes = new HealthAttributes();
        WeaponAttributes = new WeaponAttributes();
        MovementAttributes = new MovementAttributes();
        EnemySpawnAttributes = new EnemySpawnAttributes();
        TradingAttributes = new TradingAttributes();
    }
}