public class WeaponAttributes
{
    public float Damage { get; set; }
    public int ClipCapacity { get; set; }
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
        Damage = PlayerAttributesSettings.Damage;
        ProjectileAmount = PlayerAttributesSettings.ProjectileAmount;
        ProjectileSpreadAngle = PlayerAttributesSettings.ProjectileSpreadAngle;
        Penetration = PlayerAttributesSettings.Penetration;
        Accuracy = PlayerAttributesSettings.Accuracy;
        Range = PlayerAttributesSettings.Range;
        Crit = PlayerAttributesSettings.Crit;
        CritChance = PlayerAttributesSettings.CritChance;
        FireRate = PlayerAttributesSettings.FireRate;
        ReloadTime = PlayerAttributesSettings.ReloadTime;
        ClipCapacity = PlayerAttributesSettings.RoundAmmo;
    }
}