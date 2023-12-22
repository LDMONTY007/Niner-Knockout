using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;

public class Player : MonoBehaviour
{

    public enum PlayerState
    {
        None,
        attacking,
        falling
    }

    public PlayerState state = PlayerState.None;

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
    //Smash inputs
    private InputAction upSmashAction;
    private InputAction downSmashAction;
    private InputAction rightSmashAction;
    private InputAction leftSmashAction;

    #region attack bools

    bool shouldAttack;
    bool shouldAttackContinuous;

    bool shouldSpecial;
    bool shouldSpecialContinuous;

    bool shouldSmash;
    bool shouldSmashContinuous;

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


    private void Awake()
    {
        //get the player input and assign the actions.
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
        specialAction = playerInput.actions["Special"];
        upSmashAction = playerInput.actions["UpSmash"];
        downSmashAction = playerInput.actions["DownSmash"];
        rightSmashAction = playerInput.actions["RightSmash"];
        leftSmashAction = playerInput.actions["LeftSmash"];

        upSmashAction.performed += UpSmash;
        downSmashAction.performed += DownSmash;
        rightSmashAction.performed += RightSmash;
        leftSmashAction.performed += LeftSmash;
    }

    private void OnEnable()
    {
        upSmashAction.Enable();
        downSmashAction.Enable();
        rightSmashAction.Enable();
        leftSmashAction.Enable();
    }

