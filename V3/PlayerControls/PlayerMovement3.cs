using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement3 : MonoBehaviour
{
    #region Interactibles in unity
    [SerializeField] private CharacterController charControl;
    [SerializeField] private CinemachineFreeLook freelookCam;
    [SerializeField] private Transform CameraAngle;
    [SerializeField] private Animator animator;
    #endregion

    #region Physics values
    [SerializeField] private float playerSpeed = 3.2f;
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float Gravity = -9.81f;
    private Vector3 Velocity;
    #endregion

    #region Player Movements Variables
    private float horizontal;
    private float vertical;
    #endregion

    #region Booleans
    private bool isMoving;
    private bool isRunning;
    private bool isSprinting;
    private bool isWalking;
    [SerializeField] private bool isJumping;
    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    private float turnSmoothing = 0.1f;
    private float smoothingVelocity;
    #endregion

    #region Jump Motion
    private float jumpTimer = 0f;
    public float jumpCooldown = 1.4f;
    public float forwardForce = 4.8f;
    public float duration = 1f;
    private float jumpStartTime;
    #endregion

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();


        // Hides & lock the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    private void Update()
    {
        horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        
        MovePlayer();
        GravityPhysics();
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (isJumping)
        {
            // This gives the jumping motion a duration to make it smoother
            float elapsedTime = Time.time - jumpStartTime;
            if (elapsedTime < duration)
            {
                // Calculate the direction for the combined forward and upward motion
                Vector3 moveDirection = transform.forward * forwardForce + Vector3.up * Velocity.y;
                // Execute the motion
                charControl.Move(moveDirection * Time.deltaTime);
            }
        }

        if (direction.magnitude >= 0.1f)
        {
            if (!isJumping)
            {
                /*
                 * This Section will calculate the direction of the player, then smoothens it rotation based on the calulated direction
                 */
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraAngle.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // This moves the player based on its forward direction
                Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                charControl.Move(newDirection.normalized * playerSpeed * Time.deltaTime);
            }
        }

    }

    private void GravityPhysics()
    {
        if (charControl.isGrounded)
        {
            Velocity.y = -1f;

            if (Input.GetButtonDown("Jump"))
            {
                isJumping = true;
                Velocity.y = jumpForce;

                jumpTimer = jumpCooldown;
                jumpStartTime = Time.time;
            }
            else
            {
                isJumping = false;
            }
        }
        else
        {
            Velocity.y -= Gravity * -2f * Time.deltaTime;
        }

        charControl.Move(Velocity * Time.deltaTime);
    }
}
