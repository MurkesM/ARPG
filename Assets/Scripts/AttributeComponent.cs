using Unity.Netcode;
using UnityEngine;

public class AttributeComponent : NetworkBehaviour
{
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    /// <summary>
    /// Pass in a positive number if trying to add health and a negative number if trying to take away health.
    /// </summary>
    /// <param name="delta"></param>
    public void TryApplyHealthChange(int delta)
    {
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
    }
}