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
        // Reset UI, prepare for next round if applicable
        selfReady.text = "Not Ready";
        OpponentReady.text = "Not Ready";
        // Set NotReady state for both players.
        var players = GameObject.FindObjectsOfType<Gomoku_Player>();
        foreach (var item in players)
        {
            item.GetComponent<PhotonView>().RPC("SetNotReadyState", RpcTarget.All);
        }

        gameState = GameState.GameOver;
        gameOver.SetActive(true);
        winTxt.text = winColor == PieceColor.Black ? "Black Win!" : "White Win!";
        readyButtonTxt.text = "Play Again";
    }

    public void clearBoard()
    {
        GameObject[] allPieces = GameObject.FindGameObjectsWithTag("gamePiece");

        foreach (var item in allPieces)
        {
            Destroy(item);
        }
    }

    //[PunRPC]
    public void StartOver()
    {
        resetUIState();
        clearBoard();
        gameState = GameState.Ready;
        playerTurn = PieceColor.Black;
        changeSide();
    }

    public void changeSide()
    {
        var players = GameObject.FindObjectsOfType<Gomoku_Player>();
        foreach (var item in players)
        {
            item.pieceColor = item.pieceColor == PieceColor.Black ? PieceColor.White : PieceColor.Black;
        }
        OpponentPiece.text = OpponentPiece.text == "Black" ? "White" : "Black";
        selfPiece.text = selfPiece.text == "Black" ? "White" : "Black";
    }

    public void getReady()
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

    public void playClickingAudio()
    {
        if (clickingAudio == null) return;
        clickingAudio.Play();
    }

    public void onClickReadyButton()
    {
        if (gameState == GameState.GameOver)
        {
            StartOver();
        }
            getReady();

    }
    public void SetUIState()
    {
        readyButtonTxt.text = "Ready";
        selfPiece.text = "Joining...";
        selfReady.text = "";
        OpponentPiece.text = "Searching...";
        OpponentReady.text = "";
        currentRound.text = "";
        gameOverTxt.gameObject.SetActive(false);
    }

    public void resetUIState()
    {
        currentRound.text = "";
        gameOverTxt.gameObject.SetActive(false);
    }

    public void SetSelfText(PieceColor pieceColor)
    {
        selfPiece.text = pieceColor == PieceColor.Black ? "Black" : "White";
        selfReady.text = "Not Ready";
    }
    public void SetOpponentText(PieceColor pieceColor)
    {
        OpponentPiece.text = pieceColor == PieceColor.Black ? "Black" : "White";
        OpponentReady.text = "Not Ready";
    }
}
