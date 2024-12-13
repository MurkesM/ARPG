using System;
using UnityEngine;

public class ColliderEventHandler : MonoBehaviour
{
    public event Action<Collision> OnCollisionEntered;
    public event Action<Collision> OnCollisionExited;
    public event Action<Collider> OnTriggerEntered;
    public event Action<Collider> OnTriggerExited;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntered?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExited?.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExited?.Invoke(other);
    }
}