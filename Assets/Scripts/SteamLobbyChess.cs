using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class SteamLobbyChess : MonoBehaviour
{
    //Callbacks
    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    //Variables
    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddress";
    private CustomNetworkManager manager;

    //Game Objects
    private static TMPro.TextMeshProUGUI debugSubstitute;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (!SteamManager.Initialized) {
            Debug.LogWarning("Steam Not Enabled");
            ToScreen("Steam Not Enabled");
            return; 
        }
        manager = GameObject.Find("NetworkManager").GetComponent<CustomNetworkManager>();
        
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered); 

    }

    public void StartSinglePlayer()
    {
        Destroy(GameObject.Find("NetworkManager"));
        Destroy(gameObject);
        SceneManager.LoadScene("NDChessScene");
    }

    public void HostLobby()
    {

        if (manager==null)
        {
            ToScreen("Can't find manager");
            Debug.LogWarning("Can't find manager");
        }
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,manager.maxConnections);
    }
    bool testvar = false;
    public void JoinLobby()
    {
        //pop up steam overlay
        testvar = !testvar;
        ToScreen(testvar.ToString());
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK) { return; }

        ToScreen("Lobby created Succesfully");
        manager.StartHost();
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),HostAddressKey,SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");//set the Name variable to username
    }
    
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        ToScreen("Someone requested to join the lobby!");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        //Everyone
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        ToScreen("I've connected?");


        //LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        //Host
        if (NetworkServer.active == true)
        {

        }
        //Client
        else
        {
            manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
            manager.StartClient();
        }

    }

    /// <summary>
    /// A method similar to debug.log, but displays to the screen instead so it's visible in builds.
    /// </summary>
    /// <param name="s">Message to display</param>
    public static void ToScreen(string s)
    {
        if(debugSubstitute==null)
        {
            debugSubstitute = Instantiate(Resources.Load<GameObject>("Prefabs/DebugDisplayText")).GetComponent<TMPro.TextMeshProUGUI>();
            DontDestroyOnLoad(debugSubstitute.gameObject);
        }
        if (debugSubstitute.text.Length > 300)
        {
            debugSubstitute.text = (s + "\n") + debugSubstitute.text.Substring(0, 300);
        }
        else
            debugSubstitute.text = (s + "\n") + debugSubstitute.text;
        Debug.Log("Screen: "+s);
    }
}
