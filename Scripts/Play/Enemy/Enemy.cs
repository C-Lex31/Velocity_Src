using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;



//using System.Numerics;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;
    private Rigidbody rb;

    private Animator animator;

    public CapsuleCollider Capsule { get; private set; }

    private static Enemy _instance;
    public static Enemy instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Enemy>();
            }

            return _instance;
        }
    }

    public bool bFlatlined { get; private set; }

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
    [SerializeField] private RagdollController ragdoll;
    [SerializeField] private float lowCheckOffset = 4;
    [SerializeField] private float highCheckOffset = 1;
    Vector3 endPosition;
    private float globalGravity = -9.81f;
    private float horizontalInput = 1f;
    private Vector2 velocity;
    float velocityXSmoothing;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    [SerializeField] private bool bApplyGravity = true;
    private bool bLedgeClimb;

    private bool bCanDoubleJump;

    [Header("SLIDING")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    [SerializeField] private float slidingCapsultHeight = 0.8f;
    private bool bIsSliding;
    private float slideTimeCounter;
    private float slideCooldownCounter;

    [Header("ROPE")]
    public LayerMask WhatIsRope;
    RopePoint lastRopePointObj;
    [SerializeField] private float ropeCheckRadius = 6;
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

    private float gravityModifier = 1;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        Capsule = GetComponent<CapsuleCollider>();
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
        rb.useGravity = false;
        ropeRenderer = GetComponent<LineRenderer>();
        originalCharCenterY = Capsule.center.y;
        originalCharHeight = Capsule.height;

    }

    // Update is called once per frame
    void Update()
    {
        if (bFlatlined) return;
        
            // slideTimeCounter -= Time.deltaTime;
            slideCooldownCounter -= Time.deltaTime;
            CheckCollision();
            HandleMovement();

            if (climbingState == ClimbingState.None && bIsGrounded) CheckLowerLedge();
            if (climbingState == ClimbingState.None && !bIsGrounded)
                CheckLedge();

        

        if (bIsGrounded)
        {
            bCanDoubleJump = true;
            gravityModifier = 1;
        }
      

        UpdateAnimatorValues();
        CheckForSlideCancel();
        ropeRenderer.enabled = bIsGrabingRope;

        PollInput();
    }
    void FixedUpdate()
    {
        if (rb && bApplyGravity)
        {
            Vector3 gravity = globalGravity * gravityScale * gravityModifier * Vector3.up;
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
        var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = finalPos;    //keep the player stay 0 on Z axis
    }
    void UpdateAnimatorValues()
    {
        animator.SetFloat("xVel", rb.velocity.x);
        animator.SetFloat("yVel", rb.velocity.y);
        animator.SetBool("bIsGrounded", bIsGrounded);
        animator.SetBool("bCanDoubleJump", bCanDoubleJump);
        animator.SetBool("bIsSliding", bIsSliding);
        animator.SetBool("bIsGrabingRope", bIsGrabingRope);

        animator.SetBool("bWallHit", bWallDetected);
        if (rb.velocity.y < -20)
            animator.SetBool("bCanRoll", true);
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

        if (bWallDetected && climbingState == ClimbingState.None && !bFlatlined)
        {
            if (rb) rb.velocity = new Vector3(0, 0);
            HandleDeath();
            return;
        }
        if (climbingState == ClimbingState.ClimbingLedge) return;
        transform.forward = new Vector3(horizontalInput, 0, 0);
        float targetVelocityX = bIsSliding ? slideSpeed : moveSpeed * horizontalInput;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, bIsGrounded ? (bIsSliding == false ? accelerationTimeGroundedRun : accelerationTimeGroundedSliding) : accelerationTimeAirborne);

        velocity.y = rb.velocity.y;
        rb.velocity = velocity;

    }
    void Flip()
    {
        horizontalInput *= -1;
    }
    void HandleDeath(float force = 0)
    {
        //Time.timeScale = 0.1f;
        StartCoroutine(DeathCo());
    }
    IEnumerator DeathCo()
    {
        ragdoll.RagdollStart();
        bFlatlined = true;
        yield return new WaitForSeconds(2.5f);
       // PlayManager.instance.GameOver();
    }
    RaycastHit groundHit;

    void CheckCollision()
    {
        if (climbingState == ClimbingState.ClimbingLedge) return;
        RaycastHit hit;
        bIsGrounded = Physics.SphereCast(transform.position + Vector3.up * 1, Capsule.radius * 0.9f, Vector3.down, out groundHit, 1f, whatIsGround);

        // bCeillingDetected = Physics.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround);
        // bWallDetected = Physics.BoxCast(wallCheck.position, wallCheckSize, Vector3.forward, Quaternion.identity, 10, whatIsGround);

        if (Physics.SphereCast(transform.position + Vector3.up * (Capsule.height / lowCheckOffset), 0.12f, Vector3.right, out hit, 0.45f, whatIsGround))

            bWallDetected = Physics.SphereCast(transform.position + Vector3.up * (Capsule.height * highCheckOffset), 0.12f, Vector3.right, out hit, 0.45f, whatIsGround);


    }
    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 || !bIsGrounded)
            OnSlideEnd();
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
                            if (bIsGrounded)
                                Jump(jumpForce);
                            else if (bCanDoubleJump)
                            {
                                bCanDoubleJump = false;
                                Jump(doubleJumpForce);
                            }
                        }
                        else
                        {   //Down swipe
                            Debug.Log("Down Swipe");
                            OnSlideStart();
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

      

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RollEnd();
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
            // if (bIsGrounded)
            OnSlideStart();
            CancelJump();
        }
    }
    void Jump(float force = -1)
    {
        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        if (bIsSliding) OnSlideEnd();
        //if (rb)
        rb.velocity = new Vector2(rb.velocity.x, force);

    }
    void OnSlideStart()
    {
        // if (slideCooldownCounter > 0) return;
        Invoke("OnSlideEnd", slideTime);
        //  dustFx.Play();
        bIsSliding = true;
        slideTimeCounter = slideTime;
        slideCooldownCounter = slideCooldown;


        Capsule.height = slidingCapsultHeight;

        var _center = Capsule.center;
        _center.y = slidingCapsultHeight * 0.5f;

        Capsule.center = _center;
    }

    void OnSlideEnd()
    {
        if (!bIsSliding)
            return;

        Capsule.height = originalCharHeight;
        var _center = Capsule.center;
        _center.y = originalCharCenterY;
        Capsule.center = _center;
        bIsSliding = false;
    }
    public void RollEnd()
    {
           Capsule.height = originalCharHeight;
        var _center = Capsule.center;
        _center.y = originalCharCenterY;
        Capsule.center = _center;
        animator.SetBool("bCanRoll", false);
    }
    public void RollStarted()
    {
        Capsule.height = slidingCapsultHeight;

        var _center = Capsule.center;
        _center.y = slidingCapsultHeight * 0.5f;

        Capsule.center = _center;
    }
   

    void DoubleJumpStart()
    {
        animator.SetBool("bLockDoubleJump", true);
    }
    void DoubleJumpEnd()
    {
        animator.SetBool("bLockDoubleJump", false);
    }
    void CancelJump()
    {
        gravityModifier = 6;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Deadzone")
            HandleDeath();
    }



    public enum ClimbingState { None, ClimbingLedge }
    [Header("LEDGE CLIMB")]
    public ClimbingState climbingState;
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbOffsetPos = new Vector3(0, 1.3f, 0);
    [Tooltip("Adjust to fit with the climbing animation length")]
    public float climbingLedgeTime = 1;
    public Transform verticalChecker;
    public float verticalCheckDistance = 0.5f;

    [Header("---CHECK LOW CLIMB 1m---")]
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbLCOffsetPos = new Vector3(0, 1f, 0);
    public float climbingLBObjTime = 1;
    Transform ledgeTarget;      //use to update ledge moving/rotating
    Vector3 ledgePoint;
    bool CheckLowerLedge()      //check lower ledge
    {
        RaycastHit hitVertical;
        RaycastHit hitGround;
        RaycastHit hitHorizontal;

        //Debug.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance);
        if (Physics.Linecast(verticalChecker.position, new Vector3(verticalChecker.position.x, transform.position.y + 0.3f, transform.position.z), out hitVertical, whatIsGround, QueryTriggerInteraction.Ignore))
        {
            if (hitVertical.normal == Vector3.up)
            {
                //Debug.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), (horizontalInput > 0 ? Vector3.right : Vector3.left) * 2);
                if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y, verticalChecker.position.z), Vector3.down, out hitGround, 3, whatIsGround, QueryTriggerInteraction.Ignore))
                {
                    if ((int)hitGround.distance <= 1)
                    {
                        if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), horizontalInput > 0 ? Vector3.right : Vector3.left, out hitHorizontal, 2, whatIsGround, QueryTriggerInteraction.Ignore))
                        {

                            ledgeTarget = hitVertical.transform;
                            ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                            velocity = Vector2.zero;

                            rb.velocity = velocity;
                            Capsule.enabled = false;
                            rb.isKinematic = true;
                            bApplyGravity = false;
                            transform.position = CalculatePositionOnLedge(climbOffsetPos);
                            //reset other value
                            //isWallSliding = false;
                            if (bIsSliding)
                                OnSlideEnd();

                            StartCoroutine(ClimbingLedgeCo(true));
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    RaycastHit hitVertical;
    RaycastHit hitHorizontal;
    bool CheckLedge()       //check higher ledge
    {

        Debug.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance, Color.red);
        if (Physics.Raycast(verticalChecker.position, Vector2.down, out hitVertical, verticalCheckDistance, whatIsGround, QueryTriggerInteraction.Ignore))
        {

            Debug.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), (horizontalInput > 0 ? Vector3.right : Vector3.left) * 2);
            if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), horizontalInput > 0 ? Vector3.right : Vector3.left, out hitHorizontal, 2, whatIsGround, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Ledge");
                ledgeTarget = hitVertical.transform;
                ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                //   velocity = Vector2.zero;
                rb.velocity = Vector3.zero;
                Capsule.enabled = false;
                rb.isKinematic = true;
                bApplyGravity = false;
                transform.position = CalculatePositionOnLedge(climbOffsetPos);
                //reset other value
                //   isWallSliding = false;
                if (bIsSliding)
                    OnSlideEnd();
                RollEnd();

                StartCoroutine(ClimbingLedgeCo(false));
                return true;
            }
        }
        return false;
    }
    private Vector3 CalculatePositionOnLedge(Vector3 offset)
    {
        Vector3 newPos = new Vector3(ledgePoint.x - (Capsule.radius * (horizontalInput > 0 ? 1 : -1)) - offset.x, ledgePoint.y - offset.y, transform.position.z);

        return newPos;
    }

    IEnumerator ClimbingLedgeCo(bool lowClimb)
    {
        Capsule.enabled = false;
        rb.isKinematic = true;
        bApplyGravity = false;
        animator.applyRootMotion = true;
        //Debug.Log("LedgeCimb");
        climbingState = ClimbingState.ClimbingLedge;
        if (lowClimb)
            animator.SetBool("bLowLedgeClimb", true);
        else
            animator.SetBool("bLedgeClimb", true);

        UpdateAnimatorValues();

        yield return new WaitForSeconds(Time.deltaTime);

        transform.position = CalculatePositionOnLedge(lowClimb ? climbLCOffsetPos : climbOffsetPos);
        yield return new WaitForSeconds(Time.deltaTime);
        transform.position = CalculatePositionOnLedge(lowClimb ? climbLCOffsetPos : climbOffsetPos);

        yield return new WaitForSeconds(lowClimb ? climbingLBObjTime : climbingLedgeTime);
        LedgeReset();
    }

    void LedgeReset()
    {
        Capsule.enabled = true;
        animator.applyRootMotion = false;
        rb.isKinematic = false;
        bApplyGravity = true;
        climbingState = ClimbingState.None;
        animator.SetBool("bLedgeClimb", false);
        animator.SetBool("bLowLedgeClimb", false);
        ledgeTarget = null;
    }
    

    private void OnDrawGizmosSelected()
    {
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        // verticalChecker.position, Vector3.down * verticalCheckDistance, Color.red
        Gizmos.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance);
        Gizmos.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), Vector3.right * 2);
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        // Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        Gizmos.DrawSphere(transform.position + Vector3.up * (Capsule.height / lowCheckOffset) + Vector3.right * 0.45f, 0.18f);
        Gizmos.DrawSphere(transform.position + Vector3.up * (Capsule.height * highCheckOffset) + Vector3.right * 0.45f, 0.18f);






    }

}
