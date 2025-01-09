using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class MovementSystem : NetworkBehaviour
{

    private Camera mainCam;
    private Rigidbody2D rigidBody;
    private MovementSFXController movementSFXController;
    private Vector2 mousePosition;
    private Vector2 moveDirection;

    private WeaponSystem weaponSystem;

    // Create stamina bar
    public float stamina;
    private float staminaRegenCooldown;

    private float dashDuration;
    private float dashCooldown;

    public bool isRunning { get; private set; }
    public bool isDashing { get; private set; }
    public bool isBoosting { get; private set; }

    private MovementAttributes attributes;
    private float hookCooldown = 1f;
    private bool isGraplingHook;
    private float hookDuration;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGraplingHook = false;
    }

    public override void OnNetworkSpawn()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        movementSFXController = GetComponentInChildren<MovementSFXController>();
        movementSFXController.SetVolumeValue();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        weaponSystem = GetComponentInChildren<WeaponSystem>();
    }

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        attributes = playerAttributes.MovementAttributes;

        stamina = attributes.Stamina;
        staminaRegenCooldown = attributes.StaminaRegenerationCooldown;

        dashDuration = attributes.DashDuration;
        dashCooldown = attributes.DashCooldown;
    }

    void Update()
    {

        // Unable any movement if dashing
        if (!IsOwner || isDashing) return;

        mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);

        #region Move Direction

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;

        #endregion

    }

    private void LateUpdate()
    {

        if (!IsOwner) return;

        if (isGraplingHook)
        {
            hookDuration -= Time.deltaTime;
            if (hookDuration <= 0) isGraplingHook = false;
            return;
        }

        #region Dash effect

        if (isDashing)
        {
            dashDuration -= Time.deltaTime;
            if (dashDuration <= 0)
            {
                isDashing = false;
                dashDuration = attributes.DashDuration;
            }
            return;
        }

        #endregion

        rigidBody.velocity = moveDirection * attributes.MoveSpeed;

        if (rigidBody.velocity != new Vector2(0, 0)) movementSFXController.RunSFX();
        else movementSFXController.StopSFX();


        #region Rotation

        /*
        * Get angle between players' current angle and point where mouse is.
        * Rotate transform to calculated angle.
        */

        Vector2 rotation = mousePosition - rigidBody.position;
        float rotationZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;

        rigidBody.rotation = rotationZ;

        #endregion

        #region Boost

        if (Input.GetKey(KeyCode.LeftShift) && stamina >= attributes.BoostStaminaCost)
        {
            stamina -= attributes.BoostStaminaCost * Time.deltaTime;
            if (!isBoosting)
            {
                attributes.MoveSpeed *= attributes.BoostSpeedMultiplier;
                isBoosting = true;
            }
        }
        else if (isBoosting)
        {
            isBoosting = false;
            attributes.MoveSpeed /= attributes.BoostSpeedMultiplier;
        }

        #endregion

        #region Dash

        if (Input.GetKey(KeyCode.LeftControl) && dashCooldown <= 0 && stamina >= attributes.DashStaminaCost)
        {
            isDashing = true;
            stamina -= attributes.DashStaminaCost;
            rigidBody.velocity = moveDirection * attributes.DashSpeedMultiplier * attributes.MoveSpeed;
            dashCooldown = attributes.DashCooldown;
        }
        else if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
        }

        #endregion

        #region Stamina Regen

        if (staminaRegenCooldown > 0) staminaRegenCooldown -= Time.deltaTime;
        else if (stamina < attributes.Stamina)
        {
            stamina += attributes.StaminaRegenerationAmount;
            staminaRegenCooldown = attributes.StaminaRegenerationCooldown;
        }
        else if (stamina > attributes.Stamina) stamina = attributes.Stamina;

        #endregion

        #region Grapling Hook

        if (Input.GetKey(KeyCode.Mouse1) && hookCooldown <= 0)
        {
            if (!HookRayCast()) return;
            hookCooldown = 1f;
            isGraplingHook = true;
            stamina -= 5f;
            rigidBody.velocity = (mousePosition - (Vector2)transform.position).normalized * 10f;
            hookDuration = 1f;
        }
        else if (hookCooldown > 0) hookCooldown -= Time.deltaTime;

        #endregion

    }

    private bool HookRayCast()
    {
        RaycastHit2D hit = Physics2D.Raycast(weaponSystem.transform.position, mousePosition);
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<EnemyController>(out var ec))
            {
                if (IsServer) ec.Stunned(1.5f);
                else ec.StunnedRpc(1.5f);
            }
            return true;
        }
        return false;
    }
}
