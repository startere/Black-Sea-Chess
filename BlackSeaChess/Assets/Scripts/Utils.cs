using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utils : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static List<AIMove> GetMoves(string color)
    {
        List<ChessPiece> movablePieces = GetMovablePieces(color);
        List<AIMove> moves = new List<AIMove>();

        for (int i = 0; i < movablePieces.Count; i++)
        {
            List<AIMove> pieceMoves = GetPieceMoves(movablePieces[i]);

            for (int j = 0; j < pieceMoves.Count; j++) moves.Add(pieceMoves[j]);
        }
        return moves;
    }

    public static AIMove ChooseBestMove(List<AIMove> moves)
    {
        List<AIMove> bestMoves;
        AIMove chosenMove = null;

        moves = moves.OrderByDescending(move => move.Weight).ToList();

        bestMoves = moves.FindAll(move => move.Weight == moves[0].Weight);

        if (bestMoves.Count > 0) chosenMove = bestMoves[Random.Range(0, bestMoves.Count)];

        return chosenMove;
    }

    public static List<int[]> GetDestinations(string color)
    {
        List<ChessPiece> movablePieces = GetMovablePieces(color);
        List<int[]> positions = new List<int[]>();

        for (int h = 0; h < movablePieces.Count; h++)
        {
            bool[,] allowedMoves = movablePieces[h].IsMovePossible();

            for (int i = 0; i < allowedMoves.GetLength(0); i++)
            {
                for (int j = 0; j < allowedMoves.GetLength(1); j++)
                {
                    if (!allowedMoves[i, j]) continue;
                    else positions.Add(new int[] { i, j });
                }
            }
        }
        return positions;
    }

    public static List<AIMove> GetPieceMoves(ChessPiece piece)
    {
        List<AIMove> pieceMoves = new List<AIMove>();

        bool[,] allowedMoves = piece.IsMovePossible();
        List<ChessPiece> enemyArmy = piece.isWhite ? AILogic.Instance.ArmyAI : AILogic.Instance.ArmyHuman;

        for (int i = 0; i < allowedMoves.GetLength(0); i++)
        {
            for (int j = 0; j < allowedMoves.GetLength(1); j++)
            {
                if (!allowedMoves[i, j]) continue;

                if (!HasEnemyUnit(enemyArmy, new int[] { i, j })) continue;

                if (piece.type != "King")
                {
                    bool moveThreatensGeneral = CheckGeneralThreatened(piece, i, j);
                    if (moveThreatensGeneral) continue;
                }

                ChessPiece attackedPiece = enemyArmy.Find(enemyPiece => enemyPiece.CurrentX == i && enemyPiece.CurrentY == j);
                AIMove newMove = new AIMove(piece, attackedPiece);
                if (piece.type == "King" && newMove.VulnerableAfterMove) continue;

                pieceMoves.Add(newMove);
            }
        }
        return pieceMoves;
    }

    public static List<AIMove> GetPieceDestinations(ChessPiece piece)
    {
        List<AIMove> pieceMoves = new List<AIMove>();

        bool[,] allowedMoves = piece.IsMovePossible();

        for (int i = 0; i < allowedMoves.GetLength(0); i++)
        {
            for (int j = 0; j < allowedMoves.GetLength(1); j++)
            {
                if (!allowedMoves[i, j]) continue;

                if (piece.type != "King")
                {
                    bool moveThreatensGeneral = CheckGeneralThreatened(piece, i, j);
                    if (moveThreatensGeneral) continue;
                }

                AIMove newMove = new AIMove(piece, i, j);
                if (piece.type == "King" && newMove.VulnerableAfterMove) continue;

                pieceMoves.Add(newMove);
            }
        }
        return pieceMoves;
    }

    public static bool CheckGeneralThreatened(ChessPiece pieceToMove, int destinationX, int destinationY)
    {
        bool threatened = false;
        ChessPiece general = AILogic.Instance.ArmyAI.Find(p => p.type == "King");
        AILogic.Instance.RefreshBoardData();
        ChessPiece destinationPiece = AILogic.Instance.TemporaryBoard[destinationX, destinationY];
        if (destinationPiece != null && destinationPiece.type == "King") return threatened; // attack enemy king directly if possible even if white king threatened

        UpdateSlot(destinationX, destinationY, pieceToMove);
        int[] positionHolder = new int[] { pieceToMove.CurrentX, pieceToMove.CurrentY };
        UpdateSlot(positionHolder[0], positionHolder[1]);
        pieceToMove.SetPosition(destinationX, destinationY);
        AILogic.Instance.RefreshBoardData();

        for (int i = 0; i < AILogic.Instance.HumanDestinations.Count; i++)
        {
            if (pieceToMove.type != "King")
            {
                if (AILogic.Instance.HumanDestinations[i][0] == general.CurrentX && AILogic.Instance.HumanDestinations[i][1] == general.CurrentY) threatened = true;
            }
            else if (AILogic.Instance.HumanDestinations[i][0] == destinationX && AILogic.Instance.HumanDestinations[i][1] == destinationY) threatened = true;
        }

        if (!destinationPiece)
        {
            UpdateSlot(destinationX, destinationY);
        }
        else UpdateSlot(destinationX, destinationY, destinationPiece);

        UpdateSlot(positionHolder[0], positionHolder[1], pieceToMove);
        pieceToMove.SetPosition(positionHolder[0], positionHolder[1]);
        AILogic.Instance.RefreshBoardData();

        return threatened;
    }

    public static bool CheckPointSafe(int moveX, int moveY)
    {
        bool safe = true;
        AILogic.Instance.RefreshBoardData();
        for (int i = 0; i < AILogic.Instance.HumanDestinations.Count; i++)
        {
            if (AILogic.Instance.HumanDestinations[i][0] == moveX && AILogic.Instance.HumanDestinations[i][1] == moveY) safe = false;
        }

        return safe;
    }

    public static bool HasEnemyUnit(List<ChessPiece> enemyArmy, int[] pos)
    {
        for (int i = 0; i < enemyArmy.Count; i++)
        {
            if (pos[0] == enemyArmy[i].CurrentX && pos[1] == enemyArmy[i].CurrentY) return true;
        }

        return false;
    }

    public static void RepositionPiece(ChessPiece initial, ChessPiece final, int[] positionHolder = null)
    {
        if (positionHolder == null)
        {
            UpdateSlot(final.CurrentX, final.CurrentY, initial);
            initial.SetPosition(final.CurrentX, final.CurrentY);
        }
        else
        {
            UpdateSlot(final.CurrentX, final.CurrentY, final);
            initial.SetPosition(positionHolder[0], positionHolder[1]);
        }
        AILogic.Instance.RefreshBoardData();
    }

    public static void RepositionPiece(ChessPiece initial, int[] finalPos, int[] positionHolder = null)
    {
        if (positionHolder == null)
        {
            UpdateSlot(finalPos[0], finalPos[1], initial);
            initial.SetPosition(finalPos[0], finalPos[1]);
        }
        else
        {
            UpdateSlot(finalPos[0], finalPos[1], null);
            UpdateSlot(positionHolder[0], positionHolder[1], initial);
            initial.SetPosition(positionHolder[0], positionHolder[1]);
        }
        AILogic.Instance.RefreshBoardData();
    }

    public static void RemovePiece(ChessPiece pieceToRemove)
    {
        UpdateSlot(pieceToRemove.CurrentX, pieceToRemove.CurrentY, null);
    }

    public static void UpdateSlot(int x, int y, ChessPiece piece = null)
    {
        AILogic.Instance.TemporaryBoard[x, y] = piece;
    }

    public static List<ChessPiece> GetPlayerArmy(string color)
    {
        var army = new List<ChessPiece>();

        for (int i = 0; i < BoardManager.Instance.boardDimension; i++)
        {
            for (int j = 0; j < BoardManager.Instance.boardDimension; j++)
            {
                if (!AILogic.Instance.TemporaryBoard[i, j]) continue;

                bool isBlack = !AILogic.Instance.TemporaryBoard[i, j].isWhite && color == "Black";
                bool isWhite = AILogic.Instance.TemporaryBoard[i, j].isWhite && color == "White";

                if (isBlack || isWhite) army.Add(AILogic.Instance.TemporaryBoard[i, j]);
            }
        }

        return army;
    }

    public static List<ChessPiece> GetMovablePieces(string color)
    {
        List<ChessPiece> army = GetPlayerArmy(color);
        List<ChessPiece> movablePieces = new List<ChessPiece>();

        for (int i = 0; i < army.Count; i++)
        {
            if (CheckHasMoves(army[i])) movablePieces.Add(army[i]);
        }

        return movablePieces;
    }

    public static string ArmyToString(string color)
    {
        List<ChessPiece> army = GetPlayerArmy(color);
        string output = color + " army: ";

        for (int i = 0; i < army.Count; i++) output += army[i].type + ":" + army[i].CurrentX + "," + army[i].CurrentY + ";";

        return output;
    }

    public static string PiecesToString(List<ChessPiece> pieces)
    {
        string output = "";

        for (int i = 0; i < pieces.Count; i++) output += pieces[i].type + ": " + pieces[i].CurrentX + "," + pieces[i].CurrentY + ";";

        return output;
    }

    public static string MovesToString(List<AIMove> moves)
    {
        string output = "";

        for (int i = 0; i < moves.Count; i++) output += moves[i].ToString() + "; ";

        return output;
    }

    public static string DestinationToString(int[] destination)
    {
        return "destination: " + destination[0] + "," + destination[1];
    }

    public static string DestinationsToString(List<int[]> destinations)
    {
        string output = "";

        for (int i = 0; i < destinations.Count; i++) output += destinations[i][0] + "," + destinations[i][1] + "|";

        return output;
    }

    public static bool CheckHasMoves(ChessPiece piece)
    {
        bool hasMoves = false;

        bool[,] allowedMoves = piece.IsMovePossible();

        for (int i = 0; i < allowedMoves.GetLength(0); i++)
        {
            for (int j = 0; j < allowedMoves.GetLength(1); j++)
            {
                if (allowedMoves[i, j])
                {
                    hasMoves = true;
                }
            }
        }

        return hasMoves;
    }

    public static string AllowedMovesToString(bool[,] allowedMoves)
    {
        string output = "";

        for (int i = 0; i < allowedMoves.GetLength(0); i++)
        {
            for (int j = 0; j < allowedMoves.GetLength(1); j++)
            {
                output += "move:" + i + "," + j + "-" + allowedMoves[i, j] + "; ";
            }
        }

        return output;
    }

    public static string BoardToString()
    {
        string output = "";

        for (int i = 0; i < BoardManager.Instance.boardDimension; i++)
        {
            for (int j = 0; j < BoardManager.Instance.boardDimension; j++)
            {
                output += AILogic.Instance.TemporaryBoard[i, j].type + ";";
            }
        }

        return output;
    }
}
