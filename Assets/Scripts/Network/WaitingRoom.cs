using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class WaitingRoom : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;

    private int playerCount;
    private int roomSize;

    [SerializeField] private int minPlayersToStart = 1;

    [SerializeField] private TextMeshProUGUI roomCountDisplay = null;
    [SerializeField] private TextMeshProUGUI timerToStartDisplay = null;

    private bool readyToCountDown;
    private bool readyToStart;
    private bool startingGame;

    private float timerToStartGame;
    private float notFullGameTimer;
    private float fullGameTimer;

    [SerializeField] private float maxWaitTime = 0;
    [SerializeField] private float maxFullRoomWaitTime = 0;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        Init();
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        Init();
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Init()
    {
        ResetTimer();
        PlayerCountUpdate();
    }

    private void PlayerCountUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length - 1;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers - 1;

        roomCountDisplay.text = string.Format("Players: {0}/{1}", playerCount, roomSize);

        if (playerCount == roomSize)
            readyToStart = true;
        else if (playerCount >= minPlayersToStart)
            readyToCountDown = true;
        else
        {
            readyToCountDown = false;
            readyToStart = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerCountUpdate();
        if (PhotonNetwork.IsMasterClient)
            _photonView.RPC("RPC_SendTimer", RpcTarget.Others, timerToStartGame);
    }

    [PunRPC]
    private void RPC_SendTimer(float timeIn)
    {
        timerToStartGame = timeIn;
        notFullGameTimer = timeIn;

        if (timeIn < fullGameTimer) fullGameTimer = timeIn;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerCountUpdate();
    }

    private void Update()
    {
        WaitingForPlayers();
    }

    private void WaitingForPlayers()
    {
        if (playerCount <= 1) ResetTimer();

        if (readyToStart)
        {
            fullGameTimer -= Time.deltaTime;
            timerToStartGame = fullGameTimer;
        }
        else if (readyToCountDown)
        {
            notFullGameTimer -= Time.deltaTime;
            timerToStartGame = notFullGameTimer;
        }

        string tempTimer = string.Format("Starting in: {0:00}", timerToStartGame);

        string timerText = readyToStart || readyToCountDown ? tempTimer : "Waiting...";

        timerToStartDisplay.text = timerText;

        if (timerToStartGame <= 0f)
        {
            if (startingGame) return;

            StartGame();
        }
    }

    private void ResetTimer()
    {
        timerToStartGame = maxWaitTime;
        notFullGameTimer = maxWaitTime;
        fullGameTimer = maxFullRoomWaitTime;
    }

    private void StartGame()
    {
        startingGame = true;
        PhotonNetwork.LoadLevel((int)GameScenes.GAME_LEVEL);
    }

    public void DelayCancel()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel((int)GameScenes.MAIN_MENU);
    }
}
