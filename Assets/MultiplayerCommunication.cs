

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Linq;

public class MultiplayerCommunication : NetworkBehaviour
{
    //contains some functions to let clients communicate with the host


    public NetworkVariable<PieceInfo[]> HostPieces = new NetworkVariable<PieceInfo[]>(    //this is a variable on client, accessible when RPCing on the host
    new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }
    );
    public NetworkVariable<int[][]> HostPositions = new NetworkVariable<int[][]>(
    new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }
    );
    public NetworkVariable<string> HostTurnHistoryText = new NetworkVariable<string>(
    new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }
    );
    public NetworkVariable<int> HostTurnCount = new NetworkVariable<int>(
    new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }
    );
    public NetworkVariable<double[]> HostTimers = new NetworkVariable<double[]>(
    new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    }
    );
    public NetworkVariable<TurnRecord[]> TurnRecords = new NetworkVariable<TurnRecord[]>(
new NetworkVariableSettings
{
    WritePermission = NetworkVariablePermission.ServerOnly,
    ReadPermission = NetworkVariablePermission.Everyone
}
);
    public void GetSecondaryInfo(out string historyText, ref List<TurnRecord> fullRecords, out int turn, ref double blackTurn, ref double whiteTurn)
    {
        RequestSecondaryInfoServerRpc();
        historyText = HostTurnHistoryText.Value;
        turn = HostTurnCount.Value;
        
        if (HostTimers.Value != null)
        {
            if (TurnRecords.Value != null)
            {
                fullRecords = TurnRecords.Value.ToList();//somehow still getting a "Can't be null" error
            }
            if (HostTimers.Value.Length == 2)
            {
                blackTurn = HostTimers.Value[0];
                whiteTurn = HostTimers.Value[1];
            }
        }
    }
    [ServerRpc]
    void RequestSecondaryInfoServerRpc(ServerRpcParams rpcParams = default)
    {
        var controller = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (controller != null)
        {
            HostTurnHistoryText.Value = controller.GetTurnHistoryText();
            TurnRecords.Value= controller.GetFullTurnHistory().ToArray();
            HostTurnCount.Value = controller.GetTurn();
            HostTimers.Value = new double[] { controller.blackTimer, controller.whiteTimer };
        }
        else Debug.Log("Controller not found");
    }

    public PieceInfo[] GetPieceInfos(out int[][] positions)
    {
        RequestPieceInfosServerRpc();
        //Debug.Log("Client recorded:" + ChessControllerND.PiecesToText(visiblePieces.Value));
        positions = HostPositions.Value;
        return HostPieces.Value;//visiblePieces.Value;
    }
    [ServerRpc]
    void RequestPieceInfosServerRpc(ServerRpcParams rpcParams = default)
    {
        //Debug.Log("This is run on the host when the client calls it");
        var controller = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (controller != null)
        {
            HostPieces.Value = (PieceInfo[])controller.GetPieces().Clone();//visiblePieces only updates when it's set to a new value, but this solution is inefficient both in terms of processing time and network usage
            //HostPositions.Value = (int[][])controller.GetAllPositions().Clone();//clone is freshly allocated each time, so never equal to the previous, so it always updates it.
                                                                                //Debug.Log("Host recorded:" + ChessControllerND.PiecesToText(visiblePieces.Value));
                                                                                //Debug.Log("Host Positions:" + ChessControllerND.ArrayToText(visiblePositions.Value));
        }
    }
    public void MakeMoveOnHost(int[] from, int[] to)
    {
        Debug.Log("Attempting move from" + ChessControllerND.ArrayToString(from)+" to "+ ChessControllerND.ArrayToString(to));
        ClientAttemptsMoveServerRpc(from, to);
    }
    [ServerRpc]
    void ClientAttemptsMoveServerRpc(int[] to, int[] from, ServerRpcParams rpcParams = default)
    {
        var controller = GameObject.Find("Main Controller").GetComponent<ChessControllerND>();
        if (controller != null)
        {
            controller.ClientAttemptsMove(to, from);
        }
    }
}
