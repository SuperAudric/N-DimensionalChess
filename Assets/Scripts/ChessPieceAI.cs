using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceAI : MonoBehaviour
{
    //public for display
    public string pieceName = "Black Pawn";
    public bool isBlack=true;
    public int timesMoved=-1;
    public int pieceType = 0;
    
    //public for linking
    public Sprite[] pieceImages;
    //public TileAI myTile;
    public void Setup(bool black, int pieceVariety)
    {
        Setup(black, pieceVariety, -1);
    }
    public void ChangePromotion(int newType)
    {
        Debug.Log("Changing type from "+pieceType+" to "+newType);
        pieceType = newType;
        Setup(isBlack, pieceType, timesMoved);
    }
    public void Setup(bool black,int pieceType, int movesSoFar)
    {
        isBlack = black;
        timesMoved = movesSoFar;
        this.pieceType = pieceType;
        pieceName = isBlack?"Black ": "White ";
        if(pieceType < 0|| pieceType > 5)
        {
            Debug.LogWarning("Invalid piece type, defaulting to pawn");
            pieceType = 0;
        }
        switch (pieceType)
            {
            case 0:
                pieceName+= "Pawn";
                break;
            case 1:
                pieceName += "Rook";
                break;
            case 2:
                pieceName += "Knight";
                break;
            case 3:
                pieceName += "Bishop";
                break;
            case 4:
                pieceName += "King";
                break;
            case 5:
                pieceName += "Queen";
                break;
        }
        gameObject.GetComponent<SpriteRenderer>().sprite = AudioAndGraphicsSelector.GetSprite(pieceName);
    }
    
}
