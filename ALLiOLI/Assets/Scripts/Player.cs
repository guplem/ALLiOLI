﻿using System;
using System.Collections.Generic;
using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    /// <summary>
    /// The prefab of the character that whe player will use to play.
    /// </summary>
    [Space] [SerializeField] private GameObject characterPrefab;

    /// <summary>
    /// A reference to the current active character for the player
    /// </summary>
    public Character Character
    {
        get => _character;
        set {
            _character = value;
            if (IsControlledLocally)
                HumanLocalPlayer.Camera.SetTargetWithCinematics(value.cameraTarget,value.cameraTarget);
        }
    }
    // ReSharper disable once InconsistentNaming
    private Character _character;

    /// <summary>
    /// The color that identifies the player and their characters.
    /// </summary>
    [field: SyncVar(hook = nameof(SetColor))]
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    public Color Color { get; private set; }

    /// <summary>
    /// Updates the color in the playerGUI if it is a player with a humanLocalPlayer.
    /// </summary>
    /// <param name="oldColor">The old color.</param>
    /// <param name="newColor">The new color.</param>
    private void SetColor(Color oldColor, Color newColor)
    {
        if (HumanLocalPlayer)
            HumanLocalPlayer.playerGui.SetColor(newColor);
    }

    /// <summary>
    /// If the player is "ready" or if it is not.
    /// </summary>
    [field: SyncVar(hook = nameof(SetReadyOnGUI))]
    public bool isReady;

    /// <summary>
    /// Updates the playerGUI (if it is a player with a humanLocalPlayer).
    /// </summary>
    /// <param name="oldValue">The old state of readiness for the player.</param>
    /// <param name="newValue">The new state of readiness for the player.</param>
    private void SetReadyOnGUI(bool oldValue, bool newValue)
    {
        if (HumanLocalPlayer)
            HumanLocalPlayer.playerGui.ShowReadiness(newValue);
    }

    /// <summary>
    /// A reference to the HumanLocalPlayer in charge of controlling this player.
    /// <para>If it is null, it means that this player is controlled and synchronized trough network, not locally.</para>
    /// <para>Giving it a value will set the the value as the referenced/controled player in the "HumanLocalPlayer".</para>
    /// </summary>
    public HumanLocalPlayer HumanLocalPlayer
    {
        get => _humanLocalPlayer;
        /*private*/ set
        {
            if (HumanLocalPlayer != null) // TODO: add this same check (HumanLocalPlayer != null) whenever is needed to ensure consistency
            {
                Debug.LogWarning("Trying to change the humanLocalPlayer of a Player. Operation cancelled.");
                return;
            }
            
            _humanLocalPlayer = value;
            if (value != null)
            {
                _humanLocalPlayer.Player = this;
                HumanLocalPlayer.localPlayerNumber = Client.localClient.PlayersManager.players.Count;
            }
        }
    }
    private HumanLocalPlayer _humanLocalPlayer;


    [SyncVar(hook = nameof(NewIdOfHumanLocalPlayer))]
    public int idOfHumanLocalPlayer;
    private void NewIdOfHumanLocalPlayer(int oldVal, int newVal)
    {
        if (oldVal != 0)
            Debug.LogWarning("Trying to change the idOfHumanLocalPlayer of a Player. It shouldn't be done.");
    }

    /// <summary>
    /// If the player is controlled by a human in this machine (locally).
    /// </summary>
    private bool IsControlledLocally => HumanLocalPlayer != null;

    // Called on all clients (when this NetworkBehaviour is network-ready)
    public override void OnStartClient()
    {
        Client.localClient.PlayersManager.players.Add(this);

        string customName = "Player " + GameManager.TotalPlayers;

        // Is any human waiting for a player to be available? If it is, set the player as their property
        HumanLocalPlayer tempHumanLocalPlayer = null;
        
        HumanLocalPlayer[] allHumans = UnityEngine.Object.FindObjectsOfType<HumanLocalPlayer>();
        foreach (HumanLocalPlayer human in allHumans)
        {
            if (human.id == idOfHumanLocalPlayer)
                tempHumanLocalPlayer = human;
            
            // Debug.Log(GetInstanceID() + " SEARCHING HUMAN with id " + human.id + ". Player is looking for id " + idOfHumanLocalPlayer + ". Is it a match? " + (human.id == idOfHumanLocalPlayer));
        }

        if (tempHumanLocalPlayer != null) // If the player is controller locally
            gameObject.name = customName + " - Input by " + tempHumanLocalPlayer.PlayerInput.user.controlScheme;
        else
            gameObject.name = customName + " - Controlled remotely";

        HumanLocalPlayer = tempHumanLocalPlayer;

        if (hasAuthority)
        {
            CmdSetupPlayerOnServer();
            CmdSpawnNewCharacter();
        }
    }

    [Command]
    private void CmdSetupPlayerOnServer()
    {
        Color = GameManager.singleton.playerColors[GameManager.TotalPlayers - 1];
    }

    [Command]
    public void CmdSpawnNewCharacter()
    {
        Spawner.Instance.Spawn(characterPrefab, this.netId, connectionToClient);
    }

    public void SetupForCurrentPhase()
    {
        if (HumanLocalPlayer != null)
            HumanLocalPlayer.SetupForCurrentPhase();
    }

    [Command]
    public void CmdSetReady(bool newValue)
    {
        isReady = newValue;
    }
    
    [Command]
    public void CmdActivateTrap(uint trapNetId)
    {
        if (!NetworkIdentity.spawned.ContainsKey(trapNetId))
        {
            Debug.LogWarning("The trap with NetId " + trapNetId + " not found.");
            return;
        }
        
        NetworkIdentity.spawned[trapNetId].gameObject.GetComponent<Trap>().Activate();
    }
}