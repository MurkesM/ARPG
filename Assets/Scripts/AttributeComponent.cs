using Unity.Netcode;
using UnityEngine;

public class AttributeComponent : NetworkBehaviour
{
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int maxHealth = 100;

    public void TryApplyHealthChange(int delta)
    {
        if (IsServer)
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