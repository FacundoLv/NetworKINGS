using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public Transform this[int i]
    {
        get { return spawnPoints[i]; }
    }

    public Transform[] spawnPoints;

    private void Awake()
    {
        if (GameServer.Instance.IsServer) return;

        Destroy(gameObject);
    }
}
