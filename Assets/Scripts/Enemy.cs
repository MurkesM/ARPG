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
    private PlayerController targetPlayer;
    protected List<PlayerController> targetPlayers = new List<PlayerController>();

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
        MoveAndRotateTowardsTargetPlayer();

        if (inAttackRange)
            TryPrimaryAttack();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        //add players to list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController))
            targetPlayers.Add(playerController);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        //remove players from list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController) && targetPlayers.Contains(playerController))
            targetPlayers.Remove(playerController);
    }

    protected virtual void MoveAndRotateTowardsTargetPlayer()
    {
        if (targetPlayers.Count < 1)
            return;

        //always target the first player to get added to the list, which should be the first player to enter the range of this enemy
        targetPlayer = targetPlayers[0];

        //move towards the player only if we're further than the stopping distance
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, moveSpeed * Time.deltaTime);
            isMoving = true;
            inAttackRange = false;
        }
        else
        {
            isMoving = false;
            inAttackRange = true;
        }
            
        //rotate
        Vector3 direction = targetPlayer.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void TryPrimaryAttack()
    {
        if (combatController.IsAttacking)
            return;

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