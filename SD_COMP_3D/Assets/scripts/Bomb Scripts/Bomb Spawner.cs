using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [SerializeField]
    private List<Transform> BombPoints;
    [SerializeField]
    private List<GameObject> Bombs;
    public GameObject fallingBomb;
    [SerializeField]
    private int fallingMaxWaitTime;
    [SerializeField]
    private int numberofBombsToSpawn;



    public void StartSpawning()
    {
        StartCoroutine(SpawnBombs());
        StartCoroutine(SpawnFallingBombs());
        StartCoroutine(ChangeBombNumber());
    }
    IEnumerator SpawnBombs()
    {
        for (int i = 0; i < BombPoints.Count; i++)
        {
            // Check if this spawn point has no children
            if (BombPoints[i].transform.childCount == 0)
            {
                GameObject bomb = Instantiate(
                    Bombs[Random.Range(0, Bombs.Count)],
                    BombPoints[i].transform.position,
                    Quaternion.identity,
                    BombPoints[i].transform  // Pass parent directly to Instantiate
                );
            }
        }

        yield return new WaitForSeconds(20);
        StartCoroutine(SpawnBombs());
    }
    IEnumerator SpawnFallingBombs()
    {
        yield return new WaitForSeconds(fallingMaxWaitTime);

        for (int i = 0; i < numberofBombsToSpawn; i++)
        {
            Instantiate(fallingBomb, new Vector3(Random.Range(-23, 23), 80,  Random.Range(-23, 25)), Quaternion.identity);
        }

        StartCoroutine(SpawnFallingBombs());
    }

    IEnumerator ChangeBombNumber()
    {
        yield return new WaitForSeconds(40);
        numberofBombsToSpawn += 2;
    }
}
