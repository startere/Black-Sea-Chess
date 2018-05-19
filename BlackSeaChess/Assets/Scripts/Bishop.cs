using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece {
    public override bool[,] IsMovePossible()
    {
        bool[,] allowedMoves = new bool[8, 8];

        ChessPiece destinationPiece;

        int i, j;

        //TopLeft
        i = CurrentX;
        j = CurrentY;

        while (true)
        {
            i--;
            j++;

            if (i < 0 || j >= 8)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, j];

            if (destinationPiece == null)
            {
                allowedMoves[i, j] = true;
            } else
            {
                if (isWhite != destinationPiece.isWhite)
                {
                    allowedMoves[i, j] = true;
                }
                break;
            }
        }

        //TopRight
        i = CurrentX;
        j = CurrentY;

        while (true)
        {
            i++;
            j++;

            if (i >= 8 || j >= 8)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, j];

            if (destinationPiece == null)
            {
                allowedMoves[i, j] = true;
            }
            else
            {
                if (isWhite != destinationPiece.isWhite)
                {
                    allowedMoves[i, j] = true;
                }
                break;
            }
        }

        //DownLeft
        i = CurrentX;
        j = CurrentY;

        while (true)
        {
            i--;
            j--;

            if (i < 0 || j < 0)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, j];

            if (destinationPiece == null)
            {
                allowedMoves[i, j] = true;
            }
            else
            {
                if (isWhite != destinationPiece.isWhite)
                {
                    allowedMoves[i, j] = true;
                }
                break;
            }
        }

        //DownRight
        i = CurrentX;
        j = CurrentY;

        while (true)
        {
            i++;
            j--;

            if (i >= 8 || j < 0)
            {
                break;
            }

            destinationPiece = BoardManager.Instance.ChessPieces[i, j];

            if (destinationPiece == null)
            {
                allowedMoves[i, j] = true;
            }
            else
            {
                if (isWhite != destinationPiece.isWhite)
                {
                    allowedMoves[i, j] = true;
                }
                break;
            }
        }

        return allowedMoves;
    }
}
