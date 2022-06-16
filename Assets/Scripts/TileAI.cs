using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAI : MonoBehaviour
{
    public int[] pos = { -1, -1 };
    //public ChessPieceAI2D occupant = null;

    public ChessControllerND myNDController;
    private void Awake()
    {
        myNDController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
    }
    private void Start()
    {
        if (myNDController == null)
        {
            Debug.LogWarning("Cannot find controller");
        }
        return;

    }
    public void Setup(int[] posToBe)
    {
        pos = posToBe;
    }
    private void OnMouseDown()
    {
        myNDController.TileClicked(pos);
    }
    private void OnMouseOver()
    {
        myNDController.DisplayAttackers(pos);
    }
    private void OnMouseExit()
    {
        int myOccupantIndex = myNDController.GetOccupant(pos);
        if (myOccupantIndex != -1)
        {
            PieceInfo occupant = myNDController.GetPieces()[myOccupantIndex];
            if (occupant != null)
                occupant.ThisPiece.rotation = Quaternion.identity;
        }
    }
}
