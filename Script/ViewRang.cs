using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewRang : MonoBehaviour {

    GameObject[] terrains;
    public int viewDistance;

    private void Start()
    {
        terrains = GameObject.FindGameObjectsWithTag("Chunk");
    }

    private void LateUpdate()
    {
        foreach (GameObject terrain in terrains)
        {
            Vector3 terrainPosition = terrain.GetComponent<Transform>().position;
            float distance = Vector3.Distance(terrainPosition, transform.position);
            if(distance > viewDistance)
            {
                terrain.SetActive(false);
            }
            else
            {
                terrain.SetActive(true);
            }
        }
    }
}
