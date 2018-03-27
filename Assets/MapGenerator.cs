using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
	public GameObject blackTile = null;
	public GameObject whiteTile = null;
	const int NUMBER_OF_ROW = 24;
	const int NUMBER_OF_COLUMN = 13;
	const int BASE_NUMBER_OF_PLATE_PER_GROUND = 2;
	const int MAX_LENGTH_OF_PLATE = 5;
	const int MIN_LENGTH_OF_PLATE = 1;
	const int minLevelGap = 2;
	const int maxLevelGap = 7;
	const int maxGroundLevels = 5;
	int[][] cellValues = null;

	// Use this for initialization
	void Start () {
		cellValues = InitialiseMatrix (NUMBER_OF_ROW, NUMBER_OF_COLUMN);
		DetermineSolidity ();
		InstanciateMap ();
	}

	int[][] InitialiseMatrix(int length, int width){
		int[][] matrix = new int[length][];
		for (int i = 0; i < length; i++) {
			int[] row = new int[width];
			for (int j = 0; j < width; j++) {
				row [j] = 0;
			}
			matrix [i] = row;
		}

		return matrix;
	}

	void DetermineSolidity(){
		DetermineSolidityOfFloor ();
		DetermineSolidityOfInterior ();
		DetermineSolidityOfWalls ();	
		DetermineSolidityOfCeiling ();
	}

	void DetermineSolidityOfFloor(){
		for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
			cellValues[NUMBER_OF_ROW-1][j] = 1;
		}
	}

	void DetermineSolidityOfInterior(){
		int[] groundLevels = GetGroundLevels();
		Dictionary<int, int[]> grounds = GetGrounds(groundLevels);

		for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			if(grounds.ContainsKey(i)){
				for (int j = 1; j < NUMBER_OF_COLUMN-1; j++) {
					cellValues[i] = grounds[i];
				}
			}
		}
	}

	Dictionary<int, int[]> GetGrounds (int[] groundLevels){
		Dictionary<int, int[]> grounds = new Dictionary<int, int[]>();

		foreach (int rowOfGround in groundLevels) {
			if (rowOfGround != -1) {
				int[] ground = GetSolidityOfGround();
				Debug.Log (rowOfGround);
				grounds.Add (rowOfGround, ground);
			}
		}
		return grounds;
	}

	int[] GetSolidityOfGround(){
		int[] ground = new int[NUMBER_OF_COLUMN];
		int[] startingPositions = new int[BASE_NUMBER_OF_PLATE_PER_GROUND];
		float plateLength = 0;

		for (int i = 0; i < BASE_NUMBER_OF_PLATE_PER_GROUND; i++) {
			startingPositions[i] = Random.Range (0, 13);
		}

		foreach(int startingPosition in startingPositions){
			plateLength = (int) MAX_LENGTH_OF_PLATE * Mathf.Sqrt (-2.0f * Mathf.Log (Random.Range (0.0f, 1.0f)) * 
														 Mathf.Sin (2.0f * Mathf.PI * Random.Range (0.0f, 1.0f)))
													+ MIN_LENGTH_OF_PLATE;
			if (plateLength % 2 == 0) {
				for (int i = 0; i < plateLength / 2; i++) {
					ground [startingPosition+i] = 1;
					ground [startingPosition-1 - i] = 1;
				}
			}
			else{
				ground [startingPosition] = 1;
				for (int i = 0; i < (plateLength-1) / 2; i++) {
					ground [Mathf.Min(startingPosition+1 + i, NUMBER_OF_COLUMN-1)] = 1;
					ground [Mathf.Max(startingPosition-1 - i, 0)] = 1;
				}
			}
		}
		return ground;
	}

	void DebugGroundLevel_(int[] groundLevels){
		for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			if (System.Array.IndexOf(groundLevels, i) != -1){
				for (int j = 1; j < NUMBER_OF_COLUMN; j++) {
					cellValues[i][j] = 1;
				}
			}
			else {
				for (int j = 1; j < NUMBER_OF_COLUMN; j++) {
					cellValues[i][j] = 0;
				}
			}
		}
	}

	int[] GetGroundLevels(){
		int[] groundLevels = new int[maxGroundLevels];

		for (int i = 0; i < maxGroundLevels; i++) {
			groundLevels[i] = -1;
		}
		int currentGroundLevel = 0;
		for (int i = 0; i < maxGroundLevels; i++) {
			currentGroundLevel += Random.Range (minLevelGap+1, maxLevelGap+2);
			Debug.Log (currentGroundLevel);
			if (currentGroundLevel > NUMBER_OF_ROW){
				return groundLevels;
			}
			groundLevels[i] = currentGroundLevel;
		}

		return groundLevels;
	}

	void DetermineSolidityOfWalls(){
		for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			cellValues[i][0] = 1;
		}
	}
		
	void DetermineSolidityOfCeiling(){
		for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
			cellValues[0][j] = 1;
		}
	}

	void InstanciateMap(){
		for (int i = 0; i < NUMBER_OF_ROW; i++) {
			for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
				if (cellValues [i][j] == 1) {
					GameObject.Instantiate (blackTile, new Vector3 (j, -i, 0) + transform.position, Quaternion.identity, transform);
					GameObject.Instantiate (blackTile, new Vector3 (25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
				}
				else {
					GameObject.Instantiate (whiteTile, new Vector3 (j, -i, 0) + transform.position, Quaternion.identity, transform);
					GameObject.Instantiate (whiteTile, new Vector3 (25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
