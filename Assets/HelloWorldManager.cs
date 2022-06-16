
using MLAPI;
using UnityEngine;

//namespace HelloWorld
//{
    public class HelloWorldManager : MonoBehaviour
    {
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

            //if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            //{
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                var player = networkedClient.PlayerObject.GetComponent<HelloWorldPlayer>();
                if (player)
                {
                    player.Move();
                }
            }
            //}
            //SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

        static void StartButtons()
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

    static void SubmitNewPosition()
    {
        
        if (GUILayout.Button("Shift"))
        {
            //if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId, out var networkedClient))
            {
                var playerObj = GameObject.Find("DefaultMultiplayerPlayer(Clone)");
                if (playerObj != null)
                {
                    var player = playerObj.GetComponent<HelloWorldPlayer>(); //networkedClient.PlayerObject.GetComponent<HelloWorldPlayer>();
                    if (player != null)
                    {
                        player.ShiftUp();
                    }
                }
                var plane = GameObject.Find("Plane"); //networkedClient.PlayerObject.GetComponent<HelloWorldPlayer>();
                if (plane)
                {
                    plane.transform.position += new Vector3(0, 1, 0);
                }
            }
        }
    }
    //}
}