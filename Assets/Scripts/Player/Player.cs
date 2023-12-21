using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;

public class Player : MonoBehaviour
{
    public GameObject playerSprite;

    public float groundCheckDist = 0.1f;

    [Header("Movement Parameters")] //Explain that this will show a header in the inspector to categorize variables
    [Range(1, 10)] public float walkSpeed = 5f;
    [Range(1, 20)] public float runSpeed = 12f; 
    [Range(1, 20)] public float maxSpeed = 14f;
    [Range(1, 10)] public float maxDecelSpeed = 5f; //When you let go of the controls clamp speed to this value. Probs step 2

    public float xAxis;
    public float yAxis;


    private float lastXinput;
    private Vector2 moveInput;
    private Vector2 moveDirection;

    #region movement bools
    //we walk if the x input is less than or equal to 0.5f
    private bool isWalking => Mathf.Abs(xAxis) <= 0.5f ? true : false;
    //set movespeed based on if we are walking.
    private float moveSpeed => isWalking ? walkSpeed : runSpeed;
    #endregion

    private Transform camTransform;

    private Rigidbody2D rb;

    private PlayerInput playerInput;

    //Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction specialAction;
    private InputAction smashAction;

    #region attack

    bool shouldAttack;
    bool shouldAttackContinuous;

    #endregion

    #region Jumping

    public bool isGrounded => Physics2D.BoxCast(transform.position, this.GetComponent<BoxCollider2D>().size, 0f, -transform.up, groundCheckDist, rCasting);
    public bool inAir => !jumping && !isGrounded;
    public bool doJump;





    private LayerMask rCasting;

    [Header("Jumping Parameters")]
    [SerializeField] private int jumpCount = 1; //What we modify and check when jumping.
    public int jumpTotal = 1; //Total jumps, so for instance if you wanted 3 jumps set this to 3.
    [SerializeField] private bool jumpCanceled;
    [SerializeField] private bool jumping;
    public float jumpHeight = 5f; //Our jump height, set this to a specific value and our player will reach that height with a maximum deviation of 0.1
    private float buttonTime;
    private float jumpTime;
    public float jumpDist; //used to see the measured distance of the jump.
    public Vector2 ogJump; //Not included just like what I said above.
    public float fallMultiplier = 9f; //When you reach the peak of the expected arc this is the force applied to make falling more fluid.
    public float lowJumpMultiplier = 15f; //When you stop holding jump to do a low jump this is the force applied to make the jump stop short.
    private float Multiplier = 100f; //This is so we can scale with deltatime properly.

    #endregion

    

    // Start is called before the first frame update
    void Start()
    {

        rCasting = LayerMask.GetMask("Player"); //Assign our layer mask to player
        rCasting = ~rCasting; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

        //get the main camera's transform.
        //WE SHOULD NEVER HAVE MORE THAN 1 CAMERA.
        camTransform = Camera.main.transform;

        //get rigidbody.
        rb = GetComponent<Rigidbody2D>();

        //get the player input and assign the actions.
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
        specialAction = playerInput.actions["Special"];
        smashAction = playerInput.actions["SmashAttack"];
    }

