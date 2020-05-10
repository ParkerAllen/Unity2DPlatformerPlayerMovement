using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterMovement : RaycastController
{
    public float maxSlopeAngle = 60;
    public string tagOfPlatform;    //Not required

    //tells what sides are colliding
    public CollisionInfo sides;
    private Vector2 playerInput;

    public override void Start()
    {
        base.Start();
        sides.faceDir = 1;
    }

    //move without giving input
    public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    //returns the vector to move
    public Vector3 Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        sides.Reset();
        sides.moveAmountOld = velocity;
        playerInput = input;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        if (velocity.x != 0)
        {
            sides.faceDir = (int)Mathf.Sign(velocity.x);
        }

        HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        if (standingOnPlatform)
        {
            sides.below = true;
        }

        return velocity;
    }

    //check for collisions horizontaly
    private void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = sides.faceDir;
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if (Mathf.Abs(velocity.x) < skinWidth)
            rayLength = 2 * skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            //Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                    continue;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (sides.descendingSlope)
                    {
                        sides.descendingSlope = false;
                        velocity = sides.moveAmountOld;
                    }

                    float distanceToSlope = 0;
                    if (slopeAngle != sides.slopeAngleOld)
                    {
                        distanceToSlope = hit.distance - skinWidth;
                        velocity.x += distanceToSlope * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle, hit.normal);
                    velocity.x += distanceToSlope * directionX;
                }

                if (!sides.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (sides.climbingSlope)
                        velocity.y = Mathf.Tan(sides.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);

                    sides.left = directionX == -1;
                    sides.right = directionX == 1;
                }
            }
        }
    }

    //check for collisions verticaly
    private void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            //Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (!tagOfPlatform.Equals("") && hit.collider.CompareTag(tagOfPlatform))
                {
                    if (directionY == 1 || hit.distance == 0)
                        continue;
                    if (sides.fallingThroughPlatform)
                        continue;
                    if (playerInput.y == -1)
                    {
                        sides.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }

                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (sides.climbingSlope)
                    velocity.x = velocity.y / Mathf.Tan(sides.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);

                sides.below = directionY == -1;
                sides.above = directionY == 1;
            }
        }

        if (sides.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != sides.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    sides.slopeAngle = slopeAngle;
                    sides.slopeNormal = hit.normal;
                }
            }
        }
    }

    //calculates vector to climb a slope
    private void ClimbSlope(ref Vector3 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbMoveAmountY)
        {
            velocity.y = climbMoveAmountY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            sides.below = true;
            sides.climbingSlope = true;
            sides.slopeAngle = slopeAngle;
            sides.slopeNormal = slopeNormal;
        }
    }

    //check if desending slope then calculates vector desending slope, or sliding down slope
    private void DescendSlope(ref Vector3 velocity)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(velocity.y) + skinWidth, collisionMask);

        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
            SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
        }

        if (!sides.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            Vector3 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector3.down, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                        {
                            float moveDistance = Mathf.Abs(velocity.x);
                            float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendMoveAmountY;

                            sides.slopeAngle = slopeAngle;
                            sides.descendingSlope = true;
                            sides.below = true;
                            sides.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    //calculates vector to slide down slope
    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector3 velocity)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                velocity.x = Mathf.Sign(hit.normal.x * Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                sides.slopeAngle = slopeAngle;
                sides.slidingDownMaxSlope = true;
                sides.slopeNormal = hit.normal;
            }
        }
    }

    private void ResetFallingThroughPlatform()
    {
        sides.fallingThroughPlatform = false;
    }

    //information on state of object
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 moveAmountOld;
        public int faceDir;
        public bool fallingThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
