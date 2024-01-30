using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

public class Player : MonoBehaviour
{
    private float _damagePercent = 0f;

    //The gravity we return to 
    //after modifying gravity.
    float baseGravity = 9.81f;
    float gravity = 9.81f;


    public float damagePercent { get { return _damagePercent; } set { float clamped = Mathf.Clamp(value, 0f, 999.0f); _damagePercent = clamped; } }

    public CharacterIcon characterIcon;

    private Icon icon;

    public Hurtbox hurtbox;

    public ParticleSystem launchParticles;

    private bool isFacingLeft;

    //the index of this character in the GameManager.
    public int characterIndex;

    /// <summary>
    /// Used to set the angle, damage, and knockback of attacks.
    /// </summary>
    public Moveset moveset;


    //Hitstun should probably be a part of the playerstate
    //or some sort of substate so that these are 2 layers 
    //compunded.

    int hitStunFrames = 0;
    //If hitStunFrames is not 0 we are hitstunned.
    bool isHitStunned => hitStunFrames > 0;

    public enum PlayerState
    {
        None,       //Base state, no additional effects are applied.
        attacking,  //Induced when attacking. Just allows us to make sure we don't start another attack when already attacking. Might need to delete this.
        launched,   //Induced when launched. This just lets us know to stop the old launch coroutine and start a new one. Disables some physics.
        helpless,   //Induced after running out of jumps while in the air. Sometimes called "Freefall" https://www.ssbwiki.com/Helpless
        intangible, //Induced by dodging. Cannot be hit or pushed by other players. https://www.ssbwiki.com/Intangibility
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

    private Vector2 lastDirectionInput;
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

    public Animator animator;

    private PlayerInput playerInput;

    //Input actions
    private InputAction moveAction;
    private InputAction dirAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction specialAction;
    //Attack inputs
    private InputAction upTiltAction;
    private InputAction downTiltAction;
    private InputAction rightTiltAction;
    private InputAction leftTiltAction;
    //Smash inputs
    private InputAction upSmashAction;
    private InputAction downSmashAction;
    private InputAction rightSmashAction;
    private InputAction leftSmashAction;

    #region input bools

    bool shouldAttack;
    bool shouldAttackContinuous;

    bool shouldSpecial;
    bool shouldSpecialContinuous;

    bool shouldSmash;
    bool shouldSmashContinuous;

    //did the player tap this frame?
    bool didTap;
    float tapStopWindow = 0.2f;
    float tapStopTime;

    public float attackStopWindow = 0.2f;
    float attackStopTime;
    bool shouldWaitToAttack;
    bool doDelayedAttack;
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
    public double jumpHeight = 5f; //Our jump height, set this to a specific value and our player will reach that height with a maximum deviation of 0.1
    //time to reach the apex of the jump.
    //0.01f looks just like smash ultimate jumping.
    public float timeToApex = 0.01f;
    public float heightScaleConstant = 120f;
    private float buttonTime;
    private float jumpTime;
    public double jumpDist; //used to see the measured distance of the jump.
    public Vector2 ogJump; //Not included just like what I said above.
    public float fallMultiplier = 9f; //When you reach the peak of the expected arc this is the force applied to make falling more fluid.
    public float lowJumpMultiplier = 15f; //When you stop holding jump to do a low jump this is the force applied to make the jump stop short.

    #endregion


