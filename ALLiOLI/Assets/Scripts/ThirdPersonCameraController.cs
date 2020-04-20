﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Space]
    [SerializeField] private float yawRotationalSpeed = 200;
    [SerializeField] private float pitchRotationalSpeed = 200;
    [Space]
    [SerializeField] private float minPitch = 5;
    [SerializeField] private float maxPitch = 75;
    [Space]
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToTarget = 10;
    [Space]
    [SerializeField] private LayerMask layersThatCanClipCamera;
    [SerializeField] private float offsetOnCollision = 0.5f;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        float mouseAxisX = 0f;
        float mouseAxisY = 0f;
        
        if (Application.isFocused)
        {
            mouseAxisX = Input.GetAxis("Mouse X");
            mouseAxisY = Input.GetAxis("Mouse Y");
        }

        Vector3 eulerAngles = transform.eulerAngles;
        float yaw = (eulerAngles.y + 180.0f);
        float pitch = eulerAngles.x;

        yaw += yawRotationalSpeed * mouseAxisX * Time.deltaTime;
        yaw *= Mathf.Deg2Rad;
        if (pitch > 180.0f)
            pitch -= 360.0f;
        pitch += pitchRotationalSpeed * (-mouseAxisY) * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        pitch *= Mathf.Deg2Rad;
        Vector3 desiredPosition = target.position + new Vector3(Mathf.Sin(yaw) * Mathf.Cos(pitch) * distanceToTarget,  Mathf.Sin(pitch) * distanceToTarget, Mathf.Cos(yaw) * Mathf.Cos(pitch) * distanceToTarget);
        Vector3 direction = target.position - desiredPosition;


        direction /= distanceToTarget; //TODO: Isn't it 'normalize'?

        RaycastHit raycastHit;
        Ray ray = new Ray(target.position, -direction);
        Debug.DrawRay(ray.origin, ray.direction, Color.magenta, 0.1f );
        if (Physics.Raycast(ray, out raycastHit, distanceToTarget, layersThatCanClipCamera))
        {
            desiredPosition = raycastHit.point + direction * offsetOnCollision;
        }
            
        transform.forward = direction;
        transform.position = desiredPosition;
    }
}
