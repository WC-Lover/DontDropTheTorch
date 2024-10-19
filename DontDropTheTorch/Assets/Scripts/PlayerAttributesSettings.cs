using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PlayerAttributesSettings
{
    #region Weapon

    public static float Damage { get; set; }
    public static int RoundAmmo { get; set; }
    public static int ProjectileAmount { get; set; }
    public static float ProjectileSpreadAngle { get; set; }
    public static int Penetration { get; set; }
    public static float Accuracy { get; set; }
    public static float Range { get; set; }
    public static float Crit { get; set; }
    public static float CritChance { get; set; }
    public static float FireRate { get; set; }
    public static float ReloadTime { get; set; }

    #endregion

    #region  MovementAttributes

    public static float Stamina { get; set; }
    public static float StaminaRegenerationAmount { get; set; }
    public static float StaminaRegenerationCooldown { get; set; }
    public static float MoveSpeed { get; set; }
    public static float BoostSpeedMultiplier { get; set; }
    public static float BoostStaminaCost { get; set; }
    public static float DashSpeedMultiplier { get; set; }
    public static float DashStaminaCost { get; set; }
    public static float DashCooldown { get; set; }
    public static float DashDuration { get; set; }

    #endregion

    #region Trading

    public static float TraderSpawnTime { get; set; }
    public static float TradingTime { get; set; }

    #endregion

    #region Health

    public static float HealthAmount { get; set; }
    public static float HealthRegenerationPercent { get; set; }
    public static float HealthRegenerationCooldown { get; set; }
    public static float Stress { get; set; }
    public static float StressReductionAmount { get; set; }
    public static float StressReductionCooldown { get; set; }

    #endregion

    public static void SaveSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "mySettingsData.txt");
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            // Combat Attributes
            writer.WriteLine($"Damage={Damage}");
            writer.WriteLine($"RoundAmmo={RoundAmmo}");
            writer.WriteLine($"ProjectileAmount={ProjectileAmount}");
            writer.WriteLine($"ProjectileSpreadAngle={ProjectileSpreadAngle}");
            writer.WriteLine($"Penetration={Penetration}");
            writer.WriteLine($"Accuracy={Accuracy}");
            writer.WriteLine($"Range={Range}");
            writer.WriteLine($"Crit={Crit}");
            writer.WriteLine($"CritChance={CritChance}");
            writer.WriteLine($"FireRate={FireRate}");
            writer.WriteLine($"ReloadTime={ReloadTime}");

            // Movement Attributes
            writer.WriteLine($"Stamina={Stamina}");
            writer.WriteLine($"StaminaRegenerationAmount={StaminaRegenerationAmount}");
            writer.WriteLine($"StaminaRegenerationCooldown={StaminaRegenerationCooldown}");
            writer.WriteLine($"MoveSpeed={MoveSpeed}");
            writer.WriteLine($"BoostSpeedMultiplier={BoostSpeedMultiplier}");
            writer.WriteLine($"BoostStaminaCost={BoostStaminaCost}");
            writer.WriteLine($"DashSpeedMultiplier={DashSpeedMultiplier}");
            writer.WriteLine($"DashStaminaCost={DashStaminaCost}");
            writer.WriteLine($"DashCooldown={DashCooldown}");
            writer.WriteLine($"DashDuration={DashDuration}");

            // Trading Attributes
            writer.WriteLine($"TraderSpawnTime={TraderSpawnTime}");
            writer.WriteLine($"TradingTime={TradingTime}");

            // Health Attributes
            writer.WriteLine($"HealthAmount={HealthAmount}");
            writer.WriteLine($"HealthRegenerationPercent={HealthRegenerationPercent}");
            writer.WriteLine($"HealthRegenerationCooldown={HealthRegenerationCooldown}");
            writer.WriteLine($"Stress={Stress}");
            writer.WriteLine($"StressReductionAmount={StressReductionAmount}");
            writer.WriteLine($"StressReductionCooldown={StressReductionCooldown}");
        }
    }

    public static void LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "mySettingsData.txt");

        if (File.Exists(filePath))
        {
            LoadSettingsFromFile(filePath);
        }
        else
        {
            TextAsset settingsData = Resources.Load<TextAsset>("mySettingsData");

            if (settingsData != null)
            {
                File.WriteAllText(filePath, settingsData.text);
                LoadSettingsFromFile(filePath);
            }
        }

    }

    public static void LoadSettingsFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitAttribute = line.Split('=');
                    if (splitAttribute.Length == 2)
                    {
                        string key = splitAttribute[0];
                        float value = float.Parse(splitAttribute[1]);
                        switch (key)
                        {
                            // Combat Attributes
                            case "Damage":
                                Damage = value;
                                break;
                            case "RoundAmmo":
                                RoundAmmo = (int)value;
                                break;
                            case "ProjectileAmount":
                                ProjectileAmount = (int)value;
                                break;
                            case "ProjectileSpreadAngle":
                                ProjectileSpreadAngle = value;
                                break;
                            case "Penetration":
                                Penetration = (int)value;
                                break;
                            case "Accuracy":
                                Accuracy = value;
                                break;
                            case "Range":
                                Range = value;
                                break;
                            case "Crit":
                                Crit = value;
                                break;
                            case "CritChance":
                                CritChance = value;
                                break;
                            case "FireRate":
                                FireRate = value;
                                break;
                            case "ReloadTime":
                                ReloadTime = value;
                                break;

                            // Movement Attributes
                            case "Stamina":
                                Stamina = value;
                                break;
                            case "StaminaRegenerationAmount":
                                StaminaRegenerationAmount = value;
                                break;
                            case "StaminaRegenerationCooldown":
                                StaminaRegenerationCooldown = value;
                                break;
                            case "MoveSpeed":
                                MoveSpeed = value;
                                break;
                            case "BoostSpeedMultiplier":
                                BoostSpeedMultiplier = value;
                                break;
                            case "BoostStaminaCost":
                                BoostStaminaCost = value;
                                break;
                            case "DashSpeedMultiplier":
                                DashSpeedMultiplier = value;
                                break;
                            case "DashStaminaCost":
                                DashStaminaCost = value;
                                break;
                            case "DashCooldown":
                                DashCooldown = value;
                                break;
                            case "DashDuration":
                                DashDuration = value;
                                break;

                            // Trading Attributes
                            case "TraderSpawnTime":
                                TraderSpawnTime = value;
                                break;
                            case "TradingTime":
                                TradingTime = value;
                                break;

                            // Health Attributes
                            case "HealthAmount":
                                HealthAmount = value;
                                break;
                            case "HealthRegenerationPercent":
                                HealthRegenerationPercent = value;
                                break;
                            case "HealthRegenerationCooldown":
                                HealthRegenerationCooldown = value;
                                break;
                            case "Stress":
                                Stress = value;
                                break;
                            case "StressReductionAmount":
                                StressReductionAmount = value;
                                break;
                            case "StressReductionCooldown":
                                StressReductionCooldown = value;
                                break;
                        }
                    }
                }
            }
        }
    }
}
