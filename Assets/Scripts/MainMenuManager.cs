
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.SceneManagement;


//This script originally handled the multiplayer interaction using MLAPI, however I decided that I wanted to connect via steam, so it's been retired.
//It will be replaced by the script SteamLobbyChess, which should try to allow for both steam lobby hosting, and IP connection
public class MainMenuManager : MonoBehaviour
{
    /*public UnityEngine.UI.InputField serverIPText;
    bool loading = true;
    static string mode;
    public GameObject[] toLoadWhenClient;
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            if (loading)
            {
                switch (mode)
                {
                    case "Host":
                        SceneManager.LoadScene("NDChessScene");
                        break;
                    case "Client":
                        for (int i = 0; i < toLoadWhenClient.Length; i++)
                        {
                            GameObject temp = Instantiate(toLoadWhenClient[i]);
                            temp.name = toLoadWhenClient[i].name;
                        }
                        break;
                }
                loading = false;

            }
            if (mode == "Client")
            {
                //either attempting to connect or connected
            }
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("SinglePlayer"))
        {
            Destroy(gameObject);
            SceneManager.LoadScene("NDChessScene");
        }
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) JoinPressed();
        if (GUILayout.Button("Steam Host")) StartSteamHost();
        if (GUILayout.Button("Steam Host")) StartSteamJoin();
    }
    public void StartSteamHost()
    {
        Debug.Log("Trying to Host.");
        //NetworkManager.Singleton.NetworkConfig.NetworkTransport = gameObject.GetComponent<Mirror.FizzySteam.FizzySteamworks>();
    }
    public void StartSteamJoin()
    {
        Debug.Log("Trying to Join.");
        //should popup the steam join menu, then bring you to their lobby once done
    }
    public void JoinPressed()
    {
        if(serverIPText.text.Length>0)
        {
            Debug.Log("Trying to connect to |" + serverIPText.text + "|");
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = serverIPText.text;
        }
        else
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
        }
        NetworkManager.Singleton.StartClient();
    }
    public PieceInfo[] GetHostInfo(ref int[][] positions)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {//connected
            var player = networkedClient.PlayerObject;
            if (player)
            {
                PieceInfo[] temp = player.GetComponent<MultiplayerCommunication>().GetPieceInfos(out positions);
                if (temp != null)
                {
                    //Debug.Log("MainMenuManager received: " + ChessControllerND.PiecesToText(temp));
                    return temp;
                }
                //else Debug.Log("Error asking for PieceInfo"); //also occurs when connection is mid establishment
            }
            //else Debug.Log("No PlayerObject"); //also occurs when connection breaks
        }
        //else Debug.Log("No networkedClient");//also occurs before connection is established
        return null;
    }
    public void GetSecondaryHostInfo(ref string historyText, ref List<TurnRecord> fullHistory, ref int turn, ref double blackTurn, ref double whiteTurn)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {//connected
            var player = networkedClient.PlayerObject;
            if (player)
            {
                player.GetComponent<MultiplayerCommunication>().GetSecondaryInfo(out historyText, ref fullHistory, out turn, ref blackTurn, ref whiteTurn);
            }
            else Debug.Log("No PlayerObject (Secondary)"); //also occurs when connection breaks
        }
        else Debug.Log("No networkedClient (Secondary)");//also occurs before connection is established
    }

    static void StatusLabels()
    {
        mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
    public void WakeUpCall(GameObject you)//Poke!
    {
        if (you != null)
        {
            ChessControllerND created = you.GetComponent<ChessControllerND>();
            if (created != null)
                created.boardDimensions = new int[] { 8, 8, 2, 3 };//should eventually get swapped out to let host choose and client gets the host's

        }
        else
        {//this got triggered back when it used 'you = ...find main controller', so maybe GameObjects aren't fully created before awake is called.
            Debug.Log("Ah! Who poked me?!");
        }
    }
    public void MakeMoveOnHost(int[] coordsFrom, int[] coordsTo)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
        {//connected
            var player = networkedClient.PlayerObject;
            if (player)
            {
                player.GetComponent<MultiplayerCommunication>().MakeMoveOnHost(coordsFrom, coordsTo);
            }
            else Debug.Log("No PlayerObject (Secondary)"); //also occurs when connection breaks
        }
        else Debug.Log("No networkedClient (Secondary)");//also occurs before connection is established
    }*/
}
