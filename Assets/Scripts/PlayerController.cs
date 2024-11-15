using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 700f;
    [SerializeField] private float moveToThreshhold = 0.1f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isAttacking = false;

    [SerializeField] private Animator playerAnimator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";

    private Coroutine moveRoutine = null;

    public event Action<Vector3> OnMoved;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking)
                return;

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

                    if (!isMoving)
                        moveRoutine = StartCoroutine(MoveToTarget());
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            PrimaryAttack();
        }
    }

    private IEnumerator MoveToTarget()
    {
        isMoving = true;

        playerAnimator.SetBool(moveParam, isMoving);

        while (Vector3.Distance(transform.position, targetPosition) > moveToThreshhold)
        {
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

        transform.position = targetPosition;
        isMoving = false;

        playerAnimator.SetBool(moveParam, isMoving);
    }

    private void PrimaryAttack()
    {
        if (isAttacking)
            return;

        isAttacking = true;

        playerAnimator.SetTrigger(primaryAttackParam);
    }

    public void OnPrimaryAttackAnimEnd()
    {
        isAttacking = false;
    }
}