using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece {
    
    public override bool[,] IsMovePossible()
    {
        bool[,] allowedMoves = new bool[8, 8];

        ChessPiece destinationPiece;
        int i;

        //Right
        i = CurrentX;
        while (true)
        {
            i++;
            if (i >= 8)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, CurrentY];

            if (destinationPiece == null)
            {
                allowedMoves[i, CurrentY] = true;
            }
            else
            {
                if (destinationPiece.isWhite != isWhite)
                {
                    allowedMoves[i, CurrentY] = true;
                }
                break;
            }
        }

        //Left
        i = CurrentX;
        while (true)
        {
            i--;
            if (i < 0)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, CurrentY];

            if (destinationPiece == null)
            {
                allowedMoves[i, CurrentY] = true;
            }
            else
            {
                if (destinationPiece.isWhite != isWhite)
                {
                    allowedMoves[i, CurrentY] = true;
                }
                break;
            }
        }

        //Up
        i = CurrentY;
        while (true)
        {
            i++;
            if (i >= 8)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[CurrentX, i];

            if (destinationPiece == null)
            {
                allowedMoves[CurrentX, i] = true;
            }
            else
            {
                if (destinationPiece.isWhite != isWhite)
                {
                    allowedMoves[CurrentX, i] = true;
                }
                break;
            }
        }

        //Down
        i = CurrentY;
        while (true)
        {
            i--;
            if (i < 0)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[CurrentX, i];

            if (destinationPiece == null)
            {
                allowedMoves[CurrentX, i] = true;
            }
            else
            {
                if (destinationPiece.isWhite != isWhite)
                {
                    allowedMoves[CurrentX, i] = true;
                }
                break;
            }
        }
        return allowedMoves;
    }
}
