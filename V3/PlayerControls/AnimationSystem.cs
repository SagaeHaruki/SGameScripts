using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Playables;

public class AnimationSystem : MonoBehaviour
{
    PlayerMovement4 instance;
    [SerializeField] private Animator animator;
    [SerializeField] private string playerStatus;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isSprinting;

    [SerializeField] private bool currentlyMoving;

    private void Start()
    {
        instance = GetComponent<PlayerMovement4>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        playerStatus = instance.playerState;
        isMoving = instance.isMoving;
        isJumping = instance.isJumping;
        isRunning = instance.isRunning;
        isWalking = instance.isWalking;
        isSprinting = instance.isSprinting;
        currentlyMoving = instance.currentlyMoving;

        if (playerStatus != "Falling")
        {
            animator.SetBool("isFalling", false);
            if (isMoving)
            {
                if (playerStatus == "Running")
                {
                    animator.SetBool("isRunning", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isSprinting", false);
                    animator.SetBool("isJumping", false);

                }

                if (playerStatus == "Walking")
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isSprinting", false);
                    animator.SetBool("isJumping", false);
                }

                if (playerStatus == "Sprinting")
                {
                    animator.SetBool("isSprinting", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isJumping", false);
                }
            }
            else
            {
                if (!isJumping) 
                {
                    animator.SetBool("isJumping", false);
                }

                if (currentlyMoving)
                {
                    if (isJumping && isWalking && !isSprinting)
                    {
                        animator.SetBool("isJumping", true);
                    }

                    if (isJumping && isRunning && !isSprinting)
                    {
                        animator.SetBool("isJumping", true);
                    }

                    if (isJumping && isSprinting)
                    {
                        animator.SetBool("isJumping", true);
                    }
                }
                else
                {
                    if (playerStatus == "Jumped")
                    {
                        animator.SetBool("isJumping", true);
                    }

                    if(playerStatus == "Idle")
                    {
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isSprinting", false);
                        animator.SetBool("isJumping", false);
                    }
                }
            }
        }
        else
        {
            animator.SetBool("isFalling", true);
        }
    }

}
