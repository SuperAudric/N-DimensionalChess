using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MLAPI.Serialization;


//This file contains the small, helper classes used in this project. Namely: PieceInfo, MovePackage, SpecialMove, and TurnRecord.
//PieceInfo: Stores relevant information about a single piece
//MovePackage: Stores relevant information about all valid moves by a piece, used to make this move.
//SpecialMove: Stores relevant information about a single, atypical move (en passant, castle, capture), used to make this move
//TurnRecord: Stores relevant information about a single turn, and can compute a text to describe it. Used to undo a turn, and in displaying it



//I should try to localize more info into this block. Most notably the piece's moves. The potential moves will only change when a piece moves that was/becomes in it's move possibilities.
[SerializeField]
/// <summary>
/// Stores relevant information about a single piece
/// </summary>
public class PieceInfo : AutoNetworkSerializable
{
    //public for display
    public string pieceName = "Black Pawn";
    public bool isBlack = true;
    public int timesMoved = -1;
    public int pieceType = 0;
    public Transform ThisPiece;
    public int[] position;

    //not fully implemented yet
    public int index = -1;
    public MovePackage myPotentialMoves;
    public PieceInfo[] dependants;//updates myPotentialMoves when a piece this sees changes (piece moves which was/is now in what this piece could potentially move into)

    //public TileAI myTile;
    public void Setup(bool black, int pieceVariety)
    {
        Setup(black, pieceVariety, -1);
    }
    public void ChangePromotion(int newType)
    {
        var temp = ThisPiece.GetComponent<ChessPieceAI>();
        if (temp != null)
            temp.ChangePromotion(newType);
        else
            Debug.Log("I Don't exist physically?");
        pieceType = newType;
        Setup(isBlack, pieceType, timesMoved);
    }
    public void Setup(bool black, int pieceType, int movesSoFar)
    {
        isBlack = black;
        timesMoved = movesSoFar;
        this.pieceType = pieceType;
        pieceName = isBlack ? "Black " : "White ";
        if (pieceType < 0 || pieceType > 5)
        {
            Debug.LogWarning("Invalid piece type, defaulting to pawn");
            this.pieceType = 0;
        }
        switch (pieceType)
        {
            case 0:
                pieceName += "Pawn";
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
    }
    public override int GetHashCode()
    {
        return Tuple.Create(isBlack, pieceType, position).GetHashCode();
    }

}
/// <summary>
/// Stores relevant information about all valid moves by a piece, used to make this move.
/// </summary>
public class MovePackage //Describes all potential moves of one piece
{
    public PieceInfo piece;
    public List<int[]> validCoords;
    public List<SpecialMove> validSpecials;

