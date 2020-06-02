using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class MatchManager : NetworkBehaviour
{
    private float _matchTimer;

    public List<Client> clients { get; private set; }
    [SerializeField] private Color[] playerColors;

    [SyncVar(hook = nameof(SetNewPhaseById))]
    private int currentPhaseId = MatchPhaseManager.GetPhaseId(new WaitingForPlayers()); // -420; // Dummy value

    public void SetNewPhaseById(int oldVal, int newVal)
    {
        SetNewMatchPhase(MatchPhaseManager.GetNewMatchPhase(currentPhaseId));
    }

    public MatchPhase currentPhase { get; private set; }

    //public bool IsMatchRunning => Instance.CurrentPhase != null && Instance.CurrentPhase.Id() >= 0 && !(Instance.CurrentPhase is End);
    [SerializeField] public MatchGuiManager guiManager; // General GUI (not the player specific one)

    public TrapsManager AllTraps
    {
        get
        {
            if (_allTraps == null)
            {
                _allTraps = new TrapsManager();
                _allTraps.AddRange(Object.FindObjectsOfType<Trap>());
            }

            return _allTraps;
        }
    }

    private TrapsManager _allTraps;

    public float matchTimer
    {
        get => _matchTimer;
        set
        {
            _matchTimer = value;
            instance.guiManager.SetTimer(_matchTimer);
        }
    }

    public static MatchManager instance { get; private set; }

    public bool thereIsWinner => roundWinnerPlayerNetId != 0u; // '0u' is the default value for 'uint' type

    [field: SyncVar(hook = nameof(newRoundWinnerPlayerNetId))]
    public uint roundWinnerPlayerNetId { get; private set; }

    private void newRoundWinnerPlayerNetId(uint oldId, uint newId)
    {
        if (newRoundWinnerPlayerNetIdEvent != null) newRoundWinnerPlayerNetIdEvent();
        guiManager.UpdateEndScreen(); // TODO Change to event subscription like in EndRound phase
    }

    public Action newRoundWinnerPlayerNetIdEvent;

    public string roundWinnerName
    {
        get
        {
            Player winner = roundWinnerPlayer;
            string winnerName = winner != null ? winner.gameObject.name : "NULL";
            return winnerName;
        }
    }

    public Player roundWinnerPlayer
    {
        get => (NetworkManager.singleton as AllIOliNetworkManager)?.GetPlayer(roundWinnerPlayerNetId);
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple MatchManager have been created. Destroying the script of " + gameObject.name,
                gameObject);
            Destroy(this);
        }
        else
        {
            instance = this;
            clients = new List<Client>();
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        //BroadcastNewMatchPhase(new WaitingForPlayers());
    }


    private void Update()
    {
        if (isServer)
            UpdateServer();

        currentPhase?.UpdateState(Time.deltaTime);
    }

    [Server]
    private void UpdateServer()
    {
        State nextPhase = currentPhase?.GetCurrentState();

        if (currentPhase != nextPhase)
            BroadcastNewMatchPhase((MatchPhase) nextPhase);
    }


    [Server]
    public void BroadcastNewMatchPhase(MatchPhase newPhase)
    {
        currentPhaseId = MatchPhaseManager.GetPhaseId(newPhase);
    }

    [Client]
    public void FinishAndRestartCurrentPhase()
    {
        MatchPhase phase = currentPhase;

        if (phase == null)
        {
            phase = MatchPhaseManager.GetNewMatchPhase(currentPhaseId);
            Debug.Log(
                $"Restarting the CurrentPhase - obtained from the currentPhaseId '{currentPhaseId}' ({(phase != null ? phase.GetType().Name : "null")}) as a MatchPhase object.");
        }
        else
        {
            Debug.Log($"Restarting the CurrentPhase ({phase.GetType().Name}).");
        }

        SetNewMatchPhase(phase);
    }

    [Client]
    private void SetNewMatchPhase(MatchPhase newPhase)
    {
        Debug.Log(
            $"Switching match phase. From '{(currentPhase != null ? currentPhase.GetType().Name : "NULL")}' to '{(newPhase != null ? newPhase.GetType().Name : "NULL")}'.");

        if (isServer)
            SetAllPlayersAsNotReady();

        currentPhase?.EndState();

        currentPhase = newPhase;

        if (currentPhase == null)
            return;

        currentPhase.StartState();
        if (isServer) currentPhase.ServerStartState();

        guiManager.SetupForCurrentPhase(); // General GUI

        foreach (Client client in instance.clients)
            if (client.PlayersManager != null)
                foreach (Player player in client.PlayersManager.players)
                    player.SetupForCurrentPhase(); // Player's UI
    }

    [Server]
    public void SetAllPlayersAsNotReady()
    {
        foreach (Client client in instance.clients)
        {
            if (client.PlayersManager != null)
                foreach (Player player in client.PlayersManager.players)
                {
                    player.isReady = false;
                }
        }
    }

    private EventInstance outcomeInstance;

    [Server]
    public void FlagAtSpawn(Player carrier)
    {
        if (thereIsWinner)
        {
            EventInstance finishInstance =
                Client.LocalClient.SoundManager.PlayEventLocal(SoundEventPaths.finishPath, Vector3.zero);
            outcomeInstance.setVolume(50f);
            return;
        }

        roundWinnerPlayerNetId = carrier.netId;

        string path;
        if (roundWinnerPlayerNetId == Client.LocalClient.netId)
        {
            path = SoundEventPaths.winPath;
        }
        else
        {
            path = SoundEventPaths.defeatPath;
        }

        if (!path.IsNullOrEmpty())
            outcomeInstance = Client.LocalClient.SoundManager.PlayEventMovingLocal(path, carrier.Character.transform);
    }

    [Server]
    public void ResetWinner()
    {
        roundWinnerPlayerNetId = 0u;
    }

    public bool AreAllPlayersReady()
    {
        foreach (Client client in clients)
        foreach (Player player in client.PlayersManager.players)
            if (!player.isReady)
                return false;

        return true;
    }


    public static int TotalCurrentPlayers => instance.clients.Sum(client => client.PlayersManager.players.Count);
    //public static int indexOfLastPlayer = -1;

    public Color GetColor(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerColors.Length)
            return playerColors[playerIndex];

        //EasyRandom rnd = new EasyRandom(100+playerIndex*42);
        //const float hueMin = 0f;
        //const float hueMax = 1f;

        int steps = 6;
        float hue = 1f / (steps + 1f / (steps - 1)) * (playerIndex - 1);
        while (hue > 1)
            hue -= 1;
        // Debug.Log($"HUE {hue}");

        const float saturation = 0.85f;
        const float valueBrightness = 1f;

        Color rgb = Color.HSVToRGB(hue, saturation, valueBrightness, true);
        rgb.a = 1f;

        //Random.ColorHSV(0, 1f, 0.7f, 0.9f, 1f, 1f);
        return rgb;
    }

    public void StartMatch()
    {
        //MatchManager.instance.BroadcastNewMatchPhase(new WaitingForPlayers());
    }

    [Server]
    public void KillAllCharacters()
    {
        foreach (Client client in clients)
        foreach (Player player in client.PlayersManager.players)
            player.Character.ServerSuicide();
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        foreach (Trap trap in AllTraps)
        {
            Vector3 characterPosition = transform.position;
            Vector3 trapPos = trap.transform.position;
            //float halfHeight = (characterPosition.y-trapPos.y)*0.5f;
            //Vector3 offset = Vector3.up * halfHeight;

            Handles.DrawBezier(
                characterPosition, trapPos,
                characterPosition + Vector3.up, trap.transform.position + trap.transform.forward + Vector3.up,
                Color.green, EditorGUIUtility.whiteTexture, 1f);
        }
    }
#endif
}