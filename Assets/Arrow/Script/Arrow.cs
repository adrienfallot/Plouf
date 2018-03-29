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
			if (rb.velocity != Vector3.zero) {
				Vector3 vel = rb.velocity;
				float angleZ = Mathf.Atan2(vel.y,vel.x)*Mathf.Rad2Deg;

				transform.eulerAngles = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y,angleZ);
			}
		}
	}

	void OnCollisionEnter(Collision collision){
		if(collision.gameObject.layer.Equals(LayerMask.NameToLayer("arena"))){
			rb.isKinematic = true;
		}
	}
}
