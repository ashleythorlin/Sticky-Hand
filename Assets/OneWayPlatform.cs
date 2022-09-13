using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;
    public float waitTime;

    // Start is called before the first frame update
    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("s")) {
            waitTime = 0.05f;
        }
        
        if (Input.GetKey("s")) {
            if (waitTime <= 0) {
                effector.rotationalOffset = 180f;
                waitTime = 0.05f;
            } else {
                waitTime -= Time.deltaTime;
            }
        }

        if (Input.GetKey("w") || Input.GetKey("space")) {
            effector.rotationalOffset = 0;
        }
    }
}
