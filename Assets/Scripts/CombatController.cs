using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public enum AllowedTargets { Player, Enemy, Both}
    [SerializeField] private AllowedTargets allowedTargets = AllowedTargets.Player;

    public enum AttackType { PrimaryAttack }

    //States
    public bool IsAttacking { get => isAttacking; }
    private bool isAttacking = false;

    [SerializeField] private int damageToDeal = 10;

    [Header("Collidor Event Handlers")]
    [SerializeField] private ColliderEventHandler rightHandColliderHandler;

    private List<AttackData> attackDataList = new List<AttackData>();

    public event Action OnPrimaryAttackCalled;
    public event Action OnPrimaryAttackEnd;

    private void Awake()
    {
        rightHandColliderHandler.OnTriggerEntered += TryDamageWithPrimaryAttack;
    }

    public void PrimaryAttack()
    {
        if (isAttacking)
            return;

        isAttacking = true;

        OnPrimaryAttackCalled?.Invoke();
    }

    /// <summary>
    /// This is connected to the anim event for the basic attack animation.
    /// </summary>
    public void OnPrimaryAttackAnimEnd()
    {
        isAttacking = false;

        RemoveOldAttackData(AttackType.PrimaryAttack);

        OnPrimaryAttackEnd?.Invoke();
    }

    private void TryDamageWithPrimaryAttack(Collider collider)
    {
        if (!isAttacking)
            return;

        if (allowedTargets == AllowedTargets.Player)
        {
            if (collider.CompareTag(TagManager.PlayerTag) && collider.TryGetComponent(out PlayerController playerController))
                HandlePlayerHit(playerController);
        }
        else if (allowedTargets == AllowedTargets.Enemy)
        {
            if (collider.CompareTag(TagManager.EnemyTag) && collider.TryGetComponent(out Enemy enemy))
                HandleEnemyHit(enemy);
        }
        else if (allowedTargets == AllowedTargets.Both)
        {
            if (collider.CompareTag(TagManager.PlayerTag) && collider.TryGetComponent(out PlayerController playerController))
                HandlePlayerHit(playerController);

            else if (collider.CompareTag(TagManager.EnemyTag) && collider.TryGetComponent(out Enemy enemy))
                HandleEnemyHit(enemy);
        }
    }

    private void HandlePlayerHit(PlayerController playerController)
    {
        if (AttackDataAlreadyExists(playerController))
            return;

        playerController.AttributeComponent.TryApplyHealthChange(-damageToDeal);
        AttackData attackData = new AttackData(playerController, AttackType.PrimaryAttack);
        attackDataList.Add(attackData);
    }

    private void HandleEnemyHit(Enemy enemy)
    {
        if (AttackDataAlreadyExists(enemy))
            return;

        enemy.AttributeComponent.TryApplyHealthChange(-damageToDeal);
        AttackData attackData = new AttackData(enemy, AttackType.PrimaryAttack);
        attackDataList.Add(attackData);
    }

    /// <summary>
    /// The goal is to make it where the same attack cannot hit the same player more than once for attacks that are not Damage Over Time.
    /// </summary>
    /// <param name="playerController"></param>
    /// <returns></returns>
    private bool AttackDataAlreadyExists(PlayerController playerController)
    {
        AttackData attackData = attackDataList.Find(attackData => attackData.PlayerController == playerController);
        return attackData != null;
    }

    /// <summary>
    /// The goal is to make it where the same attack cannot hit the same player more than once for attacks that are not Damage Over Time.
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private bool AttackDataAlreadyExists(Enemy enemy)
    {
        AttackData attackData = attackDataList.Find(attackData => attackData.Enemy == enemy);
        return attackData != null;
    }

    private void RemoveOldAttackData(AttackType attackType)
    {
        List<AttackData> attackDataToRemoveList = new List<AttackData>();

        foreach (AttackData attackData in attackDataList)
            if (attackData.AttackType == attackType)
                attackDataToRemoveList.Add(attackData);

        foreach (AttackData attackDataToRemove in attackDataToRemoveList)
            if (attackDataList.Contains(attackDataToRemove))
                attackDataList.Remove(attackDataToRemove);
    }
    
    public void OnDestroy()
    {
        rightHandColliderHandler.OnTriggerEntered -= TryDamageWithPrimaryAttack;
    }
}