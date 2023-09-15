using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class ServerConnection : MonoBehaviourPunCallbacks
{
    public event Action OnJoinedARoom;
    public event Action OnLeftARoom;

    private string _createdRoomName;
    private string _joinedRoomName;

    private const int MAX_PLAYERS = 4;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            ConnectToServer();
    }

    private void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SetNewRoomName(string name)
    {
        _createdRoomName = name;
    }

    public void SetJoinedRoomName(string name)
    {
        _joinedRoomName = name;
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(_createdRoomName)) return;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MAX_PLAYERS + 1,
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.JoinOrCreateRoom(_createdRoomName, options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("CreateRoom");
        PhotonNetwork.Instantiate("Game/GameServer", Vector3.zero, Quaternion.identity);
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(_joinedRoomName)) return;

        PhotonNetwork.JoinRoom(_joinedRoomName);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("JoinedRoom");
        OnJoinedARoom?.Invoke();
    }

    public override void OnLeftRoom()
    {
        OnLeftARoom?.Invoke();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
