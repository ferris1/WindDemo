using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class MessInfo
{
    public string speakId;
    public string word;
}

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

    public GameObject SpeakPanel;
    public GameObject MessRoot;
    public GameObject MessText;
    public InputField messInput;
    public GameObject messList;
    
    private List<MessInfo> WordList = new List<MessInfo>();


    private void Awake()
    {
        inst = this;
        onlinePlayers = new Dictionary<string, PlayerMovementInputController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpeakPanel.SetActive(false);
        ConnectPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        WindNetwork.Agent.GetInstance().NetUpdate();
    }

    public void OnNetConnect(bool result)
    {
        if (result)
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
    public void OnCloseMess()
    {
        messList.SetActive(false);
    }
    public void OnInputValueChange(string text)
    {
        messList.SetActive(true);
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
        inGame = true;
        ConnectPanel.SetActive(false);
        SendJoinRoomPakcet();
    }
    public void SendMovePakcet()
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
        Debug.Log($"111SendMovePakcet {playerId} _move.x£º{req.Move.X}, _move.y£º{req.Move.Y}");

    }
    public void OnPlayerMove(WindNetwork.PlayerMoveResponse res)
    {
        if (res.PlayerId == playerId) return;
        if (!onlinePlayers.ContainsKey(res.PlayerId) && res.PlayerId != playerId)
        {
            PlayerJoinRoom(res.PlayerId);
        }

        var target = onlinePlayers[res.PlayerId];
        if (target != null)
        {
            target._move.x = res.Move.X;
            target._move.y = res.Move.Y;

            target._look.x = res.Look.X;
            target._look.y = res.Look.Y;
        }
        Debug.Log($"OnPlayerMove {res.PlayerId} _move.x£º{res.Move.X}, _move.y£º{res.Move.Y}");
    }
    public void SendJoinRoomPakcet()
    {
        var req = new WindNetwork.PlayerJoinRoomRequest();
        req.PlayerId = playerId;
        WindNetwork.Agent.GetInstance().SendRequest(req);
    }


    public void OnPlayerJoinRoom(WindNetwork.PlayerJoinRoomResponse res)
    {
        
        if (res.PlayerId == playerId)
        {
            SpeakPanel.SetActive(true);
        }
        else
        {
            if (onlinePlayers.ContainsKey(res.PlayerId))
            {
                Debug.Log($"always in room:{res.PlayerId}");
                return;
            }
            PlayerJoinRoom(res.PlayerId);

        }

    }
    public void PlayerJoinRoom(string playerId)
    {
        var playerInst = Instantiate(Player);
        playerInst.SetActive(true);
        onlinePlayers[playerId] = playerInst.GetComponent<PlayerMovementInputController>();
        Debug.Log($"OnPlayerJoinRoom {playerId}");
    }

    public void OnPlayerUpdateTransform(WindNetwork.PlayerUpdateTransformResponse res)
    {
        if (res.PlayerId == playerId) return;
        if (!onlinePlayers.ContainsKey(res.PlayerId))
        {
            PlayerJoinRoom(res.PlayerId);
        }

        var target = onlinePlayers[res.PlayerId];

        target.gameObject.transform.position = new Vector3(res.Position.X, res.Position.Y, res.Position.Z);
        target.gameObject.transform.rotation = Quaternion.Euler(0, res.Rotation.Y, 0);
        Debug.LogError($"OnPlayerUpdateTransform:{res.PlayerId}  position:{target.gameObject.transform.position}  " +
           $"rotation:{target.gameObject.transform.rotation}");

    }

    public void SendPlayerTransform()
    {
        var req = new WindNetwork.PlayerUpdateTransformRequest();
        req.PlayerId = playerId;
        req.Position = new WindNetwork.Vector3();
        req.Position.X = _movement.transform.position.x;
        req.Position.Y = _movement.transform.position.y;
        req.Position.Z = _movement.transform.position.z;

        req.Rotation = new WindNetwork.Vector3();
        req.Rotation.X = _movement.transform.rotation.x;
        req.Rotation.Y = _movement.transform.rotation.y;
        req.Rotation.Z = _movement.transform.rotation.z;
        WindNetwork.Agent.GetInstance().SendRequest(req);
        Debug.LogError($"SendPlayerTransform position:{playerId} {_movement.transform.position}  " +
            $"rotation:{_movement.transform.rotation}");
    }

    public void OnPlayerSpeak(WindNetwork.SpeakOnWorldResponse pck)
    {
        var data = new MessInfo();
        data.speakId = pck.SpeakId;
        data.word = pck.Content;
        WordList.Add(data);
        var MaxCount = Math.Max(WordList.Count, MessRoot.transform.childCount);
        for (int i = 0; i < MaxCount; i++)
        {

            GameObject obj;
            bool isShow = true;
            if (i + 1 <= MessRoot.transform.childCount)
            {
                obj = MessRoot.transform.GetChild(i).gameObject;
                if (i + 1 > WordList.Count)
                {
                    obj.SetActive(false);
                    isShow = false;
                }
            }
            else
            {
                obj = Instantiate(MessText, MessRoot.transform);
            }
            if (isShow)
            {
                obj.SetActive(true);
                var info = WordList[i];
                obj.GetComponent<Text>().text = $"{info.speakId}: {info.word}";
            }
        }
    }

    public void OnPlayerSend()
    {
        if (messInput.text == "") return;
        var req = new WindNetwork.SpeakOnWorldRequest();
        req.PlayerId = playerId;
        req.Name = playerId;
        req.Content = messInput.text;
        WindNetwork.Agent.GetInstance().SendRequest(req);
        messInput.text = "";
    }
}
