using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpontaneousPlayerControllerAxis : PlayerControllerAxis {

    protected Action axisAction;

    private bool m_AxisInUse = false;

    public SpontaneousPlayerControllerAxis(Player iPlayer, string iName, Action iAxisAction)
    {
        player = iPlayer;
        name = iName;
        axisAction = iAxisAction;
    }

    public override void ExecuteInput()
    {
        if(player.enabled){
            if(Input.GetButtonDown(name))
            {
                axisAction();
            }
        }
    }
}
