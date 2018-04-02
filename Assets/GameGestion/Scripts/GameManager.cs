using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Trajectory
{
    public Transform transform;
    public Vector3 start;
    public Vector3 end;
    public bool firstAxisIsX;
}

public class GameManager : MonoBehaviour {

	public static GameManager instance = null; 

	public MapGenerator m_mapGenerator = null;
    public MapGenerator m_mapGeneratorBack = null;
    public MapGenerator m_mapGeneratorDeepBack = null;
    public BackGroundGenerator m_backgroundGenerator = null;

    private int nextSpawn = 0;

    void Awake()
	{
		if (instance == null){
			instance = this;
		}
		else if (instance != this) {
			Destroy(gameObject);
		}
	}
    
    void Start()
    {
        Random.seed = 2;
        m_mapGenerator.GenerateMap();
        m_mapGeneratorBack.RegenerateMap(1);
        m_mapGeneratorDeepBack.RegenerateMap(2);
        InvokeRepeating("regenerateMap", 0, 6);
    }

	public Vector3 GetUnusedSpawn() {
		
		int[] spawn = m_mapGenerator.GetSpawns()[nextSpawn];

		nextSpawn = (nextSpawn + 1) %  MapGenerator.NUMBER_OF_PLAYER; //TODO: Number of spawns

        Vector3 spawnPos = Vector3.zero;
		spawnPos.x = spawn[1];
		spawnPos.y = -spawn[0];

		return spawnPos;

	}

    List<Transform> getBlockInBackNotInFront()
    {
        int x = 0;
        int y = 0;
        Transform blockTransform = null;
        List<Transform> blocksInBackNotInFront = new List<Transform>();
        for(int i = 0; i < m_mapGeneratorBack.transform.childCount; i++)
        {
            blockTransform = m_mapGeneratorBack.transform.GetChild(i);
            x = (int) -blockTransform.position.y;
            y = (int) blockTransform.position.x;
            if(y >= m_mapGeneratorBack.NUMBER_OF_COLUMN)
            {
                y = m_mapGeneratorBack.NUMBER_OF_COLUMN*2 - y - 1;
            }
            if(m_mapGenerator.cellValues[x][y] == 0)
            {
                blocksInBackNotInFront.Add(blockTransform);
            }
        }

        return blocksInBackNotInFront;
    }

    List<Transform> getBlockInFrontNotInBack()
    {
        int x = 0;
        int y = 0;
        Transform blockTransform = null;
        List<Transform> blocksInFrontNotInBack = new List<Transform>();
        for (int i = 0; i < m_mapGenerator.transform.childCount; i++)
        {
            blockTransform = m_mapGenerator.transform.GetChild(i);
            x = (int)-blockTransform.position.y;
            y = (int)blockTransform.position.x;
            if (y >= m_mapGenerator.NUMBER_OF_COLUMN)
            {
                y = m_mapGenerator.NUMBER_OF_COLUMN * 2 - y - 1;
            }
            if (m_mapGeneratorBack.cellValues[x][y] == 0)
            {
                blocksInFrontNotInBack.Add(blockTransform);
            }
        }

        return blocksInFrontNotInBack;
    }

    List<Transform> getBlockInBackNotInDeepBack()
    {
        int x = 0;
        int y = 0;
        Transform blockTransform = null;
        List<Transform> blocksInBackNotInDeepBack = new List<Transform>();
        for (int i = 0; i < m_mapGeneratorBack.transform.childCount; i++)
        {
            blockTransform = m_mapGeneratorBack.transform.GetChild(i);
            x = (int)-blockTransform.position.y;
            y = (int)blockTransform.position.x;
            if (y >= m_mapGeneratorBack.NUMBER_OF_COLUMN)
            {
                y = m_mapGeneratorBack.NUMBER_OF_COLUMN * 2 - y - 1;
            }
            if (m_mapGeneratorDeepBack.cellValues[x][y] == 0)
            {
                blocksInBackNotInDeepBack.Add(blockTransform);
            }
        }

        return blocksInBackNotInDeepBack;
    }

    List<Transform> getBlockInDeepBackNotInBack()
    {
        int x = 0;
        int y = 0;
        Transform blockTransform = null;
        List<Transform> blocksInDeepBackNotInBack = new List<Transform>();
        for (int i = 0; i < m_mapGeneratorDeepBack.transform.childCount; i++)
        {
            blockTransform = m_mapGeneratorDeepBack.transform.GetChild(i);
            x = (int)-blockTransform.position.y;
            y = (int)blockTransform.position.x;
            if (y >= m_mapGeneratorDeepBack.NUMBER_OF_COLUMN)
            {
                y = m_mapGeneratorDeepBack.NUMBER_OF_COLUMN * 2 - y - 1;
            }
            if (m_mapGeneratorBack.cellValues[x][y] == 0)
            {
                blocksInDeepBackNotInBack.Add(blockTransform);
            }
        }

        return blocksInDeepBackNotInBack;
    }

