﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Character : MonoBehaviour
{
    public Vector3 movement { get { return _movement;} set { _movement = value.normalized; } }
    private Vector3 _movement;
    [SerializeField] private float speed = 10;
    [SerializeField] private List<ATrap> ownedTraps;
    private Rigidbody rb { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.velocity = movement * speed;
    }

    public void ActivateTrap()
    {
       List<ATrap> trapsToActivate = new List<ATrap>();
        //TODO:If there isn't any trap activatable, then activate the nearest one if it isn't on CD
       
        //Find activatable traps
        foreach (ATrap trap in ownedTraps)
        {
            if (trap.IsActivatable())
            {
                trapsToActivate.Add(trap);
            }
        }

        //Activate activatable traps
        if (trapsToActivate.Count != 0)
        {
            foreach (ATrap trap in trapsToActivate)
            {
                trap.Activate();
            }
            return;
        }
        
        //There are no activatable traps
        //TODO: Activate the nearest Not-on-CD trap
        GetClosestTrap(false).Activate();
        throw new NotImplementedException();
    }

    public void SetUpTrap()
    {
        ATrap trap = GetClosestTrap();
        trap.SetUp();
        if (!ownedTraps.Remove(trap))
        {
            ownedTraps.Add(trap);
        }
    }

    private ATrap GetClosestTrap()
    {
        ATrap closestTrap = ownedTraps[1];
        foreach (ATrap trap in ownedTraps)
        {
            if (Vector3.Distance(trap.transform.position, transform.position) <
                Vector3.Distance(closestTrap.transform.position, transform.position))
                closestTrap = trap;
        }

        return closestTrap;
    }

    private ATrap GetClosestTrap(bool cdConstraint)
    {
        ATrap closestTrap = ownedTraps[1];
        foreach (ATrap trap in ownedTraps)
        {
            if (Vector3.Distance(trap.transform.position, transform.position) <
                Vector3.Distance(closestTrap.transform.position, transform.position) && trap.OnCd==cdConstraint)
                closestTrap = trap;
        }

        return closestTrap;
    }
}
