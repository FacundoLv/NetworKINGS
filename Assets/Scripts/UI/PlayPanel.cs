using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayPanel : MonoBehaviourPunCallbacks
{
    private ServerConnection _server;

    [SerializeField]
    private Button[] buttons;

    private void Start()
    {
        _server = FindObjectOfType<ServerConnection>();

        _server.OnJoinedARoom += LoadWaitingScene;
    }

    private void OnDestroy()
    {
        _server.OnJoinedARoom -= LoadWaitingScene;
    }

    private void LoadWaitingScene()
    {
        PhotonNetwork.LoadLevel((int)GameScenes.WAITING_ROOM);
    }

    public override void OnConnectedToMaster()
    {
        foreach (var button in buttons)
        {
            button.interactable = true;
        }
    }
}