    private IEnumerator LerpVelocityTo(List<Transform> iToMove, float iZOffset, float iTime)
    {
        float elapsedTime = 0;
        Vector3[] startPos = new Vector3[iToMove.Count];
        for (int i = 0; i < iToMove.Count; i++)
        {
            startPos[i] = iToMove[i].position;
        }

        while (elapsedTime < iTime)
        {
            for (int i = 0; i < iToMove.Count; i++)
            {
                iToMove[i].position = Vector3.Lerp(startPos[i],
                                                    new Vector3(iToMove[i].position.x,
                                                    iToMove[i].position.y,
                                                    iZOffset),
                                                    (elapsedTime / iTime));
                
            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }
    }

    private IEnumerator ChangeMapCoroutine()
    {
        int[][] tmp = m_mapGenerator.cellValues;
        List<Transform> blocksToMove = getBlockInBackNotInFront();
        List<Transform> blocksToMoveToBack = getBlockInFrontNotInBack();
        StartCoroutine(LerpVelocityTo(blocksToMoveToBack, 1, 2));
        yield return StartCoroutine(LerpVelocityTo(blocksToMove, 0, 2));
        m_mapGenerator.cellValues = m_mapGeneratorBack.cellValues;
        foreach (Transform child in m_mapGenerator.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in m_mapGeneratorBack.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_mapGeneratorBack.cellValues = tmp;
        m_mapGenerator.InstanciateMap();
        m_mapGeneratorBack.InstanciateMap(1);
    }

    private IEnumerator RemoveFrontCoroutine()
    {
        List<Transform> blocksToMove = getBlockInFrontNotInBack();
        yield return StartCoroutine(LerpVelocityTo(blocksToMove, 1, 2));
        /*foreach (Transform child in m_mapGeneratorBack.transform)
        {
            GameObject.Destroy(child.gameObject);
        }*/
        m_mapGeneratorBack.InstanciateMap(1);
    }

    private IEnumerator ChangeBackgroundCoroutine()
    {
        List<Transform> blocksToMove = getBlockInBackNotInDeepBack();
        yield return StartCoroutine(LerpVelocityTo(blocksToMove, 2, 5));
        m_mapGeneratorBack.cellValues = m_mapGeneratorDeepBack.cellValues;
        foreach (Transform child in m_mapGeneratorBack.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_mapGeneratorBack.InstanciateMap(1);
        m_mapGeneratorDeepBack.RegenerateMap(2);
    }
    private IEnumerator RemoveBackgroundCoroutine()
    {
        List<Transform> blocksToMove = getBlockInDeepBackNotInBack();
        yield return StartCoroutine(LerpVelocityTo(blocksToMove, 1, 5));
    }

    private IEnumerator ChangeAllMap()
    {
        StartCoroutine(ChangeMapCoroutine());
        yield return StartCoroutine(RemoveFrontCoroutine());
        StartCoroutine(ChangeBackgroundCoroutine());
        yield return StartCoroutine(RemoveBackgroundCoroutine());
    }

    public void regenerateMap()
    {
        StartCoroutine(ChangeMapV2Coroutine());
        //StartCoroutine(ChangeAllMap());
        //StartCoroutine(ChangeMapCoroutine());
        //StartCoroutine(RemoveFrontCoroutine());
        //StartCoroutine(ChangeBackgroundCoroutine());
        //StartCoroutine(RemoveBackgroundCoroutine());
        //m_mapGeneratorBack.RegenerateMap();
        //m_backgroundGenerator.InstantiateBackground();
    }

    private IEnumerator ChangeMapV2Coroutine()
    {
        yield return StartCoroutine(ChangeMapCoroutine());
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(LerpBackground());
        m_mapGeneratorBack.cellValues = m_mapGeneratorDeepBack.cellValues;
        foreach (Transform child in m_mapGeneratorBack.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_mapGeneratorBack.InstanciateMap(1);
        m_mapGeneratorDeepBack.RegenerateMap(2);

    }

    private IEnumerator LerpTrajectory(List<Trajectory> iToMove, float iTime)
    {
        float elapsedTime = 0;

        while (elapsedTime < iTime)
        {
            for (int i = 0; i < iToMove.Count; i++)
            {
                iToMove[i].transform.position = Vector3.Lerp(iToMove[i].start,
                                                             iToMove[i].end,
                                                             (elapsedTime / iTime));

            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }
    }

    private IEnumerator LerpBackground()
    {
        List<Transform> currentSpike = GetBlockOfType("Map_Spike", m_mapGeneratorBack.transform);
        List<Transform> futureSpike = GetBlockOfType("Map_Spike", m_mapGeneratorDeepBack.transform);
        List<Transform> currentWall = GetBlockOfType("Map_Wall", m_mapGeneratorBack.transform);
        List<Transform> futureWall = GetBlockOfType("Map_Wall", m_mapGeneratorDeepBack.transform);
        List<Transform> currentFloor = GetBlockOfType("Map_Floor", m_mapGeneratorBack.transform);
        List<Transform> futureFloor = GetBlockOfType("Map_Floor", m_mapGeneratorDeepBack.transform);

        List<Trajectory> floorTrajectories = getTrajectories(currentFloor, futureFloor);
        List<Trajectory> wallTrajectories = getTrajectories(currentWall, futureWall);
        List<Trajectory> spikeTrajectories = getTrajectories(currentSpike, futureSpike);

        floorTrajectories.AddRange(wallTrajectories);
        floorTrajectories.AddRange(spikeTrajectories);

        yield return StartCoroutine(LerpTrajectory(floorTrajectories, 3));
    }

    List<Trajectory> getTrajectories(List<Transform> start, List<Transform> end)
    {
        Trajectory trajectory;
        List<Trajectory> trajectories = new List<Trajectory>();
        bool firstAxisIsX = false;
        int randomValue = 0;

        RemoveAlreadyInPlace(start, end);
        //If to many endpoint, treat the overflow.
        while (start.Count < end.Count)
        {
            trajectory = new Trajectory();
            randomValue = Random.Range(0, end.Count);
            trajectory.transform = end[randomValue];
            trajectory.start = new Vector3(end[randomValue].position.x, end[randomValue].position.y, end[randomValue].position.z);
            trajectory.end = new Vector3(end[randomValue].position.x, end[randomValue].position.y, 1);
            trajectory.firstAxisIsX = false;
            trajectories.Add(trajectory);
            end.RemoveAt(randomValue);
        }
        //If to many startpoint, treat the overflow.
        while (end.Count < start.Count)
        {
            trajectory = new Trajectory();
            randomValue = Random.Range(0, start.Count);
            trajectory.transform = start[randomValue];
            trajectory.start = new Vector3(start[randomValue].position.x, start[randomValue].position.y, start[randomValue].position.z);
            trajectory.end = new Vector3(start[randomValue].position.x, start[randomValue].position.y, 2);
            trajectory.firstAxisIsX = false;
            trajectories.Add(trajectory);
            start.RemoveAt(randomValue);
        }

        for(int i = 0; i < end.Count; i++)
        {
            trajectory = new Trajectory();
            randomValue = Random.Range(0, start.Count);
            trajectory.transform = start[randomValue];
            trajectory.start = new Vector3(start[randomValue].position.x, start[randomValue].position.y, start[randomValue].position.z);
            trajectory.end = new Vector3(end[i].position.x, end[i].position.y, start[randomValue].position.z);
            trajectory.firstAxisIsX = false;
            trajectories.Add(trajectory);
            start.RemoveAt(randomValue);
        }

        return trajectories;
    }

    void RemoveAlreadyInPlace(List<Transform> start, List<Transform> end)
    {
        for(int i = 0; i < start.Count; i++)
        {
            for(int j = 0; j < end.Count; j++)
            {
                if(start[i].position.x == end[j].position.x && start[i].position.y == end[j].position.y)
                {
                    start.RemoveAt(i);
                    end.RemoveAt(j);
                }
            }
        }
    }

    List<Transform> GetBlockOfType(string tag, Transform parent)
    {
        List<Transform> blocksOfType = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if(child.gameObject.tag == tag)
            {
                blocksOfType.Add(child);
            }
        }
        return blocksOfType;
    }

    int GetNumberOfBlock(int type, int[][] map)
    {
        int numberOfBlock = 0;
        for (int i = 0; i < m_mapGeneratorBack.NUMBER_OF_ROW; i++)
        {
            for (int j = 0; j < m_mapGeneratorBack.NUMBER_OF_COLUMN; j++)
            {
                if (map[i][j] == type)
                {
                    numberOfBlock++;
                }
            }
        }
        return numberOfBlock;
    }
}
