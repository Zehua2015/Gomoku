using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Gomoku_NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
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
            newPlayer.GetComponent<Gomoku_Player>().pieceColor = PieceColor.Black;
        }
        else
        {
            newPlayer.GetComponent<Gomoku_Player>().pieceColor = PieceColor.White;
        }
    }

}
