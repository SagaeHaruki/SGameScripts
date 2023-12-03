using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterIK : MonoBehaviour
{
    protected Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AdjustFootIK(AvatarIKGoal.LeftFoot, leftFootTarget);
        AdjustFootIK(AvatarIKGoal.RightFoot, rightFootTarget);
    }
    public Transform leftFootTarget;
    public Transform rightFootTarget;
    public LayerMask groundLayer;
    public float maxRaycastDistance = 1.0f;
    public float footOffset = 0.1f;


    void AdjustFootIK(AvatarIKGoal goal, Transform target)
    {
        RaycastHit hit;
        if (Physics.Raycast(target.position, -Vector3.up, out hit, maxRaycastDistance, groundLayer))
        {
            Vector3 surfaceNormal = hit.normal;
            Vector3 footPosition = hit.point + surfaceNormal * footOffset;
            Quaternion footRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal) * Quaternion.LookRotation(Vector3.forward, surfaceNormal);

            animator.SetIKPositionWeight(goal, 1f);
            animator.SetIKRotationWeight(goal, 1f);
            animator.SetIKPosition(goal, footPosition);
            animator.SetIKRotation(goal, footRotation);
        }
    }
}
