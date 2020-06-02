﻿using System;
using FMOD;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Character))]
public class CharacterMovementController : NetworkBehaviour
{
    [Space] [SerializeField] private Animator animator;
    
    [NonSerialized] public Vector2 horizontalMovementInput;
    [SerializeField] private float jumpSpeed;

    private bool onGround;
    private float verticalSpeed;

    [Header("Configuration")] [SerializeField]
    private float walkSpeed;

    public bool jumping { get; set; }

    public CharacterController CharacterController { get; private set; }
    public Character Character { get; private set; }
    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = .1f;
    [SerializeField] private float turnSmoothVelocity;

    private void Awake()
    {
        CharacterController = gameObject.GetComponentRequired<CharacterController>();
        Character = gameObject.GetComponentRequired<Character>();
    }

    private void Update()
    {
        if (hasAuthority)
            AuthorityUpdate();
    }

    private void AuthorityUpdate()
    {
        Vector3 direction = GetDirectionRelativeToTheCamera();

        // Calculate walking the distance
        Vector3 displacement = direction * (walkSpeed * Time.deltaTime);

        //Jump
        if (onGround && jumping)
        {
            verticalSpeed = jumpSpeed;
            //TODO: Play jump sound event
            var instance = SoundManager.instance;
            instance.PlayOneShotAllClients(SoundEventPaths.jumpPath,this.transform.position,null);
        }

        // Apply gravity
        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        displacement.y = verticalSpeed * Time.deltaTime;

        if (Math.Abs(displacement.y) < CharacterController.minMoveDistance)
            Debug.LogWarning("WAT displacement.y = " + displacement.y);

        // Apply Movement to Player
        CollisionFlags collisionFlags = CharacterController.Move(displacement);

        //Apply rotation to Player
        if (direction.magnitude >= .1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f,angle,0f);
        }

        // Process vertical collisions
        if ((collisionFlags & CollisionFlags.Below) != 0)
        {
            onGround = true;
            verticalSpeed = 0.0f;
        }
        else
        {
            onGround = false;
        }

        if ((collisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0.0f)
            verticalSpeed = 0.0f;

        GiveStateToAnimations(displacement);
    }

    private void GiveStateToAnimations(Vector3 displacement)
    {
        animator.SetFloat("HorMove", Mathf.Abs(displacement.ToVector2WithoutY().magnitude) * 10);
        animator.SetBool("Grounded", onGround);
    }

    private Vector3 GetDirectionRelativeToTheCamera()
    {
        Vector3 targetDirection = new Vector3(horizontalMovementInput.x, 0f, horizontalMovementInput.y);
        targetDirection = Character.Owner.HumanLocalPlayer.Camera.gameObject.transform.TransformDirection(targetDirection);
        targetDirection.y = 0.0f;
        return targetDirection.normalized;
    }
}