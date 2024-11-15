using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Move/Rotate Fields")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveToThreshhold = 0.1f;
    [SerializeField] private float rotationSpeed = 700f;
    private Vector3 targetPosition = new();
    private Coroutine moveRoutine = null;
    private bool isMoving = false;
    private bool isAttacking = false;
    public event Action<Vector3> OnMoved;

    [Header("Attribute Fields")]
    [SerializeField] private AttributeComponent attributeComponent;
    [SerializeField] private int damageToDeal = 10;
    private const string damageableTag = "Damageable";

    [Header("Animation Fields")]
    [SerializeField] private Animator playerAnimator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";

    private void Update()
    {
        //ignore input if this client is not the owner of this networkbehavior
        if (!IsOwner)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking)
                return;

            TryMove();
        }

        if (Input.GetMouseButtonDown(0))
            PrimaryAttack();
    }

    private void OnTriggerEnter(Collider other)
    {
        //will need to change this later as the hit interaction here isn't a great way to implement this, has bugs, and is temporary
        if (!other.CompareTag(damageableTag) || !other.TryGetComponent(out AttributeComponent attributeComponent) || !isAttacking)
            return;

        attributeComponent.ApplyHealthChange(-damageToDeal);
    }

    #region Movement Functions

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
                    Move();

                else if (IsClient)
                    MoveServerRpc(targetPosition);
            }
        }
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 positionToMove)
    {
        targetPosition = positionToMove;
        Move();
    }

    private void Move()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveToTargetRoutine());
    }

    private IEnumerator MoveToTargetRoutine()
    {
        isMoving = true;

        playerAnimator.SetBool(moveParam, isMoving);

        while (Vector3.Distance(transform.position, targetPosition) > moveToThreshhold)
        {
            //move
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);

            OnMoved?.Invoke(transform.position);

            //rotate
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

    private void StopMoveEarly()
    {
        if (!isMoving)
            return;

        isMoving = false;
        playerAnimator.SetBool(moveParam, isMoving);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
    }

    #endregion

    #region Attack Functions

    private void PrimaryAttack()
    {
        if (isAttacking)
            return;

        StopMoveEarly();

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

    #endregion
}