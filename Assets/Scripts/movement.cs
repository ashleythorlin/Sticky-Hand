using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    private bool isJumping;
    private bool isMoveDown;
    private bool isMoveLeftGrounded;
    private bool isMoveRightGrounded;
    private bool connected;
    private bool isGrounded;
    private float jumpMult;
    private float veloPerTick;
    private float _maxAxisSpeed;
    physicsPlayer physics;
    private playerActions actions;
    private Rigidbody2D _rigidbody2D;

    // Start is called before the first frame update
    void Start()
    {
        physics = GetComponent<physicsPlayer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        actions = GetComponent<playerActions>();
        isJumping = false;
        isMoveDown = false;
        isMoveLeftGrounded = false;
        isMoveRightGrounded = false;
        connected = false;
        isGrounded = false;
        jumpMult = 30f;
        veloPerTick = 0.9f;
        _maxAxisSpeed = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        connected = actions.connected;
        isGrounded = physics.isGrounded;
        //jumping
        isJumping = (Input.GetKey("w") || Input.GetKey("space")) && !connected && isGrounded;
        
        
        //move down
        isMoveDown = Input.GetKey("s") && !connected;

        //move left
        isMoveLeftGrounded = Input.GetKey("a") && !connected;
        
        //move right
        isMoveRightGrounded = Input.GetKey("d") && !connected;
    }

    private void FixedUpdate()
    {
        //WASD physics
        //&& _rigidbody2D.velocity < 
        Vector2 v2;
        if (isJumping && _rigidbody2D.velocity.y < 5f) {
            v2 = _rigidbody2D.velocity;
            Vector2 up = new Vector2(0.0f, (jumpMult * veloPerTick));
            _rigidbody2D.AddForce(up, ForceMode2D.Impulse);
        }

        if (isMoveDown) {
            v2 = _rigidbody2D.velocity;
            if (v2.y >= -_maxAxisSpeed) {
                Vector2 down = new Vector2(0.0f, -veloPerTick);
                _rigidbody2D.AddForce(down, ForceMode2D.Impulse);
            }
        }

        if (isMoveLeftGrounded) {
            v2 = _rigidbody2D.velocity;
            if (v2.x >= -_maxAxisSpeed) {
                Vector2 left = new Vector2(-veloPerTick, 0.0f);
                _rigidbody2D.AddForce(left, ForceMode2D.Impulse);
            }
        }


        if (isMoveRightGrounded) {
            v2 = _rigidbody2D.velocity;
            if (v2.x <= _maxAxisSpeed) {
                Vector2 right = new Vector2(veloPerTick, 0.0f);
                _rigidbody2D.AddForce(right, ForceMode2D.Impulse);
            }
        }
    }
}
