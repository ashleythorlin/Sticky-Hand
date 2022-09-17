using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// create collider script for reference for collider variable




public class playerActions : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private LineRenderer _lineRenderer;
    private Transform grappleObject;
    private RaycastHit2D grappleHit;
    private SpringJoint2D _springJoint2D;
    private Camera _camera;
    Vector2 mousePos;
    Vector2 v2;
    private Vector2 playerPos;
    private int grappleLayer = 7;

    private bool isTouchingFloor;
    private float prevMissTime;
    private bool reachTop;
    // private playerCollision collider;
    private physicsPlayer physics;

    //booleans for update => fixedUpdate
    private bool isConnected = false;
    private bool isMoveLeftGrappled = false;
    private bool isMoveRightGrappled = false;

     // In order to be "grappling," must be connected
    public bool grappling;

    [Header ("Movement Settings")]
    public float maxTetherRange = 20f;
    public float forceAmount = 200f;

     [Header ("Perpendicular Grapple Physics")]
    public float maxYForce = 1.9f;
    public float perpMultiplier = 0.18f;

    [Header ("Other")]
    public GameObject hitEffect;
    public float maxTetherThresh = 17f;
    public float minTetherThresh = 15f;
    public float missGrappleAnimationDuration = 0.5f;
    public bool connected;
    public bool isGrounded;

    // This is the multiplier for all non-maxYForce points of connection on the grapple
    public float reducedPerpMult = 0.02f;

    [Header ("Sticky Hand")]
    public GameObject _stickyHand;
    private Vector3 stickyHand_safeSpace;


    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponent<LineRenderer>();
        physics = GetComponent<physicsPlayer>();
        connected = false;
        isGrounded = false;
        grappling = false;
        prevMissTime = Mathf.Infinity;

        _lineRenderer.positionCount = 0;

        _stickyHand = GameObject.Find("StickyHand_Hand");
        stickyHand_safeSpace = new Vector3(-80, 35, 0);
        reachTop = false;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = physics.isGrounded;
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
        

        //move right on grapple
        isMoveRightGrappled = Input.GetKey("d") && connected && !grappling;

        // move left on grapple
        isMoveLeftGrappled = Input.GetKey("a") && connected && !grappling;
    }


    private void FixedUpdate()
    {
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

        //grapple physics

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


    void renderStickyHand(Vector2 hitPoint, Vector2 direction) {
        Vector2 posYAxis = new Vector2(0, 1);
        float angle = Vector2.SignedAngle(posYAxis, direction);
        _stickyHand.transform.rotation = Quaternion.Euler(0, 0, angle);
        _stickyHand.transform.position = hitPoint;
    }
}
