using System;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Pipe : MonoBehaviourPun
{
    public event Action<Pipe> OnPassed;

    public Vector3 MaxBounds => _mesh.bounds.max;

    public float Speed { get => _speed; set => _speed = value; }

    private float _speed;

    private ObstacleFactory obstacleFactory;

    [SerializeField]
    private Renderer _mesh = null;

    [Header("Gate")]
    [SerializeField] private GameObject upperGate = null;

    [SerializeField] private GameObject lowerGate = null;

    [Header("Enemies")]
    public AnimationCurve enemyCurve;

    private void Awake()
    {
        if (!GameServer.Instance.IsServer) return; 
        obstacleFactory = gameObject.AddComponent<ObstacleFactory>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        transform.position -= Vector3.forward * _speed * Time.deltaTime;

        if (_mesh.bounds.min.z <= -_mesh.bounds.size.z)
            OnPassed?.Invoke(this);
    }

    public Pipe SetPosition(Vector3 position)
    {
        transform.position = position;
        return this;
    }

    public static void TurnOn(Pipe pipe)
    {
        pipe.photonView.RPC(nameof(RPC_ActivatePipe), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ActivatePipe()
    {
        gameObject.SetActive(true);
    }

    public static void TurnOff(Pipe pipe)
    {
        pipe.photonView.RPC(nameof(RPC_DeactivatePipe), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DeactivatePipe()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_ResetGate), RpcTarget.All);

        obstacleFactory?.SpawnObstaclesWithinBounds(_mesh.bounds, (int)enemyCurve.Evaluate(Time.time));
    }

    [PunRPC]
    private void RPC_ResetGate()
    {
        upperGate.transform.localPosition = Vector3.zero;
        lowerGate.transform.localPosition = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (other.TryGetComponent(out PlayerModel player))
        {
            photonView.RPC(nameof(RPC_OpenGate), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_OpenGate()
    {
        upperGate.transform.DOLocalMove(new Vector3(0, 5f, 0), 0.5f).SetEase(Ease.InCubic);
        lowerGate.transform.DOLocalMove(new Vector3(0, -5f, 0), 0.5f).SetEase(Ease.InCubic);
    }
}