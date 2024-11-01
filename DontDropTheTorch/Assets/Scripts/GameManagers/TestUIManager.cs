using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;

public class TestUIManager : MonoBehaviour
{
    #region UNITY_EDITOR
    [SerializeField] private GameObject Weapon;
    [SerializeField] private GameObject Movement;
    [SerializeField] private GameObject Health;

    [SerializeField] private TMP_InputField damage;
    [SerializeField] private TMP_InputField roundAmmo;
    [SerializeField] private TMP_InputField projectileAmount;
    [SerializeField] private TMP_InputField projectileSpreadAngle;
    [SerializeField] private TMP_InputField penetration;
    [SerializeField] private TMP_InputField accuracy;
    [SerializeField] private TMP_InputField range;
    [SerializeField] private TMP_InputField crit;
    [SerializeField] private TMP_InputField critChance;
    [SerializeField] private TMP_InputField fireRate;
    [SerializeField] private TMP_InputField reloadTime;

    [SerializeField] private TMP_InputField stamina;
    [SerializeField] private TMP_InputField staminaRegenerationRate;
    [SerializeField] private TMP_InputField staminaRegenerationCooldown;
    [SerializeField] private TMP_InputField moveSpeed;
    [SerializeField] private TMP_InputField boostSpeedMultiplier;
    [SerializeField] private TMP_InputField boostStaminaCost;
    [SerializeField] private TMP_InputField dashSpeedMultiplier;
    [SerializeField] private TMP_InputField dashStaminaCost;
    [SerializeField] private TMP_InputField dashCooldown;
    [SerializeField] private TMP_InputField dashDuration;

    [SerializeField] private TMP_InputField healthAmount;
    [SerializeField] private TMP_InputField healthRegenerationPercent;
    [SerializeField] private TMP_InputField healthRegenerationCooldown;
    [SerializeField] private TMP_InputField fearAmount;
    [SerializeField] private TMP_InputField fearIncrease;
    [SerializeField] private TMP_InputField stress;
    [SerializeField] private TMP_InputField stressReductionAmount;
    [SerializeField] private TMP_InputField stressReductionCooldown;

    [SerializeField] private TMP_InputField traderSpawnTime;
    [SerializeField] private TMP_InputField tradingTime;

    [SerializeField] private float x;
    [SerializeField] private float y = 100;
    [SerializeField] private float width = 220;
    [SerializeField] private float height = 400;

    [SerializeField] private float x2 = 350;
    [SerializeField] private float y2 = 100;
    [SerializeField] private float width2 = 220;
    [SerializeField] private float height2 = 400;

    [SerializeField] private float x3 = 645;
    [SerializeField] private float y3 = 100;
    [SerializeField] private float width3 = 225;
    [SerializeField] private float height3 = 400;
    #endregion

    LobbyManager lobbyManager;
    NetworkManager networkManager;


    void Start()
    {
        lobbyManager = GetComponent<LobbyManager>();
        networkManager = GetComponent<NetworkManager>();
        PlayerAttributesSettings.LoadSettings();
        
#if UNITY_EDITOR
        Weapon.SetActive(true);
        Movement.SetActive(true);
        Health.SetActive(true);
#endif

    }

    void OnGUI()
    {
        if (networkManager.IsClient || networkManager.IsHost) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (lobbyManager.InLobby)
        {
#if UNITY_EDITOR
            Weapon.SetActive(false);
            Movement.SetActive(false);
            Health.SetActive(false);
#endif
            LobbyUI();
        }
        else
        {
            GameUI();
        }
        GUILayout.EndArea();

#if UNITY_EDITOR

        GUILayout.BeginArea(new Rect(x, y, width, height));

        if (GUILayout.Button("Damage")) PlayerAttributesSettings.Damage = float.Parse(damage.text);
        if (GUILayout.Button("RoundAmmo")) PlayerAttributesSettings.ClipCapacity = int.Parse(roundAmmo.text);
        if (GUILayout.Button("ProjectilesAmount")) PlayerAttributesSettings.ProjectileAmount = int.Parse(projectileAmount.text);
        if (GUILayout.Button("ProjectilesAngle")) PlayerAttributesSettings.ProjectileSpreadAngle = int.Parse(projectileSpreadAngle.text);
        if (GUILayout.Button("Penetration")) PlayerAttributesSettings.Penetration = int.Parse(penetration.text);
        if (GUILayout.Button("Accuracy")) PlayerAttributesSettings.Accuracy = float.Parse(accuracy.text);
        if (GUILayout.Button("Range")) PlayerAttributesSettings.Range = float.Parse(range.text);
        if (GUILayout.Button("Crit")) PlayerAttributesSettings.Crit = float.Parse(crit.text);
        if (GUILayout.Button("CritChance")) PlayerAttributesSettings.CritChance = float.Parse(critChance.text);
        if (GUILayout.Button("FireRate")) PlayerAttributesSettings.FireRate = float.Parse(fireRate.text);
        if (GUILayout.Button("ReloadTime")) PlayerAttributesSettings.ReloadTime = float.Parse(reloadTime.text);

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(x2, y2, width2, height2));

