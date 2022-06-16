using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Numerics;

//This is a class that's made to hold the Piece Info, and to efficiently search from any data type to any other
//This may mean having several redundant arrays of pointers that are kept in various sorted orders

//this clas _should_ allow for the piece info to be found from Index, location, 
//there's a chance I'll have to make this use pointers if the dictionaries are storing separate sets of PieceInfo
public class StorageObject
{
    public PieceInfo[] pieces = new PieceInfo[80];
    private int[] boardSize;
    private Dictionary<BigInteger, PieceInfo> atCoords = new Dictionary<BigInteger, PieceInfo>();//linearizes the coordinates to put in a dict
    private Dictionary<int, PieceInfo> idToPiece= new Dictionary<int, PieceInfo>();//access a piece by it's ID
    public StorageObject(int[] boardSize)
    {
        this.boardSize = boardSize;
        InitializeDicts();
    }
    public StorageObject(PieceInfo[] pieces, int[] boardSize)
    {
        this.pieces = pieces;
        this.boardSize = boardSize;
        InitializeDicts();
    }
    public void Insert(PieceInfo piece, int index, int[] pos)
    {
        pieces[index] = piece;
        pieces[index].position = pos;
        pieces[index].index = index;
        //Debug.Log("Adding " + GetPosIndex(pieces[pieceID].position));
        atCoords.Add(GetPosIndex(pieces[index].position), pieces[index]);
    }
    public void RemoveAt(int index)
    {
        //Debug.Log("Removing " + GetPosIndex(pieces[index].position));
        atCoords.Remove(GetPosIndex(pieces[index].position));
        pieces[index] = null;   
    }
    public void Remove(PieceInfo piece)
    {
        for (int i = 0; i < pieces.Length; i++)
            if (pieces[i] == piece)//shallow comparison is right here
            {
                //Debug.Log("Removing " + GetPosIndex(piece.position));
                atCoords.Remove(GetPosIndex(piece.position));
                pieces[i] = null;
                return;
            }
    }
    private void InitializeDicts()
    {
        for (int i=0;i<pieces.Length;i++)
        {
            if(pieces[i]!=null)
            {
                atCoords.Add(GetPosIndex(pieces[i].position), pieces[i]);
            }
        }
    }
    public PieceInfo GetPieceAt(int[] pos)
    {
        if (atCoords.ContainsKey(GetPosIndex(pos))) 
            return atCoords[GetPosIndex(pos)];
        return null;
    }
    public void MovePiece(PieceInfo piece, int[] newPos)
    {
        //Debug.Log("replacing "+GetPosIndex(piece.position)+" with "+GetPosIndex(newPos));
        atCoords.Remove(GetPosIndex(piece.position));
        piece.position = newPos;
        atCoords.Add(GetPosIndex(newPos), piece);
    }
    public BigInteger GetPosIndex( int[] pieceCoords)
    {
        BigInteger index = 0;
        for(int i=0;i<boardSize.Length;i++)
        {
            index *= boardSize[boardSize.Length - i - 1];
            for (int j = 0; j < pieceCoords[boardSize.Length-i-1]; j++)
            {
                index++;
            }
        }
        //Debug.Log("Piece at index: "+index);
        return index;
    }
}