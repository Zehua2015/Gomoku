using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PieceColor
{
    Black=0,
    White=1,
}
public class Gomoku_Piece : MonoBehaviour
{
    public int row;
    public int column;
    public PieceColor pieceColor = PieceColor.Black;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void SetRowColumnValue(int[] rowColumnValues)
    {
        if (rowColumnValues.Length != 2) return;

        row = rowColumnValues[0];
        column = rowColumnValues[1];
    }

}
