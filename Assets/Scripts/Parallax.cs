using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startpos;
    public GameObject cam;
    //determines how strong the parallax effect is
    public float parallaxEffect;
    //ensures that the background repeats itself before the camera reaches the edge
    public float offset;

    void Start()
    {
        //starting position of the object
        startpos = transform.position.x;
        
        //length of the object
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        Component[] objectRenderers;

        objectRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer renderer in objectRenderers)
            length += renderer.bounds.size.x;

    }

    void FixedUpdate()
    {
        //indicates how far we have moved relative to the camera
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        //indicates how far (in world space) we have moved from the startpos
        float dist = (cam.transform.position.x * parallaxEffect);

        //moves camera
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
        

        //makes background infinite
        if (temp > startpos + (length - offset))
        {
            startpos += length;
        }
        else if (temp < startpos - (length - offset))
        {
            startpos -= length;
        }
    }
}