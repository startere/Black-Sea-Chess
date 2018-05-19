using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece {
    public override bool[,] IsMovePossible()
    {
        bool[,] allowedMoves = new bool[8, 8];

        //UpLeft
        KnightMove(CurrentX - 1, CurrentY + 2, ref allowedMoves);

        //UpRight 
        KnightMove(CurrentX + 1, CurrentY + 2, ref allowedMoves);

        //RightUp
        KnightMove(CurrentX + 2, CurrentY + 1, ref allowedMoves);

        //RightDown 
        KnightMove(CurrentX + 2, CurrentY - 1, ref allowedMoves);

        //DownLeft
        KnightMove(CurrentX - 1, CurrentY - 2, ref allowedMoves);

        //DownRight 
        KnightMove(CurrentX + 1, CurrentY - 2, ref allowedMoves);

        //LeftUp
        KnightMove(CurrentX - 2, CurrentY + 1, ref allowedMoves);

        //LeftDown 
        KnightMove(CurrentX - 2, CurrentY - 1, ref allowedMoves);

        return allowedMoves;
    }

    public void KnightMove(int x, int y, ref bool[,] allowedMoves)
    {
        ChessPiece destinationPiece;

        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            destinationPiece = BoardManager.Instance.ChessPieces[x, y];

            if (destinationPiece == null)
            {
                allowedMoves[x, y] = true;
            }
            else if (isWhite != destinationPiece.isWhite)
            {
                allowedMoves[x, y] = true;
            }
        }

    }
}
