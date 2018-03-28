using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class PlayerControllerAxis
{
    protected string name;
    protected Player player;

    abstract public void ExecuteInput();
}
