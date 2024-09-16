public class PlayerAttributes
{
    public HealthAttributes HealthAttributes;
    public WeaponAttributes WeaponAttributes;
    public MovementAttributes MovementAttributes;

    public PlayerAttributes()
    {
        HealthAttributes = new HealthAttributes();
        WeaponAttributes = new WeaponAttributes();
        MovementAttributes = new MovementAttributes();
    }
}