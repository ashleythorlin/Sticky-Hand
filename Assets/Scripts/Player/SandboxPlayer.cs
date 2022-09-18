using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxPlayer : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private LineRenderer _lineRenderer;
    private Transform grappleObject;
    private RaycastHit2D grappleHit;
    private SpringJoint2D _springJoint2D;
    private Animator _animator;
    private Camera _camera;
    Vector2 mousePos;
    Vector2 v2;
    Vector2 lastVelocity;
    private int jumpCount;
    bool jumpPadHit;
    private Vector2 playerPos;
    private float veloPerTick = 0.9f;
    private int grappleLayer = 7;
    private int oneWayLayer = 8;

    private float _maxAxisSpeed = 10;

    // Use for auto-aiming grapple, (distance from mousePos)
    // private float MAXRADIUS = 103f;
    private bool connected;
    private bool isGrounded;
    private bool isCollidingOneWay;
    private bool facingRight = true;

    // In order to be "grappling," must be connected
    private bool grappling;

    [Header ("Movement Settings")]
    public float jumpMult = 13f;
    public float maxTetherRange = 30f;
    public float forceAmount;
    public float _maxSpeed = 30f;
    public float _jumpMaxSpeed = 50f;

    [Header ("Movement Physics")]
    public float groundDrag;
    public float airDrag;

    [Header ("Perpendicular Grapple Physics")]
    public float maxYForce;
    public float perpMultiplier;

    [Header ("Other")]
    public GameObject hitEffect;
    public float maxTetherThresh = 13f;
    public float minTetherThresh = 11f;

    // This is the multiplier for all non-maxYForce points of connection on the grapple
    public float reducedPerpMult;

    private void Start()
    {
        _camera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        _animator = GetComponent<Animator>();
        connected = false;
        isGrounded = false;
        jumpPadHit = false;
        jumpCount = 0;
        grappling = false;
        isCollidingOneWay = false;

        _lineRenderer.positionCount = 0;
        
    }

    private void Update()
    {
        playerPos = new Vector2(transform.position.x, transform.position.y);
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Debug.Log(mousePos);

        //if press down on mouse
        if (Input.GetMouseButtonDown(0))
        {
            // get HIT POINT (RAYCAST2D), contains actual collision point
            grappleHit = getHitPoint(maxTetherRange, mousePos);

            // get TRANSFORM FROM HIT, collision object (not point of collision)
            grappleObject = grappleHit.transform;

            // if valid hinge, connect to it
            if (grappleHit.collider != null && grappleHit.transform.gameObject.tag == "Hinge") {
                _springJoint2D = grappleObject.GetComponent<SpringJoint2D>();
                _springJoint2D.connectedBody = _rigidbody2D;

                // convert grappleHit.point to local space relative to grappleObject
                _springJoint2D.anchor = grappleObject.InverseTransformPoint(grappleHit.point);
                // set line renderer position count
                _lineRenderer.positionCount = 2;
                connected = true;
            }
        }

        // if connected, animate grappling
        if (Input.GetMouseButton(0) && connected)
        {
            connectAnimation();

            //Pseudo animate connection
            _lineRenderer.SetPosition(0,playerPos);
            // USE GRAPPLEHIT to get collsion point
            _lineRenderer.SetPosition(1,grappleHit.point);
            _rigidbody2D.AddForce( transform.forward * forceAmount,ForceMode2D.Impulse );

            // GRAPPLE CODE HERE
            if (Input.GetMouseButton(1)) {
                grappling = true;
                if (_springJoint2D.distance > 10f) {
                    _springJoint2D.distance *= 0.4f;
                }

                //Ensures no carry over velocity
                Vector2 playerToGrapple = grappleHit.point - playerPos;
                _rigidbody2D.velocity = playerToGrapple.normalized * 50f;
            } else {
                grappling = false;
                if (_springJoint2D.distance > maxTetherThresh)
                {
                    _springJoint2D.distance *= 0.7f;
                } else if (_springJoint2D.distance < minTetherThresh) {
                    _springJoint2D.distance *= 1.1f;
                }
            }
        } // if not connected, animate a missed grapple
        else if (Input.GetMouseButton(0)) {
            missAnimation(playerPos, mousePos);
        }


        // if connected, disconnect and delete line
        if (Input.GetMouseButtonUp(0) && connected)
        {
            // remove line and hinge
            _lineRenderer.positionCount = 0;
            _springJoint2D.connectedBody = null;
            connected = false;
        } // if not connected, delete line
        else if (Input.GetMouseButtonUp(0)) {
            _lineRenderer.positionCount = 0;
            connected = false;
        }


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

        if (!connected) {
            grappling = false;
        }
        

        //WASD input
        //jumping
        if ((Input.GetKeyDown("w") || Input.GetKeyDown("space")) && !connected && isGrounded) {
            v2 = _rigidbody2D.velocity;
            Vector2 up = new Vector2(0.0f, (jumpMult * veloPerTick));
            _rigidbody2D.AddForce(up, ForceMode2D.Impulse);
        }
        //move left
        if (Input.GetKey("a") && !connected) {
            v2 = _rigidbody2D.velocity;
            if (v2.x >= -_maxAxisSpeed) {
                Vector2 left = new Vector2(-veloPerTick, 0.0f);
                _rigidbody2D.AddForce(left, ForceMode2D.Impulse);
            }
        // move left on grapple
        } else if (Input.GetKey("a") && connected && !grappling) {
            Vector2 grappleDirection = grappleHit.point - playerPos;
            Vector2 perpendicular = Vector2.Perpendicular(grappleDirection);
            // -maxYForce < perpendicular.y &
            if(perpendicular.y < -maxYForce)
            {
                Debug.Log(perpendicular);
                _rigidbody2D.AddForce(perpendicular * perpMultiplier, ForceMode2D.Impulse);

            } else {
                _rigidbody2D.AddForce(perpendicular * reducedPerpMult, ForceMode2D.Impulse);
            }
        }

        //move down
        if (Input.GetKey("s") && !connected) {
            v2 = _rigidbody2D.velocity;
            if (v2.y >= -_maxAxisSpeed) {
                Vector2 down = new Vector2(0.0f, -veloPerTick);
                _rigidbody2D.AddForce(down, ForceMode2D.Impulse);
            }
        } 

        //move right
        if (Input.GetKey("d") && !connected) {
            v2 = _rigidbody2D.velocity;
            if (v2.x <= _maxAxisSpeed) {
                Vector2 right = new Vector2(veloPerTick, 0.0f);
                _rigidbody2D.AddForce(right, ForceMode2D.Impulse);
            }

        //move right on grapple
        } else if (Input.GetKey("d") && connected && !grappling) {
            Vector2 grappleDirection = grappleHit.point - playerPos;
            Vector2 perpendicular = -Vector2.Perpendicular(grappleDirection);
            // -maxYForce < perpendicular.y && 
            if (perpendicular.y < -maxYForce)
            {
                Debug.Log(perpendicular);
                _rigidbody2D.AddForce(perpendicular * perpMultiplier, ForceMode2D.Impulse);

            } else {
                _rigidbody2D.AddForce(perpendicular * reducedPerpMult, ForceMode2D.Impulse);
            }
        }

        UpdateAnimations();
    }


    private void FixedUpdate()
    {
        if (_rigidbody2D.velocity.magnitude > _maxSpeed && !jumpPadHit)
        {
            _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * _maxSpeed;
        } else if (jumpPadHit) {
            jumpCount++;
            if (_rigidbody2D.velocity.magnitude > _jumpMaxSpeed) {
                _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * _jumpMaxSpeed;
            }
        }

        if (jumpCount > 10) {
            jumpCount = 0;
            jumpPadHit = false;
        }

        lastVelocity = _rigidbody2D.velocity;
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

    private RaycastHit2D getHitPoint(float maxTether, Vector2 mousePos) {
        RaycastHit2D hit = Physics2D.Raycast(playerPos, mousePos - playerPos, maxTether, ~grappleLayer);
        Debug.Log(hit.transform.gameObject.layer);

        return hit;
    }

    private void connectAnimation() {

    }

    private void missAnimation(Vector2 curPos, Vector2 grapplePos) {

        //Pseudo animation code
        if (_lineRenderer.positionCount != 2) {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0,curPos);
            _lineRenderer.SetPosition(1,grapplePos);
        } else {
            _lineRenderer.SetPosition(0,curPos);
        }
    }

    //make sure u replace "floor" with your gameobject name.on which player is standing
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isGrounded = true;
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

    //consider when character is jumping .. it will exit collision.
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isGrounded = false;
        }

        if (collision.gameObject.layer == oneWayLayer) {
            isCollidingOneWay = false;
        }
    }
    /// executes animations based on key input and state
    void UpdateAnimations()
    {
        if(Input.GetKey("a") && facingRight){
            Flip();
        } 
        else if (Input.GetKey("d") && !facingRight){
            Flip();
        }

        if ((Input.GetKeyDown("w") || Input.GetKeyDown("space")) && !connected && !isGrounded) {
            _animator.Play("player_jump");
        }
        //walk left/right
        else if ((Input.GetKey("a") || Input.GetKey("d")) && !connected && isGrounded) {
            _animator.Play("player_walk");
        } 
        // move left/right on grapple
        else if ((Input.GetKey("a") || Input.GetKey("d")) && connected && !grappling) {
            _animator.Play("player_idle");
        }

        //move down
        else if (Input.GetKey("s") && !connected) {
            _animator.Play("player_idle");
        } 

        else if(!isGrounded && !connected) {
            _animator.Play("player_fall");
        }
        else {
           _animator.Play("player_idle");
        }
        
    }

    void Flip(){
        // Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }

}