using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardAssessmentAlgorithm : MonoBehaviour
{
    System.Random myRandom = new System.Random();
    public float AssessBoard(int[] boardDimensions, PieceInfo[] Pieces, bool blacksTurn)
    {
        float currentScore = 0;
        
        int multipier;
        bool hasWhiteKing = false, hasBlackKing = false;
        for (int i = 0; i < Pieces.Length; i++)
        {
            if (Pieces[i] != null)
            {
                multipier = Pieces[i].isBlack ? -1 : 1;
                switch (Pieces[i].pieceType)
                {

                    case 0:
                        currentScore += 1 * multipier;
                        break;
                    case 1:
                        currentScore += 5 * multipier;
                        break;
                    case 2:
                        currentScore += 3 * multipier;
                        break;
                    case 3:
                        currentScore += 4 * multipier;
                        break;
                    case 4:
                        if (Pieces[i].isBlack)
                            hasBlackKing = true;
                        else
                            hasWhiteKing = true;
                        currentScore += 300 * multipier;
                        break;
                    case 5:
                        currentScore += 8 * multipier;
                        break;
                    default:
                        break;
                }
            }
        }
        myRandom.NextDouble();
        //currentScore += (float)(myRandom.NextDouble()/1000);
        if(hasBlackKing == hasWhiteKing)
            return currentScore*(blacksTurn?-1:1);
        if (hasWhiteKing)
            return -500;
        else
            return 500;
    }
}
