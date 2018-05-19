using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AILogic : MonoBehaviour
{
    public static AILogic instance;

    public ChessPiece[,] TemporaryBoard { get; private set; }
    public List<int[]> HumanDestinations { get; private set; }
    public List<ChessPiece> ArmyAI { get; private set; }
    public List<ChessPiece> ArmyHuman { get; private set; }
    public bool IsAITurn { get; private set; }

    private ChessPiece chosenPiece;
    private AIMove chosenMove;
    private bool shouldCharge = false;
    private bool moveThreatensGeneral = false;
    private bool initialSequenceCompleted = false;

    private List<AIMove> movesAI;
    private List<ChessPiece> movablePiecesAI;
    private int[] chosenDestination;

    public static AILogic Instance
    {
        get { return instance ?? (instance = new GameObject("AILogic").AddComponent<AILogic>()); }
    }

    // Use this for initialization
    //private void Start()
    //{
    //    Instance = this;
    //}

    public void StartAITurn()
    {
        Debug.Log("========================================START AI TURN ========================================");
        OnMoveReset();

        TemporaryBoard = BoardManager.Instance.ChessPieces;
        ArmyHuman = Utils.GetPlayerArmy("White");
        ArmyAI = Utils.GetPlayerArmy("Black");
        movablePiecesAI = Utils.GetMovablePieces("Black");
        HumanDestinations = Utils.GetDestinations("White");
        movesAI = Utils.GetMoves("Black");

        HandleAIMove();
    }

    public void RefreshBoardData()
    {
        HumanDestinations = Utils.GetDestinations("White");
        ArmyHuman = Utils.GetPlayerArmy("White");
    }

    public void ToggleAITurn()
    {
        IsAITurn = !IsAITurn;
    }

    private void OnMoveReset()
    {
        BoardManager.Instance.selectedPiece = null;
        chosenDestination = null;
        chosenMove = null;
        chosenPiece = null;

        moveThreatensGeneral = false;
        shouldCharge = false;
    }

    private void HandleAIMove()
    {
        chosenPiece = GetPiece();

        if (chosenPiece != null) BoardManager.Instance.SelectPiece(chosenPiece.CurrentX, chosenPiece.CurrentY);

        if (chosenMove != null && (chosenMove.Weight > 0 || initialSequenceCompleted))
        {
            int destX = chosenMove.Defender != null ? chosenMove.Defender.CurrentX : chosenMove.AttackX;
            int destY = chosenMove.Defender != null ? chosenMove.Defender.CurrentY : chosenMove.AttackY;
            chosenDestination = new int[2] { destX, destY };
        }
        else if (!initialSequenceCompleted) chosenDestination = GetDestination();

        if (chosenDestination == null) RunForQueen();

        if (chosenDestination == null || (chosenDestination != null && Utils.CheckGeneralThreatened(chosenPiece, chosenDestination[0], chosenDestination[1]))) ChooseGeneralSafePoint();

        if (chosenDestination == null || (chosenDestination != null && Utils.CheckGeneralThreatened(chosenPiece, chosenDestination[0], chosenDestination[1]))) ProtectGeneral();
       
        ChessPiece general = ArmyAI.Find(piece => piece.type == "King");

        if (chosenDestination != null) BoardManager.Instance.MovePiece(chosenDestination[0], chosenDestination[1]);
        else
        {
            BoardManager.Instance.WhiteTurn();
            initialSequenceCompleted = false;
            BoardManager.Instance.EndGame();
        }
    }

    private ChessPiece GetPiece()
    {
        chosenMove = Utils.ChooseBestMove(movesAI);

        if (chosenMove != null && (chosenMove.Weight > 0 || initialSequenceCompleted)) chosenPiece = chosenMove.Attacker;
        else chosenPiece = GetInitialSequencePiece();

        if (chosenPiece == null)
        {
            List<AIMove> threateningMoves = Threaten();
            if (threateningMoves.Count > 0) chosenPiece = Utils.ChooseBestMove(threateningMoves).Attacker;
        }
        return chosenPiece;
    }

    private ChessPiece GetInitialSequencePiece()
    {
        List<ChessPiece> infantry = movablePiecesAI.FindAll(piece => piece.type == "Pawn");
        List<ChessPiece> heavyInf = movablePiecesAI.FindAll(piece => piece.type == "Bishop");
        List<ChessPiece> cavalry = movablePiecesAI.FindAll(piece => piece.type == "Knight");
        List<ChessPiece> artillery = movablePiecesAI.FindAll(piece => piece.type == "Rook");
        List<ChessPiece> general = movablePiecesAI.FindAll(piece => piece.type == "King");
        List<ChessPiece> companions = movablePiecesAI.FindAll(piece => piece.type == "Queen");

        ChessPiece chosenPiece = null;

        if (infantry.Count > 0) chosenPiece = CheckInfantry(infantry);
        if (heavyInf.Count > 0 && chosenPiece == null) chosenPiece = CheckHeavyInf(heavyInf);
        if (cavalry.Count > 0 && chosenPiece == null) chosenPiece = CheckCavalry(cavalry);
        if (artillery.Count > 0 && chosenPiece == null) chosenPiece = CheckArtillery(artillery);
        if (general.Count > 0 && chosenPiece == null) chosenPiece = CheckGeneral(general);
        if (companions.Count > 0 && chosenPiece == null) chosenPiece = CheckCompanions(companions);

        return chosenPiece;
    }

    private ChessPiece CheckInfantry(List<ChessPiece> infantry)
    {
        ChessPiece chosenPiece = null;
        bool frontLineUnmoved = true;
        List<ChessPiece> unmovedPawns = new List<ChessPiece>();
        List<ChessPiece> movedPawns = new List<ChessPiece>();

        for (int i = 0; i < infantry.Count; i++)
        {
            if (infantry[i].CurrentY != 6)
            {
                frontLineUnmoved = false;
                movedPawns.Add(infantry[i]);
            }
            else unmovedPawns.Add(infantry[i]);
        }

        if (frontLineUnmoved || unmovedPawns.Count > 4)
        {
            List<ChessPiece> centralFourPawns = unmovedPawns.FindAll(pawn => pawn.CurrentX > 1 && pawn.CurrentX < 6);
            if (centralFourPawns.Count > 0) chosenPiece = centralFourPawns[Random.Range(0, centralFourPawns.Count)];
        }
        else
        {
            List<ChessPiece> centralTwoPawns = movedPawns.FindAll(pawn => pawn.CurrentX > 2 && pawn.CurrentX < 5 && pawn.CurrentY != 4); // select two central inf, not if at y 4
            if (centralTwoPawns.Count > 0) chosenPiece = centralTwoPawns[Random.Range(0, centralTwoPawns.Count)];
        }

        if (unmovedPawns.Count <= 4)
        {
            List<ChessPiece> flanks = unmovedPawns.FindAll(pawn => pawn.CurrentX == 0 || pawn.CurrentX == 7); // select unmoved flanks
            if (flanks.Count > 0) chosenPiece = flanks[Random.Range(0, flanks.Count)];
        }

        return chosenPiece;
    }

    private ChessPiece CheckHeavyInf(List<ChessPiece> heavyInf)
    {
        ChessPiece chosenPiece = null;

        List<ChessPiece> unmovedHeavyInf = new List<ChessPiece>();

        for (int i = 0; i < heavyInf.Count; i++)
        {
            if (heavyInf[i].CurrentY == 7) unmovedHeavyInf.Add(heavyInf[i]);
        }

        if (unmovedHeavyInf.Count > 0) chosenPiece = unmovedHeavyInf[Random.Range(0, unmovedHeavyInf.Count)];

        return chosenPiece;
    }

    private ChessPiece CheckCavalry(List<ChessPiece> cavalry)
    {
        ChessPiece chosenPiece = null;

        return chosenPiece;
    }

    private ChessPiece CheckArtillery(List<ChessPiece> artillery)
    {
        ChessPiece chosenPiece = null;

        return chosenPiece;
    }

    private ChessPiece CheckGeneral(List<ChessPiece> general)
    {
        ChessPiece chosenPiece = null;

        if (general[0].CurrentY == 7) chosenPiece = general[0];

        return chosenPiece;
    }

    private ChessPiece CheckCompanions(List<ChessPiece> companions)
    {
        ChessPiece chosenPiece = null;

        if (companions[0].CurrentY == 7) chosenPiece = companions[0];

        return chosenPiece;
    }

    private List<AIMove> Threaten()
    {
        List<AIMove> threateningMoves = new List<AIMove>();

        for (int h = 0; h < movablePiecesAI.Count; h++)
        {
            bool[,] AIPieceMoves = movablePiecesAI[h].IsMovePossible();
            int[] positionHolder = new int[] { movablePiecesAI[h].CurrentX, movablePiecesAI[h].CurrentY };

            for (int i = 0; i < AIPieceMoves.GetLength(0); i++)
            {
                for (int j = 0; j < AIPieceMoves.GetLength(1); j++)
                {
                    if (!AIPieceMoves[i, j]) continue;

                    ChessPiece targetPiece = TemporaryBoard[i, j];
                   
                    Utils.UpdateSlot(movablePiecesAI[h].CurrentX, movablePiecesAI[h].CurrentY);
                    Utils.RepositionPiece(movablePiecesAI[h], new int[] { i, j });

                    int totalWeight = 0;
                    List<AIMove> pieceMoves = Utils.GetPieceMoves(movablePiecesAI[h]);

                    for (int k = 0; k < pieceMoves.Count; k++)
                    {
                        totalWeight += pieceMoves[k].Weight;
                    }

                    bool moveThreatensGeneral = false;
                    if (movablePiecesAI[h].type != "King") moveThreatensGeneral = Utils.CheckGeneralThreatened(movablePiecesAI[h], i, j);

                    if (!moveThreatensGeneral) threateningMoves.Add(new AIMove(movablePiecesAI[h], i, j, totalWeight));
                    Utils.UpdateSlot(i, j, targetPiece);
                    movablePiecesAI[h].SetPosition(positionHolder[0], positionHolder[1]);
                    Utils.UpdateSlot(positionHolder[0], positionHolder[1], movablePiecesAI[h]);
                    RefreshBoardData();
                }
            }
            Utils.RepositionPiece(movablePiecesAI[h], positionHolder);
        }
        return threateningMoves;
    }

    private int[] RunForQueen()
    {
        movablePiecesAI = Utils.GetMovablePieces("Black");
        List<ChessPiece> infantry = ArmyAI.FindAll(piece => piece.type == "Pawn");
        if (infantry.Count == 0) return chosenDestination = null;

        List<ChessPiece> movableInfantry = GetCandidatesForQueen(infantry);
        if (movableInfantry.Count == 0) return chosenDestination = null;

        movableInfantry = movableInfantry.OrderBy(piece => piece.CurrentY).ToList();                    // get frontmost movable pawn to promote to queen
        BoardManager.Instance.SelectPiece(movableInfantry[0].CurrentX, movableInfantry[0].CurrentY);
        List<AIMove> pieceMoves = Utils.GetPieceDestinations(movableInfantry[0]);
        if (pieceMoves.Count == 0) return chosenDestination = null;

        chosenPiece = movableInfantry[0];
        pieceMoves = pieceMoves.OrderBy(m => m.AttackY).ToList();
        return chosenDestination = new int[] { pieceMoves[0].AttackX, pieceMoves[0].AttackY };
    }

    private List<ChessPiece> GetCandidatesForQueen(List<ChessPiece> infantry)
    {
        List<ChessPiece> movableInfantry = new List<ChessPiece>();

        for (int h = 0; h < infantry.Count; h++)
        {
            if (!Utils.CheckHasMoves(infantry[h])) continue;

            bool wayClear = true;
            for (int y = infantry[h].CurrentY - 1; y >= 0; y--)
            {
                if (TemporaryBoard[infantry[h].CurrentX, y] != null) wayClear = false;   // filter for only pawns whose columns are clear
            }
            if (wayClear) movableInfantry.Add(infantry[h]);
        };

        return movableInfantry;
    }

    private void ProtectGeneral()
    {
        chosenMove = Utils.ChooseBestMove(movesAI);

        if (chosenMove == null) return;

        BoardManager.Instance.SelectPiece(chosenMove.Attacker.CurrentX, chosenMove.Attacker.CurrentY);
        chosenDestination = new int[] {
            chosenMove.Defender != null ? chosenMove.Defender.CurrentX : chosenMove.AttackX,
            chosenMove.Defender != null ? chosenMove.Defender.CurrentY : chosenMove.AttackY
        };
    }

    private int[] GetDestination()
    {
        List<int[]> destinations = new List<int[]>();
        bool[,] allowedMoves = BoardManager.Instance.selectedPiece.IsMovePossible();

        for (int i = 0; i < allowedMoves.GetLength(0); i++)
        {
            for (int j = 0; j < allowedMoves.GetLength(1); j++)
            {
                if (!allowedMoves[i, j]) continue;

                int[] newDestination = new int[2];
                newDestination[0] = i;
                newDestination[1] = j;

                switch (BoardManager.Instance.selectedPiece.type)
                {
                    case "Pawn":
                        if (!shouldCharge)
                        {
                            if (BoardManager.Instance.selectedPiece.CurrentY - j > 1) continue; // move only one inf step
                        }
                        break;
                    case "Bishop": if (j != 5) continue; break; // move heavyInf in front of general
                    case "King": if (i != 4) continue; break; // move general behind heavy infantry
                    case "Queen":
                        if (i != 3) continue; // move companions behind heavy infantry

                        initialSequenceCompleted = true;
                        break;
                }

                if (BoardManager.Instance.selectedPiece.type != "King")
                {
                    moveThreatensGeneral = Utils.CheckGeneralThreatened(BoardManager.Instance.selectedPiece, i, j);
                    if (moveThreatensGeneral) continue;
                }

                destinations.Add(newDestination);
            }
        }

        if (destinations.Count > 0) return destinations[Random.Range(0, destinations.Count)];
        else return null;
    }

    private void ChooseGeneralSafePoint()
    {
        ChessPiece general = ArmyAI.Find(piece => piece.type == "King");
        int[] positionHolder = new int[] { general.CurrentX, general.CurrentY };
        List<int[]> safeDestinations = new List<int[]>();
        BoardManager.Instance.SelectPiece(general.CurrentX, general.CurrentY);
        chosenPiece = general;

        List<AIMove> moves = Utils.GetPieceDestinations(general);

        for (int i = 0; i < moves.Count; i++)
        {
            ChessPiece destinationPiece = TemporaryBoard[moves[i].AttackX, moves[i].AttackY];
            Utils.UpdateSlot(general.CurrentX, general.CurrentY);
            Utils.RepositionPiece(general, new int[] { moves[i].AttackX, moves[i].AttackY });


            if (Utils.CheckPointSafe(moves[i].AttackX, moves[i].AttackY)) safeDestinations.Add(new int[] { moves[i].AttackX, moves[i].AttackY });

            general.SetPosition(positionHolder[0], positionHolder[1]);
            Utils.UpdateSlot(positionHolder[0], positionHolder[1], general);
            Utils.UpdateSlot(moves[i].AttackX, moves[i].AttackY, destinationPiece);
            RefreshBoardData();
        }
        chosenDestination = safeDestinations.Count > 0 ? safeDestinations[Random.Range(0, safeDestinations.Count)] : null;
    }
}
