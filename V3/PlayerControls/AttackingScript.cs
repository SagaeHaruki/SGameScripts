using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingScript : MonoBehaviour
{
    PlayerMovement4 pmInstance;
    [SerializeField] public bool isAttacking;
    [SerializeField] private bool isFalling;
    [SerializeField] private bool isJumping;

    [SerializeField] public string attackType;

    [SerializeField] private float nextAttack = 0f;
    [SerializeField] private float lastAttack = 0f;
    [SerializeField] private int comboCount = 0;
    [SerializeField] private float attackCd = 2f;
   

    private void Start()
    {
        pmInstance = GetComponent<PlayerMovement4>();
    }

    private void Update()
    {
        isFalling = pmInstance.isFalling;
        isJumping = pmInstance.isJumping;

        if (!isJumping || !isFalling)
        {
            GetAttackInpt();
            ChangeAttack();
        }
    }

    private void GetAttackInpt()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > nextAttack)
        {
            if (Time.time - lastAttack > attackCd)
            {
                comboCount = 0;
            }

            lastAttack = Time.time;
            nextAttack = Time.time + attackCd;

            comboCount++;

            if (comboCount > 3)
            {
                comboCount = 1;
            }
        }
    }

    private void ChangeAttack()
    {
        if (comboCount == 0)
        {
            attackType = "";
        }

        if(comboCount == 1)
        {
            attackType = "FirstAttack";
        }

        if (comboCount == 2)
        {
            attackType = "SecondtAttack";
        }

        if (comboCount == 3)
        {
            attackType = "ThirdAttack";
        }
    }
}
