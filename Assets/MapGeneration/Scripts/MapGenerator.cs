using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct node {
    public int x;
    public int y;
    public int cost;
    public float heuristique;
};

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
    public float solidOverPathable = 0.03f;
    const int NUMBER_OF_PLAYER = 4;
    public GameObject[] Players = new GameObject[4];
    int[][] cellValues = null;
    int[][] spawns = null;
    public bool testDebug = false;

    // Use this for initialization
    void Start () {
		cellValues = InitialiseMatrix (NUMBER_OF_ROW, NUMBER_OF_COLUMN);
		DetermineSolidity ();
        DetermineSpawns();
		InstanciateMap ();
        PlacePlayer();
	}

    void Update(){
        if (testDebug)
        {
            RegenerateMap();
            testDebug = false;
        }
    }

    void RegenerateMap(){
        
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        cellValues = InitialiseMatrix(NUMBER_OF_ROW, NUMBER_OF_COLUMN);
        DetermineSolidity();
        DetermineSpawns();
        InstanciateMap();
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

    void ReinitialiseMatrix()
    {
        for (int i = 0; i < NUMBER_OF_ROW; i++)
        {
            for (int j = 0; j < NUMBER_OF_COLUMN; j++)
            {
                cellValues[i][j] = 0;
            }
        }
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
        do
        {
            ReinitialiseMatrix();
            GenerateSolidityMap();
        } while (!IsValidMap());

    }

    bool IsValidMap()
    {
        //IsPathBetweenTwoPoints(new int[2] { 1, 1 }, new int[2] { NUMBER_OF_ROW - 2, 1 });
        //IsPathToHole();
        return (
            IsEnoughtBlocksInMap() &&
            IsNoBlockingLayer() &&
            IsNotCollidingWithPlayer() &&
            IsPathToHole() &&
            IsPathBetweenPlayers()
            );
    }
    
    bool IsPathBetweenPlayers()
    {
        //return true;
        int[] playerOne = new int[2] { -(int)Players[0].transform.position.y, (int)Players[0].transform.position.x };
        int[] playerTwo = new int[2] { -(int)Players[1].transform.position.y, (int)Players[1].transform.position.x };
        int[] playerThree = new int[2] { -(int)Players[2].transform.position.y, (int)Players[2].transform.position.x };
        int[] playerFour = new int[2] { -(int)Players[3].transform.position.y, (int)Players[3].transform.position.x };
        if(playerOne[1] >= NUMBER_OF_COLUMN)
        {
            playerOne[1] -= NUMBER_OF_COLUMN;
        }
        if (playerTwo[1] >= NUMBER_OF_COLUMN)
        {
            playerTwo[1] -= NUMBER_OF_COLUMN;
        }
        if (playerThree[1] >= NUMBER_OF_COLUMN)
        {
            playerThree[1] -= NUMBER_OF_COLUMN;
        }
        if (playerFour[1] >= NUMBER_OF_COLUMN)
        {
            playerFour[1] -= NUMBER_OF_COLUMN;
        }
        playerOne = smoothPlayerPos(playerOne);
        playerTwo = smoothPlayerPos(playerTwo);
        playerThree = smoothPlayerPos(playerThree);
        playerFour = smoothPlayerPos(playerFour);
        return (IsPathBetweenTwoPoints(playerOne, playerTwo) &&
                IsPathBetweenTwoPoints(playerTwo, playerThree) &&
                IsPathBetweenTwoPoints(playerThree, playerFour));
    }

    int[] smoothPlayerPos(int[] player)
    {
        if(player[0] <= 0)
        {
            player[0]++;
        }
        else if (player[0] >= NUMBER_OF_ROW-1)
        {
            player[0]--;
        }
        if (player[1] <= 0)
        {
            player[1]++;
        }
        else if (player[1] >= NUMBER_OF_COLUMN)
        {
            player[1]--;
        }
        return player;
    }

    bool IsPathToHole()
    {
        for (int i = 0; i < NUMBER_OF_COLUMN; i++)
        {
            if (cellValues[NUMBER_OF_ROW - 1][i] == 0)
            {
                if (IsPathBetweenTwoPoints(new int[2] { 1, 1 }, new int[2] { NUMBER_OF_ROW - 1, i }))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsPathBetweenTwoPoints(int[] start, int[] end){
        //@params : Coordinate of start (i, j) and coordinate of end (i, j)
        List<node> closedList = new List<node>();
        List<node> openedList = new List<node>();
        node startNode = new node();
        node endNode = new node();
        node currentNode;
        node closeNode;
        int i = 0;
        int lastNodePosition = 0;
        int k = 0;
        startNode.x = start[0];
        startNode.y = start[1];
        startNode.cost = 0;
        startNode.heuristique = 0;
        endNode.x = end[0];
        endNode.y = end[1];
        endNode.cost = 1;
        endNode.heuristique = 0;
        openedList.Add(startNode);
        while (openedList.Count != 0)
        {
            currentNode = openedList[0];
            lastNodePosition = 0;   
            for (i = 1; i < openedList.Count; i++)
            {
                if (CompareTwoNodes(openedList[i], currentNode) == 1){
                    currentNode = openedList[i];
                    lastNodePosition = i;
                }
            }
            //DebugPrintList(openedList);
            openedList.RemoveAt(lastNodePosition);
            //Debug.Log("Remove currentNode from list " + lastNodePosition + " SIZEOF(openedList)" + openedList.Count);
            if (currentNode.x == endNode.x && currentNode.y == endNode.y)
            {
                //Debug.Log("Found");
                return true;
            }
            //Debug.Log("Coordonnées currentNode" + currentNode.x + " " + currentNode.y);
            if (cellValues[currentNode.x][currentNode.y-1] != 1)
            {
                closeNode.x = currentNode.x;
                closeNode.y = currentNode.y - 1;
                closeNode.cost = currentNode.cost + 1;
                closeNode.heuristique = closeNode.cost + Vector2.Distance(new Vector2(closeNode.x, closeNode.y), new Vector2(endNode.x, endNode.y));
                if(!NodeInListAndCheaper(closedList, closeNode) && !NodeInListAndCheaper(openedList, closeNode))
                {
                    //Debug.Log("Add to openedList" + closeNode.x + " " + closeNode.y + " " + closeNode.cost);
                    openedList.Add(closeNode);
                }
            }
            if (currentNode.y < NUMBER_OF_COLUMN - 1)
            {
                if (cellValues[currentNode.x][currentNode.y + 1] != 1)
                {
                    closeNode.x = currentNode.x;
                    closeNode.y = currentNode.y + 1;
                    closeNode.cost = currentNode.cost + 1;
                    closeNode.heuristique = closeNode.cost + Vector2.Distance(new Vector2(closeNode.x, closeNode.y), new Vector2(endNode.x, endNode.y));
                    if (!NodeInListAndCheaper(closedList, closeNode) && !NodeInListAndCheaper(openedList, closeNode))
                    {
                        //Debug.Log("Add to openedList" + closeNode.x + " " + closeNode.y);
                        openedList.Add(closeNode);
                    }
                }
            }
            if (cellValues[currentNode.x-1][currentNode.y] != 1)
            {
                closeNode.x = currentNode.x-1;
                closeNode.y = currentNode.y;
                closeNode.cost = currentNode.cost + 1;
                closeNode.heuristique = closeNode.cost + Vector2.Distance(new Vector2(closeNode.x, closeNode.y), new Vector2(endNode.x, endNode.y));
                if (!NodeInListAndCheaper(closedList, closeNode) && !NodeInListAndCheaper(openedList, closeNode))
                {
                    //Debug.Log("Add to openedList" + closeNode.x + " " + closeNode.y);
                    openedList.Add(closeNode);
                }
            }
            if (currentNode.x < NUMBER_OF_ROW-1) {
                if (cellValues[currentNode.x + 1][currentNode.y] != 1)
                {
                    closeNode.x = currentNode.x + 1;
                    closeNode.y = currentNode.y;
                    closeNode.cost = currentNode.cost + 1;
                    closeNode.heuristique = closeNode.cost + Vector2.Distance(new Vector2(closeNode.x, closeNode.y), new Vector2(endNode.x, endNode.y));
                    if (!NodeInListAndCheaper(closedList, closeNode) && !NodeInListAndCheaper(openedList, closeNode))
                    {
                        //Debug.Log("Add to openedList" + closeNode.x + " " + closeNode.y);
                        openedList.Add(closeNode);
                    }
                }
            }
            //Debug.Log("SIZEOF(openedList)");
            //Debug.Log(openedList.Count);
            closedList.Add(currentNode);
        }
        return false;
    }

    void DebugPrintList(List<node> list)
    {
        string s = "";
        for(int i = 0; i < list.Count; i++)
        {
            s += "(" + list[i].x + " " + list[i].y + "," + list[i].heuristique + ")";
        }
        Debug.Log(s);
    }

    bool NodeInListAndCheaper(List<node> list, node toFind)
    {
        if (toFind.x >= NUMBER_OF_ROW || toFind.x <= 0)
        {
            return true;
        }
        if (toFind.y >= NUMBER_OF_COLUMN || toFind.y <= 0)
        {
            return true;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (toFind.x == list[i].x && toFind.y == list[i].y)
            {
                if(list[i].cost <= toFind.cost)
                {
                    return true;
                }
            }
        }
        return false;
    }

    int CompareTwoNodes(node nodeOne, node nodeTwo)
    {
        if(nodeOne.heuristique < nodeTwo.heuristique)
        {
            return 1;
        }
        else if(nodeOne.heuristique == nodeTwo.heuristique)
        {
            return 0;
        }
        return -1;
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
            if(System.Array.IndexOf(cellValues[i], 0) == -1) {
                return false;
            }
        }
        return true;
    }

    bool IsNotCollidingWithPlayer()
    {
        foreach (GameObject player in Players) {
            if (player.transform.position.x > 1 && player.transform.position.x < NUMBER_OF_COLUMN - 1
               && player.transform.position.y < -1 && player.transform.position.y > -NUMBER_OF_ROW + 1) {
                if (cellValues[-(int)player.transform.position.y][(int)player.transform.position.x] == 1){
                    return false;
                }
                if (cellValues[(-(int)player.transform.position.y) - 1][(int)player.transform.position.x] == 1){
                    return false;
                }
                if (cellValues[-(int)player.transform.position.y][(int)player.transform.position.x + 1] == 1){
                    return false;
                }
                if (cellValues[(-(int)player.transform.position.y) - 1][(int)player.transform.position.x + 1] == 1){
                    return false;
                }
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
        Dictionary<int, int[]> overPathableCells;

        for (int i = 1; i < NUMBER_OF_ROW-1; i++) {
			if(pathableCells.ContainsKey(i)){
				for (int j = 1; j < NUMBER_OF_COLUMN-1; j++) {
					cellValues[i] = pathableCells[i];
				}
			}
		}

        overPathableCells = GetOverPathableCells(groundLevels);
        for (int i = 1; i < NUMBER_OF_ROW - 1; i++)
        {
            if (overPathableCells.ContainsKey(i))
            {
                for (int j = 1; j < NUMBER_OF_COLUMN - 1; j++)
                {
                    cellValues[i] = overPathableCells[i];
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

    Dictionary<int, int[]> GetOverPathableCells(int[] groundLevels)
    {
        Dictionary<int, int[]> overPathable = new Dictionary<int, int[]>();

        foreach (int rowOfGround in groundLevels)
        {
            if (rowOfGround != -1)
            {
                int[] ground = GetSolidityOfOverPathable(rowOfGround);
                overPathable.Add(rowOfGround-1, ground);
                ground = GetSolidityOfOverOverPathable(rowOfGround-1, ground);
                overPathable.Add(rowOfGround - 2, ground);
            }
        }
        return overPathable;
    }

    int[] GetSolidityOfOverOverPathable(int rowOfOverPathable, int[] overGround)
    {
        float plain = Random.Range(0.0f,1.0f);
        int[] ground = new int[NUMBER_OF_COLUMN];
        if(plain >= 0.6f)
        {
            for(int i = 0; i < NUMBER_OF_COLUMN; i++)
            {
                if (overGround[i] == 1)
                {
                    ground[i] = 1;
                }
                else
                {
                    ground[i] = 0;
                }
            }
        }
        else
        {
            for(int i = 0; i < NUMBER_OF_COLUMN; i++)
            {
                ground[i] = 0;
            }
        }
        return ground;
    }

    int[] GetSolidityOfOverPathable(int rowOfPathable)
    {
        int[] ground = new int[NUMBER_OF_COLUMN];
        ground[0] = 1;
        float probabilityOfBeeingSolid = 0.0f;
        bool alreadyOne = false;
        for(int i = NUMBER_OF_COLUMN-1; i > 0; i--)
        {
            if(cellValues[rowOfPathable][i] == 0)
            {
                probabilityOfBeeingSolid = 0.0f;
            }
            else
            {
                if (alreadyOne)
                {
                    probabilityOfBeeingSolid = 0.0f;
                }
                else
                {
                    probabilityOfBeeingSolid = 0.3f;
                }
                if(i < NUMBER_OF_COLUMN - 1)
                {
                    if(ground[i+1] == 1)
                    {
                        probabilityOfBeeingSolid += 0.4f;
                    }
                }
                if (Random.Range(0.0f, 1.0f) + i/(NUMBER_OF_COLUMN*2) < probabilityOfBeeingSolid)
                {
                    ground[i] = 1;
                    alreadyOne = true;
                }
                else
                {
                 ground[i] = 0;
                }
            }
        }
        return ground;
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
        for (int i = 0; i < NUMBER_OF_COLUMN; i++)
        {
            ground[i] = 0;
        }
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
            if (currentGroundLevel >= NUMBER_OF_ROW){
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
                else if (cellValues[i][j] == 0 && whiteTile)
                {
                    GameObject.Instantiate(whiteTile, new Vector3(j, -i, 0) + transform.position, Quaternion.identity, transform);
                    GameObject.Instantiate(whiteTile, new Vector3(25 - j, -i, 0) + transform.position, Quaternion.identity, transform);
                }
			}
		}
        if(debugTile)
        {
            foreach (int[] coordinate in spawns)
            {
                //GameObject.Instantiate(debugTile, new Vector3(coordinate[1], -coordinate[0], 0) + transform.position, Quaternion.identity, transform);
            }
        }
        
	}
}
