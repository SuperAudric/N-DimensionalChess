using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
///
///
///No longer 2D Chess
///By Audric Paris
///
///Todo:
///Better starting configurations
///implement blockers
///     Ability to change which order dimensions are rendered in
///     Drag and drop style dimension changing.
///
///Polish:
///Correct coordinate system to match chess' normal coordinates. 
///Speed up AI by moving data held in tile/piece to main controller (i.e. .myTile and .occupant)
///Convert AIMove to something that can run on the graphics card.
///Move functions into their groups (Main, Helper 1, Helper 2, maybe multiplayer if it gets too big
///
///Known bugs:
///Pieces don't update UI when promoting
///Check sensing has stopped working, likely due to threat sensing
///AI appears to have some bugs, it's allowing kings to be caputred
///En passant disabled temporarily.
/// </summary>

public class ChessControllerND : MonoBehaviour
{
#region variables
    //public so they can be changed, or viewed
    public int[] boardDimensions;
    public int forwardsDimensions;
    public double whiteTimer;//remaining time
    public double blackTimer;
    public bool initateChange;
    public int nodesExpanded = 0;
    public int nodesSkipped = 0;
    public int[] timesDone = new int[10];
    public int searchDepth;

    //public for access
    public readonly int THINGTYPES = 6; //Used for undo button, each represents a piece type.

    //public to link using Unity
    public GameObject boardTilePrefab;
    public GameObject genericChessPiecePrefab;
    public GameObject GameEndPrefab;

    //private
    private int turn = 0;
    private Array myTiles;
    private StorageObject myPieces;
    private List<int> unoccupiedPieceIndexes = new List<int>();
    private int pieceCount = 0;
    private int[] from, to;//AI's best found position
    private TileAI selected;
    private TextMesh historyText;
    private BoardAssessmentAlgorithm myBoardAssessmentAlgorithm;
    private UnityEngine.UI.Text timerDisplayText;
    private MovePackage displayedMoves = new MovePackage(null);//where the selected piece can go
    private List<int[]> discoloredSquares = new List<int[]>(); //used to return the board to it's natural colors 
    private int[] lastMovedPiece = { -1, -1 };
    private int[] mousedOverTile = { -1, -1 };
    private List<int[]> attackingPieces = new List<int[]>();//pieces that threaten the mousedOverTile
    private bool mouseOverBoard = false;//could probably be merged into mousedOverTile
    private readonly int[] backRowPieces = { 1, 2, 3, 4, 5, 3, 2, 1 }; //For generating the board
    private List<TurnRecord> turnHistory = new List<TurnRecord>(); //records all moves thusfar made (exluding those undone)
    private bool manualControlEnabled = false;
    private int manualControlMode = 0;
    private double updateRate = 0.02f;
    private int[][][] myMovesets;
    private List<int>[] ordering;
    private DateTime[] storage = new DateTime[10];
    private TimeSpan[] timeSpans = new TimeSpan[10];
    private string mode = "Singleplayer";
    private Dictionary<int,float> boardStateValues = new Dictionary<int, float>();

    #endregion variables
    /// Variable declaration above
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Main functions below. These either are called by unity or are the beating core of the game
#region MainFunctions

