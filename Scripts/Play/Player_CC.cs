using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;



//using System.Numerics;
using UnityEngine;

public class Player_CC : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public float doubleJumpForce;
    // private Rigidbody rb;

    private Animator animator;

    //public CapsuleCollider Capsule { get; private set; }
    private CharacterController cc;

    private static Player_CC _instance;
    public static Player_CC instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Player_CC>();
            }

            return _instance;
        }
    }

    public bool bFlatlined { get; private set; }
    public float GroundedOffset ;

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
    private float lowCheckOffset = 4;
    private float highCheckOffset = 1;
    Vector3 endPosition;
    private float globalGravity = -9.81f;
    private float horizontalInput = 1f;
    private Vector2 velocity;
    float velocityXSmoothing;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    private bool bApplyGravity = true;
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

        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen

        ropeRenderer = GetComponent<LineRenderer>();
        originalCharCenterY = cc.center.y;
        originalCharHeight = cc.height;

    }

    // Update is called once per frame
    void Update()
    {
        if (bFlatlined) return;
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
            HandleMovement();

            if (climbingState == ClimbingState.None && bIsGrounded) CheckLowerLedge();
            if (climbingState == ClimbingState.None && cc.velocity.y < 0)
                CheckLedge();

        }

        if (bIsGrounded)
        {
            bCanDoubleJump = true;
            gravityModifier = 1;
        }
        else CheckRopeInZone(); //Only check for rope point when in air

        UpdateAnimatorValues();
        CheckForSlideCancel();
        ropeRenderer.enabled = bIsGrabingRope;

        PollInput();
    }
    void FixedUpdate()
    {
      
        var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = finalPos;    //keep the player stay 0 on Z axis
    }
    void UpdateAnimatorValues()
    {
        animator.SetFloat("xVel", cc.velocity.x);
        animator.SetFloat("yVel", cc.velocity.y);
        animator.SetBool("bIsGrounded", bIsGrounded);
        animator.SetBool("bCanDoubleJump", bCanDoubleJump);
        animator.SetBool("bIsSliding", bIsSliding);
        animator.SetBool("bIsGrabingRope", bIsGrabingRope);

        animator.SetBool("bWallHit", bWallDetected);
        if (cc.velocity.y < -20)
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
  
            cc.Move(new Vector2(0,0));
            HandleDeath();
            return;
        }
        if (climbingState == ClimbingState.ClimbingLedge) return;
        transform.forward = new Vector3(horizontalInput, 0, 0);
        float targetVelocityX = bIsSliding ? slideSpeed : moveSpeed * horizontalInput;
        //  if (bLedgeClimb) targetVelocityX = 0;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, bIsGrounded ? (bIsSliding == false ? accelerationTimeGroundedRun : accelerationTimeGroundedSliding) : accelerationTimeAirborne);

        if (bIsGrounded && velocity.y < 0)
        {
            velocity.y = 0;
            lastRopePointObj = null;
        }
        else
            velocity.y += globalGravity*gravityScale * Time.deltaTime;     //add gravity

             Vector2 finalVelocity = velocity;
              cc.Move(finalVelocity * Time.deltaTime);
    }
    void Flip()
    {
        horizontalInput *= -1;
    }
    void HandleDeath()
    {
        //Time.timeScale = 0.1f;
     //   ragdoll.RagdollStart();
        PlayManager.instance.GameOver();
        bFlatlined = true;
    }
    RaycastHit groundHit;

    void CheckCollision()
    {
          //if (velocity.y > 0.1f)
            //return;
        RaycastHit hit;
       // if(Physics.SphereCast(transform.position + Vector3.up * 1, cc.radius *0.9f, Vector3.down, out groundHit, 1f, whatIsGround))
       // {
          //  float distance = transform.position.y - groundHit.point.y;
             //bIsGrounded = distance <= (cc.skinWidth + 0.01f);
              // Debug.Log(distance);
        //}
         Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            bIsGrounded = Physics.CheckSphere(spherePosition,cc.radius *0.9f, whatIsGround,
                QueryTriggerInteraction.Ignore);

      //  bCeillingDetected = Physics.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround);
       // bWallDetected = Physics.BoxCast(wallCheck.position, wallCheckSize, Vector3.forward, Quaternion.identity, 10, whatIsGround);
        /*
                bWallDetected = Physics.SphereCast(transform.position + Vector3.up * (Capsule.height - Capsule.radius),
                   Capsule.radius,
                   horizontalInput > 0 ? Vector3.right : Vector3.left,
                   out hit, 0.1f, whatIsGround);*/

        if (Physics.SphereCast(transform.position + Vector3.up * (cc.height / lowCheckOffset), 0.25f, Vector3.right, out hit, 0.3f, whatIsGround))


            bWallDetected = Physics.SphereCast(transform.position + Vector3.up * (cc.height * highCheckOffset), 0.25f, Vector3.right, out hit, 0.3f, whatIsGround);


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

        if (currentAvailableRope != null)
        {
            if (Input.GetMouseButtonDown(0))
                GrabRope();

            if (Input.GetMouseButtonUp(0))
                GrabRelease();
        }

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
            if (bIsGrounded)
                OnSlideStart();
            else
            if (cc.velocity.y < 0)
                CancelJump();
        }
    }
    void Jump(float force = -1)
    {
        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;
        if (bIsGrounded)
        {
             var _height = force != -1 ? force : jumpForce;
            velocity.y += Mathf.Sqrt(_height * -2 * globalGravity * gravityScale);
            velocity.x = cc.velocity.x;
            cc.Move(velocity*Time.deltaTime);
        }


    }
    void OnSlideStart()
    {
        if (cc.velocity.x == 0 || slideCooldownCounter > 0) return;
        Invoke("OnSlideEnd", slideTime);
        //  dustFx.Play();
        bIsSliding = true;
        slideTimeCounter = slideTime;
        slideCooldownCounter = slideCooldown;


        cc.height = slidingCapsultHeight;

        var _center = cc.center;
        _center.y = slidingCapsultHeight * 0.5f;

        cc.center = _center;
    }

    void OnSlideEnd()
    {
        if (!bIsSliding)
            return;

        cc.height = originalCharHeight;
        var _center = cc.center;
        _center.y = originalCharCenterY;
        cc.center = _center;
        bIsSliding = false;
    }
    void RollEnd()
    {
        animator.SetBool("bCanRoll", false);
    }
    void CancelJump()
    {
        gravityModifier = 8;
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
                            cc.Move(velocity);      
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
                velocity = Vector2.zero;
                cc.Move(velocity);
                transform.position = CalculatePositionOnLedge(climbOffsetPos);
                //reset other value
                //   isWallSliding = false;
                if (bIsSliding)
                    OnSlideEnd();

                StartCoroutine(ClimbingLedgeCo(false));
                return true;
            }
        }
        return false;
    }
    private Vector3 CalculatePositionOnLedge(Vector3 offset)
    {
        Vector3 newPos = new Vector3(ledgePoint.x - (cc.radius * (horizontalInput > 0 ? 1 : -1)) - offset.x, ledgePoint.y - offset.y, transform.position.z);

        return newPos;
    }

    IEnumerator ClimbingLedgeCo(bool lowClimb)
    {
        cc.enabled = false;
    
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
        cc.enabled = true;
        animator.applyRootMotion = false;
        climbingState = ClimbingState.None;
        animator.SetBool("bLedgeClimb", false);
        animator.SetBool("bLowLedgeClimb", false);
        ledgeTarget = null;
    }
    void CheckRopeInZone()
    {
        if (bIsGrabingRope)
            return;

        var hits = Physics.OverlapSphere(transform.position + Vector3.up * cc.height * 0.5f, ropeCheckRadius, WhatIsRope);

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
                        }
                        else
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
       
      
            bApplyGravity = false;

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
        bCanDoubleJump = false;
        
            bApplyGravity = true;
         

        velocity = releaseForce * transform.forward;
        cc.Move(velocity * Time.deltaTime);
        Time.timeScale = 1;
        //  SoundManager.PlaySfx(soundRopeJump);
        bIsGrabingRope = false;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        // verticalChecker.position, Vector3.down * verticalCheckDistance, Color.red
        Gizmos.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance);
        Gizmos.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), Vector3.right * 2);
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        // Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        //    Gizmos.DrawSphere(transform.position + Vector3.up * (Capsule.height / 4) + Vector3.right * 0.3f, 0.25f);
        // Gizmos.DrawSphere(transform.position + Vector3.up * (Capsule.height) + Vector3.right * 0.3f, 0.25f);
          //Gizmos.DrawWireSphere(groundHit.point, cc.radius * 0.9f);
           Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                 cc.radius * 0.9f);

    }

}
