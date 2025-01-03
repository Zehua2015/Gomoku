using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Phon = ExitGames.Client.Photon;
using System.Collections;

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
    public GameObject red_circle;
    public GameObject green_circle;
    public GameObject win_circle;
    public List<Gomoku_Piece> currentPieceList = new List<Gomoku_Piece>();
    public PlayerState playerState = PlayerState.NotReady;
    public Gomoku_NetworkManager networkManager;
    private float playerTime = 20f;
    public bool isTimerRunning;

    private List<Gomoku_Piece> winnerList = new List<Gomoku_Piece>();
    private List<Vector3> posList = new List<Vector3>();
    private Vector3 preClickPosition;


    public double startTime;
    private bool preClick = false;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        zeroPointPosition = new Vector3(-2.07f, -2.07f, 0);
        //cellWidth = 0.4308f; // 4.41/19
        //pv = this.GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().SetSelfText(pieceColor);
        }
        else
        {
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().SetOpponentText(pieceColor);
        }
        preClick = false;
    }
    private void Awake()
    {
        pv = this.GetComponent<PhotonView>();
        networkManager = FindObjectOfType<Gomoku_NetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only operate if this is your client player
        if (!pv.IsMine) return;

        // check if every (BOTH) player get ready, if so set game state to start and start game
        var players = GameObject.FindObjectsOfType<Gomoku_Player>();
        foreach (var item in players)
        {
            if (item.playerState != PlayerState.Ready || players.Length != 2) return;
        }

        if (networkManager.gameState != GameState.Start)
        {
            networkManager.SetGameStart();
            gameObject.GetComponent<PhotonView>().RPC("StartTimer", RpcTarget.All);

        }

        // Return if this is not client player's turn
        if (GameObject.FindObjectOfType<Gomoku_NetworkManager>().playerTurn != pieceColor) return;

        // check if time out
        if (!isTimerRunning)
        {
            var color = pieceColor == PieceColor.Black ? PieceColor.White : PieceColor.Black;
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().GetComponent<PhotonView>().RPC("GameOver", RpcTarget.All, color);
        }
        // Start timer
        gameObject.GetComponent<PhotonView>().RPC("RunTimer", RpcTarget.All);

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
            if (!preClick)
            {
                preClickPosition = piecePos;
                DrawGreenCircle(piecePos);
                preClick = true;
                return;
            }
            else if (preClickPosition != piecePos)
            {
                //preClick = false;
                preClickPosition = piecePos;
                DestroyAllGreenCircle();
                DrawGreenCircle(piecePos);
                return;
            }
            else
            { 
                DestroyAllGreenCircle();
                DestroyAllRedCircle();
                preClick = false;
            }
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

            pv.RPC("DrawRedCircle", RpcTarget.Others, piecePos);
            
            // Play clicking sound
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().playClickingAudio();

            // Check if five in a row
            currentPieceList = GameObject.FindObjectsOfType<Gomoku_Piece>().ToList();
            bool isFive = IfFivePiece(currentPieceList, currentPiece);

            if (isFive)
            {
                gameObject.GetComponent<PhotonView>().RPC("EndTimer", RpcTarget.All);
                Vector3[] posArray = posList.ToArray();
                pv.RPC("HighlightFivePieces", RpcTarget.All, posArray);
                //GameObject.FindObjectOfType<Gomoku_NetworkManager>().GetComponent<PhotonView>().RPC("GameOver",RpcTarget.All, pieceColor);
            }

            // change turn
            gameObject.GetComponent<PhotonView>().RPC("EndTimer", RpcTarget.All);
            GameObject.FindObjectOfType<Gomoku_NetworkManager>().gameObject.GetComponent<PhotonView>().RPC("ChangeTurn",RpcTarget.All);
        }
    }


    bool IfFivePiece(List<Gomoku_Piece> currentList, Gomoku_Piece currentPiece)
    {
        bool result = false;
        if (winnerList != null && winnerList.Count > 0) winnerList.Clear();
        if (posList != null && posList.Count > 0) posList.Clear();

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

        if ((upList.Count + downList.Count + 1 >= 5) ||
            (leftList.Count + rightList.Count + 1 >= 5) ||
            (topLeftList.Count + bottomRightList.Count + 1 >= 5) ||
            (topRightList.Count + bottomLeftList.Count + 1 >= 5))
            result = true;

        if (upList.Count + downList.Count + 1 >= 5)
        {
            winnerList.AddRange(upList);
            winnerList.AddRange(downList);
            winnerList.Add(currentPiece);
        }
        else if (leftList.Count + rightList.Count + 1 >= 5)
        {
            winnerList.AddRange(leftList);
            winnerList.AddRange(rightList);
            winnerList.Add(currentPiece);
        }
        else if (topLeftList.Count + bottomRightList.Count + 1 >= 5)
        {
            winnerList.AddRange(topLeftList);
            winnerList.AddRange(bottomRightList);
            winnerList.Add(currentPiece);
        }
        else if (topRightList.Count + bottomLeftList.Count + 1 >= 5)
        {
            winnerList.AddRange(topRightList);
            winnerList.AddRange(bottomLeftList);
            winnerList.Add(currentPiece);
        }
        if (result)
        {
            winnerList = winnerList.OrderBy(e => e.row)
                .ThenBy(e => e.column)
                .ToList();
            print("winnerlist lenght");
            print(winnerList.Count);
            foreach (var item in winnerList)
            {
                Vector3 piecePos = new Vector3(item.column * cellWidth, item.row * cellWidth, zeroPointPosition.z) + zeroPointPosition;
                posList.Add(piecePos);
            }
            print("poslist lenght");
            print(posList.Count);

        }


        return result;
    }


    /// <summary>
    /// Highlight the winner four pieces 
    /// </summary>
    /// <param name="fivePieces"></param>
    [PunRPC]
    public void HighlightFivePieces(Vector3[] pos)
    {
        print("start");
        List<Vector3> positions = new List<Vector3>(pos);
        StartCoroutine(SpawnObjects(positions));
        return;
    }

    IEnumerator SpawnObjects(List<Vector3> pos)
    {

        print("start2");
        foreach (var item in pos)
        {
            print(item);
            if (pv.IsMine)
            {
                Instantiate(green_circle, item, green_circle.transform.rotation);
            }
            else
            {
                Instantiate(red_circle, item, red_circle.transform.rotation);
            }
            yield return new WaitForSeconds(0.1f);
        }
        GameObject.FindObjectOfType<Gomoku_NetworkManager>().GetComponent<PhotonView>().RPC("GameOver", RpcTarget.All, pieceColor);
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
        // Hashtable first
        Phon.Hashtable props = new Phon.Hashtable
        {
            { "PlayerState", PlayerState.Ready }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        playerState = PlayerState.Ready;

        //pv = this.GetComponent<PhotonView>();
        //networkManager = FindObjectOfType<Gomoku_NetworkManager>();
        if (pv.IsMine)
        {
            GameObject.FindAnyObjectByType<Gomoku_NetworkManager>().selfReady.text = "Ready";
            networkManager.SetGameTag();
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


    /// <summary>
    /// Start timer if true, Pause timer if false
    /// Return true if time remains, return false if time is up
    /// </summary>
    /// <param name="t"></param>

    [PunRPC]
    public void RunTimer()
    {

        double elapsed = PhotonNetwork.Time - startTime;
        playerTime = Mathf.Max(0, 20f - (float)elapsed);

        if (playerTime <= 0)
        {
            isTimerRunning = false;
            return;
        }
        if (pv.IsMine)
        {
            networkManager.UpdateTimerUI(playerTime);
        }
        else
        {
            networkManager.UpdateOpponentTimerUI(playerTime);
        }
        return;
    }

    public float getPlayerTime()
    {
        return playerTime;
    }

    [PunRPC]
    public void ResetTimer()
    {
        playerTime = 20f;
    }

    // renew timer logic *******
    [PunRPC]
    public void StartTimer()
    {
        startTime = PhotonNetwork.Time;
        isTimerRunning = true;
        playerTime = 20f;
    }

    [PunRPC]
    public void EndTimer()
    {
        isTimerRunning = false;
    }

    [PunRPC]
    public void DrawRedCircle(Vector3 pos)
    {
        Instantiate(red_circle, pos, red_circle.transform.rotation);
    }

    [PunRPC]
    public void DestroyAllRedCircle()
    {
        GameObject[] reds = GameObject.FindGameObjectsWithTag("RedCircle");
        if (reds.Length == 0) return;
        foreach (var item in reds)
        {
            Destroy(item);
        }

    }

    public GameObject DrawGreenCircle(Vector3 pos)
    {
        return Instantiate(green_circle, pos, green_circle.transform.rotation);
    }

    public void DestroyAllGreenCircle()
    {
        GameObject[] greens = GameObject.FindGameObjectsWithTag("GreenCircle");
        if (greens.Length == 0) return;
        foreach (var item in greens)
        {
            Destroy(item);
        }
    }

}