    // Update is called once per frame
    void Update()
    {

        #region input bools

        //only true during the frame the button is pressed.
        shouldAttack = attackAction.WasPressedThisFrame();
        //While button is held down this is true.
        shouldAttackContinuous = attackAction.IsPressed();

        #endregion

        #region jump bool control
        doJump |= (jumpAction.WasPressedThisFrame() && jumpCount > 0 && !jumping); //We use |= because we want doJump to be set from true to false
        //This ^ Operator is the or equals operator, it's kind of hard to explain so hopefully I explain this correctly,
        //Basically its saying this : doJump = doJump || (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping)
        //Which is too say, if doJump is already true we return true otherwise we check (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping)
        //The reason we do this is because the only time we want doJump = false is when we directly set it later in the code after we
        //call the jump function. So unless we are setting doJump to false it will be able to return true and only check our conditional
        //again once it is set to false.
        //in the event that doJump is already false and our conditional returns true;
        if (isGrounded)
        {
            if (!jumping)
            {
                jumpCount = jumpTotal; //Reset jump count when we land.
                jumpCanceled = false;
            }
        }

        if (jumping) //Track the time in air when jumping
        {
            jumpTime += Time.deltaTime;
        }

        if (jumping && !jumpCanceled)
        {

            if (!jumpAction.IsPressed()) //If we stop giving input for jump cancel jump so we can have a variable jump.
            {
                jumpCanceled = true;
            }

            if (jumpTime >= buttonTime) //When we reach our projected time stop jumping and begin falling.
            {
                Debug.Log("JUMP CANCELED BY BUTTON TIME".Color("Green"));
                jumpCanceled = true;
                jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
            }
        }

        if (jumpCanceled) //Cancel the jump.
        {
            jumping = false;
        }

        #endregion

        moveInput = moveAction.ReadValue<Vector2>();
        moveDirection = ((moveInput.normalized.x * camTransform.right.normalized) + (moveInput.normalized.y * camTransform.up.normalized)) * moveSpeed;

        //TODO: check for how far stick is tilted and run instead if tilted farther than 0.5 in either direction.

        //xAxis = moveInput.x != 0 ? moveInput.x > 0 ? 1 : -1 : 0;
        //A or D is pressed return -1 or 1, relative to which
        //if tilted less than or equal to .75f then walk.
        //otherwise return zero.
        if (moveInput.x != 0 && Mathf.Abs(moveInput.x) > 0.01 && (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)))
        {
            if (moveInput.x > 0)
            {
                xAxis = moveInput.x <= 0.75f ? 0.5f : 1f;
            }
            else if (moveInput.x < 0)
            {
                xAxis = moveInput.x <= -0.75f ? -1 : -0.5f;
            }
        }
        else
        {
            xAxis = 0;
        }


        if (moveInput.y != 0 && Mathf.Abs(moveInput.y) > 0.01)
        {
            if (moveInput.y > 0)
            {
                yAxis = moveInput.y <= 0.75f ? 0.5f : 1f;
            }
            else if (moveInput.y < 0)
            {
                yAxis = moveInput.y <= -0.75f ? -1 : -0.5f;
            }
        }
        else
        {
            yAxis = 0;
        }
        
        //if the player is moving the stick in the same direction for more than one frame,
        //set the direction the player is facing.
        if ((moveInput.x > 0 && lastXinput > 0 || moveInput.x < 0 && lastXinput < 0) && moveInput.x - lastXinput > 0)
        {
            playerSprite.transform.localScale = new Vector3(xAxis < 0 ? -1 : 1, 1, 1);
        }

        lastXinput = moveInput.x;
        

        HandleJump();

        HandleAttack();

