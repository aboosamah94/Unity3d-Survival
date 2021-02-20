using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponAttackTwoState : MeleeState
{
    public override void EnterState(AgentController controller)
    {
        base.EnterState(controller);
    }

    public override void TransitionBackFromAnimation()
    {
        base.TransitionBackFromAnimation();
        DetermindNextState(controllerReference.meleeWeaponAttackThree, controllerReference.meleeWeaponAimState);
    }
}