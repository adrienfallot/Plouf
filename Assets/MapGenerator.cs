using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	public GameObject blackTile = null;
	public GameObject whiteTile = null;
	const int NUMBER_OF_ROW = 25;
	const int NUMBER_OF_COLUMN = 25;
	int[][] cellValues = new int[NUMBER_OF_ROW][];

	// Use this for initialization
	void Start () {
		DetermineSolidity ();
	}

	void DetermineSolidity(){
		for (int i = 0; i < NUMBER_OF_ROW; i++) {
			cellValues[i] = new int[NUMBER_OF_COLUMN];
		}

		for (int i = 0; i < NUMBER_OF_ROW; i++) {
			for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
				cellValues[i][j] = Random.Range(0, 2);
			}
		}
	}
	void InstanciateMap(){
		for (int i = 0; i < NUMBER_OF_ROW; i++) {
			for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
				if (cellValues [i] [j] == 1) {
					GameObject.Instantiate (blackTile, new Vector3 (i, 0, j), Quaternion.identity, transform);
				}
				else {
					GameObject.Instantiate (whiteTile, new Vector3 (i, 0, j), Quaternion.identity, transform);
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
