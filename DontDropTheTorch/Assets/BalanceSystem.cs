using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceSystem : MonoBehaviour
{
    List<Transform> playersTransform;
    List<HealthSystem> playersHS;
    List<MovementSystem> playersMS;
    List<WeaponSystem> playersWS;

    public float playersAverageDamage { get; private set; }
    public float playersAverageMoveSpeed { get; private set; }

    void SetPlayersSystems()
    {
        foreach (Transform t in playersTransform)
        {
            playersHS.Add(t.GetComponent<HealthSystem>());
            playersMS.Add(t.GetComponent<MovementSystem>());
            playersWS.Add(t.GetComponentInChildren<WeaponSystem>());
        }
    }

    // According to players Health I can set Damage of Enemies
    // vise versa knowing Players average Damage I can set Enemies Health
    void CalculateAveragePlayerDamage()
    {
        float avgDamage = 0;
        for (int i = 0; i < playersTransform.Count; i++)
        {
            HealthSystem hs = playersHS[i];
            MovementSystem ms = playersMS[i];
            WeaponSystem ws = playersWS[i];

            /*
             * EX
             */
            avgDamage += ws.weaponAttributes.Damage;
        }
        avgDamage /= playersTransform.Count;

    }

}
