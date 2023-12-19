using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;

public class Player : MonoBehaviour
{

    public float groundCheckDist = 0.1f;

    [Header("Movement Parameters")] //Explain that this will show a header in the inspector to categorize variables
    [Range(1, 20)] public float moveSpeed = 12f; //Say that this variable is for modifying our movement vector to actually create speed.
    [Range(1, 20)] public float maxSpeed = 14f;
    [Range(1, 10)] public float maxDecelSpeed = 5f; //When you let go of the controls clamp speed to this value. Probs step 2

    public float xAxis;
    private Vector2 moveDirection;

    private Transform camTransform;

    private Rigidbody2D rb;

    private PlayerInput playerInput;

    //Input actions
    private InputAction moveAction;
    private InputAction jumpAction;


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
    [SerializeField] private float buttonTime;
    [SerializeField] private float jumpTime;
    public float jumpDist; //This is not in the actual tutorial because I only used it for testing the distance of the actual jump.
    public Vector2 ogJump; //Not included just like what I said above.
    public float fallMultiplier = 2.5f; //When you reach the peak of the expected arc this is the force applied to make falling more fluid.
    public float lowJumpMultiplier = 2f; //When you stop holding jump to do a low jump this is the force applied to make the jump stop short.
    public float Multiplier = 100f; //This is so we can scale with deltatime properly.

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
    }

    // Update is called once per frame
    void Update()
    {

        #region bool control
        doJump |= (jumpAction.GetButtonDown() && jumpCount > 0 && !jumping); //We use |= because we want doJump to be set from true to false
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

            if (jumpAction.GetButtonUp()) //If we stop giving input for jump cancel jump so we can have a variable jump.
            {
                jumpCanceled = true;
            }

            if (jumpTime >= buttonTime) //When we reach our projected time stop jumping and begin falling.
            {
                jumpCanceled = true;
                jumpDist = Vector2.Distance(transform.position, ogJump); //Not needed, just calculates distance from where we started jumping to our highest point in the jump.
            }
        }

        if (jumpCanceled) //Cancel the jump.
        {
            jumping = false;
        }

        #endregion

        Vector2 currentInput = moveAction.ReadValue<Vector2>();
        moveDirection = ((currentInput.normalized.x * camTransform.right.normalized) + (currentInput.normalized.y * camTransform.up.normalized)) * moveSpeed;

        //TODO: check for how far stick is tilted and run instead if tilted farther than 0.5 in either direction.
        //A or D is pressed return -1 or 1, relative to which
        //otherwise return zero.
        xAxis = currentInput.x != 0 ? currentInput.x > 0 ? 1 : -1 : 0;
    }

    private void FixedUpdate()
    {
        HandleJump();

        ApplyFinalMovements();
    }

    private void HandleJump() //Step 2
    {


        if (doJump)
        {
            doJump = false;
            jumpCount--;
            ogJump = transform.position;
            float jumpForce;

            jumpForce = Mathf.Sqrt(2f * Physics2D.gravity.magnitude * jumpHeight) * rb.mass;
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
            Vector2 jumpVec = Multiplier * -transform.up * (fallMultiplier - 1) * Time.deltaTime;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
        else if (localVel.y > 0 && !jumpAction.GetButtonDown() && inAir) //If we stop before reaching the top of our arc then apply enough downward velocity to stop moving, then proceed falling down to give us a variable jump.
        {
            Vector2 jumpVec = Multiplier * -transform.up * (lowJumpMultiplier - 1) * Time.deltaTime;
            rb.AddForce(jumpVec, ForceMode2D.Force);
        }
    }

    private void ApplyFinalMovements() //Step 1
    {
        rb.velocity = new Vector2(xAxis * moveSpeed, rb.velocity.y);
        //rb.AddForce(moveDirection, ForceMode2D.Force);
/*        if (currentInput.x != 0) //What this does is when we stop giving input (when the player tries to stop moving) we Decel in a specific 
        {                        //range by setting velocity to our max Decel speed so it slows down to 0 from that value.
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxDecelSpeed, maxDecelSpeed), rb.velocity.y);
        }*/
    }
}
