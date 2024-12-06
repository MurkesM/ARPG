using System;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public bool IsAttacking { get => isAttacking; }
    private bool isAttacking = false;

    public event Action OnPrimaryAttackCalled;
    public event Action OnPrimaryAttackEnd;

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

        OnPrimaryAttackEnd?.Invoke();
    }
}