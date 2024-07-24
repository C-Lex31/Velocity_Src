using UnityEngine;

public static class AnimatorParams
{
    public static readonly int xVel = Animator.StringToHash("xVel");
    public static readonly int yVel = Animator.StringToHash("yVel");
    public static readonly int bIsGrounded = Animator.StringToHash("bIsGrounded");
    public static readonly int bCanJump = Animator.StringToHash("bCanJump");
    public static readonly int bIsSliding = Animator.StringToHash("bIsSliding");
    public static readonly int bIsGrabingRope = Animator.StringToHash("bIsGrabingRope");
    public static readonly int Roll = Animator.StringToHash("Roll");
    public static readonly int Land = Animator.StringToHash("Land");
     public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int DoubleJump = Animator.StringToHash("DoubleJump");
     public static readonly int RopeGrab = Animator.StringToHash("RopeGrab");
     public static readonly int RopeRelease = Animator.StringToHash("RopeRelease");
     public static readonly int LedgeClimb = Animator.StringToHash("LedgeClimb");
     public static readonly int LowLedgeClimb = Animator.StringToHash("LowLedgeClimb");
}
