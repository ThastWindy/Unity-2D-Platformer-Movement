using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{   

    #region Variables

    [Header("DEBUG")]
    [SerializeField] private float currentGravity;
    [SerializeField] private bool infiniteJump;
    [Space]

    [Header("MONITOR")]
    [SerializeField] private bool isGrounded;
    public bool isFacingRight = true;
    public bool canMove = true;
    [Space]

    [Header("Reference")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [Space]

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [Space]

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float defaultGravity = 2f;
    [SerializeField] private float fallGravity = 4f;
    [SerializeField] private float fallMultiplier = 1f;
    [SerializeField] private float maxFallSpeed = 50f;
    [Space]

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    [SerializeField] private float wallJumpingTime = 0.2f;
     private float wallJumpingCounter;
    [SerializeField] private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    private float horizontal;

    #endregion

    #region Unity Methods

    void FixedUpdate()
    {
        //Movement stuff----------------------------------

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }
        

        //Jumping stuff----------------------------------
        groundCheck();

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }

        //Fall faster
        if (rb.linearVelocity.y < 0)
        {
            //Increase gravity when falling
            SetGravityScale(fallGravity * fallMultiplier);
            //Cap max fall speed
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            //Reset gravity when not falling
            SetGravityScale(defaultGravity);
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); //get component
        rb.gravityScale = defaultGravity; //set default gravity
    }

    void Update()
    {
        //DEBUG MONITOR stuff----------------------------------
        currentGravity = rb.gravityScale;

        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            //Flip the player based on movement direction
            if (horizontal > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontal < 0 && isFacingRight)
            {
                Flip();
            }
        }
    }

    #endregion

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Flip the x scale to mirror the sprite
        transform.localScale = scale;
    }

    #region Jump

    //jump
    public void Jump(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (context.performed && infiniteJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        
        if (context.canceled && rb.linearVelocity.y > 0)
        {
            // Reduce upward velocity for variable jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private void groundCheck()
    {
        // Check if the player is grounded using a circle overlap
        isGrounded = Physics2D.OverlapCircle(groundChecker.position, 0.1f, groundLayer);
    }

    #endregion

    #region Wall Jump

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !isGrounded && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                Flip();
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    #endregion

    #region Movement

    //move
    public void Move(InputAction.CallbackContext context)
    {
        if (!canMove) return; // Prevent movement if canMove is false
        horizontal = context.ReadValue<Vector2>().x;
    }

    #endregion

    #region Others

    //set gravity
    public void SetGravityScale(float scale)
	{
		rb.gravityScale = scale;
	}

    #endregion
}
