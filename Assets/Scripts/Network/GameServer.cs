using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class GameServer : MonoBehaviourPunCallbacks
{
    public bool IsServer => PhotonNetwork.LocalPlayer == _server;

    public static GameServer Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<GameServer>();
            if (_instance != null) return _instance;

            _instance = PhotonNetwork
                .Instantiate(nameof(GameServer), Vector3.zero, Quaternion.identity)
                .GetComponent<GameServer>();

            return _instance;
        }
    }

    private static GameServer _instance;

    Player _server;

    private Dictionary<Player, PlayerModel> playerDictionary = new Dictionary<Player, PlayerModel>();

    private SpawnPoints spawnPoints;

    [SerializeField]
    private PlayerModel[] models;

    private void Awake()
    {
        if (photonView.IsMine)
            photonView.RPC(nameof(RPC_SetServer), RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer);

        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [PunRPC]
    private void RPC_SetServer(Player server)
    {
        _server = server;
    }

    public void AddCharacter(Player player)
    {
        if (player == _server) return;
        photonView.RPC(nameof(RPC_SpawnCharacter), _server, player);
    }

    [PunRPC]
    private void RPC_SpawnCharacter(Player player)
    {
        if (playerDictionary.ContainsKey(player)) return;

        int playerNumber = player.ActorNumber - 2;
        string playerModel = models[playerNumber].name;

        spawnPoints = spawnPoints ?? FindObjectOfType<SpawnPoints>();

        PlayerModel newModel = PhotonNetwork
            .Instantiate(Path.Combine("Game", playerModel), spawnPoints[playerNumber].position, Quaternion.identity)
            .GetComponent<PlayerModel>();

        newModel.OnPlayerDown += PlayerDown;

        playerDictionary.Add(player, newModel);
    }

    private void PlayerDown(PlayerModel model)
    {
        foreach (var player in playerDictionary)
        {
            if (player.Value != model) continue;

            photonView.RPC(nameof(RPC_PlayerDown), player.Key);
            playerDictionary.Remove(player.Key);
            break;
        }

        if (playerDictionary.Count != 1) return;
        var lastPlayer = playerDictionary.Single();
        photonView.RPC(nameof(RPC_DisplayWinner), lastPlayer.Key);
        playerDictionary.Remove(lastPlayer.Key);
    }

    [PunRPC]
    private void RPC_PlayerDown()
    {
        var screen = Resources.Load("UI/LooseScreen");
        var screenGo = Instantiate(screen);

        Destroy(screenGo, 2f);
    }

    [PunRPC]
    private void RPC_DisplayWinner()
    {
        var screen = Resources.Load("UI/WinScreen");
        var screenGo = Instantiate(screen);
    }

    public void SetDirection(Vector2 direction, Player player)
    {
        photonView.RPC(nameof(RPC_SetInput), _server, direction, player);
    }

    [PunRPC]
    private void RPC_SetInput(Vector2 direction, Player player)
    {
        if (!playerDictionary.ContainsKey(player)) return;
        playerDictionary[player].SetDirection(direction);
    }

    public void Dash(Vector2 direction, Player player)
    {
        photonView.RPC(nameof(RPC_DoDash), _server, direction, player);
    }

    [PunRPC]
    private void RPC_DoDash(Vector2 direction, Player player)
    {
        if (!playerDictionary.ContainsKey(player)) return;
        playerDictionary[player].Dash(direction);
    }

    public void Escape(Player player)
    {
        photonView.RPC(nameof(RPC_Escape), _server, player);
    }

    [PunRPC]
    private void RPC_Escape(Player player)
    {
        if (playerDictionary.ContainsKey(player)) return;
        photonView.RPC(nameof(RPC_Exit), player);
    }

    [PunRPC]
    private void RPC_Exit()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadSceneAsync((int)GameScenes.MAIN_MENU);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        if (playerDictionary.ContainsKey(player))
            playerDictionary.Remove(player);

        if (player == _server || (Instance.IsServer && PhotonNetwork.PlayerList.Length == 1))
        {
            bool isLeaving = PhotonNetwork.NetworkClientState == ClientState.Leaving;
            if (!isLeaving)
                PhotonNetwork.LeaveRoom();
            SceneManager.LoadSceneAsync((int)GameScenes.MAIN_MENU);
        }
    }
}

public enum GameScenes
{
    MAIN_MENU = 0,
    WAITING_ROOM = 1,
    GAME_LEVEL = 2,
}
