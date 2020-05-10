using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : MonoBehaviour
{
    private PlayerPlatformerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerPlatformerMovement>();
    }

    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        playerMovement.SetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerMovement.OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            playerMovement.OnJumpInputUp();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerMovement.OnSprintInputDown();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerMovement.OnSprintInputUp();
        }
    }

    public void FixedUpdate()
    {
        playerMovement.CalculateVelocity();
        playerMovement.HandleWallSliding();
        transform.Translate(playerMovement.GetMove());
        playerMovement.PostMoveUpdate();
    }
}
