using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class IKSystem : MonoBehaviour
{
    [SerializeField] public CharacterController charControl;
    private Animator animator;
    private Vector3 leftFootPos, leftFootIkPos;
    private Vector3 rightFootPos, rightFootIkPos;
    private Quaternion leftFootIkRot, rightFootIkRot;
    private float lastPelvisPosY, lastLeftFotPosY, lastRightFotPosY;
    private bool isJumping;
    private bool isMoving;
    private bool isFalling;
    private bool startIK;
    private bool enableFeetIk = true;
    [SerializeField] private bool useProIK = false;
    private bool showDebugs = true;
    [SerializeField] private float heightFromGround = 0.45f;
    [SerializeField] private float raycastDownDistance = 0.45f;
    [SerializeField] private LayerMask LayerMask;
    [SerializeField] private float pelvisOffset = 0f;
    [SerializeField] private float pelvisUpDownSpeed = 0.8f;
    [SerializeField] private float feetIkPosSpeed = 0.12f;
    [Range(0, 2)][SerializeField] public float pelvisLowerOffset = 0.05f;
    public string leftFootAnim = "LeftFootCurve";
    public string rightFootAnim = "RightFootCurve";

    public float maxRayDistance = 1.0f;
    float slopeAngle;

    private void Start()
    {
        animator = GetComponent<Animator>();
        charControl = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ChangeState();
        GetSlopeAngle();
        float horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            if (!isJumping)
            {
                isMoving = true;
            }
        }
        else
        {
            isMoving = false;
        }

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            isJumping = true;
        }

        if (charControl.isGrounded)
        {
            isJumping = false;
        }
    }

    private void GetSlopeAngle()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxRayDistance, LayerMask))
        {
            Vector3 groundNormal = hit.normal;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

        }
    }


    private void ChangeState()
    {
        if (isMoving)
        {
            if (slopeAngle >= 40 && slopeAngle <= 45)
            {
                heightFromGround = 0.67f;
                raycastDownDistance = 0.67f;
            }
            else if (slopeAngle >= 35 && slopeAngle <= 40)
            {
                heightFromGround = 0.62f;
                raycastDownDistance = 0.62f;
            }
            else if (slopeAngle >= 30 && slopeAngle <= 35)
            {
                heightFromGround = 0.57f;
                raycastDownDistance = 0.57f;
            }
            else if (slopeAngle >= 25 && slopeAngle <= 30)
            {
                heightFromGround = 0.52f;
                raycastDownDistance = 0.52f;
            }
            else if (slopeAngle >= 20 && slopeAngle <= 25)
            {
                heightFromGround = 0.47f;
                raycastDownDistance = 0.47f;
            }
            else if(isJumping || !charControl.isGrounded)
            {
                heightFromGround = 0.0f;
                raycastDownDistance = 0.0f;
            }
            else
            {
                heightFromGround = 0.45f;
                raycastDownDistance = 0.45f;
            }
        }
    }

    #region Ground
    private void FixedUpdate()
    {
        if (!isJumping)
        {
            if (enableFeetIk == false)
            {
                return;
            }

            if (animator == null)
            {
                return;
            }

            AdjustTargetFeet(ref rightFootPos, HumanBodyBones.RightFoot);
            AdjustTargetFeet(ref leftFootPos, HumanBodyBones.LeftFoot);

            FeetPositionSolver(rightFootPos, ref rightFootIkPos, ref rightFootIkRot);
            FeetPositionSolver(leftFootPos, ref leftFootIkPos, ref leftFootIkRot);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!isJumping)
        {
            if (enableFeetIk == false)
            {
                return;
            }
            if (animator == null)
            {
                return;
            }
            MovePelvisHeight();

            // Right Foot ik Position & Rotation
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            if (useProIK)
            {
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootAnim));
            }
            MoveToIKPoint(AvatarIKGoal.RightFoot, rightFootIkPos, rightFootIkRot, ref lastRightFotPosY);

            // Left Foot ik Position & Rotation
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            if (useProIK)
            {
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootAnim));
            }
            MoveToIKPoint(AvatarIKGoal.LeftFoot, leftFootIkPos, leftFootIkRot, ref lastLeftFotPosY);
        }
    }
    #endregion

    #region FootGround

    void MoveToIKPoint(AvatarIKGoal foot, Vector3 posIKHolder, Quaternion rotIKHolder, ref float lastFootPosY)
    {
        if (!isJumping)
        {
            Vector3 targetIkPos = animator.GetIKPosition(foot);
            if (posIKHolder != Vector3.zero)
            {
                targetIkPos = transform.InverseTransformPoint(targetIkPos);
                posIKHolder = transform.InverseTransformPoint(posIKHolder);
                float yVar = Mathf.Lerp(lastFootPosY, posIKHolder.y, feetIkPosSpeed);
                targetIkPos.y += yVar;
                lastFootPosY = yVar;
                targetIkPos = transform.TransformPoint(targetIkPos);
                animator.SetIKRotation(foot, rotIKHolder);
            }
            animator.SetIKPosition(foot, targetIkPos);
        }
    }

    private void MovePelvisHeight()
    {
        if (!isJumping)
        {
            if (rightFootIkPos == Vector3.zero || leftFootIkPos == Vector3.zero || lastPelvisPosY == 0)
            {
                lastPelvisPosY = animator.bodyPosition.y;
                return;
            }
            float leftOffsetPos = leftFootIkPos.y - transform.position.y;
            float rightOffsetPos = rightFootIkPos.y - transform.position.y;
            float totalOffsetVal = (leftOffsetPos < rightOffsetPos) ? leftOffsetPos : rightOffsetPos;

            if (!isMoving)
            {
                Vector3 newPelvisPos = animator.bodyPosition + Vector3.up * totalOffsetVal;
                newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisUpDownSpeed);
                animator.bodyPosition = newPelvisPos;
                lastPelvisPosY = animator.bodyPosition.y;
            }
            else
            {
                Vector3 upDirection = Vector3.up;
                RaycastHit hit;

                if (Physics.Raycast(transform.position, -Vector3.up, out hit))
                {
                    upDirection = hit.normal; // Get the surface normal where the character is standing
                }

                float slopeAngle = Vector3.Angle(Vector3.up, upDirection);
                float pelvisOffset = Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * totalOffsetVal - pelvisLowerOffset;

                Vector3 newPelvisPos = animator.bodyPosition + Vector3.up * pelvisOffset;
                newPelvisPos.y = Mathf.Lerp(lastPelvisPosY, newPelvisPos.y, pelvisUpDownSpeed);
                animator.bodyPosition = newPelvisPos;
                lastPelvisPosY = animator.bodyPosition.y;
            }
        }
    }

    private void FeetPositionSolver(Vector3 fromSkyPos, ref Vector3 ikFeetPos, ref Quaternion feetIkRot)
    {
        if (!isJumping)
        {
            // Raycasting Section
            RaycastHit footHit;
            if (showDebugs)
            {
                Debug.DrawLine(fromSkyPos, fromSkyPos + Vector3.down * (raycastDownDistance + heightFromGround), Color.yellow);
            }

            if (Physics.Raycast(fromSkyPos, Vector3.down, out footHit, raycastDownDistance + heightFromGround, LayerMask))
            {
                ikFeetPos = fromSkyPos;
                ikFeetPos.y = footHit.point.y + pelvisOffset;
                feetIkRot = Quaternion.FromToRotation(Vector3.up, footHit.normal) * transform.rotation;
                return;
            }
            ikFeetPos = Vector3.zero;
        }
    }

    private void AdjustTargetFeet(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        if (!isJumping)
        { 
            feetPositions = animator.GetBoneTransform(foot).position;
            feetPositions.y = transform.position.y + heightFromGround;
        }
    }
    #endregion
}
