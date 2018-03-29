using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

	public Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if (rb != null) {
			if (rb.velocity != Vector3.zero)
            {
                transform.eulerAngles = GetRotationFromVelocity(rb.velocity);
            }
		}
    }

    public static Vector3 GetRotationFromVelocity(Vector3 velocity)
    {
        float angleZ = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        return new Vector3(0, 0, angleZ);
    }

	void OnCollisionEnter(Collision collision){
		if(collision.gameObject.layer.Equals(LayerMask.NameToLayer("arena"))){
			rb.isKinematic = true;
		}
	}

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("arena")))
        {
            rb.isKinematic = true;
        }
    }
}
