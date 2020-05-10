using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    public LayerMask collisionMask;		//layer(s) to collide with

    public const float skinWidth = .015f;
    const float dstBetweenRays = .15f;
	[HideInInspector]
	public int horizontalRayCount;
	[HideInInspector]
	public int verticalRayCount;

	[HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    [HideInInspector]
    public BoxCollider2D myCollider;
    public RaycastOrigins raycastOrigins;

	public virtual void Awake()
	{
		myCollider = GetComponent<BoxCollider2D>();
	}

	public virtual void Start()
	{
		CalculateRaySpacing();
	}

	//updates the positions of the corners
	public void UpdateRaycastOrigins()
	{
		Bounds bounds = myCollider.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	//calculates between each ray
	public void CalculateRaySpacing()
	{
		Bounds bounds = myCollider.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	//the corners of a box collider
	public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
