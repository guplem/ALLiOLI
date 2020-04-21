﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class ATrap : MonoBehaviour
{

    public bool placed = false;
    public float activatableRange;
    public float cooldownTime;

    public bool OnCD => cdTimer > 0;

    private float cdTimer;
    

    private void Update()
    {
        cdTimer -= Time.deltaTime;
    }

    public void Init()
    {
        placed = true;
        
    }

    protected void Activate()
    {
        cdTimer = cooldownTime;
        
    }

    public float DistanceToNearestCharacter()
    {
        //TODO
        return 0;
    }
    public bool isActivatable()
    {
        return DistanceToNearestCharacter() <= activatableRange;
    }
    
}


