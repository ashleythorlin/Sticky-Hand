// Updates collision related variables when collisions occur.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerCollisions : MonoBehaviour
{
    public bool jumpPadHit;
    public bool isGrounded;
    public bool isCollidingOneWay;

    private Rigidbody2D _rigidbody2D;
    private int oneWayLayer = 8;
    private physicsPlayer physics;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        physics = GetComponent<physicsPlayer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //make sure u replace "floor" with your gameobject name.on which player is standing
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider.gameObject.tag == "Floor") {
                isGrounded = true;
            }
        }


        if (collision.gameObject.tag == "jumpPad") {
            Vector2 curVelocity = physics.lastVelocity;
            Vector2 normalVelocity = collision.contacts[0].normal;
            jumpPadHit = true;
            _rigidbody2D.velocity = Vector2.Reflect(curVelocity, normalVelocity) * 1000;
        }
        
        //Detect oneWayCollision
        if (collision.gameObject.layer == oneWayLayer) {
            isCollidingOneWay = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision) 
    {
        //ensures you are still grounded if touching the floor
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider.gameObject.tag == "Floor") {
                isGrounded = true;
            }
        }

    }   
    //consider when character is jumping .. it will exit collision.
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor") {
            isGrounded = false;
        }

        if (collision.gameObject.layer == oneWayLayer) {
            isCollidingOneWay = false;
        }
    }
}

