// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomMatchMaker : Photon.MonoBehaviour
{        
    public Text textCountDown;    
    public Transform[] cardHolders;
    public Transform backgrounds;
  
    private bool connectFailed = false;
    private bool joined = false;

    public static readonly string SceneNameMenu = "Scene_Waiting";
    public static readonly string SceneNameGame = "Scene_Game";

    private string errorDialog;
    private double timeToClearDialog;
    private float timeCountDown;

    private bool isStartGame;
    private float timeStart;

    private GameObject player;

    public string ErrorDialog
    {
        get { return this.errorDialog; }
        private set
        {
            this.errorDialog = value;
            if (!string.IsNullOrEmpty(value))
            {
                this.timeToClearDialog = Time.time + 4.0f;
            }
        }
    }

    public void Awake()
    {        
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.offlineMode = false;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.1");
            //PhotonNetwork.ConnectToMaster("192.168.1.18", 5055, "26dafad4-ed7b-42be-a64d-26ce161fede3", "0.1");
        }

        // generate a name for this player, if none is assigned yet
        if (String.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = PlayerData.Current.name;            
        }

        // if you wanted more debug out, turn this on:
        PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
    }

    //public void OnGUI()
    //{
    //    GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

    //    if (!string.IsNullOrEmpty(ErrorDialog))
    //    {
    //        GUILayout.Label(ErrorDialog);

    //        if (this.timeToClearDialog < Time.time)
    //        {
    //            this.timeToClearDialog = 0;
    //            ErrorDialog = "";
    //        }
    //    }

    //}

    public void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        joined = true;
        string dragonType = PlayerData.Current.CurrentDragon.element.ToString();        

        string dragonPrefab = dragonType + "Dragon";
        player = PhotonNetwork.Instantiate(dragonPrefab, cardHolders[PhotonNetwork.playerList.Length - 1].position, Quaternion.identity, 0);
        var playerController = player.GetComponent<PlayerController>();
        playerController.Name = PlayerData.Current.name;
        playerController.ParseId = PlayerData.Current.id;
        playerController.AvatarUrl = PlayerData.Current.avatarUrl;

        player.transform.parent = cardHolders[PhotonNetwork.playerList.Length - 1];
        player.transform.localPosition = Vector3.zero;

        string listName = GameUtils.GetRoomCustomProperty<string>("LIST_NAME", "");
        if (listName != "")
            listName = listName + "," + PlayerData.Current.name;
        else
            listName = PlayerData.Current.name;
        GameUtils.SetRoomCustomProperty<string>("LIST_NAME", listName);

        if (PhotonNetwork.isMasterClient)
        {
            int rand = Random.Range(0, 4);
            GameUtils.SetRoomCustomProperty<int>("MAP_ID", rand);
            backgrounds.GetChild(rand).gameObject.SetActive(true);
        }
        else
        {
            int mapId = GameUtils.GetRoomCustomProperty<int>("MAP_ID", 0);
            backgrounds.GetChild(mapId).gameObject.SetActive(true);
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate: " + info.sender);       
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("OnPhotonPlayerConnected: " + newPlayer.ID);
        GameObject[] listPlayer = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(listPlayer.Length);        
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
    }

    public void OnPhotonRandomJoinFailed()
    {
        ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { maxPlayers = 3 }, TypedLobby.Default);
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");       
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.networkingPeer.ServerAddress);
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        
    }

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");        
        // back to main menu        
        //Application.LoadLevel(WaitingMenu.SceneNameMenu);
    }

    void Update()
    {
        if (joined && PhotonNetwork.connected)
        {
            timeStart = GameUtils.GetRoomCustomProperty<float>("START_TIME", 0);
            if (!isStartGame && PhotonNetwork.playerList.Length >= 2 && timeStart == 0)
            {
                isStartGame = true;
                timeStart = (float)PhotonNetwork.time;
                GameUtils.SetRoomCustomProperty<float>("START_TIME", timeStart);
            }

            if (PhotonNetwork.playerList.Length >= 2)
            {
                timeCountDown = (float)(PhotonNetwork.time - timeStart);
                if (timeCountDown <= GameConsts.Instance.TIME_COUNT_DOWN_TO_PLAY + 1)
                    textCountDown.text = "Game Starting in " + (int)(GameConsts.Instance.TIME_COUNT_DOWN_TO_PLAY + 1 - timeCountDown) + "";

                if (timeCountDown >= GameConsts.Instance.TIME_COUNT_DOWN_TO_PLAY)
                {
                    PhotonNetwork.room.visible = false;

                    if (PhotonNetwork.isMasterClient)
                    {
                        //int rand = Random.Range(0, 4);
                        //GameUtils.SetRoomCustomProperty<int>("MAP_ID", rand);
                        PhotonNetwork.LoadLevel("Scene_Game");
                    }
                        
                    timeCountDown = GameConsts.Instance.TIME_COUNT_DOWN_TO_PLAY;                    
                }                
            }
        }
        
    }

    #region UI Delegate
    public void OnBackClick()
    {                      
        SoundManager.Instance.playButtonSound();
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        Application.LoadLevel("Scene_MainMenu");
    }
    #endregion
}
