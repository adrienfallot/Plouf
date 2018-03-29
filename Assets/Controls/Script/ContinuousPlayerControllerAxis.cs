using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousPlayerControllerAxis : PlayerControllerAxis {

    protected float value;
    protected Action<float> axisAction;

    public ContinuousPlayerControllerAxis(Player iPlayer, string iName, Action<float> iAxisAction)
    {
        player = iPlayer;
        name = iName;
        axisAction = iAxisAction;
    }

    public override void ExecuteInput()
    {
        if(player.enabled){
            value = Input.GetAxis(name);
            axisAction(value);
        }
    }
}
