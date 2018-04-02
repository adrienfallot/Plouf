using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feet : MonoBehaviour {
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Head"))
        {
            Player target = other.GetComponentInParent<Player>();
            Player source = GetComponentInParent<Player>();
            target.KillByDeathFromAbove(source);
        }
    }
}
