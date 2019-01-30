using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {

    public int speed;

    // Update is called once per frame
    void Update () {
        
        gameObject.transform.Translate(Time.deltaTime *speed , 0 , -Time.deltaTime *speed);

	}
}
