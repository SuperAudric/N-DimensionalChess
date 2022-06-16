
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

//namespace HelloWorld
//{
public class HelloWorldPlayer : NetworkBehaviour
{
    
    public NetworkVariable<Vector3> Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart()
    {
        Move();
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            //GameObject.Find("NetworkManager").GetComponent<MainMenuManager>().TestingThing();
            SubmitPositionRequestServerRpc();
        }
    }
    public void ShiftUp()
    {
        //Debug.Log("Shifting");
        transform.position += new Vector3(0, 0.1f, 0);
    }
    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("This appears on the host when the client calls it");
        //Debug.Log(NetworkManager.Singleton.IsHost ?"Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client");
        Position.Value = GetRandomPositionOnPlane();
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    void Update()
    {
        //if(NetworkManager.Singleton.IsServer)
        transform.position = Position.Value;
    }
}
//}