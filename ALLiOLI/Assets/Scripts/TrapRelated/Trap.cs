﻿using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    [SerializeField] private RadarTriggerTrap radarTrigger;
    [SerializeField] private KillZone myKillZone;
    
    [SerializeField] private float cooldownTime = 5f;
    public bool OnCd => cdTimer > 0;
    public float cdTimer { get; private set; }
    
    [SerializeField] private float durationTime = 3f; // must be greater than the cdTimer
    public bool active => activatedTimer > 0;
    public float activatedTimer  { get; private set; }

    private void Update()
    {
        if (OnCd) {
            cdTimer -= Time.deltaTime;
        }

        if (active)
        {
            activatedTimer-= Time.deltaTime;
           if(!active) Reload();
        }
    }

    protected virtual void Reload()
    {
        if (myKillZone) myKillZone.enabled = false;
    }

    public virtual void Activate()
    {
        Debug.Log("The trap "+gameObject.name+" has been activated.");
        if (myKillZone) myKillZone.enabled = true;

        cdTimer = cooldownTime;
        activatedTimer = durationTime;

    }

    public bool IsActivatable()
    {
        return !OnCd;
    }

    public float GetDistanceTo(Character character)
    {
        return radarTrigger.GetRadarDistanceTo(character);
    }

    public SortedList<float, Character> GetCharactersInTrapRadar(Player exception)
    {
        return radarTrigger.GetCharactersInTrapRadar(exception);
    }
}