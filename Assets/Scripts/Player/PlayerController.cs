using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Move/Rotate Fields")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 700f;
    private Vector3 targetPosition = new();
    private Vector3 targetDirection = new();
    private bool isMoving = false;

    [Header("Attribute Fields")]
    [SerializeField] private AttributeComponent attributeComponent;
    public AttributeComponent AttributeComponent { get => attributeComponent; }

    [Header("Combat Fields")]
    [SerializeField] private CombatController combatController;

    [Header("Animation Fields")]
    [SerializeField] private Animator playerAnimator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";

    [Header("Debug Fields")]
    [SerializeField] private MeshRenderer debugMarkerMeshRenderer;


    protected virtual void Awake()
    {
        combatController.OnPrimaryAttackCalled += OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd += OnPrimaryAttackEnd;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        debugMarkerMeshRenderer.material.color = IsServer ? Color.green : Color.blue;
    }

    private void Update()
    {
        if (isMoving)
            MoveToTarget();

        //ignore input if this client is not the owner of this networkbehavior
        if (IsOwner)
        {
            if (Input.GetMouseButtonDown(1))
                TryMove();

            if (Input.GetMouseButtonDown(0))
                TryPrimaryAttack();
        }
    }

    #region Movement Functions

    private void TryMove()
    {
        if (combatController.IsAttacking)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int floorLayerMask = LayerMask.GetMask("Floor");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
        {
            Vector3 clickedPosition = hit.point;

            //only change the target position if it's different
            if (targetPosition != clickedPosition)
            {
                targetPosition = clickedPosition;
                RequestStartMoveServerRpc(targetPosition);
            }
        }
    }

    [ServerRpc]
    private void RequestStartMoveServerRpc(Vector3 positionToMove)
    {
        NotifyStartMoveClientRpc(positionToMove);
    }

    [ClientRpc]
    private void NotifyStartMoveClientRpc(Vector3 positionToMove)
    {
        StartMove(positionToMove);
    }

    private void StartMove(Vector3 positionToMove)
    {
        isMoving = true;
        playerAnimator.SetBool(moveParam, isMoving);

        targetPosition = positionToMove;
        targetDirection = (targetPosition - transform.position).normalized;
    }

    protected void MoveToTarget()
    {
        if (transform.position == targetPosition)
            StopMove();

        //move
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);

        //rotate
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Vector3 targetRotateAsEulerAngle = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime).eulerAngles;
            transform.eulerAngles = new Vector3(transform.rotation.x, targetRotateAsEulerAngle.y, transform.rotation.z);
        }
    }

    protected void StopMove()
    {
        if (!isMoving)
            return;

        isMoving = false;
        playerAnimator.SetBool(moveParam, isMoving);
    }

    #endregion

    #region Attack Functions

    private void TryPrimaryAttack()
    {
        if (combatController.IsAttacking)
            return;

        RequestPrimaryAttackServerRpc();
    }

    [ServerRpc]
    private void RequestPrimaryAttackServerRpc()
    {
        NotifyPrimaryAttackClientRpc();
    }

    [ClientRpc]
    private void NotifyPrimaryAttackClientRpc()
    {
        combatController.PrimaryAttack();
    }

    private void OnPrimaryAttackCalled()
    {
        StopMove();

        playerAnimator.SetTrigger(primaryAttackParam);
    }

    private void OnPrimaryAttackEnd()
    {
        //keeping here for future
    }

    #endregion

    public override void OnDestroy()
    {
        base.OnDestroy();

        combatController.OnPrimaryAttackCalled -= OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd -= OnPrimaryAttackEnd;
    }
}