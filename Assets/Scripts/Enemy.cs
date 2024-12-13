using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private AttributeComponent attributeComponent;
    [SerializeField] private CombatController combatController;

    private bool inAttackRange = false;
    private bool isMoving = false;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    protected const string PlayerTag = "Player";
    protected PlayerController targetPlayer;
    protected List<PlayerController> targetPlayers = new List<PlayerController>();
    protected float distanceToPlayer = 0;

    [Header("Animation Fields")]
    [SerializeField] private Animator animator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        combatController.OnPrimaryAttackCalled += OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd += OnPrimaryAttackEnd;
    }

    private void Update()
    {
        UpdateDistanceToPlayer();
        CheckIfInAttackRange();
        MoveAndRotateTowardsTargetPlayer();

        if (inAttackRange)
            TryPrimaryAttack();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //add players to list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController))
            if (!targetPlayers.Contains(playerController))
                targetPlayers.Add(playerController);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        //remove players from list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController) && targetPlayers.Contains(playerController))
        {
            if (targetPlayers.Contains(playerController))
                targetPlayers.Remove(playerController);

            if (isMoving)
                StopMove();
        }
    }

    protected virtual void MoveAndRotateTowardsTargetPlayer()
    {
        if (inAttackRange || combatController.IsAttacking || targetPlayers.Count < 1)
            return;

        //make sure to start the animation for moving if we just started moving after not moving
        if (!isMoving)
            animator.SetBool(moveParam, true);

        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, moveSpeed * Time.deltaTime);
        isMoving = true;

        //rotate
        Vector3 direction = targetPlayer.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    protected virtual void StopMove()
    {
        if (!isMoving)
            return;

        isMoving = false;
        animator.SetBool(moveParam, isMoving);
    }

    private void UpdateDistanceToPlayer()
    {
        if (targetPlayers.Count < 1)
            return;

        //always target the first player to get added to the list, which should be the first player to enter the range of this enemy
        targetPlayer = targetPlayers[0];

        distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
    }

    private void CheckIfInAttackRange()
    {
        if (targetPlayer)
            inAttackRange = distanceToPlayer <= stoppingDistance;
        else 
            inAttackRange = false;
    }

    private void TryPrimaryAttack()
    {
        if (combatController.IsAttacking)
            return;

        StopMove();

        combatController.PrimaryAttack();
    }

    private void OnPrimaryAttackCalled()
    {
        animator.SetTrigger(primaryAttackParam);
    }

    private void OnPrimaryAttackEnd()
    {
        //keeping here for future
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        combatController.OnPrimaryAttackCalled -= OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd -= OnPrimaryAttackEnd;
    }
}