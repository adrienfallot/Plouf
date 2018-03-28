﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject debugTile = null;
    public GameObject blackTile = null;
	public GameObject whiteTile = null;
	public int NUMBER_OF_ROW = 24;
	public int NUMBER_OF_COLUMN = 13;
	public int BASE_NUMBER_OF_PLATE_PER_GROUND = 2;
	public int MAX_LENGTH_OF_PLATE = 5;
	public int MIN_LENGTH_OF_PLATE = 1;
	public int MAX_NUMBER_OF_UP_DOWN_HOLE = 1;
    public int minLevelGap = 2;
	public int maxLevelGap = 7;
	public int maxGroundLevels = 5;
    public int MIN_SOLID_BLOCK_PER_MAP_INTERIOR = 12;
    const int NUMBER_OF_PLAYER = 4;
    public GameObject[] Players = new GameObject[4];
    int[][] cellValues = null;
    int[][] spawns = null;

	// Use this for initialization
	void Start () {
		cellValues = InitialiseMatrix (NUMBER_OF_ROW, NUMBER_OF_COLUMN);
		DetermineSolidity ();
        DetermineSpawns();
		InstanciateMap ();
        PlacePlayer();
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

    void DetermineSpawns()
    {
        spawns = new int[NUMBER_OF_PLAYER][];
        DetermineUpSpawn();
        DetermineDownSpawn();
    }

    void PlacePlayer()
    {
        Vector3 spawnPos = Vector3.zero;
        for (int i = 0; i < Players.Length; i++)
        {

            spawnPos.x = spawns[i][1];
            spawnPos.y = -spawns[i][0];
            Players[i].transform.position = spawnPos;
        }
    }

    void DetermineUpSpawn(){
        for (int i = 1; i < NUMBER_OF_ROW - 1; i++)
        {
            for (int j = 1; j < NUMBER_OF_COLUMN - 1; j++)
            {
                if (cellValues[i][j] == 1){
                    if (cellValues[i-1][0] != 0 && cellValues[i-1][j] == 0){
                        spawns[0] = new int[] {i-1, j};
                        spawns[2] = new int[] {i-1, NUMBER_OF_COLUMN*2 - j - 1};
                        return;
                    }
                }
            }
        }
    }

    void DetermineDownSpawn(){
        for (int i = NUMBER_OF_ROW - 1; i > 1; i--){
            for (int j = 1; j < NUMBER_OF_COLUMN - 1; j++){
                if (cellValues[i][j] == 1){
                    if (cellValues[i-1][0] != 0 && cellValues[i - 1][j] == 0){
                        spawns[1] = new int[] {i-1, NUMBER_OF_COLUMN*2 - j - 1 };
                        spawns[3] = new int[] {i-1, j};
                        return;
                    }
                }
            }
        }
    }

	void DetermineSolidity(){
        while (!IsValidMap()){
            GenerateSolidityMap();
        }
	}

    bool IsValidMap(){
        return (
            IsEnoughtBlocksInMap() &&
            IsNoBlockingLayer()
            );
    }

    bool IsEnoughtBlocksInMap(){
        int numberOfSolidBlock = 0;
        for(int i = 1; i < NUMBER_OF_ROW-1; i++){
            for(int j = 1; j < NUMBER_OF_COLUMN-1; j++){
                if(cellValues[i][j] == 1){
                    numberOfSolidBlock++;
                }
            }
        }
        if (numberOfSolidBlock > MIN_SOLID_BLOCK_PER_MAP_INTERIOR){
            return true;
        }
        return false;
    }

    bool IsNoBlockingLayer(){
        for(int i = 1; i < NUMBER_OF_ROW-1; i++) {
            if(System.Array.IndexOf(cellValues, 0) != -1) {
                return false;
            }
        }
        return true;
    }

    void GenerateSolidityMap()
    {
        /*The order of function call is important in this function.*/
        DetermineSolidityOfInterior();
        DetermineSolidityOfFloorAndCeiling();
        DetermineSolidityOfWalls();

    }

	void DetermineSolidityOfFloorAndCeiling(){
		int[] holeCandidate = getUpDownHoleCandidate();
		int[] upDownHoleLocationSelected = new int[MAX_NUMBER_OF_UP_DOWN_HOLE];

        for (int j = 0; j < NUMBER_OF_COLUMN; j++)
        {   cellValues[0][j] = 1;
            cellValues[NUMBER_OF_ROW - 1][j] = 1;
        }

        if (holeCandidate.Length == 0){
            return;
        }
		for(int i = 0; i < upDownHoleLocationSelected.Length; i++){
			upDownHoleLocationSelected[i] = holeCandidate[Random.Range(0, holeCandidate.Length)];
		}

		for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
			if (System.Array.IndexOf(upDownHoleLocationSelected, j) != -1) {
				cellValues[0][j] = 0;
				cellValues[NUMBER_OF_ROW-1][j] = 0;
			}
			else if (System.Array.IndexOf(upDownHoleLocationSelected, j-1) != -1) {
				cellValues[0][j] = 0;
				cellValues[NUMBER_OF_ROW-1][j] = 0;
			}
		}
	}

	int[] getUpDownHoleCandidate(){
		int[] candidates = new int[NUMBER_OF_COLUMN-1];
		int i = 0;
		for (int j = 1; j < NUMBER_OF_COLUMN-1; j++) {
			if (cellValues[NUMBER_OF_ROW-2][j] == 0 && cellValues[NUMBER_OF_ROW-2][j+1] == 0) {
				candidates[i] = j;
				i++;
			}
		}
		return getSliceOfArray(candidates, 0, i);
	}

	int[] getSliceOfArray(int[] array, int start, int end){
		//start included, end excluded.
		int[] returnArray = new int[end-start];
		for (int i = start; i < end; i++){
			returnArray[i-start] = array[i];
		}
		return returnArray;
	}	

	void DetermineSolidityOfInterior(){
		int[] groundLevels = GetGroundLevels();
		Dictionary<int, int[]> pathableCells = GetPathableCells(groundLevels);
		Dictionary<int, int[]> underPathableCells;

		for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			if(pathableCells.ContainsKey(i)){
				for (int j = 1; j < NUMBER_OF_COLUMN-1; j++) {
					cellValues[i] = pathableCells[i];
				}
			}
		}
		underPathableCells = GetUnderPathableCells(groundLevels);	
		for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			if(underPathableCells.ContainsKey(i)){
				for (int j = 1; j < NUMBER_OF_COLUMN-1; j++) {
					cellValues[i] = underPathableCells[i];
				}
			}
		}
	}

	Dictionary<int, int[]> GetUnderPathableCells (int[] groundLevels){
		Dictionary<int, int[]> underPathable = new Dictionary<int, int[]> ();

		foreach (int rowOfGround in groundLevels) {
			if (rowOfGround != -1) {
				int[] ground = GetSolidityOfUnderPathable(rowOfGround);
				underPathable.Add(rowOfGround+1, ground);
			}
		}
		return underPathable;
	}

	int[] GetSolidityOfUnderPathable(int rowOfPathable){
		int[] ground = new int[NUMBER_OF_COLUMN];
		ground[0] = 1;
		float probabilityOfBeeingSolid = 0.0f;
		for (int i = 1; i < NUMBER_OF_COLUMN-2; i++) {
			probabilityOfBeeingSolid = 0.0f;
			if (rowOfPathable >= NUMBER_OF_ROW - 2) {
				ground [i] = 0;
			}
			else if (cellValues [rowOfPathable][i] == 0) {
				ground [i] = 0;
			}
			else {
				probabilityOfBeeingSolid += 0.4f;
				if (i == 1) {
					probabilityOfBeeingSolid += 0.4f;
				}
				else if(ground[i-1] == 1) {
					probabilityOfBeeingSolid += 0.3f;
					if (ground [i - 2] == 1) {
						probabilityOfBeeingSolid += 0.2f;
					}
				}
				if (Random.Range (0.0f, 1.0f) < probabilityOfBeeingSolid) {
					ground[i] = 1;
				}
				else {
					ground[i] = 0;
				}
			}
		}
		return ground;
	}

	Dictionary<int, int[]> GetPathableCells (int[] groundLevels){
		Dictionary<int, int[]> pathable = new Dictionary<int, int[]>();

		foreach (int rowOfGround in groundLevels) {
			if (rowOfGround != -1) {
				int[] ground = GetSolidityOfPathable();
				pathable.Add(rowOfGround, ground);
			}
		}
		return pathable;
	}

	int[] GetSolidityOfPathable(){
		int[] ground = new int[NUMBER_OF_COLUMN];
		int[] startingPositions = new int[BASE_NUMBER_OF_PLATE_PER_GROUND];
		float plateLength = 0;

		for (int i = 0; i < BASE_NUMBER_OF_PLATE_PER_GROUND; i++) {
			startingPositions[i] = Random.Range (0, 13);
		}

		foreach(int startingPosition in startingPositions){
			plateLength = (int) (MAX_LENGTH_OF_PLATE - MIN_LENGTH_OF_PLATE) * Mathf.Sqrt (-2.0f * Mathf.Log (Random.Range (0.0f, 1.0f)) * 
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
			if (currentGroundLevel > NUMBER_OF_ROW){
				return groundLevels;
			}
			groundLevels[i] = currentGroundLevel;
		}

		return groundLevels;
	}

	void DetermineSolidityOfWalls(){
        int[] holeCandidates = DetermineWallHoleCandidates();
        int locationSelected = 0;

        for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			cellValues[i][0] = 1;
		}
        if (holeCandidates.Length == 0)
        {
            return;
        }
        locationSelected = holeCandidates[Random.Range(0, holeCandidates.Length)];

        Debug.Log(locationSelected);
        Debug.Log(holeCandidates.Length);
        cellValues[locationSelected-1][0] = 0;
        cellValues[locationSelected][0] = 0;
    }

    int[] DetermineWallHoleCandidates(){
        int[] priorityCandidates = new int[NUMBER_OF_ROW];
        int[] secondHandCandidates = new int[NUMBER_OF_ROW];
        bool priorityCandidatesFound = false;
        int k = 0;

        for (int i = 3; i < NUMBER_OF_ROW; i++){
            if (cellValues[i][1] == 1 && cellValues[i-1][1] == 0 && cellValues[i-2][1] == 0){
                priorityCandidates[k] = i - 1;
                priorityCandidatesFound = true;
                k++;
            }
        }
        if (priorityCandidatesFound){
            return getSliceOfArray(priorityCandidates, 0, k);
        }

        k = 0;
        for (int i = 3; i < NUMBER_OF_ROW; i++)
        {
            if (cellValues[i][1] == 0 && cellValues[i-1][1] == 0)
            {
                secondHandCandidates[k] = i;
                k++;
            }
        }
        return getSliceOfArray(secondHandCandidates, 0, k);
    }

	void InstanciateMap(){
		for (int i = 0; i < NUMBER_OF_ROW; i++) {
			for (int j = 0; j < NUMBER_OF_COLUMN; j++) {
                if (cellValues[i][j] == 1)
                {
                    GameObject.Instantiate(blackTile, new Vector3(j, -i, 0) + transform.position, Quaternion.identity, transform);
                    GameObject.Instantiate(blackTile, new Vector3(25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
                }
                else if (cellValues[i][j] == 2)
                {
                    if(debugTile)
                    {
                        GameObject.Instantiate(debugTile, new Vector3(j, -i, 0) + transform.position, Quaternion.identity, transform);
                        GameObject.Instantiate(debugTile, new Vector3(25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
                    }
                    
                }
                else
                {
                    //GameObject.Instantiate(whiteTile, new Vector3(j, -i, 0) + transform.position, Quaternion.identity, transform);
                    //GameObject.Instantiate(whiteTile, new Vector3(25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
                }
			}
		}
        if(debugTile)
        {
            foreach (int[] coordinate in spawns)
            {
                GameObject.Instantiate(debugTile, new Vector3(coordinate[1], -coordinate[0], 0) + transform.position, Quaternion.identity, transform);
            }
        }
        
	}
}