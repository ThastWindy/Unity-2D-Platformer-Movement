using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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
    [SerializeField] private GameObject groundChecker;
    [SerializeField] private LayerMask groundLayer;
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

    private float horizontal;

    void FixedUpdate()
    {

        //Movement stuff----------------------------------
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        //Jumping stuff----------------------------------
        groundCheck();

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

    private void Flip()
    {
        // SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        // spriteRenderer.flipX = !spriteRenderer.flipX; // Flip the sprite horizontally

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
        isGrounded = Physics2D.OverlapCircle(groundChecker.transform.position, 0.1f, groundLayer);
    }

    #endregion

    //move
    public void Move(InputAction.CallbackContext context)
    {
        if (!canMove) return; // Prevent movement if canMove is false
        horizontal = context.ReadValue<Vector2>().x;
    }

    //set gravity
    public void SetGravityScale(float scale)
	{
		rb.gravityScale = scale;
	}
}
