﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private Character character;

    private void OnCameraMove(InputValue value)
    {
        cameraManager.cameraMovement = value.Get<Vector2>();
    }
}
