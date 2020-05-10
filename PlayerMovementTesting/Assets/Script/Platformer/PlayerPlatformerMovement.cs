using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class PlayerPlatformerMovement : MonoBehaviour
{
    CharacterMovement characterMovement;

    [SerializeField] private float walkSpeed, sprintSpeed;
    private float moveSpeed;

    [SerializeField] private float accelerationTimeAir, accelerationTimeGround;
    [SerializeField] private float stopDeacceleratingX = .5f;

    [SerializeField] private float maxJumpHeight, minJumpHeight;
    private float maxJumpVelocity, minJumpVelocity;

    [SerializeField] private float timeToJumpApex;
    private float gravity;

    [SerializeField] public int maxNumOfAirJumps;
    private int numOfAirJumps;

    [SerializeField] private Vector2 wallJump;
    [SerializeField] private float wallSlideSpeedMax;
    [SerializeField] private float wallStickTime;

    private int wallDirX;
    private float timeToWallUnstick;

    private Vector3 velocity, oldVelocity;
    private Vector2 directionalInput;
    private float velocityXSmoothing;

    private bool walking, sprinting, jumping, wallSliding;

    public void Start()
    {
        characterMovement = GetComponent<CharacterMovement>();
        CalculatePhysics();
    }

    //caculates gravity, max jump velocity and min jump velocity
    public void CalculatePhysics()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    //returns vector to move after testing collisions
    public Vector3 GetMove()
    {
        return characterMovement.Move(velocity * Time.deltaTime, directionalInput);
    }

    //when jumping button is pressed return velocity for wall jumping or max jump velocity
    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            velocity.x = -wallDirX * wallJump.x;
            velocity.y = wallJump.y;
            jumping = true;
        }

        else if (characterMovement.sides.below)
        {
            velocity.y = maxJumpVelocity;
            jumping = true;
        }
        else if (numOfAirJumps > 0)
        {
            velocity.y = maxJumpVelocity;
            jumping = true;
            numOfAirJumps--;
        }
    }

    //when jump button is released try to lower the velocity y to min jump velocity
    //this make it jump higher the longer jump is held
    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
            velocity.y = minJumpVelocity;
    }
    
    //when sprint button pressed set move speed to sprinting speed while on the ground
    public void OnSprintInputDown()
    {
        if(characterMovement.sides.below)
            moveSpeed = sprintSpeed;
    }

    //when sprint button released set move speed to walking speed
    //this allows sprint jumps to go farther while sprint is held
    public void OnSprintInputUp()
    {
        moveSpeed = walkSpeed;
    }

    //while on a wall falling slow fall speed
    //to get off wall go direction away from wall for wallStickTime
    public void HandleWallSliding()
    {
        wallDirX = (characterMovement.sides.left) ? -1 : 1;
        wallSliding = false;
        if ((characterMovement.sides.left || characterMovement.sides.right) && !characterMovement.sides.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
                velocity.y = -wallSlideSpeedMax;

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    timeToWallUnstick -= Time.deltaTime;

                else
                    timeToWallUnstick = wallStickTime;
            }
            else
                timeToWallUnstick = wallStickTime;
        }
    }

    //calculates the x for velocity
    public void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (characterMovement.sides.below) ? accelerationTimeGround : accelerationTimeAir);
        velocity.y += gravity * Time.deltaTime;
    }

    //sets y velocity to 0 if collision above or below
    //zeros out x velocity if no direction is held
    //resets number of air jumps while on the ground
    public void PostMoveUpdate()
    {
        if (characterMovement.sides.below || characterMovement.sides.above)
        {
            if (characterMovement.sides.slidingDownMaxSlope)
                velocity.y += characterMovement.sides.slopeNormal.y * -gravity * Time.deltaTime;

            else
                velocity.y = 0;
        }

        if (directionalInput.x == 0 && Mathf.Abs(velocity.x) < stopDeacceleratingX)
        {
            velocity.x = 0;
        }

        if (characterMovement.sides.below && !characterMovement.sides.slidingDownMaxSlope)
            numOfAirJumps = maxNumOfAirJumps;
    }

    /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Getters/Setters ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public CharacterMovement GetCharacterMovement()
    {
        return characterMovement;
    }

    public float WalkSpeed
    {
        get { return walkSpeed; }
        set { walkSpeed = value; }
    }

    public float SprintSpeed
    {
        get { return sprintSpeed; }
        set { sprintSpeed = value; }
    }

    public float AccelerationTimeAir
    {
        get { return accelerationTimeAir; }
        set { accelerationTimeAir = value; }
    }

    public float AccelerationTimeGround
    {
        get { return accelerationTimeGround; }
        set { accelerationTimeGround = value; }
    }

    public float StopDeacceleratingX
    {
        get { return stopDeacceleratingX; }
        set { stopDeacceleratingX = value; }
    }

    public float MaxjumpHeight
    {
        get { return maxJumpHeight; }
        set { 
            maxJumpHeight = value;
            CalculatePhysics();
        }
    }

    public float MinJumpHieght
    {
        get { return minJumpHeight; }
        set { 
            minJumpHeight = value;
            CalculatePhysics();
        }
    }

    public float TimeToJumpApex
    {
        get { return timeToJumpApex; }
        set { 
            timeToJumpApex = value;
            CalculatePhysics();
        }
    }

    public int MaxNumOfAirJumps
    {
        get { return maxNumOfAirJumps; }
        set { maxNumOfAirJumps = value; }
    }

    public Vector2 WallJump
    {
        get { return wallJump; }
        set { wallJump = value; }
    }

    public float WallSlideSpeedMax
    {
        get { return wallSlideSpeedMax; }
        set { wallSlideSpeedMax = value; }
    }

    public float WallStickTime
    {
        get { return wallStickTime; }
        set { wallStickTime = value; }
    }

    public bool Walking
    {
        get { return walking; }
        set { walking = value; }
    }

    public bool Sprinting
    {
        get { return sprinting; }
        set { sprinting = value; }
    }

    public bool Jumping
    {
        get { return jumping; }
        set { jumping = value; }
    }

    public bool WallSliding
    {
        get { return wallSliding; }
        set { wallSliding = value; }
    }
}
