﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

[RequireComponent(typeof(SimpleAnimationsManager))]
public class Shield : Trap
{
    private SimpleAnimationsManager animManager;
    [SerializeField] private Rigidbody shield;
    [SerializeField] private Vector3 throwDirection;
    [SerializeField] private float throwSpeed;

    public void Awake() 
    {
        animManager = gameObject.GetComponent<SimpleAnimationsManager>();
    }

    protected override void Reload()
    {
        ((TransformAnimation)animManager.GetAnimation(0)).originTransform.SetProperties(shield.transform);
        shield.isKinematic = true;
        animManager.Play(0);
    }

    public override void Activate()
    {
        base.Activate();
        
        animManager.Stop(0);
        shield.isKinematic = false;
        StartCoroutine(Impulse());
    }

    private IEnumerator Impulse()
    {
        yield return new WaitForFixedUpdate(); 
        shield.velocity = throwDirection.normalized * throwSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(shield.transform.position, throwDirection);
    }
}