    public MovePackage(PieceInfo Piece)//makes an empty MovePackage
    {
        piece = Piece;
        validCoords = new List<int[]>();
        validSpecials = new List<SpecialMove>();
    }
    public MovePackage(PieceInfo Piece, List<int[]> ValidCoords, List<SpecialMove> ValidSpecials)
    {
        piece = Piece;
        validCoords = ValidCoords;
        validSpecials = ValidSpecials;
    }
    public MovePackage() { }//weirdly this is needed for deserialization
}
public class SpecialMove
{
    public int[] pos;
    public readonly PieceInfo relevantPiece;
    public SpecialMove(int[] myPos, PieceInfo piece)
    {
        pos = myPos;
        relevantPiece = piece;
    }
    public SpecialMove() { }
}
[SerializeField]
public class TurnRecord: AutoNetworkSerializable //All the info needed to describe what happened in a turn, in raw data form, and described in a string
{
    public int[] from { get; }
    public int[] to { get; }
    public int special { get; } //describes captures and special moves
    public int[] specialCoords { get; }
    public int capturedAge { get; } = 0;
    public string turnDescription { get; }
    public int index { get; } = 0;
    public TurnRecord(int[] From, int[] To, PieceInfo movedPiece, PieceInfo OtherPiece)
    {
        ChessControllerND myController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        from = From;
        to = To;
        special = -1;
        specialCoords = new int[] { };
        if (myController != null)
            turnDescription = (myController.GetManualControlEnabled() ? "Poof! " : "");
        if (movedPiece != null)
        {
            for (int i = 0; i < from.Length; i++)
            {
                if (i < 1)
                    turnDescription += (char)('a' + from[i]) + ", ";
                else
                    turnDescription += (from[i] + 1) + ", ";
            }

            turnDescription += movedPiece.pieceName + (myController.GetManualControlEnabled() ? " teleports" : "") + " to ";

            for (int i = 0; i < from.Length; i++)
            {
                if (i < 1)
                    turnDescription += (char)('a' + To[i]);
                else
                    turnDescription += (To[i] + 1);
                if (i != to.Length - 1)
                {
                    turnDescription += ", ";
                }
            }
            if (OtherPiece != null)
            {
                capturedAge = OtherPiece.timesMoved;
                if (myController.CompareArrays(to, myController.GetPiecePosition(OtherPiece)))
                {
                    turnDescription += " and captures " + OtherPiece.pieceName;
                    index = (Array.IndexOf(myController.GetPieces(), OtherPiece));
                    special = OtherPiece.pieceType + (OtherPiece.isBlack ? 0 : myController.THINGTYPES);
                }
                else
                {
                    specialCoords = myController.GetPiecePosition(OtherPiece);
                    if (movedPiece.pieceName.Contains("King") && OtherPiece.pieceName.Contains("Rook") && OtherPiece.isBlack == movedPiece.isBlack)
                    {
                        turnDescription += " and castles";
                        special = -2;
                    }
                    else if (movedPiece.pieceName.Contains("Pawn") && OtherPiece.pieceName.Contains("Pawn") && OtherPiece.isBlack != movedPiece.isBlack
                                                                                               && !myController.CompareArrays(specialCoords, to))
                    {
                        turnDescription += " and captures " + OtherPiece.pieceName + " en passant";
                        index = (Array.IndexOf(myController.GetPieces(), OtherPiece));
                        special = -3;
                    }
                    else
                        Debug.Log((movedPiece.pieceName.Contains("Pawn")) + ", " + (OtherPiece.pieceName.Contains("Pawn")) +", "+ (OtherPiece.isBlack != movedPiece.isBlack)
                                                                                               + ", " + (!myController.CompareArrays(specialCoords, to)));
                }
            }
            if (movedPiece.pieceName.Contains("Pawn") && (myController.GetXCoord(to) == 0 || myController.GetXCoord(to) == myController.GetXCoord(myController.boardDimensions) - myController.forwardsDimensions))
                turnDescription += " and promotes";
            turnDescription += ".";
        }
        else //A manual control move was used (other than teleport)
        {
            if (OtherPiece == null)
            {
                if (myController != null)
                    turnDescription += "Time flies by, it's now " + (myController.GetTurn() % 2 == 0 ? "White's Turn!" : "Black's Turn!");
                else
                    turnDescription += "Time flies by, it's now the other person's turn!";
            }
            else
            {
                if (myController.WithinRange(from) && !myController.WithinRange(To))
                {
                    special += OtherPiece.pieceType + (OtherPiece.isBlack ? 0 : myController.THINGTYPES);
                    turnDescription += OtherPiece.pieceName + " vanishes from ";
                    index = (Array.IndexOf(myController.GetPieces(), OtherPiece));
                    for (int i = 0; i < from.Length; i++)
                    {
                        if (i < 1)
                            turnDescription += (char)('a' + from[i]);
                        else
                            turnDescription += (from[i] + 1);
                        if (i != to.Length - 1)
                        {
                            turnDescription += ", ";
                        }
                    }
                    turnDescription += "!";
                }
                else if (!myController.WithinRange(from) && myController.WithinRange(To))
                {
                    turnDescription += OtherPiece.pieceName + " appears at ";
                    for (int i = 0; i < to.Length; i++)
                    {
                        if (i < 1)
                            turnDescription += (char)('a' + to[i]);
                        else
                            turnDescription += (to[i] + 1);
                        if (i != to.Length - 1)
                        {
                            turnDescription += ", ";
                        }
                    }
                    turnDescription += "!";
                }
                else if (myController.WithinRange(from) && myController.WithinRange(To))
                {
                    turnDescription += "Error record created with coords from ";
                    for (int i = 0; i < from.Length; i++)
                    {
                        if (i < 1)
                            turnDescription += (char)('a' + from[i]);
                        else
                            turnDescription += (from[i] + 1);
                        if (i != to.Length - 1)
                        {
                            turnDescription += ", ";
                        }
                    }
                    turnDescription += " to ";
                    for (int i = 0; i < to.Length; i++)
                    {
                        if (i < 1)
                            turnDescription += (char)('a' + to[i]);
                        else
                            turnDescription += (to[i] + 1);
                        if (i != to.Length - 1)
                        {
                            turnDescription += ", ";
                        }
                    }
                }
                else
                {
                    turnDescription += "Error record created with invalid coordinates";
                }
            }
        }
    }
    public TurnRecord(int[] From, int[] To, PieceInfo movedPiece, PieceInfo OtherPiece, bool THIS_DOESNT_GENERATE_A_DESCRIPTION_OR_WORK_WITH_MANUAL_CONTROL_MODE)//faster
    {
        ChessControllerND myController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        from = From;
        to = To;
        special = -1;
        specialCoords = new int[] { };
        turnDescription = "?";
        if (movedPiece != null)
        {
            if (OtherPiece != null)
            {
                capturedAge = OtherPiece.timesMoved;
                if (myController.CompareArrays(to, myController.GetPiecePosition(OtherPiece)))
                {
                    special = OtherPiece.pieceType + (OtherPiece.isBlack ? 0 : myController.THINGTYPES);
                }
                else
                {
                    if (movedPiece.pieceType == 4 && OtherPiece.pieceType == 1 && OtherPiece.isBlack == movedPiece.isBlack)
                    {
                        special = -2;
                    }
                    else if (movedPiece.pieceType == 0 && OtherPiece.pieceType == 0 && OtherPiece.isBlack != movedPiece.isBlack
                                                                                               && !myController.CompareArrays(myController.GetPiecePosition(OtherPiece), to))
                    {
                        special = -3;
                    }
                }
            }
            if (movedPiece.pieceName.Contains("Pawn") && (myController.GetXCoord(to) == 0 || myController.GetXCoord(to) == myController.GetXCoord(myController.boardDimensions) - myController.forwardsDimensions))
                turnDescription += "promote";
        }

    }
    public TurnRecord()
    {

    }
}