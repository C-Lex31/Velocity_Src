using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;



//using System.Numerics;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    #region Variables

    // General
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
    public bool bFlatlined { get; private set; }
    public float moveSpeed;

    private static Player _instance;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider capsule;
    private RagdollController ragdoll;
    private bool bIsGrounded;
    private bool bCeillingDetected;
    private bool bWallDetected = false;
    private Vector3 fp;   // First touch position
    private Vector3 lp;   // Last touch position
    private float dragDistance;  // Minimum distance for a swipe to be registered
    private Vector3 endPosition;

    // Movement
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    private float globalGravity = -9.81f;
    private float horizontalInput = 1f;
    private Vector2 velocity;
    private float velocityXSmoothing;
    private bool bApplyGravity = true;
    private bool bLedgeClimb;
    private bool bCanDoubleJump;
    private bool bCancelJumpRequest;
    private float gravityModifier = 1;
    [HideInInspector] public int jumpCount = 0;

    // Sliding
    [Header("Sliding")]
    public LayerMask WhatIsRope;

    private RopePoint lastRopePointObj;
    private LineRenderer ropeRenderer;
    private Vector3 rotateAxis = Vector3.forward;
    [HideInInspector] public bool bIsSliding;

    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    [SerializeField] private float slidingCapsultHeight = 0.8f;
    private float slideTimeCounter;
    private float slideCooldownCounter;

    // Rope
    [Header("Rope")]
    [ReadOnly] public bool bIsGrabingRope = false;
    [ReadOnly] public RopePoint currentAvailableRope;

    [Tooltip("Draw rope offset")]
    [SerializeField] private float ropeCheckRadius = 6;
    [SerializeField] private Vector2 grabOffset = new Vector2(0, 1.6f);
    [SerializeField] private float releaseForce = 10;
    [SerializeField] private float ropeSpeed = 100;
    private float releasePointY;
    private float originalCharCenterY, originalCharHeight;

    // Ground and Wall Checks
    [Header("Ground and Wall Checks")]
    private bool bWasInAir;
    private bool bCanRoll;
    
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceillingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    [SerializeField] private float gravityScale;

    [SerializeField] private float lowCheckOffset = 4;
    [SerializeField] private float highCheckOffset = 1;

    // Parkour
    [HideInInspector] public ParkourPickup currentParkourPickup;

    #endregion

    #region Engine Methods
    void Start()
    {
        InitializeComponents();
        InitializeSettings();
    }
    void Update()
    {

        if (bFlatlined) return;
        if (bIsGrabingRope && !bIsGrounded)
        {
            HandleRopeSwing();
        }
        else
        {
            slideCooldownCounter -= Time.deltaTime;
            CheckCollision();
            HandleMovement();

            if (climbingState == ClimbingState.None && bIsGrounded) CheckLowerLedge();
            if (climbingState == ClimbingState.None && !bIsGrounded)
                CheckLedge();

        }

        if (bIsGrounded)
        {
            ResetJumpState();
        }
        else
        {
            CheckBounds();
        }
        UpdateAnimatorValues();
        CheckForSlideCancel();
        ropeRenderer.enabled = bIsGrabingRope;

        PollInput();
    }
    void FixedUpdate()
    {
        if (bFlatlined) return;
        if (rb && bApplyGravity && climbingState == ClimbingState.None)
        {
            ApplyGravity();
        }

        if (!bIsGrounded && bCancelJumpRequest && transform.position.y >= 7f)
        {
            CancelJump();
        }
        NullOutZ();
    }
    #endregion




    #region Initialization Methods
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        ropeRenderer = GetComponent<LineRenderer>();
        ragdoll = GetComponent<RagdollController>();
    }

    private void InitializeSettings()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        dragDistance = Screen.height * 15 / 100;
        rb.useGravity = false;
        originalCharCenterY = capsule.center.y;
        originalCharHeight = capsule.height;
    }

    #endregion



    #region Animation Methods
    void UpdateAnimatorValues()
    {
        animator.SetFloat(AnimatorParams.xVel, rb.velocity.x);
        animator.SetFloat(AnimatorParams.yVel, rb.velocity.y);
        animator.SetBool(AnimatorParams.bIsGrounded, bIsGrounded);
        animator.SetBool(AnimatorParams.bCanJump, jumpCount < 2);
        animator.SetBool(AnimatorParams.bIsSliding, bIsSliding);
        animator.SetBool(AnimatorParams.bIsGrabingRope, bIsGrabingRope);

        if (rb.velocity.y < -20)
        {
            bCanRoll = true;
        }
    }

    #endregion

    void PollInput()
    {
#if UNITY_EDITOR
        KeyboardControls();
#endif
        SwipeControls();
    }

    #region MovementMethods
    void HandleMovement()
    {

        if (bWallDetected && climbingState == ClimbingState.None && !bFlatlined)
        {
            if (rb) rb.velocity = new Vector3(0, 0);
            HandleDeath();
            return;
        }
        if (climbingState != ClimbingState.None) return;
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
    #endregion

    #region Gravity Mathods

    void ApplyGravity()
    {
        Vector3 gravity = globalGravity * gravityScale * gravityModifier * Vector3.up;
        rb.AddForce(gravity, ForceMode.Acceleration);
    }
    #endregion


    #region Death and Revive
    void HandleDeath()
    {
        bWallDetected = false;
        rb.velocity = new Vector2(0, 0);
        ragdoll.RagdollStart();
        bApplyGravity = false;
        bFlatlined = true;
        StartCoroutine(DeathCo());
    }
    IEnumerator DeathCo()
    {
        yield return new WaitForSeconds(2f);
        PlayManager.instance.GameOver(true);
    }
    RaycastHit groundHit;
    public void Revive(Transform RevivePos)
    {

        ragdoll.RagdollReset();
        bApplyGravity = true;
        bFlatlined = false;
    }
    #endregion
    public void Teleport(bool start = true)
    {
        if (start)
        {
            bApplyGravity = false;
            rb.velocity = new Vector2(0, 0);
            rb.isKinematic = true;
            ParticleManager.instance.PlayParticleEffect(ParticleList.teleport, new Vector2(transform.position.x, transform.position.y + 1), Quaternion.identity);
            gameObject.SetActive(false);
        }
        else
        {
            rb.isKinematic = false;
            gameObject.SetActive(true);
            bApplyGravity = true;
        }

    }
    #region Collision Methods
    void CheckCollision()
    {
        if (climbingState != ClimbingState.None || animator.hasRootMotion) return;
        RaycastHit hit;
        bIsGrounded = Physics.SphereCast(transform.position + Vector3.up * 1, capsule.radius * 0.9f, Vector3.down, out groundHit, 1f, whatIsGround);

        // bCeillingDetected = Physics.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround);
        // bWallDetected = Physics.BoxCast(wallCheck.position, wallCheckSize, Vector3.forward, Quaternion.identity, 10, whatIsGround);

        if (Physics.SphereCast(transform.position + Vector3.up * (capsule.height / lowCheckOffset), 0.12f, Vector3.right, out hit, 0.45f, whatIsGround))
            bWallDetected = Physics.SphereCast(transform.position + Vector3.up * (capsule.height * highCheckOffset), 0.12f, Vector3.right, out hit, 0.45f, whatIsGround);

        if (!bIsGrounded)
        {
            bWasInAir = true;
        }
        else if (bWasInAir && bIsGrounded)
        {
            OnLand();
            bWasInAir = false;
        }
    }

    void OnLand()
    {
        jumpCount = 0;
        if (bCanRoll)
        {
            animator.CrossFade(AnimatorParams.Roll, 0f);
            bCanRoll = false;
        }
        else
            animator.CrossFade(AnimatorParams.Land, 0f);
    }
    void CheckBounds()
    {
        //  if (transform.position.y + capsule.height < Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10)).y)
        //   HandleDeath();
    }
    void NullOutZ()
    {
        var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = finalPos;    //keep the player stay 0 on Z axis
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Deadzone" && !bFlatlined)
            HandleDeath();
    }

    #endregion


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
                                DoJump(jumpForce);
                            else if (bCanDoubleJump)
                            {
                                bCanDoubleJump = false;
                                DoJump(doubleJumpForce);
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
            if (currentParkourPickup != null && climbingState == ClimbingState.None && bIsGrounded && !bIsSliding && !bFlatlined)
            {
                currentParkourPickup.ActivateParkour();
                return;
            }
            PerformJump();

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (bIsGrounded)
                OnSlideStart();
            else

            {
                bCancelJumpRequest = true;
                bCanDoubleJump = false;
            }
        }
    }

    #region Jump Methods
    void PerformJump()

    {
        if (jumpCount < 1 && bIsGrounded)
        {
            jumpCount++;
            DoJump(jumpForce);
        }
        else if (jumpCount < 2)
        {
            jumpCount++;
            DoJump(doubleJumpForce);
            animator.CrossFade(AnimatorParams.DoubleJump, 0.2f);
        }

    }
    public void DoJump(float force = -1)
    {
        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        if (bIsSliding) OnSlideEnd();

        float targetVelocityY = Mathf.Sqrt(2 * Mathf.Abs(force) * Mathf.Abs(globalGravity));
        animator.SetTrigger(AnimatorParams.Jump);
        // Ensure the jump direction is correct based on jumpForce
        if (force < 0)
            targetVelocityY = -targetVelocityY;
        rb.velocity = new Vector3(rb.velocity.x, targetVelocityY, 0);
        animator.SetTrigger(AnimatorParams.Jump);

    }
    private void ResetJumpState()
    {
        bCanDoubleJump = false;
        gravityModifier = 1;
    }
    void CancelJump()
    {
        bCancelJumpRequest = false;
        gravityModifier = 6;
    }
    #endregion

    #region Slide Methods
    void OnSlideStart()
    {
        // if (slideCooldownCounter > 0) return;
        Invoke("OnSlideEnd", slideTime);
        //  dustFx.Play();
        bIsSliding = true;
        slideTimeCounter = slideTime;
        slideCooldownCounter = slideCooldown;

        ScaleCapsule();
    }

    void OnSlideEnd()
    {
        if (!bIsSliding)
            return;

        bIsSliding = false;
        ScaleCapsule(true);
    }
    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 || !bIsGrounded)
            OnSlideEnd();
    }
    #endregion
    public void RollEnd()
    {
        ScaleCapsule(true);
        animator.SetBool("bCanRoll", false);
    }
    public void RollStarted()
    {
        ScaleCapsule();
    }

    #region Misc
    void ScaleCapsule(bool bReset = false)
    {
        if (bReset)
        {
            capsule.height = originalCharHeight;
            var _center = capsule.center;
            _center.y = originalCharCenterY;
            capsule.center = _center;
            animator.SetBool("bCanRoll", false);
        }
        else
        {
            capsule.height = slidingCapsultHeight;
            var _center = capsule.center;
            _center.y = slidingCapsultHeight * 0.5f;
            capsule.center = _center;
        }
    }
    #endregion







    public enum ClimbingState { None, ClimbingLedge, Parkour }
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
                            capsule.enabled = false;
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

                ledgeTarget = hitVertical.transform;
                ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                rb.velocity = Vector3.zero;
                capsule.enabled = false;
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
        Vector3 newPos = new Vector3(ledgePoint.x - (capsule.radius * (horizontalInput > 0 ? 1 : -1)) - offset.x, ledgePoint.y - offset.y, transform.position.z);

        return newPos;
    }

    IEnumerator ClimbingLedgeCo(bool lowClimb)
    {
        capsule.enabled = false;
        rb.isKinematic = true;
        bApplyGravity = false;
        animator.applyRootMotion = true;
        climbingState = ClimbingState.ClimbingLedge;
        if (lowClimb)
            animator.CrossFade(AnimatorParams.LowLedgeClimb, 0.1f);
        else
            animator.CrossFade(AnimatorParams.LedgeClimb, 0.1f);

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
        capsule.enabled = true;
        animator.applyRootMotion = false;
        rb.isKinematic = false;
        bApplyGravity = true;
        climbingState = ClimbingState.None;
        animator.SetBool("bLedgeClimb", false);
        animator.SetBool("bLowLedgeClimb", false);
        ledgeTarget = null;
    }
    #region Rope Methods
    void HandleRopeSwing()
    {
        transform.RotateAround(currentAvailableRope.transform.position, rotateAxis, horizontalInput * (ropeSpeed * moveSpeed) * Time.deltaTime);

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
    public void GrabRope()
    {
        if (bIsGrabingRope)
            return;

        if (bIsGrounded)
            return;     //don't allow grab rope when standing on ground
        if (rb)
        {
            bApplyGravity = false;
            rb.isKinematic = true;
        }

        if (lastRopePointObj != currentAvailableRope)
        {

            if (currentAvailableRope.slowMotion)
                Time.timeScale = 1;

            lastRopePointObj = currentAvailableRope;
            bIsGrabingRope = true;
            // SoundManager.PlaySfx(soundGrap);
            animator.CrossFade(AnimatorParams.RopeGrab, 0.15f);
            float distance = Vector2.Distance(transform.position, currentAvailableRope.transform.position);
            releasePointY = currentAvailableRope.transform.position.y - distance / 10f;
        }
    }

    public void GrabRelease()
    {
        if (!bIsGrabingRope)
            return;
        bCanDoubleJump = false;
        if (rb)
        {
            bApplyGravity = true;
            rb.isKinematic = false;
            rb.velocity = releaseForce * transform.forward;
            rb.AddForce(rb.velocity, ForceMode.Impulse);
        }


        Time.timeScale = 1;
        //  SoundManager.PlaySfx(soundRopeJump);
        bIsGrabingRope = false;
        currentAvailableRope = null;
    }
    #endregion

    #region  Parkour Tricks
    public void TriggerParkourAnimation(AnimationClip animClip, float transitionTime = 0.2f, bool bScaleCapsule = false)
    {
        Debug.Log(animClip.name);
        climbingState = ClimbingState.Parkour;
        animator.applyRootMotion = true;
        animator.SetLayerWeight(1, 1);
        animator.CrossFadeInFixedTime(animClip.name, transitionTime, 1);
        capsule.enabled = false;
        rb.isKinematic = true;

        StartCoroutine(EndParkourState(animator.GetCurrentAnimatorStateInfo(1).length));
    }
    IEnumerator EndParkourState(float t)
    {
        yield return new WaitForSeconds(t);
        climbingState = ClimbingState.None;
        animator.applyRootMotion = false;
        capsule.enabled = true;
        rb.isKinematic = false;
        // animator.SetLayerWeight(1, 0);
    }
    #endregion
    private void OnDrawGizmosSelected()
    {
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        // verticalChecker.position, Vector3.down * verticalCheckDistance, Color.red
        // Gizmos.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance);
        // Gizmos.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), Vector3.right * 2);
        // Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        // Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        //  Gizmos.DrawSphere(transform.position + Vector3.up * (capsule.height / lowCheckOffset) + Vector3.right * 0.45f, 0.18f);
        //Gizmos.DrawSphere(transform.position + Vector3.up * (capsule.height * highCheckOffset) + Vector3.right * 0.45f, 0.18f);

    }

}
