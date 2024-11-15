using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 700f;
    [SerializeField] private float moveToThreshhold = 0.1f;
    [SerializeField] private int damageToDeal = 10;

    private Vector3 targetPosition = new();
    private bool isMoving = false;
    private bool isAttacking = false;

    [SerializeField] private Animator playerAnimator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";

    private const string damageableTag = "Damageable";

    private Coroutine moveRoutine = null;

    [SerializeField] private AttributeComponent attributeComponent;

    public event Action<Vector3> OnMoved;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking)
                return;

            TryMove();
        }

        if (Input.GetMouseButtonDown(0))
        {
            PrimaryAttack();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(damageableTag) || !other.TryGetComponent(out AttributeComponent attributeComponent) || !isAttacking)
            return;

        attributeComponent.ApplyHealthChange(-damageToDeal);
    }

    private IEnumerator MoveToTarget()
    {
        isMoving = true;

        playerAnimator.SetBool(moveParam, isMoving);

        while (Vector3.Distance(transform.position, targetPosition) > moveToThreshhold)
        {
            //stop move early
            if (isAttacking)
            {
                isMoving = false;
                playerAnimator.SetBool(moveParam, isMoving);
                StopCoroutine(moveRoutine);
            }

            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.position = new Vector3 (newPosition.x, transform.position.y, newPosition.z);

            OnMoved?.Invoke(transform.position);

            Vector3 direction = (targetPosition - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Vector3 targetRotateAsEulerAngle = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime).eulerAngles;
                transform.eulerAngles = new Vector3(transform.rotation.x, targetRotateAsEulerAngle.y, transform.rotation.z);
            }

            yield return null;
        }

        isMoving = false;
        playerAnimator.SetBool(moveParam, isMoving);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 positionToMove)
    {
        targetPosition = positionToMove;
        Move();
    }

    private void TryMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int floorLayerMask = LayerMask.GetMask("Floor");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
        {
            Vector3 clickedPosition = hit.point;

            // Only change the target position if it's different
            if (targetPosition != clickedPosition)
            {
                targetPosition = clickedPosition;

                if (IsServer)
                {
                    Move();
                }
                else if (IsClient)
                {
                    MoveServerRpc(targetPosition);
                }
            }
        }
    }

    private void Move()
    {
        //do we need to be checking if is moving?
        if (isMoving && moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveToTarget());
    }

    private void PrimaryAttack()
    {
        if (!IsOwner || isAttacking)
            return;

        isAttacking = true;

        playerAnimator.SetTrigger(primaryAttackParam);
    }

    /// <summary>
    /// This is connected to the anim event for the basic attack animation.
    /// </summary>
    public void OnPrimaryAttackAnimEnd()
    {
        isAttacking = false;
    }
}