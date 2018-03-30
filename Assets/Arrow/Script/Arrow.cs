using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

	public Rigidbody rb;
    public float fallMultiplier = 0.68f;
    public float lowJumpMultiplier = 1f;
    public Vector3 direction = Vector3.zero;

	public AudioClip[] arrowImpactSound = null;

	private AudioSource source = null;

	void Awake()
	{
		source = GetComponent<AudioSource>();
	}

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

            Vector3 dragVector = Vector3.zero;
            if (rb.velocity.y < 0)
            {
                dragVector = Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            //tweak de la décélération en montée
            else if (rb.velocity.y > 0)
            {
                dragVector = Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
            rb.velocity += dragVector;
        }
    }

    public static Vector3 GetRotationFromVelocity(Vector3 velocity)
    {
        float angleZ = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        return new Vector3(0, 0, angleZ);
    }

	void OnCollisionEnter(Collision collision){
		if(collision.gameObject.layer.Equals(LayerMask.NameToLayer("arena"))){
			//source.PlayOneShot(arrowImpactSound[Random.Range(0, arrowImpactSound.Length)], 0.5f);
			Debug.Log ("BURURH");
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
