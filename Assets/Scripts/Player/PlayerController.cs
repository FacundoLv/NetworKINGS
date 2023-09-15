using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    private Player localPlayer;

    private void Awake()
    {
        localPlayer = PhotonNetwork.LocalPlayer;
    }

    private void Start()
    {
        if (GameServer.Instance.IsServer)
        {
            Destroy(gameObject);
            return;
        }
        GameServer.Instance.AddCharacter(localPlayer);
    }

    private void Update()
    {
        Vector2 direction = GetInput();

        GameServer.Instance.SetDirection(direction, localPlayer);

        if (Input.GetKeyDown(KeyCode.Space))
            GameServer.Instance.Dash(direction, localPlayer);

        if (Input.GetKeyDown(KeyCode.Escape))
            GameServer.Instance.Escape(localPlayer);
    }

    private Vector2 GetInput()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        return Vector2.ClampMagnitude(playerInput, 1f);
    }
}