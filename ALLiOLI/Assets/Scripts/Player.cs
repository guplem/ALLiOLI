﻿using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [Space]
    [SerializeField] private PlayerGuiManager playerGui;
    
    [Space] 
    [SerializeField] private GameObject characterPrefab;
    public Character character { get; private set; }
    
    [Space] 
    [SerializeField] private float maxDistanceToInteractWithTrap = 10;
    [SerializeField] private LayerMask layersThatCanInterfereWithInteractions;
    public TrapManager ownedTraps { get; private set; }
    public int maxOwnableTraps => 10 / MatchManager.Instance.players.Count;
    private Trap trapInFront;
    private GameObject lastObjectInFront;
    
    private PlayerInput playerInput;
    public ThirdPersonCamera playerCamera { get; private set;  }
    
    public Color color { get { return _color; } private set { _color = value; playerGui.SetColor(_color); } }
    private Color _color;
    public bool isReady { get { return _isReady; } set { _isReady = value;  playerGui.ShowReadiness(isReady); } }
    private bool _isReady;

    public void Setup(Color color)
    {
        ownedTraps = new TrapManager();
        
        playerInput = GetComponent<PlayerInput>();
        playerCamera = playerInput.camera.gameObject.GetComponent<ThirdPersonCamera>();
        this.color = color;
        gameObject.name = "Player " + playerInput.playerIndex + " - " + playerInput.user.controlScheme;

        SpawnNewCharacter();
    }
    
    private void Update()
    {
        UpdateObjectsInFront();
        //TODO: highlight the 'trapInFront'
    }

    private void UpdateObjectsInFront()
    {
        Ray ray = new Ray(character.cameraTarget.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistanceToInteractWithTrap, layersThatCanInterfereWithInteractions)) {
            if (lastObjectInFront != hit.collider.gameObject)
            {
                lastObjectInFront = hit.collider.gameObject;
                trapInFront = hit.transform.GetComponentInParent<Trap>();
            }
        }
        else
        {
            lastObjectInFront = null;
            trapInFront = null;
        }
    }

    public void SpawnNewCharacter()
    {
        this.character = Spawner.Instance.Spawn(characterPrefab).GetComponent<Character>();
        this.character.owner = this;
        playerCamera.Setup(this.character.cameraTarget);
    }

    #region Input

    private void OnCameraMove(InputValue value)
    {
        playerCamera.movement = value.Get<Vector2>();
    }

    private void OnCharacterMove(InputValue value)
    {
        character.movementControllerController.movement = value.Get<Vector2>();
    }

    private void OnTrap()
    {

        State currentState = MatchManager.Instance.currentPhase;
        
        switch (currentState)
        {
            case Battle battle:
                ownedTraps.GetBestTrapToActivate()?.Activate();
                break;
            case TrapUp trapUp:
                SetUpTrapInFront();
                break;
        }
    }

    private void OnReady()
    {
        isReady = !isReady;
    }

    private void OnSuicide()
    {
        character.Die();
        SpawnNewCharacter();
    }
    
    private void OnJump(InputValue value)
    {
        character.movementControllerController.jumping = value.isPressed;
    }
    
    #endregion
    
    private void SetUpTrapInFront()
    {
        if (trapInFront == null)
            return;

        if (!ownedTraps.Remove(trapInFront))
            ownedTraps.Add(trapInFront);
        
        playerGui.ShowNumberOfTraps(ownedTraps.Count, maxOwnableTraps);
        DebugPro.LogEnumerable(ownedTraps, ", ", "The current owned traps for the player " + gameObject.name +" are: ", gameObject);
    }
    
    public void SetupForCurrentPhase()
    {
        playerGui.SetupForCurrentPhase(this);
    }
}