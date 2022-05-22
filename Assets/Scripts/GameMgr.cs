using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class GameMgr : MonoBehaviour
{
    public static GameMgr inst;
    public PlayerMovementInputController _movement;
    public GameObject ConnectPanel;
    public Transform onlineRoot;
    public GameObject Player;

    private Dictionary<string, PlayerMovementInputController> onlinePlayers; 
    public InputField input;
    private bool inGame;
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;
    private string playerId = "WindNetwork";

    private void Awake()
    {
        inst = this;
        WindNetwork.Agent.GenInstance();
        onlinePlayers = new Dictionary<string, PlayerMovementInputController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        WindNetwork.Agent.GetInstance().NetUpdate();
    }

    public void OnNetConnect(bool result)
    {
        if(result)
        {
            Debug.Log("OnNetConnect send PlayerLoginRequest ");
            var req = new WindNetwork.PlayerLoginRequest();
            req.PlayerId = playerId;
            WindNetwork.Agent.GetInstance().SendRequest(req);
        }
        else
        {
            Debug.Log("connect fail");
        }
    }

    public void OnNetDisconnect()
    {

    }
    public void OnConnectBtn()
    {
        Debug.Log("start connect server");
        if (input.text != "")
        {
            playerId = input.text;
            WindNetwork.Agent.GetInstance().ConnectToServer();
        }
        else
        {
            Debug.Log("please input account name");
        }
    }

    public void OnStartGame()
    {
        ConnectPanel.SetActive(false);
        SendJoinRoomPakcet();
    }
    public void SendMovePakcet()
    {
        if (inGame)
        {
            var req = new WindNetwork.PlayerMoveRequest();
            req.PlayerId = playerId;
            req.Move = new WindNetwork.Vector2();
            req.Move.X = _movement._move.x;
            req.Move.Y = _movement._move.y;

            req.Look = new WindNetwork.Vector2();
            req.Look.X = _movement._look.x;
            req.Look.Y = _movement._look.y;
            WindNetwork.Agent.GetInstance().SendRequest(req);
            Debug.Log("SendMovePakcet ");
        }
    }
    public void SendJoinRoomPakcet()
    {
        var req = new WindNetwork.PlayerJoinRoomRequest();
        req.PlayerId = playerId;
        WindNetwork.Agent.GetInstance().SendRequest(req);
    }

    public void OnPlayerMove(WindNetwork.PlayerMoveResponse res)
    {
        if (res.PlayerId == playerId) return;
        if (!onlinePlayers.ContainsKey(res.PlayerId))
        {
            Debug.LogError($"no target:{res.PlayerId} move");
            return;
        }

        var target = onlinePlayers[res.PlayerId];
        if (target != null)
        {
            target._move.x = res.Move.X;
            target._move.y = res.Move.Y;

            target._look.x = res.Look.X;
            target._look.y = res.Look.Y;
        }
    }

    public void OnPlayerJoinRoom(WindNetwork.PlayerJoinRoomResponse res)
    {
        if (res.PlayerId == playerId)
        {
            inGame = true;
        }
        else
        {
            if (onlinePlayers.ContainsKey(res.PlayerId))
            {
                Debug.Log($"always in room:{res.PlayerId}");
                return;
            }
            var playerInst = Instantiate(Player, onlineRoot);
            playerInst.SetActive(true);
            onlinePlayers[res.PlayerId] = playerInst.GetComponent<PlayerMovementInputController>();
            Debug.Log($"OnPlayerJoinRoom {res.PlayerId}");
        }

    }

}
