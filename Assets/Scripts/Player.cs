using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Look at CharacterController at a later time

public class Player : MonoBehaviour
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
    private bool isTouchingFloor;
    private float prevMissTime;
    private bool reachTop;
    
    //booleans for update => fixedUpdate
    private bool isConnected = false;
    private bool isJumping = false;
    private bool isMoveLeftGrounded = false;
    private bool isMoveLeftGrappled = false;
    private bool isMoveRightGrounded = false;
    private bool isMoveRightGrappled = false;
    private bool isMoveDown = false;


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
    public float missGrappleAnimationDuration = 0.3f;

    // This is the multiplier for all non-maxYForce points of connection on the grapple
    public float reducedPerpMult;

    [Header ("Sticky Hand")]
    public GameObject _stickyHand;
    private Vector3 stickyHand_safeSpace;

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
        prevMissTime = Mathf.Infinity;

        _lineRenderer.positionCount = 0;

        _stickyHand = GameObject.Find("StickyHand_Hand");
        stickyHand_safeSpace = new Vector3(-80, 35, 0);
        reachTop = false;
        
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

                // render hand sprite at hitpoint
                // _stickyHand.transform.position = grappleHit.point;
                renderStickyHand((Vector2)grappleHit.point, new Vector2(0, 0));
            }
        }


        // if connected, animate grappling
        if (Input.GetMouseButton(0) && connected)
        {
            //Pseudo animate connection
            _lineRenderer.SetPosition(0,playerPos);
            // USE GRAPPLEHIT to get collsion point
            _lineRenderer.SetPosition(1,grappleHit.point);

            isConnected = true;
            connectAnimation();

            
        } // if not connected AND we can maintain current grapple, then animate a missed grapple
        else if (Input.GetMouseButton(0) && (Time.time - prevMissTime) < missGrappleAnimationDuration) {
            missAnimation(playerPos, mousePos, maxTetherRange);
        } 

        //make prevMissTime infinity to ensure we can animate miss grapple again
        if (Input.GetMouseButtonUp(0)) {
            prevMissTime = Mathf.Infinity;
        }

        // if connected, disconnect and delete line
        if (Input.GetMouseButtonUp(0) && connected)
        {
            // remove sticky hand
            // _stickyHand.transform.position = stickyHand_safeSpace;
            renderStickyHand((Vector2)stickyHand_safeSpace, new Vector2(0, 0));

            // remove line and hinge
            _lineRenderer.positionCount = 0;
            _springJoint2D.connectedBody = null;
            isConnected = false;
            connected = false;
        } // if not connected OR if we have held miss for too long, delete line 
        else if (Input.GetMouseButtonUp(0) || (Time.time - prevMissTime) >= missGrappleAnimationDuration) {
            // remove sticky hand
            // _stickyHand.transform.position = stickyHand_safeSpace;
            renderStickyHand((Vector2)stickyHand_safeSpace, new Vector2(0, 0));
            
            _lineRenderer.positionCount = 0;
            isConnected = false;
            connected = false;
        }

        if (!connected) {
            grappling = false;
        }
        

        //jumping
        isJumping = (Input.GetKey("w") || Input.GetKey("space")) && !connected && isGrounded;
        

        //move left
        isMoveLeftGrounded = Input.GetKey("a") && !connected;
        
        // move left on grapple
        isMoveLeftGrappled = Input.GetKey("a") && connected && !grappling;
        
        //move down
        isMoveDown = Input.GetKey("s") && !connected;

        //move right
        isMoveRightGrounded = Input.GetKey("d") && !connected;

        //move right on grapple
        isMoveRightGrappled = Input.GetKey("d") && connected && !grappling;

        UpdateAnimations();
    }


    private void FixedUpdate()
    {
        
        //WASD input
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


        if (isConnected) {
            _rigidbody2D.AddForce( transform.forward * forceAmount,ForceMode2D.Impulse );

            // GRAPPLE CODE HERE
            if (Input.GetMouseButton(1)) {
                grappling = true;
                Vector2 playerToGrapple = grappleHit.point - playerPos;
                // reachTop is to ensure once we are at the top we reset
                if (playerToGrapple.magnitude < 0.6 && !reachTop) {
                    _rigidbody2D.velocity = new Vector2(0, 0);
                    reachTop = true;
                } else if (!reachTop) {
                    _rigidbody2D.velocity = playerToGrapple.normalized * 55f;
                }
            } else {
                grappling = false;
                reachTop = false;
                if (_springJoint2D.distance > maxTetherThresh)
                {
                    _springJoint2D.distance *= 0.7f;
                } else if (_springJoint2D.distance < minTetherThresh) {
                    _springJoint2D.distance *= 1.1f;
                }
            }
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


        //WASD physics
        if (isJumping) {
            v2 = _rigidbody2D.velocity;
            Vector2 up = new Vector2(0.0f, (jumpMult * veloPerTick));
            _rigidbody2D.AddForce(up, ForceMode2D.Impulse);
        }
        if (isMoveLeftGrounded) {
            v2 = _rigidbody2D.velocity;
            if (v2.x >= -_maxAxisSpeed) {
                Vector2 left = new Vector2(-veloPerTick, 0.0f);
                _rigidbody2D.AddForce(left, ForceMode2D.Impulse);
            }
        }

        if (isMoveLeftGrappled) {
            Vector2 grappleDirection = grappleHit.point - playerPos;
            Vector2 perpendicular = Vector2.Perpendicular(grappleDirection);
            // -maxYForce < perpendicular.y &
            if(perpendicular.y < -maxYForce)
            {
                // Debug.Log(perpendicular);
                _rigidbody2D.AddForce(perpendicular * perpMultiplier, ForceMode2D.Impulse);

            } else {
                _rigidbody2D.AddForce(perpendicular * reducedPerpMult, ForceMode2D.Impulse);
            }
        }

        if (isMoveDown) {
            v2 = _rigidbody2D.velocity;
            if (v2.y >= -_maxAxisSpeed) {
                Vector2 down = new Vector2(0.0f, -veloPerTick);
                _rigidbody2D.AddForce(down, ForceMode2D.Impulse);
            }
        }

        if (isMoveRightGrounded) {
            v2 = _rigidbody2D.velocity;
            if (v2.x <= _maxAxisSpeed) {
                Vector2 right = new Vector2(veloPerTick, 0.0f);
                _rigidbody2D.AddForce(right, ForceMode2D.Impulse);
            }
        }

        if (isMoveRightGrappled) {
            Vector2 grappleDirection = grappleHit.point - playerPos;
            Vector2 perpendicular = -Vector2.Perpendicular(grappleDirection);
            // -maxYForce < perpendicular.y && 
            if (perpendicular.y < -maxYForce)
            {
                // Debug.Log(perpendicular);
                _rigidbody2D.AddForce(perpendicular * perpMultiplier, ForceMode2D.Impulse);

            } else {
                _rigidbody2D.AddForce(perpendicular * reducedPerpMult, ForceMode2D.Impulse);
            }
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

    private RaycastHit2D getHitPoint(float maxTether, Vector2 mousePos) {
        RaycastHit2D hit = Physics2D.Raycast(playerPos, mousePos - playerPos, maxTether, ~grappleLayer);
        // Debug.Log(hit.transform.gameObject.layer);

        return hit;
    }

    private void connectAnimation() {

    }

    private void missAnimation(Vector2 curPos, Vector2 grapplePos, float maxTether) {
        Vector2 direction = grapplePos - curPos;
        if (direction.magnitude > maxTether) {
            direction.Normalize();
            direction.Set(direction.x * maxTether, direction.y * maxTether);
            grapplePos = curPos + direction;
        }

        
        //Pseudo animation code
        if (_lineRenderer.positionCount != 2) {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0,curPos);
            _lineRenderer.SetPosition(1,grapplePos);
            renderStickyHand(grapplePos, direction);
            prevMissTime = Time.time;
        } else {
            _lineRenderer.SetPosition(0,curPos);
            // _stickyHand.transform.position = stickyHand_safeSpace;
        }
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
    /// executes animations based on key input and state
    void UpdateAnimations()
    {
        if(Input.GetKey("a") && facingRight){
            Flip();
        } 
        else if (Input.GetKey("d") && !facingRight){
            Flip();
        }

        //used previous isJumping variable to sync with update
        if (isJumping) {
            _animator.Play("player_jump");
        }
        //walk left/right
        else if ((Input.GetKey("a") || Input.GetKey("d")) && !connected && isGrounded) {
            _animator.Play("player_walk");
        } 
        // move left/right on grapple
        else if ((Input.GetKey("a") || Input.GetKey("d")) && connected && !grappling) {
            _animator.Play("player_fall");
        }

        //move down
        else if (Input.GetKey("s") && !connected && isGrounded) {
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

    void renderStickyHand(Vector2 hitPoint, Vector2 direction) {
        Vector2 posYAxis = new Vector2(0, 1);
        float angle = Vector2.SignedAngle(posYAxis, direction);
        _stickyHand.transform.rotation = Quaternion.Euler(0, 0, angle);
        _stickyHand.transform.position = hitPoint;
    }
}