using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece {

    public override bool[,] IsMovePossible()
    {
        bool[,] allowedMoves = new bool[8, 8];

        ChessPiece destinationPiece;

        int i, j;

        //Up
        i = CurrentX - 1;
        j = CurrentY + 1;
        if (CurrentY != 7)
        {
            for (int k = 0; k < 3; k++)
            {
                if (i >= 0 && i < 8)
                {
                    destinationPiece = BoardManager.Instance.ChessPieces[i, j];
                    if (destinationPiece == null)
                    {
                        allowedMoves[i, j] = true;
                    }
                    else if (isWhite != destinationPiece.isWhite)
                    {
                        allowedMoves[i, j] = true;
                    }
                }
                i++;
            }
        }

        //Down
        i = CurrentX - 1;
        j = CurrentY - 1;
        if (CurrentY != 0)
        {
            for (int k = 0; k < 3; k++)
            {
                if (i >= 0 && i < 8)
                {
                    destinationPiece = BoardManager.Instance.ChessPieces[i, j];
                    if (destinationPiece == null)
                    {
                        allowedMoves[i, j] = true;
                    }
                    else if (isWhite != destinationPiece.isWhite)
                    {
                        allowedMoves[i, j] = true;
                    }
                }
                i++;
            }
        }

        //Left
        if (CurrentX != 0)
        {
            destinationPiece = BoardManager.Instance.ChessPieces[CurrentX - 1, CurrentY];
            if (destinationPiece == null)
            {
                allowedMoves[CurrentX - 1, CurrentY] = true;
            }
            else if (isWhite != destinationPiece.isWhite)
            {
                allowedMoves[CurrentX - 1, CurrentY] = true;
            }
        }

        //Right
        if (CurrentX != 7)
        {
            destinationPiece = BoardManager.Instance.ChessPieces[CurrentX + 1, CurrentY];
            if (destinationPiece == null)
            {
                allowedMoves[CurrentX + 1, CurrentY] = true;
            }
            else if (isWhite != destinationPiece.isWhite)
            {
                allowedMoves[CurrentX + 1, CurrentY] = true;
            }
        }


        return allowedMoves;
    }
}
