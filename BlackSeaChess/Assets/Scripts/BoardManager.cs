using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    public static BoardManager Instance { get; set; }
    public bool[,] allowedMoves;

    public ChessPiece[,] ChessPieces { get; set; }
    public ChessPiece selectedPiece;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;
    public int boardDimension = 8;

    public List<GameObject> chessPiecePrefabs;
    private List<GameObject> alivePieces;

    public int[] EnPassantMoves { set; get; }

    private Quaternion pieceOrientation = Quaternion.Euler(0, 180, 0);

    public bool isWhiteTurn = true;

    private void Start()
    {
        Instance = this;
        SpawnAllChessPieces();
    }

    private void Update()
    {
        UpdateSelection();
        DrawChessboard();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedPiece == null)
                {
                    SelectPiece(selectionX, selectionY);
                }
                else
                {
                    MovePiece(selectionX, selectionY);
                }
            }
        }
    }

    public void SelectPiece(int x, int y)
    {   
        if (ChessPieces[x, y] == null)
        {
            return;
        }

        if (ChessPieces[x, y].isWhite != isWhiteTurn)
        {
            return;
        }

        CheckForMoves(ChessPieces[x, y]);

        if (!ChessPieces[x, y].hasMoves)
        {
            return;
        }

        selectedPiece = ChessPieces[x, y];
        BoardHighlighting.Instance.Highlight(allowedMoves);
    }

    public void CheckForMoves(ChessPiece piece)
    {
        bool hasAtLeastOneMove = false;
        allowedMoves = piece.IsMovePossible();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                }
            }
        }

        piece.hasMoves = hasAtLeastOneMove;
    }

    public void MovePiece(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            ChessPiece destinationPiece = ChessPieces[x, y];
            if (destinationPiece != null && destinationPiece.isWhite != isWhiteTurn)
            {
                //Capture piece

                //win if captured is king
                if (destinationPiece.GetType() == typeof(King))
                {
                    EndGame();
                    return;
                }

                alivePieces.Remove(destinationPiece.gameObject);
                Destroy(destinationPiece.gameObject);
            }

            if (x == EnPassantMoves[0] && y == EnPassantMoves[1])
            {
                if (isWhiteTurn)
                {
                    destinationPiece = ChessPieces[x, y - 1];
                }
                else
                {
                    destinationPiece = ChessPieces[x, y + 1];
                }
                alivePieces.Remove(destinationPiece.gameObject);
                Destroy(destinationPiece.gameObject);
            }
            EnPassantMoves[0] = -1;
            EnPassantMoves[1] = -1;

            if (selectedPiece.GetType() == typeof(Pawn))
            {
                if (y == 7)
                {
                    alivePieces.Remove(selectedPiece.gameObject);
                    Destroy(selectedPiece.gameObject);

                    SpawnChessPiece(1, x, y);

                    selectedPiece = ChessPieces[x, y];
                }
                else if (y == 0)
                {
                    alivePieces.Remove(selectedPiece.gameObject);
                    Destroy(selectedPiece.gameObject);

                    SpawnChessPiece(7, x, y);

                    selectedPiece = ChessPieces[x, y];
                }

                if (selectedPiece.CurrentY == 1 && y == 3)
                {
                    EnPassantMoves[0] = x;
                    EnPassantMoves[1] = y - 1;
                }
                else if (selectedPiece.CurrentY == 6 && y == 4)
                {
                    EnPassantMoves[0] = x;
                    EnPassantMoves[1] = y + 1;
                }
            }

            ChessPieces[selectedPiece.CurrentX, selectedPiece.CurrentY] = null;
            selectedPiece.transform.position = GetTileCenter(x, y);
            selectedPiece.SetPosition(x, y);
            ChessPieces[x, y] = selectedPiece;
            isWhiteTurn = !isWhiteTurn;

            AILogic.Instance.ToggleAITurn();

            if (AILogic.Instance.IsAITurn == true)
            {
                AILogic.Instance.StartAITurn();
            }
        }

        BoardHighlighting.Instance.HideHighlighting();
        selectedPiece = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;

        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit,
            25.0f,
            LayerMask.GetMask("ChessPlane")))
        {
            //Debug.Log(hit.point);
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        } else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessPiece(int index, int x, int y)
    {
        GameObject go = Instantiate(
                chessPiecePrefabs[index], 
                GetTileCenter(x, y),
                pieceOrientation
            ) as GameObject;

        go.transform.SetParent(transform);
        ChessPieces[x, y] = go.GetComponent<ChessPiece>();
        ChessPieces[x, y].SetPosition(x, y);

        // assign piece type to each ChessPiece
        string color;
        if (ChessPieces[x, y].name.Contains("White")) color = "White";
        else color = "Black";

        string name = ChessPieces[x, y].name.Substring(0, ChessPieces[x, y].name.IndexOf(color));
        ChessPieces[x, y].type = name;

        alivePieces.Add(go);
    }

    private void SpawnAllChessPieces()
    {
        alivePieces = new List<GameObject>();
        ChessPieces = new ChessPiece[8, 8];
        EnPassantMoves = new int[2] { -1, -1 };

        // SPAWN the brood!!!

        //White=================================
        //King
        SpawnChessPiece(0, 3, 0);

        //Queen
        SpawnChessPiece(1, 4, 0);

        //Rooks
        SpawnChessPiece(2, 0, 0);
        SpawnChessPiece(2, 7, 0);

        //Bishops
        SpawnChessPiece(3, 2, 0);
        SpawnChessPiece(3, 5, 0);

        //Knights
        SpawnChessPiece(4, 1, 0);
        SpawnChessPiece(4, 6, 0);

        //Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessPiece(5, i, 1);
        }

        //Black=================================
        //King
        SpawnChessPiece(6, 4, 7);

        //Queen
        SpawnChessPiece(7, 3, 7);

        //Rooks
        SpawnChessPiece(8, 0, 7);
        SpawnChessPiece(8, 7, 7);

        //Bishops
        SpawnChessPiece(9, 2, 7);
        SpawnChessPiece(9, 5, 7);

        //Knights
        SpawnChessPiece(10, 1, 7);
        SpawnChessPiece(10, 6, 7);

        //Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessPiece(11, i, 6);
        }
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void DrawChessboard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightine = Vector3.forward * 8;

        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine, Color.black);
            for (int j = 0; j <= 0; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightine, Color.black);
            }
        }

        // Draw the selection 
        if(selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }

    public void WhiteTurn()
    {
        isWhiteTurn = true;
    }

    public void EndGame()
    {
        if (isWhiteTurn) 
        {
            Debug.Log("White wins.");
        }
        else
        {
            Debug.Log("Black wins.");
        }

        foreach (GameObject go in alivePieces)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        AILogic.instance = null;
        BoardHighlighting.Instance.HideHighlighting();
        SpawnAllChessPieces();
    }
}
