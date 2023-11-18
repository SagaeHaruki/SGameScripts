using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    // Serizalize field allows you to change the value on unity

    #region Variables: Character Speed & Stamina
    [SerializeField] 
    private float charSpeed = 3.2f;
    [SerializeField]
    public float maxStamina = 100f;
    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    public float turnSmoothing = 0.1f; // Player direction smoothing
    public Transform CameraAngle; // Default camera angle
    float smoothingVelocity;
    #endregion

    #region Variables: Character Control
    public CharacterController charControl;
    #endregion

    #region Variables: Walking, Running, Sprinting & Jumping
    public int isWalking = 1;
    public bool isSprinting;
    public bool isJumping;
    [SerializeField]
    private bool isRunning = true;
    #endregion

    #region Variables: Stamina Depletion & Regeneration
    public float currentStamina;
    public float staminaRegen = 12f;
    public float staminaDeplete = 15f;
    #endregion

    #region Variables: Gravity Physics
    [SerializeField]
    public float jumpSpeed = 5.0f;
    [SerializeField]
    public float gravity = 20.0f;
    [SerializeField]
    private float verticalSpeed = 0.0f;
    #endregion

    #region Value: Ground Distance & Mask
    [SerializeField]
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    #endregion

    #region Variables: Character Dashing
    [SerializeField]
    public float dashDuration = 0.2f; // Duration of the dash
    [SerializeField]
    public float dashCooldown = 1.2f; // Cooldown between dashes
    [SerializeField]
    public float dashSpeed = 8.2f; // Dash Speed
    private float cooldownTimer = 0.0f;
    private bool isDashing = false;
    #endregion


    private void Start()
    {
        currentStamina = maxStamina;
        // Hides the cursor
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GravityAndJump();
        SprintingStatus();
        RunningToggle();

        /*
         * Keyboard inputs (wasd or arrows)
         */
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get the direction based on the movement, always normalize movement
        Vector3 direction = new Vector3 (horizontal, 0f, vertical).normalized;

        // Direction Movement
        if (direction.magnitude >= 0.1f)
        {
            // Calculate the direction of the player
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraAngle.eulerAngles.y;
            // This will smoothen the rotation of the angle (More math)
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
            // Rotates the player based on the direction
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Direction base on the camera angle
            Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            // This will move the character
            charControl.Move(newDirection.normalized * charSpeed * Time.deltaTime);
        }
    }

    void GravityAndJump()
    {
        if (charControl.isGrounded && verticalSpeed < 0.0f)
        {
            verticalSpeed = -1.0f;
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }

        // Make the Player Jump
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            isJumping = true;
            verticalSpeed = jumpSpeed;
        }

        Vector3 moveDirection = new Vector3(0, verticalSpeed, 0);
        charControl.Move(moveDirection * Time.deltaTime);

        // Reset the jump flag after the character is grounded again
        if (charControl.isGrounded)
        {
            isJumping = false;
        }
    }

    void RunningToggle()
    {
        // Running and Walking key toggle
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isRunning == false)
            {
                isRunning = true;
                return;
            }
            else if (isRunning == true)
            {
                isRunning = false;
                return;
            }

        }
    }

    /*
     * Sprinting & Stamina Physics
     */

    void SprintingStatus()
    {

        // Stamina regeneration over time when not performing actions or if the stamina reaches zero
        if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegen * Time.deltaTime);
        }

        // Simulating a player action that consumes stamina (e.g., sprinting)
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentStamina > 0 && cooldownTimer <= 0.0f)
        {
            isSprinting = true;

            // Checks if the player is on the ground
            if(charControl.isGrounded)
            {
                // Starts the dash if its on the ground
                isDashing = true;
                cooldownTimer = dashCooldown;
                StartCoroutine(Dash());
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || currentStamina <= 0)
        {
            isSprinting = false;
        }

        // This reduces the stamina
        if (isSprinting)
        {
            currentStamina -= staminaDeplete * Time.deltaTime;
        }

        // This increases the character speed
        if (isSprinting == true)
        {
            charSpeed = 6.2f;
        }
        else if (isSprinting == false && isRunning == true)
        {
            // Set the character speed to the running speed after sprinting
            charSpeed = 4.2f;
        }
        else
        {
            // Set the character speed to the walking speed after sprinting
            charSpeed = 3.2f;
        }

        // Cooldown for the dash
        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    private System.Collections.IEnumerator Dash()
    {
        // Starts the dash time upon key press
        float startTime = Time.time;
        Vector3 dashDirection = transform.forward; // Change to your desired direction

        while (Time.time < startTime + dashDuration)
        {
            // Starts the dash movement
            charControl.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }
        isDashing = false;
    }
}
