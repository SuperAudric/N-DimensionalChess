using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


/// <summary>
/// Deletes
/// </summary>




//Must be spawned by NetworkServer.Spawn, so that it gets created with an ID. Currently one of these is made for each player, but I really only need one total.
public class NetworkCommunications : NetworkBehaviour
{
    private ChessControllerND myChessController;
    int undoCount = 0;
    System.DateTime undoRequestTime= new System.DateTime(0);
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        if(undoCount>0)
            if ((System.DateTime.Now - undoRequestTime).TotalSeconds < 20)
                RemoveUndoRequest();
    }

    //sends the game state to clients. This could maybe be replaced with a synchVar, but this gives me more contol of what happens when synching
    [ClientRpc]
    public void BroadcastGameState(TurnRecord[] history, string historyText, PieceInfo[] pieces, int turn, double blackTurnTimer, double whiteTurnTimer)
    {
        //SteamLobbyChess.ToScreen("On their screen it's: " + blackTurnTimer.ToString() + " and " + whiteTurnTimer.ToString());
        myChessController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (myChessController == null)
        {
            SteamLobbyChess.ToScreen("Can't find chess controller");
            return;
        }
        myChessController.SetPieces(pieces);
        myChessController.SetHistory(history, historyText);
        myChessController.SetTurnInfo(turn, whiteTurnTimer, blackTurnTimer);
    }

    ///<summary>
    ///Called by a client on the server, checks if it's allowed to make a move, then performs it, provided it's valid
    /// </summary>
    [Command(requiresAuthority = false)]
    public void RequestMove(int[] coordsFrom, int[] coordsTo)
    {
        SteamLobbyChess.ToScreen("1");
        //RequestMoveOnHost(coordsFrom, coordsTo);
        //check if other person's ID matches the player who's turn it is.
        myChessController = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (myChessController == null)
        {
            SteamLobbyChess.ToScreen("Can't find chess controller");
            return;
        }
        myChessController.ClientAttemptsMove(coordsFrom, coordsTo);
        RemoveUndoRequest();
    }

    /// <summary>
    /// states that the client has requested to undo [count] times
    /// </summary>
    /// <param name="count">How many times to repeat "undo" if approved</param>
    [Command(requiresAuthority = false)]
    public void RequestUndo(int count)
    {
        if(count==0)
        {
            RemoveUndoRequest();
            return;
        }
        //set a button to active and set it's text to "undo [x] times?"
        //disable it for 1 second to prevent someone from changing it as you click
        undoRequestTime = System.DateTime.Now;
        undoCount = count;
    }
    //server accepts the offer of an undo
    public void AcceptUndo()
    {
        double secondsSinceAccept = (System.DateTime.Now- undoRequestTime).TotalSeconds;
        if (secondsSinceAccept<1)
            return;
        for(int i=0;i<undoCount;i++)
        {
            myChessController.UndoButton();
        }
        undoCount = 0;
    }
    //undoes the effects of offering an Undo, run only by server
    void RemoveUndoRequest()
    {
        undoCount = 0;
        //sets the 'accept request' button to be active=false
    }
}
