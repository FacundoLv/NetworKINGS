using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstacleFactory : MonoBehaviour
{
    private ObjectPool<Obstacle> obstaclePool;

    private void Awake()
    {
        if (!GameServer.Instance.IsServer)
        {
            Destroy(this);
            return;
        }

        obstaclePool = new ObjectPool<Obstacle>(NewObstacle, Obstacle.TurnOn, Obstacle.TurnOff, 5);
    }

    public void SpawnObstaclesWithinBounds(Bounds bounds)
    {
        Vector3 spawnPosition = GetValidSpawnPosition(bounds);

        obstaclePool
            .GetObject()
            .SetPosition(spawnPosition)
            .transform.parent = transform;
    }

    public void SpawnObstaclesWithinBounds(Bounds bounds, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(bounds);

            obstaclePool
                .GetObject()
                .SetPosition(spawnPosition)
                .transform.parent = transform;
        }
    }

    private static Vector3 GetValidSpawnPosition(Bounds bounds)
    {
        Vector3 position = Vector3.zero;
        float radius = 1f;
        bool isValid = true;

        do
        {
            float offsetX = Random.Range(-bounds.extents.x + radius, bounds.extents.x - radius);
            float offsetY = Random.Range(-bounds.extents.y + radius, bounds.extents.y - radius);
            float offsetZ = Random.Range(-bounds.extents.z + radius, bounds.extents.z - radius);

            position = bounds.center + new Vector3(offsetX, offsetY, offsetZ);

            var colliders = Physics.OverlapSphere(position, radius, LayerMask.NameToLayer("Actor"));
            isValid = colliders.Length == 0;
        } while (!isValid);

        return position;
    }

    private Obstacle NewObstacle()
    {
        var obs = PhotonNetwork
            .Instantiate("Obstacles/Obstacle", Vector3.zero, Quaternion.identity)
            .GetComponent<Obstacle>();

        obs.OnObstacleDown += ReturnObstacle;

        return obs;
    }

    private void ReturnObstacle(Obstacle obs)
    {
        obstaclePool.ReturnObject(obs);
    }
}