    private void OnDisable()
    {
        upSmashAction.Disable();
        downSmashAction.Disable();
        rightSmashAction.Disable();
        leftSmashAction.Disable();
    }


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
    }

    // Update is called once per frame
    void Update()
    {

        #region input bools

        //only true during the frame the button is pressed.
        shouldAttack = attackAction.WasPressedThisFrame();
        //While button is held down this is true.
        shouldAttackContinuous = attackAction.IsPressed();

        //only true during the frame the button is pressed.
        shouldSpecial = specialAction.WasPressedThisFrame();
        //While button is held down this is true.
        shouldSpecialContinuous = attackAction.IsPressed();

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


        HandleJump();

        if (isGrounded)
        {
            //only rotate when grounded,
            //or doing back aerial.
            HandleRotation();
            HandleAttack();
            HandleSpecial();
            //HandleSmash();
        }
        else if (inAir)
        {
            HandleAerial();
            HandleSpecial();
            //HandleSmash();
        }
        

        lastXinput = moveInput.x;
        

        

        

        //We are able to 
        //apply forces to the rigidbody during
        //update because we set the rigidbody
        //to interpolate. Normally, this 
        //wouldn't work.
        ApplyFinalMovements();
    }

    private void HandleRotation()
    {
        //We do all rotations after
        //input so that the back 
        //aerial can be registered.
        //if the player is moving the stick in the same direction for more than one frame,
        //set the direction the player is facing.
        if ((moveInput.x > 0 && lastXinput > 0 || moveInput.x < 0 && lastXinput < 0) && moveInput.x - lastXinput > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(1, xAxis < 0 ? 180 : 0, 1);
        }
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
                    //Forward Aerial
                    //only if our sprite is 
                    //facing the same direction of 
                    //our current input.
                    if (Vector2.Dot(playerSprite.transform.right, directionInput) > 0)
                    {
                        ForwardAerial();
                    }
                    //We are inputting the opposite of 
                    //our facing direction. 
                    else
                    {
                        BackAerial();
                    }
                }
                //choose vertical Aerial.
                else
                {
                    //Up Aerial
                    if (dotVector.y > 0)
                    {
                        UpAerial();
                    }//Down Aerial
                    else
                    {
                        DownAerial();
                    }
                }
            }
            //Horizontal attacking (Forward & Back Aerial)
            else if (xAxis != 0 && yAxis == 0)
            {
                //Forward Aerial
                //only if our sprite is 
                //facing the same direction of 
                //our current input.
                if (Vector2.Dot(playerSprite.transform.right, directionInput) > 0)
                {
                    ForwardAerial();
                }
                //We are inputting the opposite of 
                //our facing direction. 
                else
                {
                    BackAerial();
                }
            }
            //Vertical attacking (Up & Down Aerial)
            else if (xAxis == 0 && yAxis != 0)
            {
                //Up Aerial
                if (dotVector.y > 0)
                {
                    UpAerial();
                }//Down Aerial
                else
                {
                    DownTilt();
                }
            }
            //Neutral Aerial
            else
            {
                NeutralAerial();
            }
        }
    }

    private void HandleSpecial()
    {
        //TODO: 
        //Code an if statement for each attack input, a neutral and 4 directions.
        //Make sure to change this if we are handling air attacks.
        if (shouldSpecial)
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
                    //Right Special
                    if (dotVector.x > 0)
                    {
                        RightSpecial();
                    }//Left Special
                    else
                    {
                        LeftSpecial();
                    }
                }//choose vertical attack.
                else
                {
                    //Up Special
                    if (dotVector.y > 0)
                    {
                        UpSpecial();
                    }//Down Special
                    else
                    {
                        DownSpecial();
                    }
                }
            }
            //Horizontal attacking (Left & Right Special)
            else if (xAxis != 0 && yAxis == 0)
            {
                //Right Special
                if (dotVector.x > 0)
                {
                    RightSpecial();
                }
                //Left Special
                else
                {
                    LeftSpecial();
                }
            }
            //Vertical attacking (Up & Down Special)
            else if (xAxis == 0 && yAxis != 0)
            {
                //Up Special
                if (dotVector.y > 0)
                {
                    UpSpecial();
                }//Down Special
                else
                {
                    DownSpecial();
                }
            }
            //Neutral attacking
            else
            {
                NeutralSpecial();
            }
        }
    }

    private void ApplyFinalMovements() //Step 1
    {
        //set velocity directly, don't override y velocity.
        rb.velocity = new Vector2(xAxis * moveSpeed, rb.velocity.y);
    }

    #region Attack Methods

    private void Neutral()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: Neutral ".Color("yellow"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void LeftTilt()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: LeftTilt ".Color("yellow"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    private void RightTilt()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: RightTilt ".Color("yellow"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void UpTilt()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: UpTilt ".Color("yellow"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void DownTilt()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: DownTilt ".Color("yellow"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Aerial Methods

    private void NeutralAerial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: NeutralAerial ".Color("white"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void ForwardAerial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: ForwardAerial ".Color("white"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void BackAerial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        //the only time we rotate when jumping
        //is if we do a back aerial.
        //back aerial is always an input opposite of 
        //the direction the player is facing so we always
        //invert rotation on this attack.
        playerSprite.transform.rotation = Quaternion.Euler(0f, playerSprite.transform.rotation.eulerAngles.y == 0 ? 180f : 0f, 0f);
        Debug.Log("Player 1: BackAerial ".Color("white"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void UpAerial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: UpAerial ".Color("white"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void DownAerial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: DownAerial ".Color("white"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Special Attack Methods

    private void NeutralSpecial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: NeutralSpecial ".Color("orange"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void LeftSpecial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: LeftSpecial ".Color("orange"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void RightSpecial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: RightSpecial ".Color("orange"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void UpSpecial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: UpSpecial ".Color("orange"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    private void DownSpecial()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: DownSpecial ".Color("orange"));
        
        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Smash Attack Methods

    private void LeftSmash(InputAction.CallbackContext context)
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }


        Debug.Log("Player 1: LeftSmash ".Color("purple"));

        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    private void RightSmash(InputAction.CallbackContext context)
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }


        Debug.Log("Player 1: RightSmash ".Color("purple"));

        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    private void UpSmash(InputAction.CallbackContext context)
    {

        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }

        Debug.Log("Player 1: UpSmash ".Color("purple"));

        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    private void DownSmash(InputAction.CallbackContext context)
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }

        state = PlayerState.attacking;
        Debug.Log("Player 1: DownSmash ".Color("purple"));

        //TODO: Actually code this attack.

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion
}