using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Mirror;
public class CustomNetworkManager : NetworkManager
{
    public GameObject[] toLoadWhenClient;
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        SteamLobbyChess.ToScreen("I have connected!");
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        //SteamLobbyChess.ToScreen("Player number " + NetworkServer.connections.Count + " has started joining");
    }
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
        SteamLobbyChess.ToScreen("Player number " + NetworkServer.connections.Count + " has start joined");
    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        SceneManager.LoadScene("NDChessScene");
        return;
    }
    public override void OnStartClient()
    {
        for (int i = 0; i < toLoadWhenClient.Length; i++)
        {
            GameObject temp = Instantiate(toLoadWhenClient[i]);
            temp.name = toLoadWhenClient[i].name;
        }
        base.OnStartClient();
    }
}
