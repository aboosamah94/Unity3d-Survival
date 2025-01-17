﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementState : BaseState
{
    protected float _defaultFallingDelay = 0.2f;
    protected float _fallingDelay = 0;

    public override void EnterState(AgentController controller)
    {
        base.EnterState(controller);
        _fallingDelay = _defaultFallingDelay;
    }

    public override void HandleCameraDirection(Vector3 input)
    {
        base.HandleCameraDirection(input);
        controllerReference.Movement.HandleMovementDirection(input);
    }

    public override void HandleEquipItemInput()
    {
        if (controllerReference.InventorySystem.WeaponEquipped)
        {
            controllerReference.TransitionToState(controllerReference.equipItemState);
        }
        else
        {
            controllerReference.TransitionToState(controllerReference.meleeUnarmedAim);
        }
    }

    public override void HandleMovement(Vector2 input)
    {
        base.HandleMovement(input);
        controllerReference.Movement.HandleMovement(input);
    }

    public override void HandleJumpInput()
    {
        controllerReference.TransitionToState(controllerReference.jumpState);
    }

    public override void HandleInventoryInput()
    {
        controllerReference.TransitionToState(controllerReference.inventoryState);
    }

    public override void HandlePrimaryInput()
    {
        base.HandlePrimaryInput();
    }

    public override void HandleSecondaryClickInput()
    {
        base.HandleSecondaryClickInput();
        controllerReference.TransitionToState(controllerReference.interactState);
    }

    public override void HandleHotBarInput(int hotbarKey)
    {
        base.HandleHotBarInput(hotbarKey);
        controllerReference.InventorySystem.HotbarShortKeyHandler(hotbarKey);
    }

    public override void HandleMenuInput()
    {
        base.HandleMenuInput();
        controllerReference.TransitionToState(controllerReference.menuState);
    }

    public override void HandlePlacementInput()
    {
        base.HandlePlacementInput();
        controllerReference.TransitionToState(controllerReference.placementState);
    }

    public override void Update()
    {
        base.Update();
        PreformDetection();
        HandleMovement(controllerReference.InputFromPlayer.MovementInputVector);
        HandleCameraDirection(controllerReference.InputFromPlayer.MovementDirectionVector);
        HandleFallingDown();
    }

    protected void HandleFallingDown()
    {
        if (controllerReference.Movement.CharacterIsGrounded() == false)
        {
            if (_fallingDelay > 0)
            {
                _fallingDelay -= Time.deltaTime;
                return;
            }
            controllerReference.TransitionToState(controllerReference.fallingState);
        }
        else
        {
            _fallingDelay = _defaultFallingDelay;
        }
    }

    private void PreformDetection()
    {
        controllerReference.DetectionSystem.PreformDetection(controllerReference.InputFromPlayer.MovementDirectionVector);
    }
}
