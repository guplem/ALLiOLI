﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrap : Trap
{
    [SerializeField] private float explosionForce;
    [SerializeField] private Transform explosionPos;
    [SerializeField] private float explosionRadius;
    private Pool pool;

    [SerializeField] private List<Transform> projectilePos;
    [SerializeField] private GameObject projectilePrefab;

    private void Awake()
    {
        pool = new Pool(projectilePrefab, projectilePos.Count, transform.position, Quaternion.identity);
    }

    protected override void Reload()
    {
    }

    public override void Activate()
    {
        base.Activate();

        foreach (Transform trans in projectilePos)
        {
            GameObject projectile = pool.Spawn(trans.position, Quaternion.identity, Vector3.one);
            StartCoroutine(AddForce(projectile));
        }
    }

    private IEnumerator AddForce(GameObject projectile)
    {
        yield return new WaitForFixedUpdate();
        projectile.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionPos.position, explosionRadius,
            0.03f, ForceMode.Impulse);
    }
}