    //setup board, pieces, and movesets
    void Start()
    {
        if (GameObject.Find("AudioController") != null)
            if (GameObject.Find("AudioController").GetComponent<TerribleCludgeSolution>() != null)
                GameObject.Find("AudioController").GetComponent<TerribleCludgeSolution>().poke();
                    myPieces = new StorageObject(boardDimensions);
        myTiles = Array.CreateInstance(typeof(TileAI), boardDimensions);
        updateRate = Time.fixedDeltaTime;
        historyText = GameObject.Find("TurnHistoryText").GetComponent<TextMesh>();
        myBoardAssessmentAlgorithm = gameObject.GetComponent<BoardAssessmentAlgorithm>();
        timerDisplayText = GameObject.Find("TurnsAndTimersText").GetComponent<UnityEngine.UI.Text>();
        ordering = new List<int>[3];
        for (int i = 0; i < 3; i++)
        {
            ordering[i] = new List<int>();
        }
        for (int i = 0; i < boardDimensions.Length; i++)
        {
            ordering[i % 3].Add(i);
        }
        for(int i=0;i<myPieces.pieces.Length;i++)
        {
            //declares every slot available for a piece
            unoccupiedPieceIndexes.Add(i);
        }
        //making the board
        RecursiveMakeBoard(new int[] { });
        if (mode == "Client")
        {
            //get piece position from host
            //BroadcastGameState();
        }
        else
        {
            int defaultBoardConfig = 0;

            if (boardDimensions.Length >= 2)
            {
                if (boardDimensions[0] == 8 && boardDimensions[1] == 8)
                {
                    defaultBoardConfig = 1;
                    int[] targetedPos;
                    for (int i = 0; i < 32; i++)
                    {
                        int selectedRow = i / 8 > 1 ? 4 + i / 8 : i / 8;
                        targetedPos = new int[boardDimensions.Length];
                        targetedPos[0] = selectedRow;
                        targetedPos[1] = i % 8;//add 4 if row >= 2 so it goes 0,1,6,7
                        int pieceType = (selectedRow == 1 || selectedRow == 6) ? 0 : backRowPieces[i % 8];//selects the appropreate piece type
                        CreatePiece(targetedPos, selectedRow < 4, pieceType);
                    }
                }
                if (boardDimensions[0] == 3 && boardDimensions[1] == 4)
                {
                    defaultBoardConfig = 2;
                    CreatePiece(new int[] { 2, 0 }, false, 0);
                    CreatePiece(new int[] { 1, 0 }, false, 4);
                    CreatePiece(new int[] { 0, 3 }, true, 0);
                    CreatePiece(new int[] { 1, 2 }, true, 4);
                }
                if (boardDimensions[0] == 3 && boardDimensions[1] == 2)
                {
                    defaultBoardConfig = 2;
                    CreatePiece(new int[] { 2, 0 }, false, 4);
                    CreatePiece(new int[] { 0, 0 }, true, 4);
                }
            }
            if (defaultBoardConfig == 0)
            {/*
            foreach (TileAI test in myTiles)
            {

                int x = GetXCoord(test.pos);
                int y = sumArrayElements(test.pos) - x;
                int pieceToMake = x == 0 ? 5 - y : 0;
                if (x < 2 && y < 6)
                {
                    PieceInfo NewlyCreatedPiece = Instantiate(genericChessPiecePrefab, test.transform).GetComponent<PieceInfo>();// create a piece on the correct tile
                    NewlyCreatedPiece.setup(GetXCoord(test.pos) * 2 < GetXCoord(boardDimensions) - forwardsDimensions, pieceToMake);//tels each piece what type it is
                    myPieces.pieces.Add(NewlyCreatedPiece);
                    MovePiece(myPieces.pieces.Count - 1, test.pos);
                }
            }*/
            }

            //StorageObject testStore = new StorageObject(myPieces.pieces, boardDimensions);

        }
        
        fillMovesets();
        lastMovedPiece = new int[] { -1, -1 };
        ColorBoard();  //color the chessboard



    }
    void CreatePiece(int[] pos, bool isBlack, int type)
    {
        CreatePiece(pos, isBlack, type, -1);
    }
    void CreatePiece(int[] pos, bool isBlack, int type, int movesSoFar)
    {
        ChessPieceAI NewlyCreatedPiece = Instantiate(genericChessPiecePrefab, GetTile(pos).transform).GetComponent<ChessPieceAI>();// create a piece on the correct tile
        NewlyCreatedPiece.Setup(isBlack, type, movesSoFar);//tels each piece what type it is
        PieceInfo infoToSave = new PieceInfo();
        infoToSave.Setup(isBlack, type, movesSoFar);
        infoToSave.ThisPiece = NewlyCreatedPiece.transform;
        int pieceID = unoccupiedPieceIndexes[0];
        unoccupiedPieceIndexes.RemoveAt(0);
        myPieces.Insert(infoToSave, pieceID, pos);
        MovePiece(pieceID, pos);
        pieceCount++;
    }
    void CreatePieceFast(int[] pos, PieceInfo myPiece, int index)
    {
        //PieceInfo NewlyCreatedPiece = Instantiate(genericChessPiecePrefab, GetTile(pos).transform).GetComponent<PieceInfo>();// create a piece on the correct tile
        //NewlyCreatedPiece.setup(isBlack, type, age);//tels each piece what type it is
        myPieces.Insert(myPiece, index, pos);
        MovePiece(index, pos);
    }
    void fillMovesets()
    {//Fills out the movesets for each pieceType
        List<int[]> QueenMoves = new List<int[]>();
        RecursiveGetQueenMoves(ref QueenMoves, new int[] { });
        QueenMoves.RemoveAt(QueenMoves.Count / 2);//removes the move that is standing still
        List<int[]> BishopMoves = new List<int[]>();
        List<int[]> RookMoves = new List<int[]>();
        int dimensionsUsed;
        for (int i = 0; i < QueenMoves.Count; i++)
        {
            dimensionsUsed = 0;
            for (int j = 0; j < QueenMoves[i].Length; j++)
            {
                if (QueenMoves[i][j] != 0)
                    dimensionsUsed++;
            }
            if (dimensionsUsed % 2 == 0)
            {
                BishopMoves.Add(QueenMoves[i]);
            }
            else
            {
                RookMoves.Add(QueenMoves[i]);
            }
        }
        List<int[]> KnightMoves = new List<int[]>();//could be converted into an array as size will be boardDimensions.Length*(boardDimensions.Length-1)
        int[] moveToAdd = new int[boardDimensions.Length];
        for (int i = 0; i < boardDimensions.Length; i++)
        {
            for (int j = 0; j < boardDimensions.Length; j++)
            {
                if (i != j)
                {
                    moveToAdd[i] = 2;
                    moveToAdd[j] = 1;
                    KnightMoves.Add((int[])moveToAdd.Clone());
                    moveToAdd[i] = 2;
                    moveToAdd[j] = -1;
                    KnightMoves.Add((int[])moveToAdd.Clone());
                    moveToAdd[i] = -2;
                    moveToAdd[j] = 1;
                    KnightMoves.Add((int[])moveToAdd.Clone());
                    moveToAdd[i] = -2;
                    moveToAdd[j] = -1;
                    KnightMoves.Add((int[])moveToAdd.Clone());
                    moveToAdd[i] = 0;
                    moveToAdd[j] = 0;
                }
            }
        }
        List<int[]> blackPawnAttacks = new List<int[]>();
        List<int[]> blackPawnForwards = new List<int[]>();
        List<int[]> whitePawnAttacks = new List<int[]>();
        List<int[]> whitePawnForwards = new List<int[]>();
        List<int[]> pawnEnPassants = new List<int[]>();
        for (int i = 0; i < forwardsDimensions; i++)
        {
            moveToAdd[i] = -1;
            whitePawnForwards.Add((int[])moveToAdd.Clone());
            for (int j = forwardsDimensions; j < boardDimensions.Length; j++)
            {
                moveToAdd[j] = 1;
                whitePawnAttacks.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = -1;
                whitePawnAttacks.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = 0;
            }
            moveToAdd[i] = 1;
            blackPawnForwards.Add((int[])moveToAdd.Clone());
            for (int j = forwardsDimensions; j < boardDimensions.Length; j++)
            {
                moveToAdd[j] = 1;
                blackPawnAttacks.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = -1;
                blackPawnAttacks.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = 0;
            }
            moveToAdd[i] = 0;
            for (int j = forwardsDimensions; j < boardDimensions.Length; j++)
            {
                moveToAdd[j] = 1;
                pawnEnPassants.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = -1;
                pawnEnPassants.Add((int[])moveToAdd.Clone());
                moveToAdd[j] = 0;
            }
        }
        myMovesets = new int[][][] { QueenMoves.ToArray(), RookMoves.ToArray(), BishopMoves.ToArray(), KnightMoves.ToArray(),
            blackPawnAttacks.ToArray(), blackPawnForwards.ToArray(),
            whitePawnAttacks.ToArray(), whitePawnForwards.ToArray(), pawnEnPassants.ToArray() };
    }
    void RecursiveMakeBoard(int[] input)
    {
        //input is however many dimensions it's gone through so far.
        if (input.Length < boardDimensions.Length)
        {
            int[] nextInput = new int[input.Length + 1];
            for (int i = 0; i < input.Length; i++)
            {
                nextInput[i] = input[i];
            }
            for (int i = 0; i < boardDimensions[input.Length]; i++)
            {
                nextInput[input.Length] = i;
                RecursiveMakeBoard(nextInput);
            }
        }
        else
        {
            int[] position = (int[])input.Clone();
            GameObject NewlyCreatedTile = Instantiate(boardTilePrefab, DraggableDimensionsController.PosToCoord(input, ordering,boardDimensions), Quaternion.identity);//create board tile
            myTiles.SetValue(NewlyCreatedTile.GetComponent<TileAI>(), position);//remember it
            GetTile(input).Setup(position);
            discoloredSquares.Add(position);
        }
    }
   
    void MovePiece(int index, int[] endPos)
    {
        lastMovedPiece = endPos;
        int othersIndex = GetOccupant(endPos);
        if ((othersIndex != -1) &&(othersIndex!=index))
        {
            RemovePiece(GetOccupant(endPos));
        }
        myPieces.MovePiece(myPieces.pieces[index], endPos);
        myPieces.pieces[index].timesMoved++;
        myPieces.pieces[index].ThisPiece.transform.SetParent(GetTile(endPos).transform, false);
    }
    //moves the piece without generating a history text or grapical update (in theory)
    void MovePieceFast(int index, int[] endPos)
    {
        lastMovedPiece = endPos;
        if (GetOccupant(endPos) != -1)
        {
            FastRemovePiece(GetOccupant(endPos));
        }
        myPieces.MovePiece(myPieces.pieces[index], endPos);
        myPieces.pieces[index].timesMoved++;
        myPieces.pieces[index].ThisPiece.SetParent(GetTile(endPos).transform, false);//should eventually be replaced with reseting their transforms to the right spot at the end of the AI move (it's messed up when the piece is captured/revived)
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            UndoButton();
        if (!mouseOverBoard)
        {
            attackingPieces.Clear();
            mousedOverTile = new int[] { -1, -1 };
        }
        mouseOverBoard = false;
        SendInfoToClients(false);

        
        ColorBoard();
    }
    int multiplayerSynchCounter = 0;
    void FixedUpdate()
    {
        if (turn % 2 == 0)
        {
            whiteTimer -= updateRate / 60;
            if (whiteTimer < 0)
            {
                historyText.text += "\nWhite Timed out!";
                whiteTimer += 5;
            }
        }
        else
        {
            blackTimer -= updateRate / 60;
            if (blackTimer < 0)
            {
                historyText.text += "\nBlack Timed out!";
                blackTimer += 5;
            }
        }
        if (timerDisplayText != null)
            timerDisplayText.text = (int)whiteTimer + ":" + (int)((whiteTimer % 1) * 60) + " White | Black " + (int)blackTimer + ":" + (int)((blackTimer % 1) * 60) +
                "\n" + (turn % 2 == 0 ? "<b>White's turn</b>" : "<b>Black's turn</b>");
    }

    public string ArrayToString(List<int>[] toConvert)//not sure why the ienumberable<ienumberable<int>> doesn't cover this
    {
        string output = "";
        for(int i=0;i<toConvert.Length;i++)
        {
            output += ArrayToString(toConvert[i]) + (i==toConvert.Length-1?"":" and ");
        }
        return output;
    }

