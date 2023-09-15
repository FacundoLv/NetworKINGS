using Photon.Pun;
using UnityEngine;

public class PlayerView : MonoBehaviourPun
{
    private PlayerModel _model;

    private Animator _anim;

    private Vector2 _input = Vector2.zero;

    [SerializeField]
    private ParticleSystem fx_hit;

    [SerializeField]
    private ParticleSystem fx_explosion;

    void Start()
    {
        if (!photonView.IsMine) return;
        _model = GetComponent<PlayerModel>();
        _anim = GetComponentInChildren<Animator>();

        _model.OnMove += OnMove;
        _model.OnDash += OnDash;
        _model.OnDamage += OnDamage;
        _model.OnPlayerDown += PlayerDown;
    }

    private void OnMove(Vector2 input)
    {
        _input = Vector2.Lerp(_input, input, 0.5f);
        _anim.SetFloat("InputX", _input.x);
        _anim.SetFloat("InputY", _input.y);
    }

    private void OnDash()
    {
        _anim.SetTrigger("Dash");
    }

    private void OnDamage(PlayerModel obj)
    {
        photonView.RPC(nameof(RPC_OnDamage), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_OnDamage()
    {
        Instantiate(fx_hit, transform.position, Quaternion.identity);
    }

    private void PlayerDown(PlayerModel player)
    {
        photonView.RPC(nameof(RPC_PlayerDown), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayerDown()
    {
        Instantiate(fx_explosion, transform.position, Quaternion.identity);
    }
}
