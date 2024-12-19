using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Intended to be the base class of all player and enemy related behaviors.
/// </summary>
public class CharacterBehavior : NetworkBehaviour
{
    public AttributeComponent AttributeComponent { get => attributeComponent; }
    [SerializeField] protected AttributeComponent attributeComponent;

    public static event Action<CharacterBehavior> OnNetworkSpawned;
    public static event Action<CharacterBehavior> OnDestroyed;

    protected virtual void Awake() { }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        OnNetworkSpawned?.Invoke(this);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        OnDestroyed?.Invoke(this);
    }
}