using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStartPositionCheck : MonoBehaviour {



	void Update ()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -transform.up, out hit);
        Debug.Log(hit.distance);
	}
	
}
