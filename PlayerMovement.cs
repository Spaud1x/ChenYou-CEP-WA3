using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{


    [Header("Movement")]
    private float moveSpeed; //player speed
    public float walkSpeed;
    public float sprintSpeed;


    public float groundDrag;
    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Other player stuff")]
    public Transform orientation;
    public int playerHealth = 20;
    public int playerDamage = 2;
    public Text healthDisplay;
    public GameObject projectile;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; //to stop player from falling over

        readyToJump = true;

        startYScale = transform.localScale.y;


    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;


        UpdateHealthText();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {

        horizontalInput = Input.GetAxis("Horizontal"); //left, right
        verticalInput = Input.GetAxis("Vertical"); //forward, backward

        //when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //able to continuously jump if you hold jump key
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); //shrink player
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); //add force to shrunk player so he won't float
        }

        //stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
        
    }

    private void StateHandler()
    {
        //Mode - Crouching
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }


        // Mode - Sprinting
        if(grounded && Input.GetKey(sprintKey))
        {
            
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;

        }

        // Mode - Walking
        else if (grounded)
        {

            state = MovementState.walking;
            moveSpeed = walkSpeed;

        }

        // Mode - Air
        else
        {

            state = MovementState.air;

        }


    }


    private void MovePlayer()
    {

        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        //in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {

        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {

        readyToJump = true;
    }

    
    public void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Projectile")
        {
            Debug.Log("you got hit");
            playerHealth -= playerDamage;
            UpdateHealthText();
          

            if(playerHealth <= 0)
            {
                Debug.Log("YOU DIED");
            }
        }

        
    }

    public int GetHealth()
    {
        return playerHealth;
    }


    

    public void UpdateHealthText()
    {

        healthDisplay.text = $"Health:{playerHealth}";
    }

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
    }

    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        Vector3 position;
        position.x = data.position[0];
        position.y = data.position[1];
        position.z = data.position[2];
        transform.position = position;
    }


}