        //We are able to 
        //apply forces to the rigidbody during
        //update because we set the rigidbody
        //to interpolate. Normally, this 
        //wouldn't work.
        ApplyFinalMovements();
    }

    private void HandleJump()
    {


        if (doJump)
        {
            doJump = false;
            jumpCount--;
            ogJump = transform.position;
            float jumpForce;

            jumpForce = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * jumpHeight) * rb.mass; //multiply by mass at the
            
            //end so that it reaches the height regardless of weight.
            buttonTime = (jumpForce / (rb.mass * Physics2D.gravity.magnitude)); //initial velocity divided by player accel for gravity gives us the amount of time it will take to reach the apex.
            
            rb.velocity = new Vector2(rb.velocity.x, 0); //Reset y velocity before we jump so it is always reaching desired height.
            
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse); //don't normalize transform.up cus it makes jumping more inconsistent.
            jumpTime = 0;
            jumping = true;
            jumpCanceled = false;
        }

        //Where I learned this https://www.youtube.com/watch?v=7KiK0Aqtmzc
        //This is what gives us consistent fall velocity so that jumping has the correct arc.
        Vector2 localVel = transform.InverseTransformDirection(rb.velocity);

        if (localVel.y < 0 && inAir) //If we are in the air and at the top of the arc then apply our fall speed to make falling more game-like
        {
            //we don't multiply by mass because forceMode2D.Force includes that in it's calculation.
            Vector2 jumpVec = /*Multiplier * */-transform.up * (fallMultiplier - 1)/* * Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
        else if (localVel.y > 0 && !jumpAction.IsPressed() && inAir) //If we stop before reaching the top of our arc then apply enough downward velocity to stop moving, then proceed falling down to give us a variable jump.
        {
            Vector2 jumpVec = /*Multiplier * */-transform.up * (lowJumpMultiplier - 1) /** Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
    }

    private void HandleAttack()
    {
        //TODO: 
        //Code an if statement for each attack input, a neutral and 4 directions.
        //Make sure to change this if we are handling air attacks.
        if (shouldAttack)
        {
            Vector2 directionInput = new Vector2(xAxis, yAxis);
            Vector2 dotVector = new Vector2(Vector2.Dot(Vector2.right, directionInput), Vector2.Dot(Vector2.up, directionInput));

            if (dotVector.x != 0 && dotVector.x == dotVector.y)
            {
                //I think if they're the same I'm just going to 
                //make it do up/down attacks depending on if y is positive or negative.
                Debug.LogWarning("The user input equal weight on both the x and y axes when attacking. Please figure out how to avoid this happening.");
            }
            //if we have a mixed input, let's see which is greater.
            else if (dotVector.x != 0 && dotVector.y != 0)
            {
                //Choose horizontal attack
                if (Mathf.Abs(dotVector.x) > Mathf.Abs(dotVector.y))
                {
                    //Right Tilt
                    if (dotVector.x > 0)
                    {
/*                        if (inAir)
                        {
                            ForwardAerial();
                        }*/
                        RightTilt();
                    }//Left Tilt
                    else
                    {
                        LeftTilt();
                    }
                }//choose vertical attack.
                else
                {
                    //Up Tilt
                    if (dotVector.y > 0)
                    {
                        UpTilt();
                    }//Down Tilt
                    else
                    {
                        DownTilt();
                    }
                }
            }
            //Horizontal attacking (Left & Right Tilt)
            else if (xAxis != 0 && yAxis == 0)
            {
                //Right Tilt
                if (dotVector.x > 0)
                {
                    RightTilt();
                }
                //Left Tilt
                else
                {
                    LeftTilt();
                }
            }
            //Vertical attacking (Up & Down Tilt)
            else if (xAxis == 0 && yAxis != 0)
            {
                //Up Tilt
                if (dotVector.y > 0)
                {
                    UpTilt();
                }//Down Tilt
                else
                {
                    DownTilt();
                }
            }
            //Neutral attacking
            else
            {
                Neutral();
            }
        }
    }

    private void HandleAerial()
    {

    }

    private void HandleSpecial()
    {

    }

    private void ApplyFinalMovements() //Step 1
    {
        //set velocity directly, don't override y velocity.
        rb.velocity = new Vector2(xAxis * moveSpeed, rb.velocity.y);
    }

    #region Attack Methods

    private void Neutral()
    {
        Debug.Log("Player 1: Neutral ".Color("green"));
    }

    private void LeftTilt()
    {
        Debug.Log("Player 1: LeftTilt ".Color("yellow"));
    }

    private void RightTilt()
    {
        Debug.Log("Player 1: RightTilt ".Color("yellow"));
    }

    private void UpTilt()
    {
        Debug.Log("Player 1: UpTilt ".Color("yellow"));
    }

    private void DownTilt()
    {
        Debug.Log("Player 1: DownTilt ".Color("yellow"));
    }

    #endregion

    #region Aerial Methods

    private void NeutralAerial()
    {
        Debug.Log("Player 1: NeutralAerial ".Color("white"));
    }

    private void ForwardAerial()
    {
        Debug.Log("Player 1: ForwardAerial ".Color("white"));
    }

    private void BackAerial()
    {
        Debug.Log("Player 1: BackAerial ".Color("white"));
    }

    private void UpAerial()
    {
        Debug.Log("Player 1: UpAerial ".Color("white"));
    }

    private void DownAerial()
    {
        Debug.Log("Player 1: DownAerial ".Color("white"));
    }

    #endregion
}
