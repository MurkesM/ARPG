using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private AttributeComponent attributeComponent;

    [SerializeField] private float radius = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;
    public float rotationSpeed = 5f;
    public float cooldownTime = 1f;

    private GameObject closestPlayer;
    private Vector3 lastKnownPlayerPosition;
    private float lastUpdateTime;

    private void Update()
    {
        // Update the closest player only if the cooldown period has elapsed
        if (Time.time >= lastUpdateTime + cooldownTime)
        {
            closestPlayer = FindClosestPlayer();
            lastUpdateTime = Time.time; // Reset the cooldown timer

            // Update last known position if a player is found
            if (closestPlayer != null)
            {
                lastKnownPlayerPosition = closestPlayer.transform.position;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, lastKnownPlayerPosition);

        // Calculate the direction towards the player
        Vector3 direction = lastKnownPlayerPosition - transform.position;
        direction.y = 0; // Keep Y axis unchanged for horizontal rotation

        // Only rotate if the direction is not zero
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move towards the player only if we're further than the stopping distance
        if (distanceToPlayer > stoppingDistance)
            transform.position = Vector3.MoveTowards(transform.position, lastKnownPlayerPosition, moveSpeed * Time.deltaTime);
    }

    private GameObject FindClosestPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject.layer == 7) //player layer
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = collider.gameObject;
                }
            }
        }
        return closestPlayer;
    }
}