        if (GUILayout.Button("Stamina")) PlayerAttributesSettings.Stamina = float.Parse(stamina.text);
        if (GUILayout.Button("StaminaRegenerationAmount")) PlayerAttributesSettings.StaminaRegenerationAmount = float.Parse(staminaRegenerationRate.text);
        if (GUILayout.Button("StaminaRegenerationCd")) PlayerAttributesSettings.StaminaRegenerationCooldown = float.Parse(staminaRegenerationCooldown.text);
        if (GUILayout.Button("MoveSpeed")) PlayerAttributesSettings.MoveSpeed = float.Parse(moveSpeed.text);
        if (GUILayout.Button("BoostSpeedMultiplier")) PlayerAttributesSettings.BoostSpeedMultiplier = float.Parse(boostSpeedMultiplier.text);
        if (GUILayout.Button("BoostStaminaCost")) PlayerAttributesSettings.BoostStaminaCost = float.Parse(boostStaminaCost.text);
        if (GUILayout.Button("DashSpeedMultiplier")) PlayerAttributesSettings.DashSpeedMultiplier = float.Parse(dashSpeedMultiplier.text);
        if (GUILayout.Button("DashStaminaCost")) PlayerAttributesSettings.DashStaminaCost = float.Parse(dashStaminaCost.text);
        if (GUILayout.Button("DashCooldown")) PlayerAttributesSettings.DashCooldown = float.Parse(dashCooldown.text);
        if (GUILayout.Button("DashDuration")) PlayerAttributesSettings.DashDuration = float.Parse(dashDuration.text);

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(x3, y3, width3, height3));

        if (GUILayout.Button("HealthAmount")) PlayerAttributesSettings.HealthAmount = float.Parse(healthAmount.text);
        if (GUILayout.Button("HealthRegenerationPercent")) PlayerAttributesSettings.HealthRegenerationPercent = float.Parse(healthRegenerationPercent.text);
        if (GUILayout.Button("HealthRegenerationCooldown")) PlayerAttributesSettings.HealthRegenerationCooldown = float.Parse(healthRegenerationCooldown.text);
        if (GUILayout.Button("Stress")) PlayerAttributesSettings.Stress = float.Parse(stress.text);
        if (GUILayout.Button("StressReductionAmount")) PlayerAttributesSettings.StressReductionAmount = float.Parse(stressReductionAmount.text);
        if (GUILayout.Button("StressReductionCooldown")) PlayerAttributesSettings.StressReductionCooldown = float.Parse(stressReductionCooldown.text);
        
        if (GUILayout.Button("TraderSpawnTime")) PlayerAttributesSettings.TraderSpawnTime = int.Parse(traderSpawnTime.text);
        if (GUILayout.Button("TradingTime")) PlayerAttributesSettings.TradingTime = int.Parse(tradingTime.text);

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(650, 10, 300, 300));
        if (GUILayout.Button("SaveSettings"))
        {
            PlayerAttributesSettings.SaveSettings();
        }
        if (GUILayout.Button("LoadSettings"))
        {
            PlayerAttributesSettings.LoadSettings();
        }
        GUILayout.EndArea();
#endif
    }

    void LobbyUI()
    {
        if (lobbyManager.IsLobbyLeader)
        {
            if (GUILayout.Button($"Start Game")) lobbyManager.StartGame();
        }
        //else if (GUILayout.Button($"Start Client")) networkManager.StartClient();

        if (GUILayout.Button("Leave Lobby")) lobbyManager.LeaveLobbyAsync();
    }

    void GameUI()
    {
        if (GUILayout.Button("Create Lobby")) lobbyManager.CreateLobbyAsync();
        if (GUILayout.Button("Connect to Lobby")) lobbyManager.ListLobbies();

    }

}
