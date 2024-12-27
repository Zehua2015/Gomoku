using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public enum GameState {
    Ready = 1,
    GameOver = 3,
}

public class Gomoku_NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public PieceColor playerTurn = PieceColor.Black;
    public GameState gameState = GameState.Ready;
    //public TextMesh
    public GameObject gameOver;
    public AudioSource clickingAudio;

    public TextMeshProUGUI readyButtonTxt;
    public TextMeshProUGUI selfPiece;
    public TextMeshProUGUI selfReady;
    public TextMeshProUGUI OpponentPiece;
    public TextMeshProUGUI OpponentReady;
    public TextMeshProUGUI currentRound;
    public TextMeshProUGUI gameOverTxt;
    public TextMeshProUGUI winTxt;


    // Start is called before the first frame update
    void Start()
    {
        SetUIState(); 
        // Connect Server
        PhotonNetwork.ConnectUsingSettings();
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("Server Connected.");

        // Join or Create Room
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom("Gomoku", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("Room Joined.");

        // Create online player (in both room);
        if (player == null) return;
        GameObject newPlayer = PhotonNetwork.Instantiate(player.name, Vector3.zero, player.transform.rotation);

        // Initialize player properties
        if (PhotonNetwork.IsMasterClient)
        {
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.Black); 
        }
        else
        {
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.White) ;
        }
    }

    [PunRPC]
    public void ChangeTurn()
    {
        playerTurn = playerTurn == PieceColor.Black ? PieceColor.White : PieceColor.Black;
        currentRound.text = playerTurn == PieceColor.Black ? "Current Round: Black" : "Current Round: White";
    }

    [PunRPC]
    public void GameOver(PieceColor winColor)
    {
        gameState = GameState.GameOver;
        gameOver.SetActive(true);
        winTxt.text = winColor == PieceColor.Black ? "Black Win!" : "White Win!"; 
    }

    public void playClickingAudio()
    {
        if (clickingAudio == null) return;
        clickingAudio.Play();
    }

    public void onClickReadyButton()
    {
        readyButtonTxt.text = "Ready~";
        var players = GameObject.FindObjectsOfType<Gomoku_Player>();
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PhotonView>().RPC("SetReadyState", RpcTarget.All); 
            }
        }
    }
    public void SetUIState()
    {
        readyButtonTxt.text = "Ready";
        selfPiece.text = "";
        selfReady.text = "";
        OpponentPiece.text = "";
        OpponentReady .text = "";
        currentRound.text = "";
        gameOverTxt.gameObject.SetActive(false);
    }

    public void SetSelfText(PieceColor pieceColor)
    {
        selfPiece.text = pieceColor == PieceColor.Black ? "Black" : "White";
        selfReady.text = "Not Ready...";
    }
    public void SetOpponentText(PieceColor pieceColor)
    {
        OpponentPiece.text = pieceColor == PieceColor.Black ? "Black" : "White";
        OpponentReady.text = "Not Ready...";
    }
}
