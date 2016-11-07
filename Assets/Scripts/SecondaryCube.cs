using UnityEngine;
using System.Collections;

public class SecondaryCube : MonoBehaviour
{
    //The velocity that the cubes will move
    public Vector3 MovementFactor = new Vector3(0.0f, 0.0f, 2.0f);

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        //Transform
        Transform cubeTransform = GetComponent<Transform>();

        //The spectrum line is below the cube, make it fall  
		cubeTransform.position += this.MovementFactor;
    }
}
