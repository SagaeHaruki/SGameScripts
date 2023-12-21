using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingScript : MonoBehaviour
{
    PlayerMovement4 pmInstance;
    [SerializeField] public bool isAttacking;
    [SerializeField] private bool isFalling;
    [SerializeField] private bool isJumping;
    [SerializeField] public int attackType;


    private float attackTimer = 0f;
    private float attackDuration = 2.2f;

    private void Start()
    {
        pmInstance = GetComponent<PlayerMovement4>();
    }

    private void Update()
    {
        pmInstance.isFalling = isFalling;
        pmInstance.isJumping = isJumping;

        GetAttackInpt();
    }

    private void GetAttackInpt()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAttacking = true;
            attackType++;
            print("Clicked!");
        }

        if (attackType == 3)
        {
            attackType = 0;
        }

        if (isAttacking)
        {
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                attackType = 0;
                attackTimer = 0f;
            }
        }
    }
}
