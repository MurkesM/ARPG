using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Enemy : CharacterBehavior
{
    [Header("General Fields")]
    [SerializeField] private ColliderEventHandler proximityColliderEventListener;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float timeToDestroyOnDeath = 5;
    private bool isMoving = false;

    [Header("Combat Fields")]
    [SerializeField] private CombatController combatController;
    [SerializeField] private float lineOfSightConeAngle = 20f;
    [SerializeField] private float lineOfSightMaxDistance = 30f;
    private bool inAttackRange = false;
    private bool inLineOfSite = false;

    //player related fields
    protected PlayerController targetPlayer;
    protected List<PlayerController> targetPlayers = new List<PlayerController>();
    protected float distanceToTarget = 0;
    protected Vector3 directionOfTarget = Vector3.zero;

    [Header("Animation Fields")]
    [SerializeField] private Animator animator;
    private const string moveParam = "IsMoving";
    private const string primaryAttackParam = "IsPrimaryAttacking";
    private const string isAliveParam = "IsAlive";

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        attributeComponent.OnKilled += OnKilled;
        combatController.OnPrimaryAttackCalled += OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd += OnPrimaryAttackEnd;
        proximityColliderEventListener.OnTriggerEntered += OnProximityZoneEnter;
        proximityColliderEventListener.OnTriggerExited += OnProximityZoneExit;
    }

    private void Update()
    {
        if (!attributeComponent.IsAlive)
            return;

        UpdateTargetPlayerData();
        CheckIfInAttackRange();
        MoveTowardsTargetPlayer();
        CheckLineOfSite();
        RotateTowardsTargetPlayer();
        TryPrimaryAttack();
    }

    private void OnProximityZoneEnter(Collider other)
    {
        //add players to list
        if (other.CompareTag(TagManager.PlayerTag) && other.TryGetComponent(out PlayerController playerController))
            if (!targetPlayers.Contains(playerController))
                targetPlayers.Add(playerController);
    }

    private void OnProximityZoneExit(Collider other)
    {
        //remove players from list
        if (other.CompareTag(TagManager.PlayerTag) && other.TryGetComponent(out PlayerController playerController))
        {
            print("test");

            if (targetPlayers.Contains(playerController))
                targetPlayers.Remove(playerController);

            if (targetPlayers.Count < 1)
            {
                targetPlayer = null;
                inLineOfSite = false;

                if (isMoving)
                    StopMove();
            }
        }
    }

    protected virtual void MoveTowardsTargetPlayer()
    {
        if (inAttackRange || combatController.IsAttacking || targetPlayers.Count < 1)
            return;

        //make sure to start the animation for moving if we just started moving after not moving
        if (!isMoving)
            animator.SetBool(moveParam, true);

        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, moveSpeed * Time.deltaTime);
        isMoving = true;
    }

    protected virtual void RotateTowardsTargetPlayer()
    {
        if (inLineOfSite || combatController.IsAttacking || targetPlayers.Count < 1)
            return;

        if (directionOfTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionOfTarget);
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

    private void CheckLineOfSite()
    {
        if (!targetPlayer)
            return;

        float angle = Vector3.Angle(transform.forward, directionOfTarget);

        if (angle <= lineOfSightConeAngle / 2 && directionOfTarget.magnitude <= lineOfSightMaxDistance)
            if (Physics.Raycast(transform.position, directionOfTarget.normalized, out RaycastHit hit, lineOfSightMaxDistance) && hit.transform == targetPlayer.transform)
            {
                inLineOfSite = true;
                return;
            }
                
         inLineOfSite = false;
    }

    private void UpdateTargetPlayerData()
    {
        if (targetPlayers.Count < 1)
            return;

        //always target the first player to get added to the list, which should be the first player to enter the range of this enemy
        targetPlayer = targetPlayers[0];

        //update distance
        distanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);

        //update direction
        directionOfTarget = targetPlayer.transform.position - transform.position;
        directionOfTarget.y = 0;
    }

    private void CheckIfInAttackRange()
    {
        if (targetPlayer)
            inAttackRange = distanceToTarget <= stoppingDistance;
        else 
            inAttackRange = false;
    }

    private void TryPrimaryAttack()
    {
        if (!inAttackRange || !inLineOfSite || combatController.IsAttacking)
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

    private void OnKilled()
    {
        //probably want to cache these at some point but for now things might change so much as we are making the app that can simplify the code for sake of prototyping
        Collider[] colliders = GetComponentsInChildren<Collider>().ToArray();

        foreach (Collider collider in colliders)
            collider.enabled = false;

        animator.SetBool(isAliveParam, false);

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(timeToDestroyOnDeath);
        Destroy(gameObject);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        attributeComponent.OnKilled -= OnKilled;
        combatController.OnPrimaryAttackCalled -= OnPrimaryAttackCalled;
        combatController.OnPrimaryAttackEnd -= OnPrimaryAttackEnd;
        proximityColliderEventListener.OnTriggerEntered -= OnProximityZoneEnter;
        proximityColliderEventListener.OnTriggerExited -= OnProximityZoneExit;
    }
}