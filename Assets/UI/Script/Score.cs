using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

	public Player player;

	// Update is called once per frame
	public void UpdateScore () {	
		gameObject.GetComponent<Text>().text = ""+player.Score;
	}
}