    private void Awake()
    {
        //get the player input and assign the actions.
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        dirAction = playerInput.actions["Direction"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
        specialAction = playerInput.actions["Special"];


        //Smash Attacks
        upSmashAction = playerInput.actions["UpSmash"];
        downSmashAction = playerInput.actions["DownSmash"];
        rightSmashAction = playerInput.actions["RightSmash"];
        leftSmashAction = playerInput.actions["LeftSmash"];

/*        upSmashAction.performed += UpSmash;
        downSmashAction.performed += DownSmash;
        rightSmashAction.performed += RightSmash;
        leftSmashAction.performed += LeftSmash;*/
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

        rCasting = LayerMask.GetMask("Player", "Ignore Raycast"); //Assign our layer mask to player
        rCasting = ~rCasting; //Invert the layermask value so instead of being just the player it becomes every layer but the mask

        //get the main camera's transform.
        //WE SHOULD NEVER HAVE MORE THAN 1 CAMERA.
        camTransform = Camera.main.transform;

        //get rigidbody.
        rb = GetComponent<Rigidbody2D>();

        //get animator if it isn't manually assigned.
        if (animator == null)
        animator = GetComponent<Animator>();

        //DISABLE GRAVITY SO WE CAN USE OUR OWN.
        //rb.gravityScale = 0;
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

        //this deterimines if the player tapped an input direction this frame.
        Vector2 curDir = dirAction.ReadValue<Vector2>();

        //Detect if the player tapped.
        //the first 2 parts of teh conditional
        //insure that we aren't detecting the joystick 
        //flinging back to the center after the player lets
        //go of the joystick.

        //but we are just checking if the speed of the joystick
        //input is fast enough and if it is we detect a tap.
        if (curDir.magnitude > 0.09 && curDir.magnitude > lastDirectionInput.magnitude && ((curDir - lastDirectionInput).magnitude / Time.deltaTime) >= 20f)
        {
            didTap = true;
            //Debug.Log("TAP! ".Color("red") + ((curDir - lastDirectionInput).magnitude / Time.deltaTime));
        }

        
        //window until we stop telling 
        //the code that we tapped. 
        if (didTap /*&& !startedTapInput*/)
        {
            if (tapStopTime >= tapStopWindow)
            {
                tapStopTime = 0f;
                didTap = false;
            }
            else
            tapStopTime += Time.deltaTime;
        }

        if (shouldAttack || shouldWaitToAttack)
        {
            //remove the !shouldWaitToAttack later if need be,
            //but it makes it so that we attack after the delay
            //and if we press attack again it DOESN'T reset the 
            //attack delay thus canceling the attack and extending
            //the timer. 
            if (attackAction.WasPressedThisFrame() && !shouldWaitToAttack)
            {
                attackStopTime = 0f;
                Debug.Log("Attack hit before tapping".Color("red"));
            }
           
            if (attackStopTime >= attackStopWindow)
            {
                //attackStopTime = 0f;
                shouldWaitToAttack = false;
                doDelayedAttack = true;
            }
            else
            {
                attackStopTime += Time.deltaTime;
                shouldWaitToAttack = true;
            }
        }


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
                //animator.SetTrigger("landing");
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

            if (jumpTime < buttonTime)
            {
                jumpDist = transform.position.y - ogJump.y;
            }

            if (jumpTime >= buttonTime) //When we reach our projected time stop jumping and begin falling.
            {
                Debug.Log("JUMP CANCELED BY BUTTON TIME".Color("Green"));
                //pause the editor
                //Debug.Break();
                jumpCanceled = true;

                //set gravity back to normal
                gravity = baseGravity;

                //jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
                jumpDist = transform.position.y - ogJump.y;
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
        
        if (isGrounded)
        {
            //only rotate when grounded,
            //or doing back aerial.
            if (!isHitStunned && state != PlayerState.helpless)
            {
                HandleRotation();
                HandleAttack();
                HandleSpecial();
            }
        }
        else if (inAir)
        {
            if (!isHitStunned && state != PlayerState.helpless)
            {
                HandleAerial();
                HandleSpecial();
            }
        }

        //currently
        //you can jump while doing an attack,
        //I think this is how smash works.
        
        if (!isHitStunned)
        {
            if (doJump && isGrounded)
            {
                //play the jump sequence
                animator.SetTrigger("jump");
                //wait 1 frame then call HandleJump().
                StartCoroutine(LDUtil.WaitFrames(HandleJump, 1));
                /*            if (doJump)
                StartCoroutine(JumpCoroutine(10, jumpHeight));*/
            }
            else
            {
                //if we are air jumping then we don't need a windup frame.
                //HandleJump();

                /*if (doJump)
                    StartCoroutine(JumpCoroutine(10, jumpHeight));*/
            }
        }


        lastXinput = moveInput.x;

        lastDirectionInput = dirAction.ReadValue<Vector2>();



        //We are able to 
        //apply forces to the rigidbody during
        //update because we set the rigidbody
        //to interpolate. Normally, this 
        //wouldn't work.
        //ApplyFinalMovements();

        HandlePassiveAnimation();

        HandleUI();

        //we check for the state after
        //everything else because it would
        //be annoying to input an attack 
        //and see it start just for the 
        //forces from the attack to not
        //be applied.
        //Also I don't have a good explanation
        //for it.
        HandleState();
    }

    private void FixedUpdate()
    {
        HandleJump();
        ApplyFinalMovements();
    }

    private void HandleState()
    {
        #region Helpless check
        //did we run out of jumps?
        if (jumpCount == 0)
        {
            //set state to helpless.
            state = PlayerState.helpless;
        }
        if (state == PlayerState.helpless && isGrounded)
        {
            state = PlayerState.None;
        }
        #endregion


    }

    private void HandleUI()
    {
        if (characterIcon)
        {
            if (characterIcon.GetPercent() != damagePercent)
            characterIcon.SetPercent(damagePercent);
        }
        else
        {
            Debug.LogWarning("This character has no icon assigned!");
        }
    }

    private void HandlePassiveAnimation()
    {
        Vector2 localVel = transform.InverseTransformDirection(rb.velocity);

        animator.SetBool("inAir", inAir);
        //only when we are falling do we turn this var on.
        animator.SetBool("falling", localVel.y < 0);

        animator.SetFloat("speed", moveInput.magnitude > 0.7 ? 1f : moveInput.magnitude < 0.1f ? 0f : 0.5f);

        animator.SetBool("holdAttack", shouldAttackContinuous);
    }

    private void HandleRotation()
    {
        //We do all rotations after
        //input so that the back 
        //aerial can be registered.
        //if the player is moving the stick in the same direction for more than one frame,
        //set the direction the player is facing.
        if (playerInput.currentControlScheme.Equals("Gamepad") && (moveInput.x > 0 && lastXinput > 0 || moveInput.x < 0 && lastXinput < 0) && moveInput.x - lastXinput > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(1, xAxis < 0 ? 180 : 0, 1);
            isFacingLeft = xAxis < 0 ? true : false;
        }
        
        //if user is inputting via keyboard
        if (playerInput.currentControlScheme.Equals("Keyboard&Mouse") && Mathf.Abs(moveInput.x) > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(1, xAxis < 0 ? 180 : 0, 1);
            isFacingLeft = xAxis < 0 ? true : false;
        }
    }

    private void HandleJump()
    {


        if (doJump)
        {
            //this constant (1.2) was discovered
            //by dividing the desired jump height by
            //the height actually reached.
            //this was the value I got regardless of the 
            //jump time. I also think that the timeToApex
            //probably affects this value, for this constant
            //the timeToApex was set to 0.01f.
            //I will look more into this at some other point.

            //I ACTUALLY GOT TO A HEIGHT OF 5 WHEN I MULTIPLIED 
            //BY THIS CONSTANT. 

            //I am genuinely impressed how accurate this formula
            //now is.


            //make the jump height 1/60 
            //then take the actual value reached by the jump
            //then do 1/60 / actual value
            //then do Desired height / actual value
            //and that gives you the most accurate
            //value to input as height for the jump.

            //OR set jump height to 1
            //and take the jump height reached by the jump
            //and do 
            
            //float modifier = 1.2f;//timeToApex / 0.00833333333f;
            float modifiedJumpHeight = (float)jumpHeight * 1.2f; //* modifier;


            //play crouch animation.
            animator.ResetTrigger("jump");
            doJump = false;
            jumpCount--;
            ogJump = transform.position;
            float jumpForce;


            //I did the work out and 2 * h / t = gravity so I'm going to do that.
            gravity = 2 * modifiedJumpHeight / timeToApex;

            float projectedHeight = timeToApex * gravity / 2f;
            Debug.Log(timeToApex + " " + projectedHeight + " " + gravity);
            Debug.Log(("Projected Height " + projectedHeight).ToString().Color("Cyan"));


            //set gravity so that we jump in the amount of time we want
            //Gravity = 2 * height / time^2
            //gravity = 2 * jumpHeight / timeToApex * timeToApex;

            jumpForce = Mathf.Sqrt(2f * gravity * modifiedJumpHeight) * rb.mass; //multiply by mass at the
            
            //end so that it reaches the height regardless of weight.
            buttonTime = (jumpForce / (rb.mass * gravity)); //initial velocity divided by player accel for gravity gives us the amount of time it will take to reach the apex.

            /*            Debug.Log(("Force: " + jumpForce + " " + "Time: " + buttonTime).ToString().Color("white"));


                        jumpForce = Mathf.Sqrt(2f * gravity * jumpHeight * 2f) * rb.mass;*/

            //get the new button time.
            //buttonTime /= 2;

            Debug.Log(("Force: " + jumpForce + " " + "Time: " + buttonTime).ToString().Color("orange"));

            rb.velocity = new Vector2(rb.velocity.x, 0f); //Reset y velocity before we jump so it is always reaching desired height.
            
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
            //animator.SetBool("falling", true);
            //we don't multiply by mass because forceMode2D.Force includes that in it's calculation.
            Vector2 jumpVec = -transform.up * (fallMultiplier - 1)/* * 100f * Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
        else if (localVel.y > 0 && !jumpAction.IsPressed() && inAir) //If we stop before reaching the top of our arc then apply enough downward velocity to stop moving, then proceed falling down to give us a variable jump.
        {
            Debug.Log("Low Jump".Color("cyan"));
            //animator.SetBool("falling", true);
            Vector2 jumpVec = -transform.up * (lowJumpMultiplier - 1) /* * 100f * Time.deltaTime*/;
            rb.AddForce(jumpVec, ForceMode2D.Force);
            Debug.Log(rb.velocity);
        }

/*        if (localVel.y > 0 && jumpTime >= buttonTime)
        {
            //rb.AddForce(-transform.up * Mathf.Sqrt(2f * Physics2D.gravity.magnitude * jumpHeight) * rb.mass);
            rb.AddForce(-transform.up * Physics2D.gravity.magnitude);
        }*/

    }

    private IEnumerator JumpCoroutine(int framesTotal, float height)
    {
        int frames = framesTotal;
        float jumpForce = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * height) * rb.mass;

        while (frames > 0)
        {
            /*if (frames / framesTotal > 2f/3f)
            {
                rb.AddForce(transform.up * jumpForce / framesTotal, ForceMode2D.Force);
            }*/
            rb.AddForce(transform.up * jumpForce / framesTotal, ForceMode2D.Force);
            frames--;
            yield return null;
        }
    }

    private void HandleAttack()
    {
        //TODO: 
        //Code an if statement for each attack input, a neutral and 4 directions.
        //Make sure to change this if we are handling air attacks.
        if (shouldAttack || shouldWaitToAttack || doDelayedAttack)
        {
            if (doDelayedAttack)
            {
                doDelayedAttack = false;
            }
            shouldSmash = didTap && attackAction.IsPressed();

            //if the player inputted the attack button recently and
            //they haven't tapped give them a small window to tap
            //so that they can smash attack. 
            if (!didTap && shouldWaitToAttack)
            {
                //Debug.Log("Should wait");
                return;
            }
            else if (didTap && shouldWaitToAttack)
            {
                //Debug.Log("Should Attack");
                shouldSmash = true;
                shouldWaitToAttack = false;
                doDelayedAttack = false;
                shouldAttack = false;
            }

            //turn off the didtap var.
            didTap = false;

            Vector2 directionInput = new Vector2(xAxis, yAxis);
            Vector2 dotVector = new Vector2(Vector2.Dot(Vector2.right, directionInput), Vector2.Dot(Vector2.up, directionInput));
            if (dotVector.x != 0 && dotVector.x == dotVector.y)
            {
                //These are only reached if the user is inputting
                //the same value for both xAxis and yAxis.
                //we should just assume that if the user 
                //is inputting up, it's with intent
                //and if they are inputting down,
                //it's probably not on purpose.

                //if we are grounded and aiming down
                //then assume we are doing a side attack.
                if (isGrounded && yAxis < 0)
                {
                    if (shouldSmash)
                    {
                        ForwardSmash();
                    }
                    else
                    {
                        ForwardTilt();
                    }
                }

                //if grounded and aiming up
                //then assume we are trying to do an
                //up attack while moving in a direction.
                if (isGrounded && yAxis > 0)
                {
                    if (shouldSmash)
                    {
                        UpSmash();
                    }
                    else
                    {
                        UpTilt();
                    }
                }
              
            }
            //if we have a mixed input, let's see which is greater.
            else if (dotVector.x != 0 && dotVector.y != 0)
            {
                //Choose horizontal attack
                if (Mathf.Abs(dotVector.x) > Mathf.Abs(dotVector.y))
                {
                    if (shouldSmash)
                    {
                        ForwardSmash();
                    }
                    else
                    {
                        ForwardTilt();
                    }
                }//choose vertical attack.
                else
                {
                    //Up Tilt
                    if (dotVector.y > 0)
                    {
                        if (shouldSmash)
                        {
                            UpSmash();
                        }
                        else
                        {
                            UpTilt();
                        }
                    }//Down Tilt
                    else
                    {
                        if (shouldSmash)
                        {
                            DownSmash();
                        }
                        else
                        {
                            DownTilt();
                        }
                    }
                }
            }
            //Horizontal attacking (Left & Right Tilt)
            else if (xAxis != 0 && yAxis == 0)
            {
                if (shouldSmash)
                {
                    ForwardSmash();
                }
                else
                {
                    ForwardTilt();
                }
            }
            //Vertical attacking (Up & Down Tilt)
            else if (xAxis == 0 && yAxis != 0)
            {
                //Up Tilt
                if (dotVector.y > 0)
                {
                    if (shouldSmash)
                    {
                        UpSmash();
                    }
                    else
                    {
                        UpTilt();
                    }
                }//Down Tilt
                else
                {
                    if (shouldSmash)
                    {
                        DownSmash();
                    }
                    else
                    {
                        DownTilt();
                    }
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
                    DownAerial();
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
                    //Forward Special
                    ForwardSpecial();
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
                //Forward Special
                ForwardSpecial();
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
        //Do not let player use inputs when launched.
        if (state == PlayerState.launched)
        {
            return;
        }

        //TODO:
        //we need to check if we are helpless here and only apply Directional Influence (DI) 
        //instead of the normal movement input.

        //set velocity directly, don't override y velocity.
        rb.velocity = new Vector2(xAxis * moveSpeed, rb.velocity.y);

        //Apply gravity, because gravity is not affected by mass and 
        //we can't use ForceMode.acceleration with 2D just multiply
        //by mass at the end. It's basically the same.
        //In unity it factors in mass for this calculation so 
        //multiplying by mass cancels out mass entirely.
        rb.AddForce(-transform.up * gravity * rb.mass);
    }

    #region Attack Methods

    public virtual void Neutral()
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
        SetHurtboxAttackInfo(moveset.Neutral);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void ForwardTilt()
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

        Debug.Log("Player 1: ForwardTilt ".Color("yellow"));

        //handle rotation

        HandleRotation();

        //TODO: Actually code this attack.
        SetHurtboxAttackInfo(moveset.ForwardTilt);

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    public virtual void UpTilt()
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
        SetHurtboxAttackInfo(moveset.UpTilt);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void DownTilt()
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
        SetHurtboxAttackInfo(moveset.DownTilt);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Aerial Methods

    public virtual void NeutralAerial()
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
        SetHurtboxAttackInfo(moveset.NeutralAerial);


        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void ForwardAerial()
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
        SetHurtboxAttackInfo(moveset.ForwardAerial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void BackAerial()
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
        SetHurtboxAttackInfo(moveset.BackAerial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void UpAerial()
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
        SetHurtboxAttackInfo(moveset.UpAerial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void DownAerial()
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
        SetHurtboxAttackInfo(moveset.DownAerial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Special Attack Methods

    public virtual void NeutralSpecial()
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
        SetHurtboxAttackInfo(moveset.NeutralSpecial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void ForwardSpecial()
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

        Debug.Log("Player 1: ForwardSpecial ".Color("orange"));

        //Handle switching facing directions
        HandleRotation();

        //TODO: Actually code this attack.
        SetHurtboxAttackInfo(moveset.ForwardSpecial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    public virtual void UpSpecial()
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

    public virtual void DownSpecial()
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
        SetHurtboxAttackInfo(moveset.DownSpecial);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion

    #region Smash Attack Methods

    public virtual void ForwardSmash()
    {
        //please call "HandleRotation" before calling this method.
        //it is imperative so that your character is facing forward. (the direction of input horizontally)

        //you cannot smash attack while in the air.
        if (inAir)
        {
            return;
        }

        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }
        else
        {
            state = PlayerState.attacking;
        }


        Debug.Log("Player 1: ForwardSmash ".Color("purple"));

        //TODO: Actually code this attack.
        SetHurtboxAttackInfo(moveset.ForwardSmash);

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    public virtual void UpSmash()
    {
        //you cannot smash attack while in the air.
        if (inAir)
        {
            return;
        }

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
        SetHurtboxAttackInfo(moveset.UpSmash);

        //lastly set the playerState back to none.
        state = PlayerState.None;
    }

    public virtual void DownSmash()
    {
        if (state == PlayerState.attacking)
        {
            //cannot attack because we are already attacking.
            return;
        }

        state = PlayerState.attacking;
        Debug.Log("Player 1: DownSmash ".Color("purple"));

        //TODO: Actually code this attack.
        SetHurtboxAttackInfo(moveset.DownSmash);

        //lastly set the playerState back to none.
        state = PlayerState.None;

    }

    #endregion


    public void Launch(float angleDeg, Vector2 attackerDirection, float damageDelt, float baseKnockback, float knockbackScale, int hitLag)
    {
        state = PlayerState.launched;
        //rb.AddForce(direction * SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale), ForceMode2D.Impulse);
        //apparently I'm supposed to multiply the knockback value by 0.03 for the launch but it isn't the right value
        //I don't think. I'm very tired so I'll have to come back and work on this.

        //Ok, so I'm pretty sure that they don't use a weight while applying forces to the character,
        //as in they only add the weight in the formula and don't account for it later because their 
        //formula does 200 / w + 100 which scales the weight to be a 0-2f value. 
        //float totalKB = SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale);
        //rb.velocity = Vector2.one * totalKB * 0.03f * 10f;
        //Debug.Log(totalKB);
        //rb.velocity = angleDeg == 361f ? RadiansToVector(Mathf.Deg2Rad * SakuraiAngle(totalKB, false)) : RadiansToVector(Mathf.Deg2Rad * angleDeg) * SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale) * 0.03f * 10f;
        //rb.AddForce(direction * Mathf.Sqrt(2 * 9.81f * SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale)), ForceMode2D.Impulse);
        StartCoroutine(LaunchCoroutine(angleDeg, attackerDirection, damageDelt, damagePercent, baseKnockback, knockbackScale, hitLag));
        //Debug.Log(rb.velocity + " " + direction * SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale));
        //rb.mass = 1f;
        //rb.AddForce(direction.normalized * SmashKnockback(damageDelt, damagePercent, baseKnockback, knockbackScale) * 0.03f * 10f, ForceMode2D.Impulse);
    }


    //https://www.ssbwiki.com/Knockback#Formula
    public float SmashKnockback(float damageDelt, float currentDamage, float baseKnockback, float knockbackScale)
    {
        float knockback = 0f;
        float p = currentDamage;
        float d = damageDelt;
        //if an attack is weight independent set this value to 100.
        float w = rb.mass;
        //knockback scaling (s / 100) so s = 110 would be 110/100 = 1.1 scale.
        float s = knockbackScale;
        s /= 100f;
        //the attack's base knockback.
        float b = baseKnockback;
        //we aren't going to use the r yet as it is overly complex for our current design.

        //SUPER IMPORTANT NOTE:
        //To determine how far a character is launched away, the numerical amount of knockback caused is multiplied by 0.03 to
        //calculate launch speed, and the initial value of launch speed then decays by 0.051 every frame, so that the character
        //eventually loses all momentum from the knockback. During this time, character-specific attributes such as air friction
        //are disabled; however, falling speed still takes effect, giving fast fallers better endurance against vertical knockback
        //than others of their weight.

        //because weight is input to our rigidbody by the default physics we have to modify this a little bit.
        knockback = ((((p / 10f + p * d / 20f) * 200f / (w + 100f) * 1.4f) + 18) * s) + b;
        Debug.Log(knockback.ToString().Color("blue"));
        return knockback;
    }

    public void Knockback(Vector2 hitDirection, float damageDelt, float currentDamage, float baseKnockback, float knockbackScale)
    {
        float knockback = 0f;
        float p = currentDamage;
        float d = damageDelt;
        //if an attack is weight independent set this value to 100.
        float w = rb.mass;
        //knockback scaling (s / 100) so s = 110 would be 110/100 = 1.1 scale.
        float s = knockbackScale;
        s /= 100f;
        //the attack's base knockback.
        float b = baseKnockback;
        //we aren't going to use the r yet as it is overly complex for our current design.

        //SUPER IMPORTANT NOTE:
        //To determine how far a character is launched away, the numerical amount of knockback caused is multiplied by 0.03 to
        //calculate launch speed, and the initial value of launch speed then decays by 0.051 every frame, so that the character
        //eventually loses all momentum from the knockback. During this time, character-specific attributes such as air friction
        //are disabled; however, falling speed still takes effect, giving fast fallers better endurance against vertical knockback
        //than others of their weight.

        //because weight is input to our rigidbody by the default physics we have to modify this a little bit.
        knockback = ((((p / 10f + p * d / 20f) * 200f / (w + 100f) * 1.4f) + 18) * s) + b;
        Debug.Log(knockback.ToString().Color("green"));

        // Apply the force to the Rigidbody in the hitDirection
        //launch speed is calculated by multiplying knockback by 0.3. 
        rb.velocity = hitDirection.normalized * knockback * 0.03f;

        //Directly from Unity's rb.AddForce docs:
        //"Apply the impulse force instantly with a single function call. This mode depends on the mass of rigidbody so more force must be applied to push or twist higher-mass objects the same amount as lower-mass objects. This mode is useful for applying forces that happen instantly, such as forces from explosions or collisions. In this mode, the unit of the force parameter is applied to the rigidbody as mass*distance/time."

        //mass*distance/time is the important part.
        //What does it mean distance? the magnitude of the vector?
    }

    public IEnumerator LaunchCoroutine(float angleDeg, Vector2 hitDirection, float damageDelt, float currentDamage, float baseKnockback, float knockbackScale, int hitLag)
    {
        //I need to make this coroutine exit when we start a new LaunchCoroutine.

        state = PlayerState.launched;
        float knockback = 0f;
        float p = currentDamage;
        float d = damageDelt;
        //if an attack is weight independent set this value to 100.
        float w = rb.mass;
        //knockback scaling (s / 100) so s = 110 would be 110/100 = 1.1 scale.
        float s = knockbackScale;
        s /= 100f;
        //the attack's base knockback.
        float b = baseKnockback;
        //we aren't going to use the r yet as it is overly complex for our current design.

        //SUPER IMPORTANT NOTE:
        //To determine how far a character is launched away, the numerical amount of knockback caused is multiplied by 0.03 to
        //calculate launch speed, and the initial value of launch speed then decays by 0.051 every frame, so that the character
        //eventually loses all momentum from the knockback. During this time, character-specific attributes such as air friction
        //are disabled; however, falling speed still takes effect, giving fast fallers better endurance against vertical knockback
        //than others of their weight.

        //because weight is input to our rigidbody by the default physics we have to modify this a little bit.
        knockback = ((((p / 10f + p * d / 20f) * 200f / (w + 100f) * 1.4f) + 18) * s) + b;
        Debug.Log(knockback.ToString().Color("red"));
        Debug.Log(angleDeg);
        float angleRad = Mathf.Deg2Rad * angleDeg;
        Debug.Log(angleRad + ":" + angleRad * Mathf.Rad2Deg);
        //Sakurai angle check
        if (Mathf.Abs(angleDeg) == 361f)
        {
            //for now, we don't know if the other player did this as an aerial so input false.
            angleRad = Mathf.Deg2Rad * SakuraiAngle(knockback, false);
            Debug.Log(angleRad + ":" + angleRad * Mathf.Rad2Deg);
            hitDirection = RadiansToVector(angleRad);
            //if the angle is negative flip it over the x axis.
/*            if (angleDeg < 0)
            {
                hitDirection = new Vector2(-hitDirection.x, hitDirection.y);
            }*/
        }
        else
        {
            Debug.Log("Shouldn't be here!");
            hitDirection = RadiansToVector(angleDeg);
        }
        Debug.DrawRay(transform.position, hitDirection * 5f, Color.blue, 1.5f);

        //reflect over x axis if the angle is negative.
        if (angleDeg < 0)
        {
            hitDirection.x = -hitDirection.x;
        }


        //we multiply by 10f because in smash the game unit is actually 1 decimeter, https://www.ssbwiki.com/Distance_unit
        //so if we want to keep a "normal" sized character to be 1x1 unity units we need to multiply by 10
        //to scale our formula properly. 
        //multiply by the launch speed factor 0.03 just like smash.
        //but here it would be 0.3 because we already multiplied by 10
        //but I think it's actually 0.2 because the knockback values only
        //look accurate to the smash ultimate calculator: https://rubendal.github.io/SSBU-Calculator/
        //at this value. Not sure why.
        float launchSpeed = knockback * 0.2f;
        float mass = rb.mass;
        //rb.mass = 1f;
        
        //used for adding gravity.
        float t = 0f;

        //Do launch particles when launch 
        //is strong enough.
        if (knockback > 30f)
        {
            launchParticles.Play();
        }
        else
        {
            launchParticles.Stop();
        }

        float waitTime = launchSpeed;

        //launch for this many frames.
        hitStunFrames = Hitstun(knockback, false);

        while (/*launchSpeed > 0*/ hitStunFrames > 0)
        {
            //If you decide not to apply gravity to the y axis during a launch don't forget
            //to remove these floats and just do rb.velocity = hitDirection * launchSpeed;

            //you need to look into how coroutines work and make sure this while loop isn't
            //updated every frame and that you properly applying this formula.
            //you may need to deprecate the angle so that it applies the force as an arc
            //or do the thing where you apply gravity again.

            //OG
            //float horizontalLaunchSpeed = launchSpeed * Mathf.Cos(angleRad);
            //float verticalLaunchSpeed = launchSpeed * Mathf.Sin(angleRad) - Physics2D.gravity.magnitude * t;

            //NEW
            //float horizontalLaunchSpeed = launchSpeed * Mathf.Cos(angleRad) * t;
            //float verticalLaunchSpeed = launchSpeed * Mathf.Sin(angleRad) * t - (9.8f*t*t)/2;

            //Debug.Log(new Vector2(horizontalLaunchSpeed, verticalLaunchSpeed).ToString().Color("cyan"));
            //rb.velocity = new Vector2(hitDirection.x * Mathf.Clamp(horizontalLaunchSpeed, 0f, Mathf.Infinity), hitDirection.y * verticalLaunchSpeed);
            
            rb.velocity = hitDirection * launchSpeed;//new Vector2(horizontalLaunchSpeed, verticalLaunchSpeed);
            //apply gravity.
            Debug.DrawRay(transform.position, rb.velocity, Color.red, 1.5f);
            if (!isGrounded)
            rb.velocity = rb.velocity + new Vector2(0f, -baseGravity /** t*/);
            launchParticles.gameObject.transform.rotation = Quaternion.LookRotation(rb.velocity);
            launchSpeed -= 0.51f;
            hitStunFrames--;
            //waitTime -= 0.51f;
            t += Time.deltaTime;
            yield return null;
        }
        //stop doing launch particles.
        launchParticles.Stop();
        Debug.Log("CoroutineStop");
        state = PlayerState.None;
    }

    //this is from https://github.com/rubendal/SSBU-Calculator/blob/gh-pages/js/formulas.js#L162
    private int Hitstun(float kb, bool windbox)
    {
        if (windbox)
        {
            return 0;
        }
        var hitstun = (kb * /*parameters.hitstun*/ 0.4);
        if (hitstun < 0)
        {
            return 0;
        }

        //Minimum hitstun for non-windbox hitboxes
        if (hitstun < 5)
            hitstun = 5;

        //convert from double to int.
        return (int)Math.Floor(hitstun) - 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if we are hit by a hurtbox that isn't a child of us then calculate damage and launch.
        if (collision.CompareTag("Hurtbox") && !collision.transform.IsChildOf(this.transform))
        {
            Debug.Log("We were Hit!".Color("red"));
            //get hurtbox
            Hurtbox h = collision.gameObject.GetComponent<Hurtbox>();
            //add damage delt to the total percent
            //https://rubendal.github.io/SSBU-Calculator/
            //Set the damage of a custom attack to 1 and you'll get the constant
            //1.26 and it doesn't scale with percent, the attach damage is always
            //multiplied by this value.
            damagePercent += (h.attackInfo.attackDamage * 1.26f);

            //launch the player based off of the attack damage.
            Launch(h.attackInfo.launchAngle, RadiansToVector(Mathf.Deg2Rad * (h.attackInfo.launchAngle)), h.attackInfo.attackDamage, h.attackInfo.baseKnockback, h.attackInfo.knockbackScale, h.attackInfo.hitLag);
        }
        //the player entered the kill trigger. (kill bounds).
        else if (collision.gameObject.CompareTag("Kill"))
        {
            Debug.Log(gameObject.name + " was killed!");
            //kill player.
            //we destroy both player and the icon for it
            //because I am too lazy to just make code
            //to re-assign it.
            Destroy(characterIcon.gameObject);
            Destroy(gameObject);
        }
    }


    //Use the following methods below if you ever decide to add
    //the warning for the player that they've gone too close to the
    //kill area like in smash where it shows a little circle.
    private void OnBecameInvisible()
    {
        Debug.Log("Player is Offscreen!");
    }

    private void OnBecameVisible()
    {
        Debug.Log("Player is Onscreen!");
    }

    //https://github.com/rubendal/SSBU-Calculator
    private float SakuraiAngle(float kb, bool aerial)
    {
        if (aerial)
        {
            //I don't know why they used this calculation 
            //when they could've printed this value and
            //just put a constant here.
            return Mathf.Rad2Deg * 0.663225f;
        }
        if (kb < 60)
        {
            return 0;
        }
        if (kb >= 88)
        {
            return 38;
        }
        return Mathf.Min((kb - 60) / (88 - 60) * 38 + 1, 38); //https://twitter.com/BenArthur_7/status/956316733597503488
    }

    private Vector2 RadiansToVector(float radians)
    {
        return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
    }

    private void SetHurtboxAttackInfo(AttackInfo attackInfo)
    {
        if (hurtbox != null)
        {
            if (isFacingLeft)
            {
                //flip angle over x axis.
                float angle = -attackInfo.launchAngle;

                AttackInfo invertedX = new AttackInfo(angle, attackInfo.attackDamage, attackInfo.baseKnockback, attackInfo.knockbackScale, attackInfo.hitLag);
                
                hurtbox.attackInfo = invertedX;
            }
            else
            {
                hurtbox.attackInfo = attackInfo;
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance.characterManager != null)
        {
            GameManager.instance.characterManager.PlayerDied(characterIndex);
        }
    }
}

//used for storing the data of attacks in 
//the player's attack data dictionary.
[Serializable]
public struct AttackInfo
{
    public AttackInfo(float launchAngle, float attackDamage, float baseKnockback, float knockbackScale, int hitLag)
    {
        this.launchAngle = launchAngle;
        this.attackDamage = attackDamage;
        this.baseKnockback = baseKnockback;
        this.knockbackScale = knockbackScale;
        this.hitLag = hitLag;
    }

    /// <summary>
    /// The percentage of damage added to the player's damage meter upon a successful hit.
    /// </summary>
    public float attackDamage;

    /// <summary>
    /// The amount of additional hitlag 
    /// Applied by this attack when 
    /// launching.
    /// </summary>
    public int hitLag;

    /// <summary>
    /// The direction the enemy is sent in if this attack lands. In Degrees.
    /// </summary>
    public float launchAngle;

    /// <summary>
    /// The base knockback of this attack, regardless of the player's percentage.
    /// </summary>
    public float baseKnockback;

    /// <summary>
    /// Describes how much knockback and percent scale.
    /// </summary>
    public float knockbackScale;


}
