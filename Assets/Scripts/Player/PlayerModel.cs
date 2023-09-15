using DG.Tweening;
using Photon.Pun;
using System;
using UnityEngine;

public class PlayerModel : MonoBehaviourPun
{
    public event Action<Vector2> OnMove;

    public event Action OnDash;

    public event Action<PlayerModel> OnDamage;

    public event Action<PlayerModel> OnPlayerDown;

    public float Health { get => _health; }

    public float CurrentDash { get => currentDash; }

    [Header("Movement")]
    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f;

    [SerializeField, Range(0f, 50f)]
    private float dashForce = 0f;

    [SerializeField, Range(0f, 10f)]
    private float bounciness = 0.5f;

    private Rigidbody _body;

    private Vector3 _velocity = default;

    private Vector2 _direction;

    [Header("Dash")]
    [SerializeField, Range(0.5f, 2f)]
    private float dashCooldown = 0f;

    private float currentDash = 0f;

    private bool canDash = false;

    private bool isDashing = false;

    [Header("Stats")]
    [SerializeField] private float _health = 30;

    public void SetDirection(Vector2 input)
    {
        _direction = input;
    }

    public async void Dash(Vector2 input)
    {
        if (!canDash || isDashing) return;

        OnDash?.Invoke();
        isDashing = true;
        currentDash = 0f;

        _velocity = Vector3.zero;
        Vector3 dash = new Vector3(input.x, input.y, 0f).normalized * dashForce;
        await DOVirtual
            .Vector3(_velocity, dash, 0.25f, (x) => { _body.velocity = x; })
            .SetEase(Ease.OutFlash)
            .AsyncWaitForCompletion();

        isDashing = false;
    }

    public void GetDamage(float damage)
    {
        _health -= damage;
        OnDamage?.Invoke(this);

        if (_health <= 0) Die();
    }

    private void Start()
    {
        _body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (!canDash) currentDash += Time.deltaTime;
        canDash = currentDash >= dashCooldown;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (isDashing) return;

        CalculateVelocity(_direction);

        _body.velocity = _velocity;
    }

    private void CalculateVelocity(Vector2 inputVector)
    {
        OnMove?.Invoke(inputVector);
        Vector3 desiredVelocity = new Vector3(inputVector.x, inputVector.y, 0f) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        _velocity.x = Mathf.MoveTowards(_velocity.x, desiredVelocity.x, maxSpeedChange);
        _velocity.y = Mathf.MoveTowards(_velocity.y, desiredVelocity.y, maxSpeedChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;
        BounceOff(collision);
    }

    private void BounceOff(Collision collision)
    {
        Vector3 contactNormal = Vector3.zero;

        for (int i = 0; i < collision.contactCount; i++)
            contactNormal += collision.GetContact(i).normal;

        _velocity = _velocity.normalized + (new Vector3(contactNormal.x, contactNormal.y, 0f).normalized * bounciness);
    }

    private void Die()
    {
        OnPlayerDown?.Invoke(this);
        photonView.RPC(nameof(RPC_Die), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Die()
    {
        gameObject.SetActive(false);
    }
}
