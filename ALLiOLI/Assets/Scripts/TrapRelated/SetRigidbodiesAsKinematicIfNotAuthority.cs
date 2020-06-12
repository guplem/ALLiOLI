﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SetRigidbodiesAsKinematicIfNotAuthority : NetworkBehaviour
{
    [SerializeField] private bool searchChildren = true;

    public override void OnStartClient()
    {
        Rigidbody[] rigidbodies;
        if (searchChildren)
            rigidbodies = GetComponentsInChildren<Rigidbody>();
        else
            rigidbodies = GetComponents<Rigidbody>();

        foreach (Rigidbody rigidbody in rigidbodies)
            if (!rigidbody.isKinematic && !hasAuthority)
                rigidbody.isKinematic = true;
    }
}
