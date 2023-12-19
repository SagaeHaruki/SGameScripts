using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimationSystem : MonoBehaviour
{
    PlayerMovement4 instance;
    [SerializeField] private Animator animator;
    [SerializeField] private string playerStatus;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isJumping;

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
                }

                if (playerStatus == "Walking")
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isSprinting", false);
                }

                if (playerStatus == "Sprinting")
                {
                    animator.SetBool("isSprinting", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                }
            }
            else
            {
                if (!isJumping) 
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isSprinting", false);
                    animator.SetBool("isFalling", false);
                }

            }
        }
        else
        {
            animator.SetBool("isFalling", true);
        }
    }

}
