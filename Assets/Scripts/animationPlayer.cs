using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationPlayer : MonoBehaviour
{
    private bool facingRight = true;
    private bool connected;
    private bool isGrounded;
    private physicsPlayer physics;
    private Animator _animator;
    private playerActions actions;


    // Start is called before the first frame update
    void Start()
    {
        physics = GetComponent<physicsPlayer>();
        actions = GetComponent<playerActions>();
        _animator = GetComponent<Animator>();
        connected = false;
        isGrounded = false;
    }

    // Update is called once per frame
    void Update()
    {
        connected = actions.connected;
        isGrounded = physics.isGrounded;
        UpdateAnimations();
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
        // if (isJumping) {
        //     _animator.Play("player_jump");
        // }
        //walk left/right
        if ((Input.GetKey("a") || Input.GetKey("d")) && !connected && isGrounded) {
            _animator.Play("player_walk");
        } 
        // move left/right on grapple
        else if (connected) {
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
}
