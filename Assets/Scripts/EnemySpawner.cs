using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] protected Transform enemyContainer;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && IsOwner)
            SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        Enemy enemy = Instantiate(enemyPrefab, enemyContainer.position, Quaternion.identity, enemyContainer);
        enemy.NetworkObject.Spawn();
    }
}