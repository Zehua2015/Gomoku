//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;

public enum GameState {
    Ready = 1,
    Start = 2,
    GameOver = 3,
}

public class Gomoku_NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player;
    public PieceColor playerTurn = PieceColor.Black;
    public GameState gameState = GameState.Ready;
    public GameObject gameOver;
    public AudioSource clickingAudio;

    public Button readyBtn;
    public TextMeshProUGUI readyButtonTxt;
    public TextMeshProUGUI selfPiece;
    public TextMeshProUGUI selfReady;
    public TextMeshProUGUI OpponentPiece;
    public TextMeshProUGUI OpponentReady;
    public TextMeshProUGUI currentRound;
    public TextMeshProUGUI waitTxt;
    public TextMeshProUGUI gameOverTxt;
    public TextMeshProUGUI winTxt;

    public TextMeshProUGUI selfTimerTxt;
    public TextMeshProUGUI opponentTimerTxt;

    GameObject newPlayer;
    Gomoku_Player gomoku_Player;
    Gomoku_Player[] players;
    


    // Start is called before the first frame update
    void Start()
    {
        SetUIState(); 
        // Connect Server
        PhotonNetwork.ConnectUsingSettings();
        readyBtn.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintLocalPlayerProperties();
        }
        players = FindObjectsOfType<Gomoku_Player>();
    }

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
        readyBtn.gameObject.SetActive(true);

        // Create online player (in both room);
        if (player == null) return;
        newPlayer = PhotonNetwork.Instantiate(player.name, Vector3.zero, player.transform.rotation);
        gomoku_Player = newPlayer.GetComponent<Gomoku_Player>();

        // Initialize player properties
        if (PhotonNetwork.IsMasterClient)
        {
            // TODO: delete RPC
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.Black);
            Hashtable props = new Hashtable
            {
                { "PlayerState", PlayerState.NotReady },
                { "PieceColor", PieceColor.Black }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        else
        {
            // TODO: delete RPC
            newPlayer.GetComponent<PhotonView>().RPC("SetPieceColor", RpcTarget.All, PieceColor.White) ;

            Hashtable props = new Hashtable
            {
                { "PlayerState", PlayerState.NotReady },
                { "PieceColor", PieceColor.White }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        }
    }

    public override void OnPlayerEnteredRoom(Player nPlayer)
    {
        base.OnPlayerEnteredRoom(nPlayer);
        if (newPlayer.GetComponent<Gomoku_Player>().playerState == PlayerState.NotReady) return;
        foreach (var item in players)
        {
            if (item.GetComponent<PhotonView>().IsMine)
            {
                item.GetComponent<PhotonView>().RPC("SetReadyState", RpcTarget.All);
            }
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        foreach (var keys in changedProps.Keys)
        {
            if (keys.Equals("PlayerState"))
            {
                PlayerState newState = (PlayerState)changedProps["PlayerState"];
                // ......
            }
            // Renew UI here (instead of using RPC)
            if (targetPlayer.IsLocal)
            {
                // ......
                // selfReadyTxt.text = ....
            }
        }



    }


    public void PrintLocalPlayerProperties()
    {
        var customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        foreach (var key in customProperties.Keys)
        {
            Debug.Log($"Key: {key}, Value: {customProperties[key]}");
        }
    }




    [PunRPC]
    public void ChangeTurn()
    {
        playerTurn = playerTurn == PieceColor.Black ? PieceColor.White : PieceColor.Black;
        foreach (var item in players)
        {
            if (item.pieceColor == playerTurn)
            {
                item.StartTimer();
            }
        }
        if (gameState == GameState.GameOver)
        {
            currentRound.text = "";
        }
        else
        {
            currentRound.text = currentRound.text == "Your round!" ? "Opponent's Round" : "Your round!";
        }

    }



    ///
    /// Player Timer moved to Gomoku_Player.cs
    ///

    [PunRPC]
    public void SetGameStart()
    {
        GameObject redCircle = GameObject.FindGameObjectWithTag("RedCircle");
        if (redCircle != null)
            Destroy(redCircle);
        clearBoard();
        gameOverTxt.gameObject.SetActive(false);
        waitTxt.gameObject.SetActive(false);
        gameState = GameState.Start;
        currentRound.text = gomoku_Player.pieceColor == PieceColor.Black ? "Your round!" : "Opponent's Round";
    }

    [PunRPC]
    public void GameOver(PieceColor winColor)

    {
        Color color = readyBtn.GetComponent<Image>().color;
        color.a = 1f;
        readyBtn.GetComponent<Image>().color = color;
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
        winTxt.text = winColor == gomoku_Player.pieceColor ? "You Win!" : "You lose..";
        winTxt.color = winColor == gomoku_Player.pieceColor ? Color.green : Color.red;
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

    public void StartOver()
    {
        gomoku_Player.GetComponent<PhotonView>().RPC("ResetTimer", RpcTarget.All);
        gomoku_Player.DestroyAllGreenCircle();
        gomoku_Player.DestroyAllRedCircle();
        resetUIState();
        //clearBoard();
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
        // Change ready button transparency
        Color color = readyBtn.GetComponent<Image>().color;
        color.a = 0.5f;
        readyBtn.GetComponent<Image>().color = color;
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
        foreach (var item in players)
        {
            Debug.Log(item.playerState);
        }

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
        waitTxt.gameObject.SetActive(true);

        //gameOverTxt.gameObject.SetActive(false);
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

    public void UpdateTimerUI(float time)
    {
        //int min = Mathf.FloorToInt(time / 60);
        //int sec = Mathf.FloorToInt(time % 60);

        //selfTimerTxt.text = $"{min:00}:{sec:00}";
        selfTimerTxt.text = $"{time:00}S";
    }

    public void UpdateOpponentTimerUI(float time)
    {
        //int min = Mathf.FloorToInt(time / 60);
        //int sec = Mathf.FloorToInt(time % 60);

        //opponentTimerTxt.text = $"{min:00}:{sec:00}";
        opponentTimerTxt.text = $"{time:00}S";
    }

}
