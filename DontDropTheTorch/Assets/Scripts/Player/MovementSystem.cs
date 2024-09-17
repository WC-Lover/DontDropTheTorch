using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MovementSystem : NetworkBehaviour
{

    private Camera mainCam;
    private Rigidbody2D rigidBody;

    private Vector2 mousePosition;
    private Vector2 moveDirection;

    // Create stamina bar
    [SerializeField] private float stamina;
    private float staminaRegenCooldown;

    private float dashDuration;
    private float dashCooldown;
    private bool isDashing = false;

    private bool isBoosting = false;

    private MovementAttributes attributes;
    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        attributes = playerAttributes.MovementAttributes;

        stamina = attributes.Stamina;
        staminaRegenCooldown = attributes.StaminaRegenerationCooldown;

        dashDuration = attributes.DashDuration;
        dashCooldown = attributes.DashCooldown;

        rigidBody = GetComponent<Rigidbody2D>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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

        if (Input.GetKey(KeyCode.LeftControl) && dashCooldown <= 0)
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

    }
}
