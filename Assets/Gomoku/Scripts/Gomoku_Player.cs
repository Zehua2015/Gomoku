using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
//using static UnityEditor.Progress;

public enum PlayerState
{
    NotReady = 0,
    Ready = 1,
}

public class Gomoku_Player : MonoBehaviour
{
    public Vector3 zeroPointPosition;
    public float cellWidth;
    public PieceColor pieceColor = PieceColor.Black;
    private PhotonView pv;
    private int row;
    private int column;

    public GameObject black_piece;
    public GameObject white_piece;
    public List<Gomoku_Piece> currentPieceList = new List<Gomoku_Piece>();
    public PlayerState playerState = PlayerState.NotReady;

    // Start is called before the first frame update
    void Start()
    {
        zeroPointPosition = new Vector3(-2.07f, -2.07f, 0);
        //cellWidth = 0.4308f; // 4.41/19
        pv = this.GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().SetSelfText(pieceColor);
        }
        else
        {
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().SetOpponentText(pieceColor);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine) return;
        if (GameObject.FindObjectOfType<Gomoku_NetworkManager>().playerTurn != pieceColor) return;
        // Check..../////
        if (GameObject.FindObjectOfType<Gomoku_NetworkManager>().gameState != GameState.Ready) return;

        var players = GameObject.FindObjectsOfType<Gomoku_Player>();
        foreach (var item in players)
        {
            if (item.playerState != PlayerState.Ready) return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            GameObject newPiece;
            Gomoku_Piece currentPiece = new Gomoku_Piece();
            // Compute new piece position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offsetPos = mousePos - zeroPointPosition;
            row = (int)Mathf.Round(offsetPos.y / cellWidth);
            column = (int)Mathf.Round(offsetPos.x / cellWidth);

            int[] rowColumnValue = { row, column };
            // If Exceed board boundary
            if (row < 0 || row > 18 || column < 0 || column > 18)
                return;
            // If a piece already exists at current position
            currentPieceList = GameObject.FindObjectsOfType<Gomoku_Piece>().ToList();
            foreach(var item in currentPieceList)
            {
                if (item.row == row && item.column == column)
                {
                    print("already a piece here!");
                    return;
                }    
            }



            Vector3 piecePos = new Vector3(column * cellWidth, row * cellWidth, zeroPointPosition.z) + zeroPointPosition;

            // Generate online piece
            if (pieceColor == PieceColor.Black)
            {
                newPiece = PhotonNetwork.Instantiate(black_piece.name, piecePos, black_piece.transform.rotation);
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValue);
                currentPiece = newPiece.GetComponent<Gomoku_Piece>();
            }
            else
            {
                newPiece = PhotonNetwork.Instantiate(white_piece.name, piecePos, white_piece.transform.rotation);
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValue);
                currentPiece = newPiece.GetComponent<Gomoku_Piece>();
            }

            // Play clicking sound
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().playClickingAudio();

            // Check if five in a row
            currentPieceList = GameObject.FindObjectsOfType<Gomoku_Piece>().ToList();
            bool isFive = IfFivePiece(currentPieceList, currentPiece);

            if (isFive)
            {
                GameObject.FindObjectOfType<Gomoku_NetworkManager>().GetComponent<PhotonView>().RPC("GameOver",RpcTarget.All, pieceColor);
            }



            // change turn
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().gameObject.GetComponent<PhotonView>().RPC("ChangeTurn",RpcTarget.All);


        }
    }

    bool IfFivePiece(List<Gomoku_Piece> currentList, Gomoku_Piece currentPiece)
    {
        bool result = false;

        // using current piece to find surrounding piece
        List<Gomoku_Piece> currentColorList = currentList.Where(en => en.pieceColor == pieceColor).ToList();
        var upList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.Up);
        var downList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.Down);
        var leftList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.Left);
        var rightList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.Right);
        var topLeftList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.TopLeft);
        var bottomRightList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.BottomRight);
        var topRightList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.TopRight);
        var bottomLeftList = GetSameColorDirection(currentColorList, currentPiece, PieceDirection.BottomLeft);
        //print(upList.Count + ":" + downList.Count + ":" + leftList.Count + ":" + rightList.Count + ":" + topLeftList.Count + ":" + bottomRightList.Count + ":" + topRightList.Count + ":" + bottomLeftList.Count);

        if ((upList.Count + downList.Count + 1 == 5) ||
            (leftList.Count + rightList.Count + 1 == 5) ||
            (topLeftList.Count + bottomRightList.Count + 1 == 5) ||
            (topRightList.Count + bottomLeftList.Count + 1 == 5))
            result = true;

        return result;
    }

    List<Gomoku_Piece> GetSameColorDirection(List<Gomoku_Piece> currentColorList, Gomoku_Piece currentPiece, PieceDirection direction)
    {
        List<Gomoku_Piece> list = new List<Gomoku_Piece>();
        switch(direction)
        {
            case PieceDirection.Up:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.Up);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.Down:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.Down);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.Left:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column - 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.Left);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.Right:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row && item.column == currentPiece.column + 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.Right);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.TopLeft:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column - 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.TopLeft);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.BottomRight:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column + 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.BottomRight);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.TopRight:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row + 1 && item.column == currentPiece.column + 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.TopRight);
                        list.AddRange(resultList);
                    }
                }
                break;

            case PieceDirection.BottomLeft:
                foreach (var item in currentColorList)
                {
                    if (item.row == currentPiece.row - 1 && item.column == currentPiece.column - 1)
                    {
                        list.Add(item);
                        var resultList = GetSameColorDirection(currentColorList, item, PieceDirection.BottomLeft);
                        list.AddRange(resultList);
                    }
                }
                break;
        }


        return list;
    }

    public enum PieceDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        TopLeft = 4,
        BottomRight = 5,
        TopRight = 6,
        BottomLeft = 7,
    }


    [PunRPC]
    public void SetPieceColor(PieceColor pc)
    {
        pieceColor = pc; 
    }

    [PunRPC]
    public void SetReadyState()
    {
        playerState = PlayerState.Ready;
        if (pv.IsMine)
        {
            GameObject.FindAnyObjectByType<Gomoku_NetworkManager>().selfReady.text = "Ready";
        }
        else
        {
            GameObject.FindAnyObjectByType<Gomoku_NetworkManager>().OpponentReady.text = "Ready";
        }
    }

    [PunRPC]
    public void SetNotReadyState()
    {
        playerState = PlayerState.NotReady;
    }
}