    public void StartMiniMaxAI()
    {
        Deselect();
        //System.Threading.Thread AIThread = new System.Threading.Thread(StartThreading);
        //AIThread.Start();
        nodesExpanded = 0;
        nodesSkipped = 0;
        from = NullPosition();
        to = NullPosition();
        storage = new DateTime[10];
        timeSpans = new TimeSpan[10];
        timesDone = new int[10];
        storage[0] = DateTime.Now;
        timesDone[0]++;
        double timePerHalfCycle = 0.0022; //(double)(timeSpans[8]).TotalMilliseconds / timesDone[9]; 
        double timePerFullCycle = 0.0053; //How long it takes to fully sample the time, on average
        //Debug.Log(timeSpans[8].TotalMilliseconds + ", " + timeSpans[9].TotalMilliseconds + ", " + (double)(timeSpans[8] - timeSpans[9]).TotalMilliseconds + ", "+ timesDone[9] + ", " + timePerCycle);
        timesDone[9] = 0;
        boardStateValues.Clear();//This allows it to search deeper as the game progresses, rather than relying on old data
        float expected = MiniMaxAI(searchDepth, 0, float.MinValue, float.MaxValue);//Run AI and store best value
        Debug.Log("Expected value: " + expected + "\nTotal time taken: " + ((DateTime.Now - storage[0]).TotalMilliseconds - sumArrayElements(timesDone) * timePerFullCycle) + "\nTotal ignored from testing: " + (sumArrayElements(timesDone) * timePerFullCycle));
        Debug.Log("Times spent finding moves: " + (timeSpans[1].TotalMilliseconds - timesDone[1] * timePerHalfCycle) + "\nTimes spent in GetCheck: " + (timeSpans[2].TotalMilliseconds - timesDone[2] * timePerHalfCycle));
        Debug.Log("Time spent in getPiece: " + (timeSpans[3].TotalMilliseconds - timesDone[3] * timePerHalfCycle));
        Deselect();
        if (!CompareArrays(from, NullPosition()))
        {
            TileClicked(from);
            TileClicked(to);
        }
        else
        {
            Debug.Log("Mate in " + searchDepth + ", AI concedes!");
        }
    }
    //does work, needs profiling
    float MiniMaxAI(int currentDepth, int distFromRoot, float alpha, float beta)//
    {
        int myHash = 0;
        for(int i=0;i<myPieces.pieces.Length;i++)
            if(myPieces.pieces[i]!=null)
                myHash^= myPieces.pieces[i].GetHashCode();
        if (boardStateValues.ContainsKey(myHash))
            return boardStateValues[myHash];
        if (currentDepth <= 0)
        {
            return myBoardAssessmentAlgorithm.AssessBoard(boardDimensions, myPieces.pieces, turn % 2 == 1);
            //boardStateValues.Add(myHash, myBoardAssessmentAlgorithm.AssessBoard(boardDimensions, myPieces.pieces, turn % 2 == 1));
            //return boardStateValues[myHash];
        }
        float thisTileValue;
        int[] currentPos;
        int pieceIndex;
        bool promoted = false;
        PieceInfo tempOcc = null;
        int capturedIndex = 0;
        PieceInfo pieceMoved;
        MovePackage movesToSearch;
        for (int i = 0; i < pieceCount; i++)//extract relevant info before the list reorders its self 
        {
            if (myPieces.pieces[i] != null)
            {
                if (myPieces.pieces[i].isBlack == (turn % 2 == 1))//only move your own pieces
                {
                    currentPos = myPieces.pieces[i].position;
                    pieceIndex = i;
                    pieceMoved = myPieces.pieces[pieceIndex];
                    storage[1] = DateTime.Now;
                    timesDone[1]++;
                    movesToSearch = FindMoves(currentPos);//get all moves of current position
                    timeSpans[1] += (DateTime.Now - storage[1]);
                    for (int j = 0; j < movesToSearch.validCoords.Count; j++)
                    {
                        turn++;

                        capturedIndex = GetOccupant(movesToSearch.validCoords[j]);
                        if (capturedIndex != -1)
                            tempOcc = myPieces.pieces[capturedIndex];
                        MovePieceFast(pieceIndex, movesToSearch.validCoords[j]);
                        if (pieceMoved.pieceType == 0)
                            if ((GetXCoord(movesToSearch.validCoords[j]) == 0 && !pieceMoved.isBlack) //promote piece if applicable
                             || (GetXCoord(movesToSearch.validCoords[j]) == GetXCoord(boardDimensions) - forwardsDimensions && pieceMoved.isBlack))
                            {
                                promoted = true;
                                pieceMoved.ChangePromotion(5);
                            }
                        storage[2] = DateTime.Now;
                        bool temp = GetCheckStatus(turn % 2 == 1);
                        timesDone[2]++;
                        timeSpans[2] += (DateTime.Now - storage[2]);
                        if (temp) //remove illegal moves
                        {
                            thisTileValue = float.MinValue / 2;
                        }
                        else
                        {
                            thisTileValue = -MiniMaxAI(currentDepth - 1, distFromRoot + 1, -beta, -alpha);
                        }
                        nodesExpanded++;

                        //Fast Undo
                        if (promoted)
                        {
                            promoted = false;
                            pieceMoved.ChangePromotion(0);
                        }
                        MovePieceFast(pieceIndex, currentPos);
                        if (capturedIndex != -1)
                        {
                            CreatePieceFast(movesToSearch.validCoords[j], tempOcc, capturedIndex);
                            /*FastMakePiece(tempOcc,index)
                            GetTile(movesToSearch.validCoords[j]).occupant = tempOcc;
                            myPieces.pieces[pieceCount]=(tempOcc);
                            pieceCount++;*/
                        }
                        turn--;

                        if (thisTileValue >= beta)//Alpha-Beta pruning, if you're already worse than your best competetor, then we don't care about you
                        {
                            nodesSkipped++;
                            return beta;
                        }
                        if (thisTileValue > alpha)
                        {
                            alpha = thisTileValue;
                            if (distFromRoot == 0)
                            {
                                from = currentPos;
                                to = movesToSearch.validCoords[j];
                            }
                        }


                    }
                }
            }
        }
        return alpha;
    }
    /* void MovePieceFast(PieceInfo piece, int[] xy)
     {
         lastMovedPiece = xy;
         piece.myTile.occupant = null;
         piece.myTile = GetTile(xy);
         if (piece.myTile.occupant != null)       //capture
         {
             myPieces.pieces.Remove(piece.myTile.occupant);
             //Destroy(piece.myTile.occupant.gameObject);
         }
         piece.myTile.occupant = piece;
         piece.timesMoved++;
     }*/
    int sumArrayElements(int[] a)
    {
        int sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += a[i];
        }
        return sum;
    }
    void ColorBoard()//makes every tile appear white/black according to the standard chessboard.
    {
        for (int i = 0; i < discoloredSquares.Count; i++)
        {
            GetTile(discoloredSquares[i]).GetComponent<SpriteRenderer>().color = sumArrayElements(discoloredSquares[i]) % 2 == 0 ? new Color(0.49f, 0.45f, 0.45f, 0.8f) :
                                                                                                                                   new Color(0.94f, 0.98f, 0.98f, 0.8f);//default board colors
        }
        discoloredSquares.Clear();
        if (WithinRange(lastMovedPiece))
        {
            AlterColor(lastMovedPiece, new Color(.9f, .6f, .3f, 1f));
        }
        if (WithinRange(mousedOverTile))
        {
            AlterColor(mousedOverTile, new Color(.7f, .7f, .3f, 1f));
        }
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            AlterColor(attackingPieces[i], new Color(1f, .5f, 0f, 1f));
        }
        if (selected != null)
            if (WithinRange(selected.pos))
            {
                AlterColor(selected.pos, new Color(1f, 0f, 0.2f, 1f));
            }
        for (int i = 0; i < displayedMoves.validCoords.Count; i++)
        {
            AlterColor(displayedMoves.validCoords[i], Color.blue);
        }
        for (int i = 0; i < displayedMoves.validSpecials.Count; i++)
        {
            AlterColor(displayedMoves.validSpecials[i].pos, new Color(0.5f, 0f, 0.5f, 1f));
        }

    }
    void AlterColor(int[] xy, Color c)
    {
        GetTile(xy).GetComponent<SpriteRenderer>().color = (GetTile(xy).GetComponent<SpriteRenderer>().color * 2 + c * 3) / 5;
        discoloredSquares.Add(xy);
    }

    public void TileClicked(int[] xy)//occurs when a tile is clicked
    {
        switch (mode)
        {
            case "Singleplayer":
                SinglePlayerMove:
                if (!manualControlEnabled)
                {
                    NormalMove(xy);
                    return;
                }
                else
                {
                    if (manualControlMode == 0)
                    {//wait 1 turn
                        turn++;
                        turnHistory.Add(new TurnRecord(NullPosition(), NullPosition(), null, null));
                        historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription;
                    }
                    else
                    {
                        if (selected == null)
                        {//just clicked on first tile
                            selected = GetTile(xy);
                            if (GetOccupant(xy) == -1)
                            {

                                if (manualControlMode > 1 && manualControlMode < 14)
                                {//createPiece
                                    CreatePiece(xy, (manualControlMode - 2) / THINGTYPES == 0, (manualControlMode - 2) % THINGTYPES, -1000);//must have bogus move count or else pawns can always en passant when created and other bugs
                                    turnHistory.Add(new TurnRecord(NullPosition(), xy, null, myPieces.pieces[pieceCount - 1]));
                                    historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription;
                                    turn++;
                                }
                                Deselect();
                            }
                            else
                            {
                                if (manualControlMode == 14)
                                {//vaporize
                                    turnHistory.Add(new TurnRecord(xy, NullPosition(), null, myPieces.pieces[GetOccupant(xy)]));
                                    historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription;
                                    turn++;
                                    RemovePiece(myPieces.pieces[GetOccupant(xy)].index);
                                    Deselect();
                                }
                            }
                        }
                        else
                        {
                            if (CompareArrays(selected.pos, xy))
                            {
                                //clicked on same piece twice
                                Deselect();
                            }
                            else
                            {
                                //Teleport
                                if (manualControlMode == 1)
                                {
                                    turnHistory.Add(new TurnRecord(selected.pos, xy, myPieces.pieces[GetOccupant(selected.pos)], myPieces.pieces[GetOccupant(xy)]));
                                    historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription;
                                    turn++;
                                    MovePiece(myPieces.pieces[GetOccupant(selected.pos)].index, xy);
                                    Deselect();
                                }
                            }
                        }
                    }
                }
                //Debug.Log(GetCheckStatus(true) + ", " + GetCheckStatus(false));
                break;
            case "Host":
                if (manualControlEnabled || (turn % 2 == 0))
                {
                    //Debug.Log("Host in control");
                    goto SinglePlayerMove;
                }
                goto DisplayButCantMove;
                //XXXXX needs to wait for the connected client to send a move over
            case "Client":
                if (turn % 2 == 0)
                {
                    goto DisplayButCantMove;
                }
                //else, basically normal move but send it to the host on success
                if (selected == null)
                {//just clicked on first tile
                    selected = GetTile(xy);

                    if (GetOccupant(xy) == -1)
                    {
                        Deselect();
                    }
                    else
                    {
                        if ((myPieces.pieces[GetOccupant(xy)].isBlack == true ? 1 : 0) == turn % 2)
                        {
                            DisplayMoves(xy);
                        }
                        else
                            Deselect();
                        
                    }
                }
                else
                {
                    if (CompareArrays(selected.pos, xy))
                    {
                        //clicked on same piece twice
                    }
                    else
                    {        //just clicked on a second tile
                        int[] temp = selected.pos;
                        Deselect();
                        TileClicked(temp);//this slight recursion prevents errors caused by updating the piece-list since the first click by refreshing.
                        if (AttemptMove(xy, displayedMoves, selected))
                        {
                            //Debug.Log("Attempting to send.");
                            GameObject.FindGameObjectWithTag("NetworkCom").GetComponent<NetworkCommunications>().RequestMove(selected.pos, xy);
                            TurnConclusion();
                        }
                        else
                        {
                        }
                        //selected = GetTile(xy);
                    }
                    Deselect();

                }
                break;
            default:
                DisplayButCantMove:

                if (selected == null)
                {//just clicked on first tile
                    selected = GetTile(xy);

                    if (GetOccupant(xy) == -1)
                    {
                        Deselect();
                    }
                    else
                    {
                        if ((myPieces.pieces[GetOccupant(xy)].isBlack == true ? 1 : 0) == turn % 2)
                        {
                            DisplayMoves(xy);
                        }
                        else
                        {
                            Deselect();
                        }
                    }
                }
                else
                {
                    Deselect();
                }
                break;
        }
        
    }

    public void NormalMove(int[] xy)
    {
        if (selected == null)
        {//just clicked on first tile
            selected = GetTile(xy);

            if (GetOccupant(xy) == -1)
            {
                Deselect();
            }
            else
            {
                if ((myPieces.pieces[GetOccupant(xy)].isBlack == true ? 1 : 0) == turn % 2)
                {
                    DisplayMoves(xy);
                }
                else
                {
                    Deselect();
                }
            }
        }
        else
        {
            if (CompareArrays(selected.pos, xy))
            {
                //clicked on same piece twice
            }
            else
            {        //just clicked on a second tile
                if (AttemptMove(xy, displayedMoves, selected))
                {
                    //move successful
                    AudioAndGraphicsSelector.PlaySound("Piece Move");
                    TurnConclusion();
                }
                //selected = GetTile(xy);
            }
            Deselect();

        }
    }
