using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private AttributeComponent attributeComponent;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotationSpeed = 5f;

    protected const string PlayerTag = "Player";
    protected List<PlayerController> targetPlayers = new List<PlayerController>();

    protected virtual void OnTriggerEnter(Collider other)
    {
        //add players to list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController))
            targetPlayers.Add(playerController);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(PlayerTag))
            MoveAndRotateTowardsTargetPlayer();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        //remove players from list
        if (other.CompareTag(PlayerTag) && other.TryGetComponent(out PlayerController playerController) && targetPlayers.Contains(playerController))
            targetPlayers.Remove(playerController);
    }

    protected virtual void MoveAndRotateTowardsTargetPlayer()
    {
        if (targetPlayers.Count < 1)
            return;

        //always target the first player to get added to the list, which should be the first player to enter the range of this enemy
        PlayerController targetPlayer = targetPlayers[0];

        //rotate
        Vector3 direction = targetPlayer.transform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        //move towards the player only if we're further than the stopping distance
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer > stoppingDistance)
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.transform.position, moveSpeed * Time.deltaTime);
    }
}