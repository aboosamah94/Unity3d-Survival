﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponAttackState : MeleeState
{
    public override void EnterState(AgentController controller)
    {
        base.EnterState(controller);
    }

    public override void TransitionBackFromAnimation()
    {
        base.TransitionBackFromAnimation();
        DetermindNextState(controllerReference.meleeWeaponAttackState, controllerReference.meleeWeaponAimState);
    }
}
