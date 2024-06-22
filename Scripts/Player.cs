using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;

//using System.Numerics;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;
    private Rigidbody rb;

    private Animator animator;
    private CharacterController characterController;

    public CapsuleCollider Capsule { get; private set; }

    private static Player _instance;
    public static Player instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Player>();
            }

            return _instance;
        }
    }

    private bool bIsGrounded;
    private bool bCeillingDetected;
    private bool bWallDetected;
    private bool bIsVaulting;
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered
 
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceillingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] private float gravityScale;
     Vector3 endPosition;
    private float globalGravity = -9.81f;
    private float horizontalInput=1f;
    private Vector2 velocity;
    float velocityXSmoothing;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    private bool bApplyGravity =true;
    [Header("LEDGE CLIMB")]
    private bool bLedgeClimb;
   [HideInInspector] public bool bLedgeDetected;
    [SerializeField] private Vector2 offset1; // offset for position before climb
    [SerializeField] private Vector2 offset2; // offset for position AFTER climb

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;

    private bool bCanDoubleJump;

    [Header("SLIDING")]
    [SerializeField]private float slideSpeed;
   [SerializeField] private float slideTime;
   [SerializeField]private float slideCooldown;
   [SerializeField]private float slidingCapsultHeight=0.8f;
    private bool bIsSliding;
    private float slideTimeCounter;
    private float slideCooldownCounter;
    
    [Header("ROPE")]
    public LayerMask WhatIsRope;
    RopePoint lastRopePointObj;
    [SerializeField]private float ropeCheckRadius = 6;
    [Tooltip("draw rope offset")]
    [SerializeField] private Vector2 grabOffset = new Vector2(0, 1.6f);
    LineRenderer ropeRenderer;
    [SerializeField] private float releaseForce = 10;
    private float releasePointY;
    [SerializeField] private float ropeSpeed = 100;
    private Vector3 rotateAxis = Vector3.forward;
    [ReadOnly] public bool bIsGrabingRope = false;
    [ReadOnly] public RopePoint currentAvailableRope;
    private float originalCharCenterY, originalCharHeight;




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
      //  characterController = GetComponent<CharacterController>();
        Capsule=GetComponent<CapsuleCollider>();
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
        rb.useGravity = false;
        ropeRenderer = GetComponent<LineRenderer>();
        originalCharCenterY = Capsule.center.y;
        originalCharHeight =Capsule.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (bIsGrabingRope && !bIsGrounded)
        {
            transform.RotateAround(currentAvailableRope.transform.position, rotateAxis, horizontalInput * ropeSpeed * Time.deltaTime);


            transform.up = currentAvailableRope.transform.position - transform.position;
            transform.Rotate(0, horizontalInput > 0 ? 90 : -90, 0);

            ropeRenderer.SetPosition(0, transform.position + transform.forward * grabOffset.x + transform.up * grabOffset.y);
            ropeRenderer.SetPosition(1, currentAvailableRope.transform.position);

            if (transform.position.y >= releasePointY)
            {
                if ((horizontalInput > 0 && transform.position.x > currentAvailableRope.transform.position.x) || (horizontalInput < 0 && transform.position.x < currentAvailableRope.transform.position.x))
                    //GrabRelease();      //disconnect grab if player reach to the limit position
                    Flip();
            }
        }
        else
        {
           // slideTimeCounter -= Time.deltaTime;
            slideCooldownCounter -= Time.deltaTime;
            CheckCollision();
            //rb.velocity = new Vector2(bIsSliding?slideSpeed:moveSpeed ,rb.velocity.y);
            HandleMovement();
            CheckForLedge();
        
        }

        if (bIsGrounded)
        {
           // Time.timeScale = 1;
            bCanDoubleJump = true;
        }
        else CheckRopeInZone(); //Only check for rope point when in air
        
        UpdateAnimatorValues();
        CheckForSlideCancel();
        ropeRenderer.enabled = bIsGrabingRope;
        
        PollInput();
    }
    void FixedUpdate()
    {
        if(bApplyGravity)
        {
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
        }
      
    }
    void UpdateAnimatorValues()
    {
        animator.SetFloat("xVel" ,rb.velocity.x);
        animator.SetFloat("yVel" ,rb.velocity.y);
        animator.SetBool("bIsGrounded" ,bIsGrounded);
        animator.SetBool("bLedgeClimb" ,bLedgeClimb);
        animator.SetBool("bCanDoubleJump", bCanDoubleJump);
        animator.SetBool("bIsSliding", bIsSliding);
        animator.SetBool("bIsGrabingRope", bIsGrabingRope);
        animator.SetBool("bCanRoll", velocity.y < -20);
        animator.SetBool("bWallHit", bWallDetected);
    }

    void PollInput()
    {
        #if UNITY_EDITOR
            KeyboardControls();
        #endif
        SwipeControls();
    }
    void HandleMovement()
    {
        if(bWallDetected)
        {
            rb.velocity=new Vector3(0,0);
            animator.applyRootMotion =true;
            HandleDeath();
            return;
        }

        transform.forward = new Vector3(horizontalInput, 0, 0);
         float targetVelocityX = bIsSliding?slideSpeed:moveSpeed * horizontalInput;
          velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, bIsGrounded ? (bIsSliding == false ? accelerationTimeGroundedRun : accelerationTimeGroundedSliding) : accelerationTimeAirborne);
           velocity.y = rb.velocity.y;
           // if (bIsGrounded && velocity.y < 0)
             //{
               // velocity.y = 0f;
           //  }
            // else
           //  velocity.y += -9.81f*gravityScale * Time.deltaTime;     //add gravity
            
        Vector2 finalVelocity = velocity;
       // characterController.Move(finalVelocity * Time.deltaTime);
        rb.velocity = finalVelocity;

    }
    void Flip()
    {
        horizontalInput *= -1;
    }
   void HandleDeath()
   {
     Time.timeScale = 0.1f;
     
   }

    void CheckCollision()
    {
         RaycastHit hit;
        bIsGrounded = Physics.Raycast(transform.position+ Vector3.up * 1, Vector3.down, groundCheckDistance, whatIsGround);
        Debug.DrawLine(transform.position+ Vector3.up * 1,transform.position+ Vector3.up * 1+Vector3.down*groundCheckDistance ,Color.red ,0.25f);
        bCeillingDetected = Physics.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround);
       // bWallDetected = Physics.BoxCast(wallCheck.position, wallCheckSize,  Vector3.forward,Quaternion.identity, 10, whatIsGround);
        bWallDetected = Physics.SphereCast(transform.position + Vector3.up * (Capsule.height - Capsule.radius),
           Capsule.radius,
           horizontalInput > 0 ? Vector3.right : Vector3.left,
           out hit, 0.1f,whatIsGround);
        Debug.Log(bWallDetected);
    }
     private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 && !bCeillingDetected)
            bIsSliding = false;
    }
    void SwipeControls()
    {
        if (Input.touchCount > 0) // user is touching the screen with a single touch
        {
            Touch touch = Input.GetTouch(0); // get the touch
            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list
 
                //Check if drag distance is greater than 20% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe
                            Debug.Log("Right Swipe");
                        }
                        else
                        {   //Left swipe
                            Debug.Log("Left Swipe");
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            Debug.Log("Up Swipe");
                            if(bIsGrounded)
                                Jump(jumpForce);
                        }
                        else
                        {   //Down swipe
                            Debug.Log("Down Swipe");
                        }
                    }
                }
                else
                {   //It's a tap as the drag distance is less than 20% of the screen height
                    Debug.Log("Tap");
                }
            }
        }
    }
    void KeyboardControls()
    {

         if (currentAvailableRope != null)
        {
            if (Input.GetMouseButtonDown(0))
                GrabRope();

            if (Input.GetMouseButtonUp(0))
                GrabRelease();
        }
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (bIsGrounded)
                Jump(jumpForce);
            else if (bCanDoubleJump)
            {
                bCanDoubleJump = false;
                Jump(doubleJumpForce);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (rb.velocity.x != 0 && slideCooldownCounter < 0)
            {
              OnSlideStart();
              Invoke("OnSlideEnd", slideTime);
            }
        }
    }
    void Jump(float force)
    {
    rb.velocity = new Vector2(rb.velocity.x, force);
    }
    void OnSlideStart()
    {
        //  dustFx.Play();
        bIsSliding = true;
        slideTimeCounter = slideTime;
        slideCooldownCounter = slideCooldown;
        
        // characterController.height = slidingCapsultHeight;
        Capsule.height = slidingCapsultHeight;
        //var _center = characterController.center;
        var _center = Capsule.center;
        _center.y = slidingCapsultHeight * 0.5f;
        // characterController.center = _center;
        Capsule.center = _center;
    }

    void OnSlideEnd()
    {
        if (!bIsSliding)
            return;

        //characterController.height = originalCharHeight;
        Capsule.height = originalCharHeight;
        // var _center = characterController.center;
        var _center = Capsule.center;
        _center.y = originalCharCenterY;
        // characterController.center = _center;
        Capsule.center = _center;
        bIsSliding = false;
    }
    public void StartVault(Vector3 vaultPosition)
    {
        if (bIsVaulting) return;
            bIsVaulting = true;
            bApplyGravity=false;
         // Trigger the vault animation
        animator.SetTrigger("Vault");
        // Calculate the end position based on the vault distance
        endPosition = vaultPosition + Vector3.right *2f;
       //characterController.enabled = false;
       animator.applyRootMotion = true;
       Capsule.isTrigger=true;

    
    }


    private void VaultOver()
    {
        Debug.Log("VaultOver");
        transform.position = endPosition;
        //  characterController.enabled = true;
        animator.applyRootMotion = false;
        bIsVaulting = false;
        bApplyGravity = true;
        Capsule.isTrigger = false;
    }
    private void CheckForLedge()
    {
        if (bLedgeDetected && canGrabLedge)
        {
            canGrabLedge = false;
            bApplyGravity = false;

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetector>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            bLedgeClimb = true;
        }

        if (bLedgeClimb)
            transform.position = climbBegunPosition;
    }
    private void LedgeClimbOver()
    {

        bLedgeClimb=false;
        bApplyGravity = true;
        transform.position = climbOverPosition;
        Invoke("AllowLedgeGrab", .1f);
    }
    private void AllowLedgeGrab() => canGrabLedge = true;


    void CheckRopeInZone()
    {
        if (bIsGrabingRope)
            return;

        var hits = Physics.OverlapSphere(transform.position + Vector3.up * Capsule.height * 0.5f, ropeCheckRadius, WhatIsRope);
        
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (horizontalInput > 0)
                {
                    if (hits[i].transform.position.x > transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.slowMotion)
                                Time.timeScale = 0.1f;
                        }else
                            currentAvailableRope = null;
                    }
                    else
                        currentAvailableRope = null;
                }
                else
                {
                    if (hits[i].transform.position.x < transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.slowMotion)
                            Time.timeScale = 0.1f;
                        }
                        else
                            currentAvailableRope = null;
                    }
                    else
                        currentAvailableRope = null;
                }
            }
        }
        else
        {
            if (currentAvailableRope != null)       //set time scale back to normal if it active the slow motion before but player don't grab
            {
                if (currentAvailableRope.slowMotion)
                    Time.timeScale = 1;
            }

            currentAvailableRope = null;
        }
    }

     public void GrabRope()
    {
        if (bIsGrabingRope)
            return;

        if (bIsGrounded)
            return;     //don't allow grab rope when standing on ground
        bApplyGravity =false;
        rb.isKinematic =true;
        if (lastRopePointObj != currentAvailableRope)
        {
            
            if (currentAvailableRope.slowMotion)
                Time.timeScale = 1;

            lastRopePointObj = currentAvailableRope;
            bIsGrabingRope = true;
           // SoundManager.PlaySfx(soundGrap);
           float distance = Vector2.Distance(transform.position, currentAvailableRope.transform.position);
            releasePointY = currentAvailableRope.transform.position.y - distance / 10f;
        }
    }

    public void GrabRelease()
    {
        if (!bIsGrabingRope)
            return;
        bApplyGravity =true;
        rb.isKinematic =false;
        bCanDoubleJump =false;
       // currentAvailableRope =null;
      //  velocity = releaseForce * transform.forward;
        rb.velocity =releaseForce * transform.forward;
       // characterController.Move(velocity * Time.deltaTime);
        rb.AddForce(rb.velocity, ForceMode.Acceleration);
        Time.timeScale = 1;
      //  SoundManager.PlaySfx(soundRopeJump);
        bIsGrabingRope = false;
    }

 private void OnDrawGizmos()
    {
       // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
       // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    
    }

}
