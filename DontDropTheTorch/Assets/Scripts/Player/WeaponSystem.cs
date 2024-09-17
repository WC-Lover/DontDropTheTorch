using System;
using Unity.Netcode;
using UnityEngine;

public class WeaponSystem : NetworkBehaviour
{
    WeaponAttributes weaponAttributes;

    float fireRate;
    bool fireAlready;
    [SerializeField] Transform shotTrailPistol;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        weaponAttributes = playerAttributes.WeaponAttributes;

        fireAlready = false;
        fireRate = weaponAttributes.FireRate;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        if (Input.GetKey(KeyCode.Mouse0) && !fireAlready)
        {
            Fire();
        }

        if (fireAlready)
        {
            fireRate -= Time.deltaTime;
            if (fireRate <= 0)
            {
                fireRate = weaponAttributes.FireRate;
                fireAlready = false;
            }
        }
    }

    private void Fire()
    {
        fireAlready = true;

        #region Ray Cast Shooting

        Vector2 weaponDirection = transform.right;
        Vector2 rayDirection = weaponDirection;

        for (int i = 0; i < weaponAttributes.ProjectileAmount; i++)
        {
            if (weaponAttributes.ProjectileAmount > 1)
            {
                float angle = (-weaponAttributes.ProjectileSpreadAngle / 2) + (weaponAttributes.ProjectileSpreadAngle / (weaponAttributes.ProjectileAmount - 1) * i);
                rayDirection = Quaternion.Euler(0, 0, angle) * weaponDirection;
            }

            CreateShotTrail(rayDirection);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, weaponAttributes.Range);
            if (hit.collider != null)
            {
                if (hit.transform.TryGetComponent<EnemyController>(out var enemyController))
                {
                    enemyController.DealDamageToEnemyRpc(weaponAttributes.Damage); // if true -> killed an enemy
                }
            }

            Debug.DrawRay(transform.position, rayDirection * 100f, Color.red, 2f);
        }

        #endregion
    }

    private void CreateShotTrail(Vector2 rayDirection)
    {
        Instantiate(shotTrailPistol, transform.position, Quaternion.LookRotation(rayDirection));
    }
}
