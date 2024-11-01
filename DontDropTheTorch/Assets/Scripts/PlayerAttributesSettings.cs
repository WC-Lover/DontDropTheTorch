using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PlayerAttributesSettings
{
    #region Weapon

    public static float Damage { get; set; }
    public static int ClipCapacity { get; set; }
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
            writer.WriteLine($"ClipCapacity={ClipCapacity}");
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
        TextAsset settingsData = Resources.Load<TextAsset>("mySettingsData");
        string settingsString = settingsData.text;
        string[] splitSettingsString = settingsString.Split("\n");
        if (splitSettingsString.Length != 0)
        {
            foreach (string keyValue in splitSettingsString)
            {
                string[] splitKeyValue = keyValue.Split("=");
                string key = splitKeyValue[0];
                string valueString = splitKeyValue[1];
                if (float.TryParse(valueString, out float result))
                {
                    float floatValue = result;
                    switch (key)
                    {
                        // Combat Attributes
                        case "Damage":
                            Damage = floatValue;
                            break;
                        case "ClipCapacity":
                            ClipCapacity = (int)floatValue;
                            break;
                        case "ProjectileAmount":
                            ProjectileAmount = (int)floatValue;
                            break;
                        case "ProjectileSpreadAngle":
                            ProjectileSpreadAngle = floatValue;
                            break;
                        case "Penetration":
                            Penetration = (int)floatValue;
                            break;
                        case "Accuracy":
                            Accuracy = floatValue;
                            break;
                        case "Range":
                            Range = floatValue;
                            break;
                        case "Crit":
                            Crit = floatValue;
                            break;
                        case "CritChance":
                            CritChance = floatValue;
                            break;
                        case "FireRate":
                            FireRate = floatValue / 100;
                            break;
                        case "ReloadTime":
                            ReloadTime = floatValue;
                            break;

                        // Movement Attributes
                        case "Stamina":
                            Stamina = floatValue;
                            break;
                        case "StaminaRegenerationAmount":
                            StaminaRegenerationAmount = floatValue;
                            break;
                        case "StaminaRegenerationCooldown":
                            StaminaRegenerationCooldown = floatValue;
                            break;
                        case "MoveSpeed":
                            MoveSpeed = floatValue / 100;
                            break;
                        case "BoostSpeedMultiplier":
                            BoostSpeedMultiplier = floatValue / 100;
                            break;
                        case "BoostStaminaCost":
                            BoostStaminaCost = floatValue / 100;
                            break;
                        case "DashSpeedMultiplier":
                            DashSpeedMultiplier = floatValue;
                            break;
                        case "DashStaminaCost":
                            DashStaminaCost = floatValue;
                            break;
                        case "DashCooldown":
                            DashCooldown = floatValue / 100;
                            break;
                        case "DashDuration":
                            DashDuration = floatValue / 100;
                            break;

                        // Trading Attributes
                        case "TraderSpawnTime":
                            TraderSpawnTime = floatValue;
                            break;
                        case "TradingTime":
                            TradingTime = floatValue;
                            break;

                        // Health Attributes
                        case "HealthAmount":
                            HealthAmount = floatValue;
                            break;
                        case "HealthRegenerationPercent":
                            HealthRegenerationPercent = floatValue;
                            break;
                        case "HealthRegenerationCooldown":
                            HealthRegenerationCooldown = floatValue;
                            break;
                        case "Stress":
                            Stress = floatValue;
                            break;
                        case "StressReductionAmount":
                            StressReductionAmount = floatValue;
                            break;
                        case "StressReductionCooldown":
                            StressReductionCooldown = floatValue;
                            break;
                    }
                }
            }
        }
        //if (File.Exists(filePath))
        //{
        //    using (StreamReader reader = new StreamReader(filePath))
        //    {
        //        string line;
        //        while ((line = reader.ReadLine()) != null)
        //        {
        //            string[] splitAttribute = line.Split('=');
        //            if (splitAttribute.Length == 2)
        //            {
        //                string key = splitAttribute[0];
        //                string value = splitAttribute[1];
        //                Debug.Log($"key: {key}, value: {value}");
        //                float floatValue = float.Parse(value);
        //                switch (key)
        //                {
        //                    // Combat Attributes
        //                    case "Damage":
        //                        Damage = floatValue;
        //                        break;
        //                    case "RoundAmmo":
        //                        RoundAmmo = (int)floatValue;
        //                        break;
        //                    case "ProjectileAmount":
        //                        ProjectileAmount = (int)floatValue;
        //                        break;
        //                    case "ProjectileSpreadAngle":
        //                        ProjectileSpreadAngle = floatValue;
        //                        break;
        //                    case "Penetration":
        //                        Penetration = (int)floatValue;
        //                        break;
        //                    case "Accuracy":
        //                        Accuracy = floatValue;
        //                        break;
        //                    case "Range":
        //                        Range = floatValue;
        //                        break;
        //                    case "Crit":
        //                        Crit = floatValue;
        //                        break;
        //                    case "CritChance":
        //                        CritChance = floatValue;
        //                        break;
        //                    case "FireRate":
        //                        FireRate = floatValue;
        //                        break;
        //                    case "ReloadTime":
        //                        ReloadTime = floatValue;
        //                        break;

        //                    // Movement Attributes
        //                    case "Stamina":
        //                        Stamina = floatValue;
        //                        break;
        //                    case "StaminaRegenerationAmount":
        //                        StaminaRegenerationAmount = floatValue;
        //                        break;
        //                    case "StaminaRegenerationCooldown":
        //                        StaminaRegenerationCooldown = floatValue;
        //                        break;
        //                    case "MoveSpeed":
        //                        MoveSpeed = floatValue;
        //                        break;
        //                    case "BoostSpeedMultiplier":
        //                        BoostSpeedMultiplier = floatValue;
        //                        break;
        //                    case "BoostStaminaCost":
        //                        BoostStaminaCost = floatValue;
        //                        break;
        //                    case "DashSpeedMultiplier":
        //                        DashSpeedMultiplier = floatValue;
        //                        break;
        //                    case "DashStaminaCost":
        //                        DashStaminaCost = floatValue;
        //                        break;
        //                    case "DashCooldown":
        //                        DashCooldown = floatValue;
        //                        break;
        //                    case "DashDuration":
        //                        DashDuration = floatValue;
        //                        break;

        //                    // Trading Attributes
        //                    case "TraderSpawnTime":
        //                        TraderSpawnTime = floatValue;
        //                        break;
        //                    case "TradingTime":
        //                        TradingTime = floatValue;
        //                        break;

        //                    // Health Attributes
        //                    case "HealthAmount":
        //                        HealthAmount = floatValue;
        //                        break;
        //                    case "HealthRegenerationPercent":
        //                        HealthRegenerationPercent = floatValue;
        //                        break;
        //                    case "HealthRegenerationCooldown":
        //                        HealthRegenerationCooldown = floatValue;
        //                        break;
        //                    case "Stress":
        //                        Stress = floatValue;
        //                        break;
        //                    case "StressReductionAmount":
        //                        StressReductionAmount = floatValue;
        //                        break;
        //                    case "StressReductionCooldown":
        //                        StressReductionCooldown = floatValue;
        //                        break;
        //                }
        //            }
        //        }
            //}
        //}
    }
}
