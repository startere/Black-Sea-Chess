using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour {
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    public bool isWhite;
    public bool hasMoves;

    public string type;

    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] IsMovePossible()
    {
        return new bool[8,8];
    }

    public override string ToString()
    {
        string color = isWhite ? "White" : "Black";
        return "type: " + type + color + ": " + CurrentX + "," + CurrentY + ";";
    }
}
