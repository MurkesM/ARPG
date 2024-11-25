using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private AttributeComponent attributeComponent;

    [SerializeField] private float radius = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f; //distance to stop from the player
    public float rotationSpeed = 5f;

    private void Update()
    {
        GameObject closestPlayer = FindClosestPlayer();

        if (closestPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.transform.position);

            // Calculate the direction towards the player
            Vector3 direction = closestPlayer.transform.position - transform.position;
            direction.y = 0; // Keep Y axis unchanged for horizontal rotation

            // Only rotate if the direction is not zero
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Move towards the player only if we're further than the stopping distance
            if (distanceToPlayer > stoppingDistance)
                transform.position = Vector3.MoveTowards(transform.position, closestPlayer.transform.position, moveSpeed * Time.deltaTime);
        }
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