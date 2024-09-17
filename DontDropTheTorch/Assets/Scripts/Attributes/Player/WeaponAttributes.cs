public class WeaponAttributes
{
    public float Damage { get; set; }
    public int ProjectileAmount { get; set; }
    public float ProjectileSpreadAngle { get; set; }
    public int Penetration { get; set; }
    public float Accuracy { get; set; }
    public float Range { get; set; }
    public float Crit { get; set; }
    public float CritChance { get; set; }
    public float FireRate { get; set; }
    public float ReloadTime { get; set; }

    public WeaponAttributes()
    {
        Damage = 10f;
        ProjectileAmount = 1;
        ProjectileSpreadAngle = 15f;
        Penetration = 0;
        Accuracy = 100f;
        Range = 10f;
        Crit = 2f;
        CritChance = 1f;
        FireRate = 0.25f;
        ReloadTime = 1f;
    }
}