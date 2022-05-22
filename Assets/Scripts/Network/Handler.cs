using System;
using UnityEngine;
using System.Collections.Generic;


public static partial class WindHandler
{
    public static void On_PlayerLoginResponse(WindNetwork.PlayerLoginResponse pck)
    {
        Debug.Log($"recv packet:{pck}");
        GameMgr.inst.OnStartGame();
    }
    public static void On_CreateRoleResponse(WindNetwork.CreateRoleResponse pck)
    {
        Debug.Log($"recv packet:{pck}");
    }
    public static void On_SpeakOnWorldResponse(WindNetwork.SpeakOnWorldResponse pck)
    {
        Debug.Log($"recv packet:{pck}");
    }
    public static void On_PlayerMoveResponse(WindNetwork.PlayerMoveResponse pck)
    {
        Debug.Log($"recv packet:{pck}");
        GameMgr.inst.OnPlayerMove(pck);
    }
    public static void On_PlayerJoinRoomResponse(WindNetwork.PlayerJoinRoomResponse pck)
    {
        Debug.Log($"recv packet:{pck}");
        GameMgr.inst.OnPlayerJoinRoom(pck);
    }

}

