using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeUnarmedAttackOneState : MeleeState
{
    public override void EnterState(AgentController controller)
    {
        base.EnterState(controller);
    }

    public override void TransitionBackFromAnimation()
    {
        base.TransitionBackFromAnimation();
        DetermindNextState(controllerReference.meleeUnarmedAttackTwo, controllerReference.meleeUnarmedAim);
    }
}