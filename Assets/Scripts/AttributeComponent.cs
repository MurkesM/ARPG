using System;
using Unity.Netcode;
using UnityEngine;

public class AttributeComponent : NetworkBehaviour
{
    public bool IsAlive { get => isAlive; }
    private bool isAlive = true;

    public int CurrentHealth { get => currentHealth; }
    [SerializeField] private int currentHealth = 100;

    public int MaxHealth { get => maxHealth; }
    [SerializeField] private int maxHealth = 100;

    public event Action<int> OnHealthChanged;
    public event Action OnKilled;

    /// <summary>
    /// Pass in a positive number if trying to add health and a negative number if trying to take away health.
    /// </summary>
    /// <param name="delta"></param>
    public void TryApplyHealthChange(int delta)
    {
        if (!isAlive)
            return;

        if (IsServer)
            ApplyHealthChangeServerRpc(delta);
    }

    [ServerRpc]
    private void ApplyHealthChangeServerRpc(int delta)
    {
        ApplyHealthChangeClientRpc(delta);
    }

    [ClientRpc]
    private void ApplyHealthChangeClientRpc(int delta)
    {
        ApplyHealthChange(delta);
    }

    private void ApplyHealthChange(int delta)
    {
        int healthBeforeChange = currentHealth;
        currentHealth += delta;

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
            Kill();
    }

    private void Kill()
    {
        isAlive = false;
        OnKilled?.Invoke();
    }
}