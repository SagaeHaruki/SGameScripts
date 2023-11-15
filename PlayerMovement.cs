using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Player Movement
    Rigidbody rigBody;
    // Acces the player Controller
    private CharacterController CController;

    [SerializeField]
    private float moveSpeed = 2.7f;

    [SerializeField]
    private float jumpHeight = 0.25f;

    [SerializeField]
    private float rotationSpeed = 2.5f;

    [SerializeField]
    private float gravityStr = -10.81f;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private Transform mainPlayerCam;

    private bool atGround;
    private Vector3 playerVelocity;
    private Vector3 dashDirection;
    private Quaternion currentRot;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rigBody = GetComponent<Rigidbody>();
        CController = GetComponent<CharacterController>();

        CController.skinWidth = 0.0001f;
        CController.minMoveDistance = 0f;
        CController.radius = 0.5f;
        playerCamera = Camera.main;
        mainPlayerCam = Camera.main.transform;
    }

    void Update()
    {
        MovementPlayer();
    }

    private void MovementPlayer()
    {
        dashDirection = transform.forward;
        atGround = CController.isGrounded;

        if (atGround && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Keyboard input
        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        // Get the Camera Direction then translate it to where the front of player is
        Vector3 movementInput = mainPlayerCam.forward * verticalAxis + mainPlayerCam.right * horizontalAxis;
        Vector3 movementDirection = movementInput.normalized;
        movementDirection.y = 0f;

        // This moves the player
        CController.Move(movementDirection.normalized * moveSpeed * Time.deltaTime);

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }


        // Disable the jump if the player is on mid air
        if (Input.GetButtonDown("Jump") && atGround)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -5.0f * gravityStr);
            atGround = false;
        }
        // Gravity
        playerVelocity.y += gravityStr * Time.deltaTime;
        CController.Move(playerVelocity * Time.deltaTime);
    }


    // This will enable the player to jump again
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            atGround = true;
        }
    }
}
