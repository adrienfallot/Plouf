using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportOnMapExit : MonoBehaviour {

	public float teleportOffset = 0.3f;

	private Bounds m_bounds;

	void Start() {
		m_bounds = GetComponent<Collider>().bounds;
	}


	void OnTriggerExit(Collider other) 
	{
		Vector3 otherPosition = other.transform.position;
		Vector3 thisPosition = transform.position;

		float newXPosition = otherPosition.x;
		float newYPosition = otherPosition.y;
		
		float distanceX = thisPosition.x - otherPosition.x;
		float distanceY = thisPosition.y - otherPosition.y;

		if( distanceX > m_bounds.extents.x || distanceX < -m_bounds.extents.x){
			newXPosition += (otherPosition.x < thisPosition.x ? (m_bounds.size.x - teleportOffset) : (-m_bounds.size.x + teleportOffset));
		}
		if( distanceY > m_bounds.extents.y || distanceY < -m_bounds.extents.y){
			newYPosition += (otherPosition.y < thisPosition.y ? (m_bounds.size.y - teleportOffset) : (-m_bounds.size.y  + teleportOffset));
		}
	
		other.transform.position = new Vector3(newXPosition, newYPosition, otherPosition.z);

	}

}
