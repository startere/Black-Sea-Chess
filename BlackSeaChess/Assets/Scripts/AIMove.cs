using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove
{
    public ChessPiece Attacker { get; private set; }
    public ChessPiece Defender { get; private set; }
    public int AttackX { get; private set; }
    public int AttackY { get; private set; }
    public int Weight { get; private set; }

    public bool VulnerableInitially { get; private set; }
    public bool VulnerableAfterMove { get; private set; }
    private static string[] pieceTypes = new string[6] { "Pawn", "Knight", "Bishop", "Rook", "Queen", "King" };

    public AIMove(ChessPiece attacker, ChessPiece defender)
    {
        this.Attacker = attacker;
        this.AttackX = defender.CurrentX;
        this.AttackY = defender.CurrentY;
        this.Defender = defender;

        int[] positionHolder = new int[2] { attacker.CurrentX, attacker.CurrentY };

        this.VulnerableInitially = CheckVulnerable(false);

        Utils.RepositionPiece(attacker, defender);

        this.VulnerableAfterMove = CheckVulnerable(true);

        Utils.RepositionPiece(attacker, defender, positionHolder);

        this.Weight = GetWeight();
    }

    public AIMove(ChessPiece attacker, int attackX, int attackY, int weight = 0)
    {
        this.Attacker = attacker;
        this.AttackX = attackX;
        this.AttackY = attackY;
        this.Weight = weight;

        int[] positionHolder = new int[2] { attacker.CurrentX, attacker.CurrentY };

        this.VulnerableInitially = CheckVulnerable(false);
        
        ChessPiece displacedPiece = AILogic.Instance.TemporaryBoard[attackX, attackY];
        Utils.RepositionPiece(attacker, new int[] { attackX, attackY });

        this.VulnerableAfterMove = CheckVulnerable(true);

        Utils.RepositionPiece(attacker, new int[] { attackX, attackY }, positionHolder);
        Utils.UpdateSlot(attackX, attackY, displacedPiece);
    }

    private bool CheckVulnerable(bool afterMove)
    {
        bool vulnerable = false;
        AILogic.Instance.RefreshBoardData();

        for (int i = 0; i < Utils.GetDestinations("White").Count; i++)
        {
            if (this.Attacker.CurrentX == AILogic.Instance.HumanDestinations[i][0] && this.Attacker.CurrentY == AILogic.Instance.HumanDestinations[i][1]) vulnerable = true;

            if (!afterMove || AILogic.Instance.TemporaryBoard[AILogic.Instance.HumanDestinations[i][0], AILogic.Instance.HumanDestinations[i][1]] == null) continue;

            if (AILogic.Instance.TemporaryBoard[AILogic.Instance.HumanDestinations[i][0], AILogic.Instance.HumanDestinations[i][1]].isWhite) continue;

            switch (AILogic.Instance.TemporaryBoard[AILogic.Instance.HumanDestinations[i][0], AILogic.Instance.HumanDestinations[i][1]].type)
            {
                case "Pawn": this.Weight -= 1; break;
                case "Knight": this.Weight -= 2; break;
                case "Bishop": this.Weight -= 3; break;
                case "Rook": this.Weight -= 4; break;
                case "Queen": this.Weight -= 5; break;
            }
        }

        return vulnerable;
    }

    private int GetWeight()
    {
        int totalWeight = 0;

        totalWeight += GetPiecesWeight();

        if (VulnerableInitially) totalWeight++; else totalWeight--;

        if (VulnerableAfterMove) totalWeight--; else totalWeight++;

        return totalWeight;
    }

    private int GetPiecesWeight()
    {
        int piecesWeight = 0;

        for (int i = 0; i < pieceTypes.Length; i++) if (pieceTypes[i] == Attacker.type) piecesWeight -= i + 1;

        for (int i = 0; i < pieceTypes.Length; i++) if (pieceTypes[i] == Defender.type) piecesWeight += i + 1;
        if (Defender.type == "King") piecesWeight += 6;

        return piecesWeight;
    }

    public override string ToString()
    {
        int finalX = Defender != null ? Defender.CurrentX : AttackX;
        int finalY = Defender != null ? Defender.CurrentY : AttackY;

        return Attacker.CurrentX + ":" + Attacker.CurrentY + " "
            + finalX + ":" + finalY + " "
            + VulnerableInitially + " "
            + VulnerableAfterMove + " "
            + Weight;
    }
}