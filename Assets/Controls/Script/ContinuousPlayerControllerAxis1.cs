using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousPlayerControllerAxis : PlayerControllerAxis {

    protected float value;
    protected Action<float> axisAction;

    public ContinuousPlayerControllerAxis(string iName, Action<float> iAxisAction)
    {
        name = iName;
        axisAction = iAxisAction;
    }

    public override void ExecuteInput()
    {
        value = Input.GetAxisRaw(name);
        axisAction(value);
    }
}
