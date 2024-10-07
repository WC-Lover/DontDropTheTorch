using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Cinemachine;

public class WeaponSystem : NetworkBehaviour
{
    WeaponAttributes weaponAttributes;

    float fireRate;
    bool fireAlready;
    int clipCapacity;

    private WeaponSFXController weaponSFXController;

    [SerializeField] private Image reloadImage;

    [SerializeField] Transform shotTrailPistol;

    private Camera mainCam;
    private float reloadTime;

    [SerializeField] private ScreenShakeProfile screenShakeProfile;
    private CinemachineImpulseSource impulseSource;

    public override void OnNetworkSpawn()
    {
        weaponSFXController = GetComponent<WeaponSFXController>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        weaponAttributes = playerAttributes.WeaponAttributes;

        fireAlready = false;
        fireRate = weaponAttributes.FireRate;

        clipCapacity = weaponAttributes.ClipCapacity;
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

        if (fireAlready)
        {
            fireRate -= Time.deltaTime;
            if (fireRate <= 0)
            {
                fireRate = weaponAttributes.FireRate;
                fireAlready = false;
            }
        }

        if (Input.GetKey(KeyCode.Mouse0) && clipCapacity == 0)
        {
            weaponSFXController.EmptyAmmoShotSFX();
            return;
        }

        if (Input.GetKey(KeyCode.Mouse0) && !fireAlready && clipCapacity > 0)
        {
            Fire();
        }

        if (Input.GetKey(KeyCode.R))
        {
            Reload();
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
        clipCapacity = weaponAttributes.ClipCapacity;
        reloadTime = weaponAttributes.ReloadTime;
        weaponSFXController.ReloadSFX();
    }

    private void Fire()
    {
        fireAlready = true;
        clipCapacity--;

        #region Ray Cast Shooting

        Vector2 weaponDirection = transform.right;
        Vector2 rayDirection = weaponDirection;

        screenShakeProfile.defaultVelocity = -weaponDirection;
        CameraShakeManager.instance.ScreenShakeFromProfile(screenShakeProfile, impulseSource);
        //cameraShake.ShakeCamera();

        for (int i = 0; i < weaponAttributes.ProjectileAmount; i++)
        {
            if (weaponAttributes.ProjectileAmount > 1)
            {
                float angle = (-weaponAttributes.ProjectileSpreadAngle / 2) + (weaponAttributes.ProjectileSpreadAngle / (weaponAttributes.ProjectileAmount - 1) * i);
                rayDirection = Quaternion.Euler(0, 0, angle) * weaponDirection;
            }

            CreateShotTrail(rayDirection);
            ShootSFX();

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rayDirection, weaponAttributes.Range);

            Array.Sort(hits, (RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));

            for (int j = 0; j < hits.Length; j++)
            {
                if (j >= weaponAttributes.Penetration) break;

                if (hits[j].collider != null)
                {
                    if (hits[j].transform.TryGetComponent<EnemyController>(out var enemyController))
                    {
                        int critRandom = Random.Range(1, 100);
                        int accuracyRandom = Random.Range(1, 100);

                        float damage = weaponAttributes.Damage;
                        if (critRandom <= weaponAttributes.CritChance) damage *= weaponAttributes.Crit;
                        if (accuracyRandom <= weaponAttributes.Accuracy) enemyController.DealDamageToEnemy(damage, rayDirection); // if true -> killed an enemy
                    }
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
