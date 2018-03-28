using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlayerControllerAxis
{
    protected string name;

    abstract public void ExecuteInput();
}
