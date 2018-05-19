using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece {

    public override bool[,] IsMovePossible()
    {
        bool[,] allowedMoves = new bool[8, 8];
        ChessPiece destinationPiece1, destinationPiece2;
        int[] enPassantHolder = BoardManager.Instance.EnPassantMoves;

        // White team pawn moves
        if (isWhite)
        {
            //diagonal left
            if (CurrentX != 0 && CurrentY != 7)
            {
                if (enPassantHolder[0] == CurrentX - 1 && enPassantHolder[1] == CurrentY + 1)
                {
                    allowedMoves[CurrentX - 1, CurrentY + 1] = true;
                }

                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX - 1, CurrentY + 1];
                if (destinationPiece1 != null && !destinationPiece1.isWhite)
                {
                    allowedMoves[CurrentX - 1, CurrentY + 1] = true;
                }
            }

            //diagonal right
            if (CurrentX != 7 && CurrentY != 7)
            {
                if (enPassantHolder[0] == CurrentX + 1 && enPassantHolder[1] == CurrentY + 1)
                {
                    allowedMoves[CurrentX + 1, CurrentY + 1] = true;
                }

                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX + 1, CurrentY + 1];
                if (destinationPiece1 != null && !destinationPiece1.isWhite)
                {
                    allowedMoves[CurrentX + 1, CurrentY + 1] = true;
                }
            }

            //middle
            if (CurrentY != 7)
            {
                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY + 1];
                if (destinationPiece1 == null)
                {
                    allowedMoves[CurrentX, CurrentY + 1] = true;
                }
            }

            //charge!
            if (CurrentY == 1)
            {
                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY + 1];
                destinationPiece2 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY + 2];
                if (destinationPiece1 == null & destinationPiece2 == null)
                {
                    allowedMoves[CurrentX, CurrentY + 2] = true;
                }
            }
        }
        else // black team pawn moves
        {

            //diagonal left
            if (CurrentX != 0 && CurrentY != 0)
            {
                if (enPassantHolder[0] == CurrentX - 1 && enPassantHolder[1] == CurrentY - 1)
                {
                    allowedMoves[CurrentX - 1, CurrentY - 1] = true;
                }

                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX - 1, CurrentY - 1];
                if (destinationPiece1 != null && destinationPiece1.isWhite)
                {
                    allowedMoves[CurrentX - 1, CurrentY - 1] = true;
                }
            }

            //diagonal right
            if (CurrentX != 7 && CurrentY != 0)
            {
                if (enPassantHolder[0] == CurrentX + 1 && enPassantHolder[1] == CurrentY - 1)
                {
                    allowedMoves[CurrentX + 1, CurrentY - 1] = true;
                }

                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX + 1, CurrentY - 1];
                if (destinationPiece1 != null && destinationPiece1.isWhite)
                {
                    allowedMoves[CurrentX + 1, CurrentY - 1] = true;
                }
            }

            //middle
            if (CurrentY != 0)
            {
                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY - 1];
                if (destinationPiece1 == null)
                {
                    allowedMoves[CurrentX, CurrentY - 1] = true;
                }
            }

            //initial jump
            if (CurrentY == 6)
            {
                destinationPiece1 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY - 1];
                destinationPiece2 = BoardManager.Instance.ChessPieces[CurrentX, CurrentY - 2];
                if (destinationPiece1 == null & destinationPiece2 == null)
                {
                    allowedMoves[CurrentX, CurrentY - 2] = true;
                }
            }
        }
        return allowedMoves;
    }

}
