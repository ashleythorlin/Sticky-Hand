using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Look at CharacterController at a later time

// Change physicsPlayer name later in all scripts

public class physicsPlayer : MonoBehaviour
{
    
    Vector2 lastVelocity; // **
    private int jumpCount; // **
    bool jumpPadHit; // **
    private int oneWayLayer = 8;
    private Rigidbody2D _rigidbody2D;
    private bool grappling;
    private playerActions actions;
    
    // Use for auto-aiming grapple, (distance from mousePos)
    // private float MAXRADIUS = 103f;
    private bool isCollidingOneWay; // **

   
    public float _maxSpeed = 30f; // **
    public float _jumpMaxSpeed = 50f; // **

    [Header ("Movement Physics")]
    public float groundDrag; // **
    public float airDrag; // **
    public bool isGrounded;
    





    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        actions = GetComponent<playerActions>();
        
    }

    private void Update()
    {
        grappling = actions.grappling;
    }


    private void FixedUpdate()
    {
        //WASD input
        if (_rigidbody2D.velocity.magnitude > _maxSpeed && !jumpPadHit && !grappling)
        {
            _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * _maxSpeed;
        } else if (jumpPadHit) {
            jumpCount++;
            if (_rigidbody2D.velocity.magnitude > _jumpMaxSpeed) {
                Vector2 jumpVelo = _rigidbody2D.velocity.normalized * _jumpMaxSpeed;

                // used to ensure we always have a large y-component for our velocity (want to go up)
                if (jumpVelo.y < 40f) {
                    // calculates newX such that it has the SAME magnitude as jumpVelo
                    // but with 40f as the y-component
                    float newX = (float) Math.Sqrt(jumpVelo.sqrMagnitude - (40f * 40f));
                    if (jumpVelo.x < 0) {
                        newX = -newX;
                    }
                    jumpVelo = new Vector2(newX, 40f);
                }

                _rigidbody2D.velocity = jumpVelo;
            }
        }

        if (jumpCount > 10) {
            jumpCount = 0;
            jumpPadHit = false;
        }

        lastVelocity = _rigidbody2D.velocity;


        

        //Grounded physics and air physics
        if (isGrounded) {
            // Code for detecting oneWay
            if (isCollidingOneWay && _rigidbody2D.velocity.y != 0) {
                _rigidbody2D.drag = 0;
                
            }
            else {
                _rigidbody2D.drag = groundDrag;
            }
        }
        else {
            _rigidbody2D.drag = airDrag;
        }
    }

    // returns closest hinge to cursor within a specified range
    // Transform GetClosestHinge(GameObject[] hinges, float maxRadius, float maxTetherRange)
    // {
    //     Transform closest = null;
    //     var closestDistanceSqr = Mathf.Infinity;
    //     var currentPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
    //     foreach (var potentialTarget in hinges)
    //     {
    //         var directionToTarget = potentialTarget.transform.position - currentPosition;
    //         var dSqrToTarget = directionToTarget.sqrMagnitude;
    //         if (!(dSqrToTarget < closestDistanceSqr)) continue;
    //         closestDistanceSqr = dSqrToTarget;
    //         closest = potentialTarget.transform;
    //     }
        
    //     // If distance from CURSOR, is greater than the maxRadius, then return null
    //     if (closestDistanceSqr > maxRadius) {
    //         return null;
    //     }

    //     // If our PLAYER's distance from the CLOSEST hinge is FARTHER than the maxTetherRange, then return null
    //     float distFromPlayer = Mathf.Sqrt((closest.position - transform.position).sqrMagnitude);
    //     if (distFromPlayer > maxTetherRange) {
    //         return null;
    //     }

    //     return closest;
    // }

    

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
            Vector2 curVelocity = lastVelocity;
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