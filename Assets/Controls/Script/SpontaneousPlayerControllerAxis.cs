using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpontaneousPlayerControllerAxis : PlayerControllerAxis {

    protected Action axisAction;

    private bool m_AxisInUse = false;

    public SpontaneousPlayerControllerAxis(string iName, Action iAxisAction)
    {
        name = iName;
        axisAction = iAxisAction;
    }

    public override void ExecuteInput()
    {
        if(Input.GetButtonDown(name))
        {
            axisAction();
        }
    }
}
