using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSystem : NetworkBehaviour
{
    WeaponAttributes weaponAttributes;

    float fireRate;
    bool fireAlready;
    int roundAmmo;

    private WeaponSFXController weaponSFXController;

    [SerializeField] private Image reloadImage;

    [SerializeField] Transform shotTrailPistol;

    private Camera mainCam;
    private float reloadTime;

    public override void OnNetworkSpawn()
    {
        weaponSFXController = GetComponent<WeaponSFXController>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        
    }

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        weaponAttributes = playerAttributes.WeaponAttributes;

        fireAlready = false;
        fireRate = weaponAttributes.FireRate;

        roundAmmo = weaponAttributes.RoundAmmo;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        if (reloadTime > 0)
        {
            ReloadingProcess();
            return;
        }

        if (!Cursor.visible) Cursor.visible = true;

        if (Input.GetKey(KeyCode.Mouse0) && roundAmmo == 0)
        {
            weaponSFXController.EmptyAmmoShotSFX();
            return;
        }

        if (Input.GetKey(KeyCode.Mouse0) && !fireAlready && roundAmmo > 0)
        {
            Fire();
        }

        if (Input.GetKey(KeyCode.R))
        {
            Reload();
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

    private void ReloadingProcess()
    {
        reloadTime -= Time.deltaTime;
        var mouasePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouasePos.z = 1;
        reloadImage.transform.position = mouasePos;
        reloadImage.fillAmount = Mathf.Clamp(reloadTime / weaponAttributes.ReloadTime, 0, 1);
    }

    private void Reload()
    {
        Cursor.visible = false;
        roundAmmo = weaponAttributes.RoundAmmo;
        reloadTime = weaponAttributes.ReloadTime;
        weaponSFXController.ReloadSFX();
    }

    private void Fire()
    {
        fireAlready = true;
        roundAmmo--;

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
            ShootSFX();

            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, weaponAttributes.Range);
            if (hit.collider != null)
            {
                if (hit.transform.TryGetComponent<EnemyController>(out var enemyController))
                {
                    enemyController.DealDamageToEnemyRpc(weaponAttributes.Damage, rayDirection); // if true -> killed an enemy
                }
            }
        }

        #endregion
    }

    private void ShootSFX()
    {
        weaponSFXController.ShootSFX();
        if (IsOwner) ShootSFXRpc(); 
    }

    [Rpc(SendTo.NotMe)]
    private void ShootSFXRpc()
    {
        ShootSFX();
    }

    private void CreateShotTrail(Vector2 rayDirection)
    {
        Instantiate(shotTrailPistol, transform.position, Quaternion.LookRotation(rayDirection));
    }
}
