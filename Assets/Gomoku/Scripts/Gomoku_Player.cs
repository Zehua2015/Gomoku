using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
    // Start is called before the first frame update
    void Start()
    {
        zeroPointPosition = new Vector3(-2.07f, -2.07f, 0);
        //cellWidth = 0.4308f; // 4.41/19
        pv = this.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine) return;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject newPiece;
            // Compute new piece position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 offsetPos = mousePos - zeroPointPosition;
            row = (int)Mathf.Round(offsetPos.y / cellWidth);
            column = (int)Mathf.Round(offsetPos.x / cellWidth);

            int[] rowColumnValue = { row, column };
            // If Exceed board boundary
            if (row < 0 || row > 18 || row < 0 || row > 18)
                return;


            Vector3 piecePos = new Vector3(column * cellWidth, row * cellWidth, zeroPointPosition.z) + zeroPointPosition;

            // Generate online piece
            if (pieceColor == PieceColor.Black)
            {
                newPiece = PhotonNetwork.Instantiate(black_piece.name, piecePos, black_piece.transform.rotation);
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValue);
            }
            else
            {
                newPiece = PhotonNetwork.Instantiate(white_piece.name, piecePos, white_piece.transform.rotation);
                newPiece.GetComponent<PhotonView>().RPC("SetRowColumnValue", RpcTarget.All, rowColumnValue);
            }
        }
    }

}
