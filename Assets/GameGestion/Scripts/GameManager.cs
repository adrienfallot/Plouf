using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null; 

	public MapGenerator m_mapGenerator = null;
    public MapGenerator m_mapGeneratorBack = null;
    public MapGenerator m_mapGeneratorDeepBack = null;
    public Material m_ForegroundMapMaterial = null;
    public Material m_BackgroundMapMaterial = null;
    public Material m_DeepBeckgroundMapMaterial = null;
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
        InvokeRepeating("regenerateMap",5,10);
	    m_mapGenerator.GenerateMap();
        m_mapGeneratorBack.RegenerateMap(1);
        m_mapGeneratorDeepBack.RegenerateMap(2);
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

    private IEnumerator LerpBlockPosTo(List<Transform> iToMove, float iZOffset, float iTime)
    {
        //on set le material de chaque cube à toBg
        List<Vector3> startPos = new List<Vector3>();
        foreach(Transform cube in iToMove)
        {
            //cube.gameObject.GetComponent<Renderer>().material = m_BackgroundMapMaterial;
            startPos.Add(cube.position);
        }

        float elapsedTime = 0;
        /*Vector3[] startPos = new Vector3[iToMove.Count];
        for (int i = 0; i < iToMove.Count; i++)
        {
            startPos[i] = iToMove[i].position;
        }*/
        //Vector3 lerpedColor = Vector3.zero;
        while (elapsedTime < iTime)
        {
            for (int i = 0; i < iToMove.Count; i++)
            {
                iToMove[i].position = Vector3.Lerp(startPos[i],
                                                    new Vector3(iToMove[i].position.x,
                                                    iToMove[i].position.y,
                                                    iZOffset),
                                                    (elapsedTime / iTime));
                /*lerpedColor = Vector3.Lerp(Vector3.one,
                                            new Vector3(.5f, .5f, .5f),
                                            elapsedTime / iTime);
                m_BackgroundMapMaterial.SetColor("Albedo", new Color(lerpedColor.x, lerpedColor.y, lerpedColor.z));*/

                
            }
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }

    }
 
    private IEnumerator ChangeMapCoroutine()
    {
        int[][] tmp = m_mapGenerator.cellValues;
        List<Transform> blocksToMove = getBlockInBackNotInFront();
        Debug.Log("A");
        yield return StartCoroutine(LerpBlockPosTo(blocksToMove, 0, 2));
        m_mapGenerator.cellValues = m_mapGeneratorBack.cellValues;
        Debug.Log("B");
        foreach (Transform child in m_mapGenerator.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_mapGeneratorBack.cellValues = tmp;
        m_mapGenerator.InstanciateMap();
    }

    private IEnumerator RemoveFrontCoroutine()
    {
        List<Transform> blocksToMove = getBlockInFrontNotInBack();
        Debug.Log("C");
        yield return StartCoroutine(LerpBlockPosTo(blocksToMove, 1, 2));
        Debug.Log("D");
        /*foreach (Transform child in m_mapGeneratorBack.transform)
        {
            GameObject.Destroy(child.gameObject);
        }*/
        m_mapGeneratorBack.InstanciateMap(1);
    }

    private IEnumerator ChangeBackgroundCoroutine()
    {
        List<Transform> blocksToMove = getBlockInBackNotInDeepBack();
        Debug.Log("E");
        yield return StartCoroutine(LerpBlockPosTo(blocksToMove, 2, 5));
        Debug.Log("F");
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
        yield return StartCoroutine(LerpBlockPosTo(blocksToMove, 1, 5));
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
        StartCoroutine(ChangeAllMap());
        //StartCoroutine(ChangeMapCoroutine());
        //StartCoroutine(RemoveFrontCoroutine());
        //StartCoroutine(ChangeBackgroundCoroutine());
        //StartCoroutine(RemoveBackgroundCoroutine());
        //m_mapGeneratorBack.RegenerateMap();
        //m_backgroundGenerator.InstantiateBackground();
    }
}
