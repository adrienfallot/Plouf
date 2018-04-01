using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMover : MonoBehaviour {

    private IEnumerator LerpVelocityTo(Transform iToMove, Vector3 iNewPos, float iTime)
    {
        float elapsedTime = 0;
        Vector3 startingPos = iToMove.position;
        while (elapsedTime < iTime)
        {
            iToMove.transform.position = Vector3.Lerp(startingPos,
                                                iNewPos,
                                                (elapsedTime / iTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