#endregion MainFunctions
    /// Main functions above
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Helper 1 functions below. These accomplish one complex task and don't call any Main functions
#region Helper1
    void Deselect()
    {
        selected = null;
        displayedMoves.validCoords.Clear();
        displayedMoves.validSpecials.Clear();
    }
    bool AttemptMove(int[] xy, MovePackage potentials, TileAI priorTile)
    {
        for (int i = 0; i < potentials.validCoords.Count; i++)//if it's a valid move, move there
        {
            if (CompareArrays(potentials.validCoords[i], xy))
            {
                int myIndex = potentials.piece.index;
                turnHistory.Add(new TurnRecord(priorTile.pos, potentials.validCoords[i], GetPieceAt(priorTile.pos), (GetOccupant(potentials.validCoords[i]) == -1 ? null : GetPieceAt(potentials.validCoords[i]))));
                turn++;
                MovePiece(myIndex, xy);
                if ((GetXCoord(xy) == 0 && myPieces.pieces[GetOccupant(xy)].pieceName == "White Pawn") || (GetXCoord(xy) == GetXCoord(boardDimensions) - forwardsDimensions && myPieces.pieces[GetOccupant(xy)].pieceName == "Black Pawn"))
                {
                    myPieces.pieces[GetOccupant(xy)].Setup(myPieces.pieces[GetOccupant(xy)].isBlack, 5);
                }
                return true;
            }
        }
        for (int i = 0; i < potentials.validSpecials.Count; i++)//specialMoves
        {
            if (CompareArrays(potentials.validSpecials[i].pos, xy))
            {
                int myIndex = potentials.piece.index;
                PieceInfo specialPiece = potentials.validSpecials[i].relevantPiece;
                if (specialPiece != null)
                {
                    if (potentials.piece.pieceName.Contains("King"))//castling
                    {
                        turnHistory.Add(new TurnRecord(priorTile.pos, potentials.validSpecials[i].pos, potentials.piece, specialPiece));
                        turn++;
                        int[] incrementor = new int[] { 0, (specialPiece.position[1] == 0 ? 1 : -1) };
                        MovePiece(myIndex, xy);
                        MovePiece(specialPiece.index, AddSlow(xy, incrementor));
                        return true;
                    }
                    //en passant
                    if (specialPiece.pieceName.Contains("Pawn") && potentials.validSpecials[i].relevantPiece != null)//en passant
                    {
                        turnHistory.Add(new TurnRecord(priorTile.pos, potentials.validSpecials[i].pos, potentials.piece, specialPiece));
                        turn++;
                        MovePiece(myIndex, xy);
                        RemovePiece(GetOccupant(specialPiece.position));
                        return true;
                    }
                }
            }
        }

        return false;
    }
    void TurnConclusion()
    {
        displayedMoves.validCoords.Clear();
        displayedMoves.validSpecials.Clear();
        Debug.Log("White in check?:" + GetCheckStatus(false) + ", Black in check? " + GetCheckStatus(true));
        if (GetCheckStatus(turn % 2 == 1))
        {
            historyText.text += "\nIllegal Move, " + (turn % 2 == 1 ? "White" : "Black") + " King capturable";
            Undo(false);
            return;
        }
        if (historyText != null)
        {
            //Debug.Log("!blackTurn: " + (turn % 2 == 0) + " then " + GetMateStatus(true) + ", " + GetMateStatus(false) + "Check status" + GetCheckStatus(true) + ", " + GetCheckStatus(false));
            if (GetMateStatus(turn % 2 == 0))
            {
                if (GetCheckStatus(turn % 2 == 0))
                {
                    historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription + "<b><color=#ff0000ff> - CHECKMATE!</color></b>";
                    for (int i = 0; i < pieceCount; i++)
                    {
                        if (myPieces.pieces[i] != null)
                        {
                            if (myPieces.pieces[i].pieceName.Contains("King"))
                            {
                                if (IsThreatenedAs(myPieces.pieces[i].isBlack, myPieces.pieces[i].position))
                                {
                                    Instantiate(GameEndPrefab, myPieces.pieces[i].ThisPiece);
                                }
                            }
                        }
                    }
                }
                else
                {
                    historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription + "<b><color=#ff1f1fff> - STALEMATE!</color></b>";
                    for (int i = 0; i < pieceCount; i++)
                    {
                        if (myPieces.pieces[i] != null)
                        {
                            if (myPieces.pieces[i].pieceName.Contains("King") && myPieces.pieces[i].isBlack == (turn % 2 == 1))
                            {
                                Instantiate(GameEndPrefab, myPieces.pieces[i].ThisPiece);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Piece missing! " + i);//at the time of writing this, it is assumed that pieces should be kept in the piece list with no gaps, shuffled down when one is removed
                        }
                    }
                }
            }
            else if (GetCheckStatus(turn % 2 == 0))
            {
                historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription + "<b><color=#ff0000ff> - Check!</color></b>";
            }
            else
                historyText.text += "\n" + turnHistory[turnHistory.Count - 1].turnDescription;
        }
        SendInfoToClients(true);
    }

    bool GetMateStatus(bool blackTurn)//attempts then undoes every move until it finds a way out of checkmate
    {
        MovePackage potentialMoves;
        int[][] currentPiecePositions = new int[pieceCount][];
        for (int i = 0; i < pieceCount; i++)
        {
            if(myPieces.pieces[i]!=null)
                currentPiecePositions[i] = myPieces.pieces[i].position;
        }
        for (int i = 0; i < currentPiecePositions.Length; i++)
        {
            if (myPieces.pieces[i] != null)
            {
                PieceInfo currentPiece = myPieces.pieces[i];
                if (currentPiece.isBlack != blackTurn)
                {
                    potentialMoves = FindMoves(currentPiecePositions[i]);
                    for (int j = 0; j < potentialMoves.validCoords.Count; j++)
                    {
                        if (!AttemptMove(potentialMoves.validCoords[j], potentialMoves, GetTile(currentPiece.position)))
                            Debug.LogWarning("Move Failed?");
                        else
                        {
                            if (!GetCheckStatus(blackTurn))
                            {
                                //Debug.Log("Move found from "+ArrayToText(currentPiecePositions[i])+" to "+ArrayToText(potentialMoves.validCoords[j])+"Black's turn? "+blackTurn+" black check? "+GetCheckStatus(true));
                                Undo(false);
                                return false;
                            }
                            else
                                Undo(false);
                        }
                    }
                    for (int j = 0; j < potentialMoves.validSpecials.Count; j++)
                    {
                        if (!AttemptMove(potentialMoves.validSpecials[j].pos, potentialMoves, GetTile(currentPiece.position)))
                            Debug.LogWarning("Move Failed?!");
                        else
                        {

                            if (!GetCheckStatus(blackTurn))
                            {
                                Undo(false);
                                return false;
                            }
                            else
                                Undo(false);
                        }
                    }
                }
            }
        }
        return true;
    }
    bool GetCheckStatus(bool blackTurn)
    {
        for (int i = 0; i < pieceCount; i++)
        {
            if (myPieces.pieces[i] != null)
            {
                if (myPieces.pieces[i].pieceType == 4)
                {
                    if (myPieces.pieces[i].isBlack == !blackTurn)
                    {
                        if (FindIfAttacked(myPieces.pieces[i].position, myPieces.pieces[i].isBlack))
                            return true;
                    }
                }
            }

        }
        return false;
    }
    void DisplayMoves(int[] xy) //displays where this piece can attack, called every frame when the mouse is over the board
    {
        displayedMoves.validCoords.Clear();
        displayedMoves = FindMoves(xy);
    }
    private MovePackage FindMoves(int[] xy)
    {
        MovePackage movesFoundSoFar = new MovePackage(myPieces.pieces[GetOccupant(xy)]);

        PieceInfo movingPiece = myPieces.pieces[GetOccupant(xy)];
        if (movingPiece != null)
        {
            int[][] myAdjacents;
            switch (movingPiece.pieceType)
            {
                //case "Black Pawn":
                case 0://"White Pawn":
                    int[][] Forwards = GetMoveset(movingPiece.pieceName + " Forwards");
                    int[][] DiagonalAttacks = GetMoveset(movingPiece.pieceName + " Attacks");
                    myAdjacents = GetMoveset("Adjacents");
                    int[] placeToCheck;

                    for (int i = 0; i < DiagonalAttacks.Length; i++)
                    {
                        placeToCheck = AddSlow(xy, DiagonalAttacks[i]);
                        if (WithinRange(placeToCheck))
                            if (HasOccupant(placeToCheck))//pawn diagonal attacking
                            {
                                if (myPieces.pieces[GetOccupant(placeToCheck)].isBlack != movingPiece.isBlack)
                                {
                                    movesFoundSoFar.validCoords.Add(placeToCheck);
                                }
                            }
                            else //en passant
                            {
                                int[] potentialPawn = AddSlow(xy, myAdjacents[i]);
                                if (CompareArrays(potentialPawn, lastMovedPiece))
                                {
                                    if (myPieces.pieces[GetOccupant(potentialPawn)].pieceType == 0 && myPieces.pieces[GetOccupant(potentialPawn)].isBlack != movingPiece.isBlack
                                     && myPieces.pieces[GetOccupant(potentialPawn)].timesMoved == 1 && (GetXCoord(placeToCheck) == 2 || GetXCoord(placeToCheck) == 5))
                                    {
                                        movesFoundSoFar.validSpecials.Add(new SpecialMove(placeToCheck, myPieces.pieces[GetOccupant(potentialPawn)]));
                                    }
                                }
                            }
                    }
                    for (int i = 0; i < Forwards.Length; i++)
                    {
                        placeToCheck = AddSlow(xy, Forwards[i]);
                        if (WithinRange(placeToCheck))
                            if (!HasOccupant(placeToCheck))
                            {
                                movesFoundSoFar.validCoords.Add((int[])placeToCheck.Clone() );//regular one step forwards
                                if ((GetXCoord(xy) == 1 && movingPiece.isBlack) || (!movingPiece.isBlack && GetXCoord(xy) == GetXCoord(boardDimensions) - forwardsDimensions - 1))//pawns may move 2 if they're 1 square from an edge
                                {
                                    Add(ref placeToCheck, Forwards[i]);
                                    if (WithinRange(placeToCheck))
                                        if (!HasOccupant(placeToCheck))
                                        {
                                            movesFoundSoFar.validCoords.Add(placeToCheck);
                                        }
                                }
                            }
                    }
                    break;
                // case "Black Rook":
                case 1://"White Rook":

                    StraightLineMove(ref movesFoundSoFar.validCoords, ref myMovesets[1], xy, true, movingPiece.isBlack);
                    break;
                //case "Black Knight":
                case 2://"White Knight":
                    StraightLineMove(ref movesFoundSoFar.validCoords, ref myMovesets[3], xy, false, movingPiece.isBlack);
                    break;
                //case "Black Bishop":
                case 3://"White Bishop":
                    StraightLineMove(ref movesFoundSoFar.validCoords, ref myMovesets[2], xy, true, movingPiece.isBlack);
                    break;
                //case "Black Queen":
                case 5://"White Queen":
                    StraightLineMove(ref movesFoundSoFar.validCoords, ref myMovesets[0], xy, true, movingPiece.isBlack);
                    break;
                //case "Black King":
                case 4://"White King":
                    StraightLineMove(ref movesFoundSoFar.validCoords, ref myMovesets[0], xy, false, movingPiece.isBlack);
                    if (movingPiece.timesMoved == 0)
                    {
                        myAdjacents = GetMoveset("Adjacents");
                        for (int i = 0; i < myAdjacents.Length; i++)
                        {
                            int[] sq1 = AddSlow(xy, myAdjacents[i]);
                            int[] sq2 = AddSlow(sq1, myAdjacents[i]);
                            int[] sq3 = AddSlow(sq2, myAdjacents[i]);
                            int[] sq4 = AddSlow(sq3, myAdjacents[i]);

                            if (WithinRange(sq1) && WithinRange(sq2) && WithinRange(sq3))//castling Kingside
                            {
                                if (!HasOccupant(sq1) && !HasOccupant(sq2) && HasOccupant(sq3))
                                {
                                    if (GetPieceAt(sq3).timesMoved == 0 && GetPieceAt(sq3).pieceName.Contains("Rook"))
                                    {
                                        if (!IsThreatenedAs(movingPiece.isBlack, xy) && !IsThreatenedAs(movingPiece.isBlack, sq1) && !IsThreatenedAs(movingPiece.isBlack, sq2))
                                            movesFoundSoFar.validSpecials.Add(new SpecialMove(sq2, GetPieceAt(sq3)));
                                    }
                                }
                            }
                            if (WithinRange(sq4))//castling queenside
                            {
                                if (!HasOccupant(sq1) && !HasOccupant(sq2) && !HasOccupant(sq3) && HasOccupant(sq4))
                                {
                                    if (GetPieceAt(sq4).timesMoved == 0 && GetPieceAt(sq4).pieceName.Contains("Rook"))
                                    {

                                        if (!IsThreatenedAs(movingPiece.isBlack, xy) && !IsThreatenedAs(movingPiece.isBlack, sq1) && !IsThreatenedAs(movingPiece.isBlack, sq2))
                                            movesFoundSoFar.validSpecials.Add(new SpecialMove(sq2, GetPieceAt(sq4)));
                                    }
                                }
                            }
                        }
                    }
                    break;

            }
        }
        for (int i = 0; i < movesFoundSoFar.validCoords.Count; i++)
        {
            if (!WithinRange(movesFoundSoFar.validCoords[i]))
                Debug.LogError("Error, move invalid! " + movingPiece.pieceName);
        }
        return movesFoundSoFar;
    }
#endregion Helper1
    /// Helper 1 functons above
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Helper 2 functions below. These accomplish one simple task and only call other helper 2 functions
#region Helper2
    bool IsThreatenedAs(bool isBlack, int[] pos)
    {
        int blackAttackers;
        int countAttackers = FindAttackers(pos, out blackAttackers).Count;
        if (isBlack)
        {
            return blackAttackers < countAttackers;
        }
        else
        {
            return blackAttackers > 0;
        }
    }

    public void EnableManualControl(bool state)
    {
        manualControlEnabled = state;
        selected = null;
        displayedMoves.validCoords.Clear();
        displayedMoves.validSpecials.Clear();
    }
    public void SetManualControlMode(int state)
    {
        manualControlMode = state;
    }
    public bool HasOccupant(int[] xy)
    {
        return GetOccupant(xy) != -1;
    }
    void StraightLineMove(ref List<int[]> potentialMoves, ref int[][] moveset, int[] xy, bool continues, bool isThisBlack)
    {
        int[] nextPos;
        for (int i = 0; i < moveset.Length; i++)
        {
            int breaker = 0;
            nextPos = AddSlow(xy, moveset[i]); ;
            while (WithinRange(nextPos))
            {
                breaker++;
                if (breaker > 10000)
                {
                    Debug.LogWarning("Broke here!");
                    break;

                }
                if (!HasOccupant(nextPos))//if empty
                {
                    potentialMoves.Add((int[])nextPos.Clone());
                }
                else
                {
                    if (GetPieceAt(nextPos).isBlack != isThisBlack)
                    {// if the tiles are of opposite colors
                        potentialMoves.Add(nextPos);
                    }
                    break;
                }
                if (!continues)
                    break;
                Add(ref nextPos, moveset[i]);
            }
        }
    }
    public bool WithinRange(int[] xy)
    {
        for (int i = 0; i < xy.Length; i++)
        {
            if (xy[i] < 0 || xy[i] >= boardDimensions[i])
                return false;
        }
        return true;
    }
    public void DisplayAttackers(int[] xy)
    {
        attackingPieces.Clear();
        attackingPieces = FindAttackers(xy, out _);//This highlights threatening pieces.
        if (HasOccupant(xy))
            GetPieceAt(xy).ThisPiece.rotation = Quaternion.Euler(0, 0, Mathf.Sin(8 * Time.time) * 15);
    }
    public bool FindIfAttacked(int[] xy, bool isBlack)//marks any square that can attack xy to be highlighted
    {
        if (FastFindAttackerByType(ref myMovesets[3], xy, 2, isBlack, false))
            return true;
        if (FastFindAttackerByType(ref myMovesets[0], xy, 4, isBlack, false))
            return true;
        if (isBlack)
        {
            if (FastFindAttackerByType(ref myMovesets[4], xy, 0, isBlack, false))
                return true;
        }
        else
        {
            if (FastFindAttackerByType(ref myMovesets[6], xy, 0, isBlack, false))
                return true;
        }
        if (FastFindAttackerByType(ref myMovesets[2], xy, 3, isBlack, true))
            return true;
        return (FastFindAttackerByType(ref myMovesets[1], xy, 1, isBlack, true));

    }
    private bool FastFindAttackerByType(ref int[][] moveset, int[] xy, int type, bool isBlack, bool continues)//checks if any tiles match the name given in that direction
    {
        int[] tileToCheck;
        int pieceIndex;
        if (!continues)
        {
            for (int i = 0; i < moveset.Length; i++)
            {
                tileToCheck = AddSlow(xy, moveset[i]);
                if (WithinRange(tileToCheck))
                {
                    pieceIndex = GetOccupant(tileToCheck);
                    if (pieceIndex!=-1)
                    {
                        if (myPieces.pieces[pieceIndex].pieceType == type)
                        {
                            if (myPieces.pieces[pieceIndex].isBlack != isBlack)
                                return true;
                        }
                        break;
                    }
                    Add(ref tileToCheck, moveset[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < moveset.Length; i++)
            {
                tileToCheck = AddSlow(xy, moveset[i]);
                while (WithinRange(tileToCheck))
                {
                    pieceIndex = GetOccupant(tileToCheck);
                    if (pieceIndex != -1)
                    {
                        if (myPieces.pieces[pieceIndex].pieceType == type)
                        {
                            if (myPieces.pieces[pieceIndex].isBlack != isBlack)
                                return true;
                        }
                        break;
                    }
                    Add(ref tileToCheck, moveset[i]);
                }
            }
        }

        return false;
    }
    public List<int[]> FindAttackers(int[] xy, out int thereofBlack)//marks any square that can attack xy to be highlighted
    {
        thereofBlack = 0;
        mousedOverTile = xy;
        mouseOverBoard = true;
        List<int[]> foundAttackerOfThisType = FindAttackerByType(ref myMovesets[3], xy, 2, ref thereofBlack, false);
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[0], xy, 4, ref thereofBlack, false));
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[6], xy, 0, ref thereofBlack, true));  //Most moves are already the same inverted, but pawns aren't so the other pawn's attack pattern is used 
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[4], xy, 0, ref thereofBlack, false));
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[2], xy, 3, true, ref thereofBlack));
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[1], xy, 1, true, ref thereofBlack));
        foundAttackerOfThisType.AddRange(FindAttackerByType(ref myMovesets[0], xy, 5, true, ref thereofBlack));
        return foundAttackerOfThisType;
    }
    private List<int[]> FindAttackerByType(ref int[][] moveset, int[] xy, int type, ref int thereofBlack, bool isBlack)//checks if any tiles match the name given in that direction
    {
        List<int[]> foundAttackerOfThisType = new List<int[]>();
        int[] tileToCheck;
        if (type == 0)
        {
            for (int i = 0; i < moveset.Length; i++)
            {
                tileToCheck = AddSlow(moveset[i], xy);
                if (WithinRange(tileToCheck))
                {
                    if (HasOccupant(tileToCheck))
                        if (GetPieceAt(tileToCheck).pieceType == type)
                        {

                            if (GetPieceAt(tileToCheck).isBlack == isBlack)
                            {
                                foundAttackerOfThisType.Add(tileToCheck);
                            }
                        }
                }
            }
            if (isBlack)
                thereofBlack += foundAttackerOfThisType.Count;
        }
        else
        {
            for (int i = 0; i < moveset.Length; i++)
            {
                tileToCheck = AddSlow(moveset[i], xy);
                if (WithinRange(tileToCheck))
                {
                    if (HasOccupant(tileToCheck))
                        if (GetPieceAt(tileToCheck).pieceType == type)
                        {
                            foundAttackerOfThisType.Add(tileToCheck);
                            if (GetPieceAt(tileToCheck).isBlack)
                                thereofBlack++;
                        }
                }

            }
        }
        return foundAttackerOfThisType;
    }
    private List<int[]> FindAttackerByType(ref int[][] moveset, int[] xy, int type, bool continues, ref int thereofBlack)//checks if any tiles match the name given in that direction
    {
        List<int[]> foundAttackerOfThisType = new List<int[]>();
        int breaker = 0;
        int[] tileToCheck;
        for (int i = 0; i < moveset.Length; i++)
        {

            tileToCheck = AddSlow(xy, moveset[i]);
            while (WithinRange(tileToCheck))
            {
                breaker++;
                if (breaker > 10000)
                {
                    Debug.LogWarning("Null move was not removed or board size>10000!");
                    break;
                }
                if (HasOccupant(tileToCheck))
                {
                    if (GetPieceAt(tileToCheck).pieceType == type)
                    {
                        foundAttackerOfThisType.Add(tileToCheck);
                        if (GetPieceAt(tileToCheck).isBlack)
                            thereofBlack++;
                    }
                    break;
                }

                Add(ref tileToCheck, moveset[i]);
            }
        }

        return foundAttackerOfThisType;
    }
    public void UndoButton()
    {
        Deselect();
        Undo(true);
    }
    public void Undo(bool DisplayUndo) //Undoes the last turn using turnHistory
    {
        if (turn <= 0)
        {
            return;
        }
        if (turnHistory.Count <= turn)
        {
            if (turn > 0)
            {
                TurnRecord turnUndone = turnHistory[turn - 1];
                turnHistory.RemoveAt(turn - 1);
                turn--;
                if (WithinRange(turnUndone.from) && WithinRange(turnUndone.to))
                {
                    if (!HasOccupant(turnUndone.to))
                    {
                        Debug.LogWarning("Error while undoing; piece missing");
                    }
                    else
                    {

                        int pieceIndex = GetOccupant(turnUndone.to);
                        //Debug.Log(ArrayToText(turnUndone.to) + " Goes back to" + ArrayToText(turnUndone.from) + "As index " +pieceIndex + ", piece at 1,1: " + GetOccupant(turnUndone.to));
                        MovePiece(pieceIndex, turnUndone.from);
                        GetPieceAt(turnUndone.from).timesMoved -= 2;
                        if (DisplayUndo)
                        {
                            discoloredSquares.Add(lastMovedPiece);
                            discoloredSquares.Add(turnUndone.to);
                            if (historyText != null)
                            {
                                historyText.text = ReplaceLastOccurrence(historyText.text, turnUndone.turnDescription, "<color=#808080ff>" + turnUndone.turnDescription + "</color>");
                                historyText.text = historyText.text.Replace("</color><b><color=#ff0000ff> - Check!</color></b>", "- Check!</color>");
                                string finalLine = historyText.text.Split('\n')[historyText.text.Split('\n').Length - 1];
                                if (finalLine.Contains("Undoing"))
                                {
                                    int trialNum;
                                    int.TryParse(finalLine.Split(' ')[finalLine.Split(' ').Length - 1], out trialNum);
                                    historyText.text = ReplaceLastOccurrence(historyText.text, "\n" + finalLine, "\nUndoing x " + (trialNum + 1));
                                }
                                else
                                {
                                    historyText.text += "\nUndoing x 1";
                                }
                            }
                        }
                        lastMovedPiece = turnUndone.from;
                        if (turnUndone.turnDescription.Contains("promote"))             //Temp solution: info should be packaged into the special (i.e. if special >2*THINGTYPES then unpromote and capture)
                        {
                            GetPieceAt(turnUndone.from).Setup(GetPieceAt(turnUndone.from).isBlack, 0);//unpromote
                        }
                        if (turnUndone.special > -1)
                        {  //generic capture
                            CreatePiece(turnUndone.to, turnUndone.special / THINGTYPES == 0, turnUndone.special % THINGTYPES, turnUndone.capturedAge - 1);
                        }
                        else if (turnUndone.special == -2)// castling
                        {
                            MovePiece(GetOccupant(Average(turnUndone.from, turnUndone.to)), turnUndone.specialCoords);
                            GetPieceAt(turnUndone.specialCoords).timesMoved -= 2;
                        }
                        else if (turnUndone.special == -3)//en passant
                        {
                            CreatePiece(turnUndone.specialCoords, !GetPieceAt(turnUndone.from).isBlack, 0, turnUndone.capturedAge - 1);
                        }

                    }
                    if (turn > 1)
                    {
                        lastMovedPiece = turnHistory[turn - 1].to;
                    }
                }
                else
                {//undoing a manual control move
                    if (!WithinRange(turnUndone.from) && !WithinRange(turnUndone.to))
                    {//undoing skip
                    }
                    else if (!WithinRange(turnUndone.from) && WithinRange(turnUndone.to))
                    {//undoing create
                        RemovePiece(GetOccupant(turnUndone.to));
                    }
                    else if (WithinRange(turnUndone.from) && !WithinRange(turnUndone.to))
                    {//undoing vaporize
                        CreatePiece(turnUndone.from, (turnUndone.special+1) / THINGTYPES == 0, (turnUndone.special+1) % THINGTYPES, turnUndone.capturedAge - 1);
                    }
                    if (DisplayUndo)
                    {
                        if (historyText != null)
                        {
                            historyText.text = ReplaceLastOccurrence(historyText.text, turnUndone.turnDescription, "<color=#808080ff>" + turnUndone.turnDescription + "</color>");
                            historyText.text = historyText.text.Replace("</color><b><color=#ff0000ff> - Check!</color></b>", "- Check!</color>");
                            string finalLine = historyText.text.Split('\n')[historyText.text.Split('\n').Length - 1];
                            if (finalLine.Contains("Undoing"))
                            {
                                int trialNum;
                                int.TryParse(finalLine.Split(' ')[finalLine.Split(' ').Length - 1], out trialNum);
                                historyText.text = ReplaceLastOccurrence(historyText.text, "\n" + finalLine, "\nUndoing x " + (trialNum + 1));
                            }
                            else
                            {
                                historyText.text += "\nUndoing x 1";
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Failure to Undo, turn History is smaller than turns");
        }
    }
    void RemovePiece(int index)
    {
        Destroy(myPieces.pieces[index].ThisPiece.gameObject);
        myPieces.RemoveAt(index);
        unoccupiedPieceIndexes.Add(index);
        pieceCount--;
    }
    void FastRemovePiece(int index)//pretend that it's dead for a minute
    {
        myPieces.RemoveAt(index);
    }
    public static string ReplaceLastOccurrence(string input, string thingToReplace, string toReplaceWith)
    {
        int index = input.LastIndexOf(thingToReplace);
        if (index == -1)
            return input;
        string result = input.Remove(index, thingToReplace.Length).Insert(index, toReplaceWith);
        return result;
    }
    private int[] Average(int[] a, int[] b)
    {
        if (a.Length != b.Length)
        {
            Debug.LogWarning("Failure to Average, please input arrays of equal size");
            return a;
        }
        int[] output = new int[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            output[i] = (a[i] + b[i]) / 2;
        }
        return output;
    }
    public static void Add(ref int[] a, int[] b) //Adds 2 arrays by element
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (b[i] != 0)
                a[i] += b[i];
        }
    }
    public static int[] AddSlow(int[] a, int[] b) //Adds 2 arrays by element
    {
        int[] output = (int[])a.Clone();
        for (int i = 0; i < a.Length; i++)
        {
            if (b[i] != 0)
                output[i] += b[i];
        }
        return output;
    }
    public int[] NullPosition()//this could just return new int[], but is more obvious this way.
    {
        int[] myOutput = new int[boardDimensions.Length];
        for (int i = 0; i < boardDimensions.Length; i++)
            myOutput[i] = -1;
        return myOutput;
    }
    public bool CompareArrays(int[] a, int[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public int[][] GetMoveset(string piece)
    {
        switch (piece)
        {
            case "King":
            case "Queen":
                return myMovesets[0];
            case "Rook":
                return myMovesets[1];
            case "Bishop":
                return myMovesets[2];
            case "Knight":
                return myMovesets[3];
            case "Black Pawn Attacks":
                return myMovesets[4];
            case "Black Pawn Forwards":
                return myMovesets[5];
            case "White Pawn Attacks":
                return myMovesets[6];
            case "White Pawn Forwards":
                return myMovesets[7];
            case "Adjacents":
                return myMovesets[8];
            default:
                return myMovesets[8];
        }
    }
    void RecursiveGetQueenMoves(ref List<int[]> foundSoFar, int[] coords)
    {
        if (coords.Length < boardDimensions.Length)
        {
            int[] nextCoords = new int[coords.Length + 1];
            for (int i = 0; i < coords.Length; i++)
            {
                nextCoords[i] = coords[i];
            }
            for (int j = -1; j <= 1; j++)
            {
                nextCoords[coords.Length] = j;
                RecursiveGetQueenMoves(ref foundSoFar, (int[])nextCoords.Clone());
            }

        }
        else
            foundSoFar.Add((int[])coords.Clone());
    }
    public bool GetManualControlEnabled()
    {
        return manualControlEnabled;
    }
    public PieceInfo[] GetPieces()
    {
        return myPieces.pieces;
    }
    public int[] GetPiecePosition(PieceInfo piece)
    {
        return piece.position;
    }
    public int GetTurn()
    {
        return turn;
    }

    public TileAI GetTile(int[] a)
    {
        return (TileAI)myTiles.GetValue(a);
    }
    public int GetOccupant(int[] a)
    {
        var occupant = GetPieceAt(a);
        if (occupant == null)
            return -1;
        return occupant.index;
    }
    public int GetXCoord(int[] xy)
    {
        int soFar = 0;
        for (int i = 0; i < forwardsDimensions; i++)
            soFar += xy[i];
        return soFar;
    }
    PieceInfo GetPieceAt(int[] pos)
    {
        return myPieces.GetPieceAt(pos);
    }
    public string GetTurnHistoryText()
    {
        return historyText.text;
    }
    public List<TurnRecord> GetFullTurnHistory()
    {
        return turnHistory;
    }
    public string ArrayToString(int[] a)
    {
        string output = "";
        for(int i=0;i<a.Length;i++)
        {
            output += a[i] + ", ";
        }
        return output;
    }
    public string ArrayToString(Array a)
    {
        return ArrayToString(a, new List<int> { });
    }
    //I think I could make this non-recursive by having an index of which value it's adjusting and overflowing it like an integer would
    public string ArrayToString(Array a, List<int> lastCoord)
    {
        string output = "";
        if (a.Rank > lastCoord.Count)
        {
            lastCoord.Add(0);
            for (int i = 0; i < a.GetLength(lastCoord.Count - 1);i++) 
            {
                lastCoord[lastCoord.Count - 1] = i;
                output += ArrayToString(a, lastCoord)+(a.Rank>lastCoord.Count? "\n":", ");
            }
        }
        else
            return a.GetValue(lastCoord.ToArray()).ToString();//may need to cast as an interger before doing 'ToString'
        return output;
    }
    public static string ArrayToString(IEnumerable<IEnumerable<int>> a)
    {
        string myOut = "";
        foreach (IEnumerable<int> m in a)
        {
            if (m != null)
                myOut += ArrayToString(m) + "\n";
            else
                myOut += "#";
        }
        return (myOut);

    }
    public static string ArrayToString(IEnumerable<int> a)
    {
        string myOut = "";
        foreach (int m in a)
        {
            myOut += m + ", ";
        }
        return (myOut);
    }
    public static string PiecesToString(IEnumerable<PieceInfo> theirPieces)
    {
        string myOutput = "";
        if (theirPieces != null)
            foreach (PieceInfo p in theirPieces)
            {
                if (p != null)
                    myOutput += SinglePieceToString(p) + "|";
            }
        return myOutput;
    }
    public static string SinglePieceToString(PieceInfo a)
    {
        return a.pieceName;
    }
    public Array getAllTiles()
    {
        return myTiles;
    }
    #endregion Helper2
    ///Helper 2 functions above (end of chain, calls no helper 1 or main functions)
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///Multiplayer only functions below. Multiplayer is a beast that was added late, so technically should be part of main functions set.
    #region Multiplayer
    public void SetPieces(PieceInfo[] theirPieces)
    {
        for (int i = 0; i < myPieces.pieces.Length; i++)
        {
            if (myPieces.pieces[i] != null)
                RemovePiece(i);
        }
        for (int i = 0; i < theirPieces.Length; i++)
        {
            if (theirPieces[i] != null)
                CreatePiece(theirPieces[i].position, theirPieces[i].isBlack, theirPieces[i].pieceType, theirPieces[i].timesMoved);
        }

    }
    public void SetHistory(TurnRecord[] history, String text)
    {
        turnHistory.Clear();
        turnHistory.AddRange(history);
        historyText.text = text;
    }
    public void SetTurnInfo(int turn, double whiteTimer, double blackTimer)
    {
        SteamLobbyChess.ToScreen(turn.ToString()+", "+whiteTimer.ToString()+", "+blackTimer.ToString());
        this.turn = turn;
        this.whiteTimer = whiteTimer;
        this.blackTimer = blackTimer;
    }
    public void ClientAttemptsMove(int[] from, int[] to)
    {
        Debug.Log("Client says "+ ArrayToString((IEnumerable<int>)from) + ArrayToString((IEnumerable<int>)to));
        if (turn%2==1)
        {
            Deselect();
            NormalMove(from);
            NormalMove(to);
        }
    }
    void GetMultiplayer()
    {
        if(Mirror.NetworkServer.active == true)
        {
            if (Mirror.NetworkClient.active == true)
                mode = "Host";
            else
                mode = "Server";
        }
        else
        {
            if (Mirror.NetworkClient.active == true)
                mode = "Client";
            else
                mode = "Singleplayer";
        }
    }
    void SendInfoToClients(bool forceSynch)
    {
        GetMultiplayer();
        if (mode == "Singleplayer")
            return;
        int synchRate = 300;//should maybe expose synchrate
        multiplayerSynchCounter++;
        if (forceSynch || multiplayerSynchCounter >= synchRate)
        {
            multiplayerSynchCounter = 0;
            if (mode == "Host")
            {
                SteamLobbyChess.ToScreen("Host");

                var NetworkComs = GameObject.FindGameObjectsWithTag("NetworkCom");
                for (int i = 0; i < NetworkComs.Length; i++)
                    NetworkComs[i].GetComponent<NetworkCommunications>().BroadcastGameState(turnHistory.ToArray(), historyText.text, myPieces.pieces, turn, blackTimer, whiteTimer);

            }
            else if (mode == "Client")
            {
                SteamLobbyChess.ToScreen("Client");
            }
            else
            {
                SteamLobbyChess.ToScreen("Server, not implemented");
            }
        }
    }
# endregion Multiplayer
}
