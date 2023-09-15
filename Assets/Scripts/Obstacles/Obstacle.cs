using Photon.Pun;
using System;
using UnityEngine;

public class Obstacle : MonoBehaviourPun
{
    public event Action<Obstacle> OnObstacleDown;

    [SerializeField]
    private float damageAmount = 10f;

    public static void TurnOn(Obstacle obs)
    {
        obs.photonView.RPC(nameof(RPC_ActivateObstacle), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ActivateObstacle()
    {
        gameObject.SetActive(true);
    }

    public static void TurnOff(Obstacle obs)
    {
        obs.photonView.RPC(nameof(RPC_DeactivateObstacle), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DeactivateObstacle()
    {
        gameObject.SetActive(false);
    }

    public Obstacle SetPosition(Vector3 position)
    {
        transform.position = position;
        return this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.TryGetComponent(out PlayerModel player))
        {
            // Maybe RPC here
            player.GetDamage(damageAmount);
            OnObstacleDown?.Invoke(this);
        }
    }

    private void OnBecameInvisible()
    {
        if (!photonView.IsMine) return;
        OnObstacleDown?.Invoke(this);
    }
}
