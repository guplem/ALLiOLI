﻿using System;
using UnityEngine;

public class WaitingForPlayers : MatchPhase
{
    public override string informativeText
    {
        get => "Press any button to join.\nWaiting for everyone to be ready.";
        protected set { }
    }

    public override bool showTrapsCounter
    {
        get => false;
        protected set { }
    }

    public override bool showReadiness
    {
        get => true;
        protected set { }
    }

    public override bool showMatchTimer
    {
        get => false;
        protected set { }
    }

    public override void StartState()
    {
        Debug.Log("STAGE 0 - Starting phase 'WaitingForPlayers'.");
        MatchManager.Instance.MatchTimer = 1; // Securing timing, can not end instantly

        GameManager.Instance.GUI.SetStartMatchConfiguration();
    }
    
    public override void ServerStartState() {}

    public override void UpdateState(float deltaTime)
    {
        if (MatchManager.Instance.MatchTimer > 0)
            MatchManager.Instance.MatchTimer -= deltaTime;
    }

    public override State GetCurrentState()
    {
        //Debug.Log($"TotalCurrentPlayers = {MatchManager.TotalCurrentPlayers}, MatchTimer = {MatchManager.Instance.MatchTimer}");
        
        if (MatchManager.TotalCurrentPlayers <= 0 || MatchManager.Instance.MatchTimer > 0)
            return this;
        
        if ( MatchManager.Instance.AreAllPlayersReady())
            return new StartCountdown();

        return this;
    }

    public override void EndState()
    {
        
    }
}