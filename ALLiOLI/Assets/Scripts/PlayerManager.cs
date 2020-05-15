﻿using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : NetworkBehaviour
{
    public HashSet<Player> players { get; private set; }

    // Called on the local client (when this player object is network-ready)
    public override void OnStartClient()
    {
        base.OnStartClient();
        players = new HashSet<Player>();
    }
    

    /*private void OnPlayerJoined(PlayerInput playerInput)
    {
        Player player = playerInput.GetComponent<Player>();
        players.Add(player);
        player.Setup(GameManager.singleton.playerColors[playerInput.playerIndex]);

        MatchManager.Instance.SetAllPlayersAsNotReady();

        Debug.Log("Player " + playerInput.playerIndex + " joined with input device: " + playerInput.devices[0],
            playerInput.gameObject);

        Debug.Log("Player " + playerInput.playerIndex + " joined with input device: " + playerInput.devices[0],
            playerInput.gameObject);
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log("Player left with input device: " + playerInput.devices[0], playerInput.gameObject);
        players.Remove(playerInput.gameObject.GetComponent<Player>());
    }*/
}