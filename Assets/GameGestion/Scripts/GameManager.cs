using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null; 

	public MapGenerator m_mapGenerator = null;
    public MapGenerator m_mapGeneratorBack = null;
    //public BackGroundGenerator m_backgroundGenerator = null;

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
        InvokeRepeating("regenerateMap",5,5);
	    m_mapGenerator.GenerateMap();
        m_mapGeneratorBack.RegenerateMap();
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
        Debug.Log("test");
        List<Transform> blocksToMove = getBlockInBackNotInFront();
        yield return StartCoroutine(LerpVelocityTo(blocksToMove, 0, 2));
        m_mapGenerator.cellValues = m_mapGeneratorBack.cellValues;
        foreach (Transform child in m_mapGenerator.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_mapGenerator.InstanciateMap();
        m_mapGeneratorBack.RegenerateMap();
    }

    public void regenerateMap()
    {
        StartCoroutine(ChangeMapCoroutine());
        //m_mapGeneratorBack.RegenerateMap();
		//m_backgroundGenerator.InstantiateBackground();
    }
}
