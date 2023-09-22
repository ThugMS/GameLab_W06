using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float playerHeight;

    public Transform orientation;

    [Header("Movement")]
    public float moveSpeed;
    public float moveMultiplier;
    public float airMultiplier;
    public float counterMovement;

    public float jumpForce;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("CounterMovement")]
    public float maxSpeed;
    public float walkMaxSpeed;
    public float sprintMaxSpeed;
    public float airMaxSpeed;

    [Header("Ground Detection")]
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public float groundCheckRadius;

    private float horizontalInput;
    private float verticalInput;

    public bool grounded;

    private Vector3 moveDirection;
    private Vector3 slopeMoveDirection;

    private Rigidbody rb;

    RaycastHit slopeHit;

    public TextMeshProUGUI text_speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, whatIsGround);

        MyInput();
        ControlSpeed();

        if (Input.GetKeyDown(jumpKey) && grounded)
        {
            // Jump
            Jump();
        }

        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void ControlSpeed()
    {
        if (grounded && Input.GetKey(sprintKey))
            maxSpeed = sprintMaxSpeed;

        else if (grounded)
            maxSpeed = walkMaxSpeed;

        // no specific airMaxSpeed for now;
        //else
        //    maxSpeed = airMaxSpeed;
    }

    private void MovePlayer()
    {
        float x = horizontalInput;
        float y = verticalInput;

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        moveDirection = orientation.forward * y + orientation.right * x;

        // on slope
        if (OnSlope())
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * moveMultiplier, ForceMode.Force);

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * moveMultiplier * airMultiplier, ForceMode.Force);

        // limit rb velocity
        Vector3 rbFlatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (rbFlatVelocity.magnitude > maxSpeed)
        {
            rbFlatVelocity = rbFlatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(rbFlatVelocity.x, rb.velocity.y, rbFlatVelocity.z);
        }
    }

    private void Jump()
    {
        if (!grounded)
            return;

        // reset rb y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded) return;

        float threshold = 0.01f;

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
                return true;
        }

        return false;
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }
}

//using JetBrains.Annotations;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.VisualScripting;
//using UnityEngine;

//public class PlayerMovement : MonoBehaviour
//{   
//    public enum MOVEMENT_STATE
//    {
//        Walking, Sprinting, Crouching, Air
//    }

//    #region PublicVariables
//    public float m_groundDrag;

//    public MOVEMENT_STATE m_movementState;
//    #endregion

//    #region PrivateVariables
//    [Header("Movement")]
//    private float m_moveSpeed;
//    [SerializeField] private float m_walkSpeed;
//    [SerializeField] private float m_sprintSpeed;

//    [SerializeField] private Transform m_orientation;
//    private Vector3 m_moveDir;

//    [Header("Ground Check")]
//    [SerializeField] private float m_playerHeight;
//    [SerializeField] private LayerMask m_groundLayer;
//    [SerializeField] private bool m_isGround = false;

//    [Header("Jump")]
//    [SerializeField] private float m_jumpForce;
//    [SerializeField] private float m_jumpCooldown;
//    [SerializeField] private float m_airMultiplier;
//    [SerializeField] private bool m_readyJump;

//    [Header("Crouch")]
//    [SerializeField] private float m_crouchSpeed;
//    [SerializeField] private float m_crouchYScale;
//    [SerializeField] private float m_startYScale;

//    [Header("SlopeHandling")]
//    [SerializeField] public float m_maxSlopeAngle;
//    private RaycastHit m_slopeHit;
//    private bool exitSlope;

//    [Header("KeyBinds")]
//    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
//    [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
//    [SerializeField] private KeyCode m_crouchKey = KeyCode.LeftControl;

//    private float m_horizontalInput;
//    private float m_verticalInput;

//    private Rigidbody m_rigidbody;
//    #endregion

//    #region PublicMethod
//    private void Start()
//    {
//        TryGetComponent<Rigidbody>(out m_rigidbody);
//        m_rigidbody.freezeRotation = true;

//        m_readyJump = true;
//        m_startYScale = transform.localScale.y;
//    }

//    private void Update()
//    {
//        // ground check
//        m_isGround = Physics.Raycast(transform.position, Vector3.down, m_playerHeight * 0.5f + 0.2f, m_groundLayer);

//        CallInput();
//        ControlSpeed();
//        StateHandler();

//        // drag handle
//        if(m_isGround == true)
//        {
//            m_rigidbody.drag = m_groundDrag;
//        }
//        else
//        {
//            m_rigidbody.drag = 0;
//        }
//    }

//    private void FixedUpdate()
//    {
//        MovePlayer();
//    }
//    #endregion

//    #region PrivateMethod
//    private void StateHandler()
//    {
//        // Mode - Sprinting
//        if(m_isGround == true && Input.GetKey(m_sprintKey))
//        {
//            m_movementState = MOVEMENT_STATE.Sprinting;
//            m_moveSpeed = m_sprintSpeed;
//        }
//        // Mode - Walking
//        else if (m_isGround == true)
//        {
//            m_movementState = MOVEMENT_STATE.Walking;
//            m_moveSpeed = m_walkSpeed;
//        }
//        // Mode - Crouching
//        else if (Input.GetKey(m_crouchKey))
//        {
//            m_movementState |= MOVEMENT_STATE.Crouching;
//            m_moveSpeed = m_crouchSpeed;

//        }
//        //Mode - Air
//        else
//        {
//            m_movementState = MOVEMENT_STATE.Air;
//        }
//    }
//    private void CallInput()
//    {
//        m_horizontalInput = Input.GetAxisRaw("Horizontal");
//        m_verticalInput = Input.GetAxisRaw("Vertical");

//        // when jump key pressed
//        if(Input.GetKey(m_jumpKey) && m_readyJump && m_isGround)
//        {
//            m_readyJump = false;

//            Jump();

//            Invoke(nameof(ResetJump), m_jumpCooldown);
//        }

//        if (Input.GetKeyDown(m_crouchKey))
//        {
//            transform.localScale = new Vector3(transform.localScale.x, m_crouchYScale, transform.localScale.z);
//            m_rigidbody.AddForce(Vector3.down * 10f, ForceMode.Impulse);
//        }

//        if (Input.GetKeyUp(m_crouchKey))
//        {
//            transform.localScale = new Vector3(transform.localScale.x, m_startYScale, transform.localScale.z);
//        }
//    }

//    private void MovePlayer()
//    {
//        // calculate direction
//        m_moveDir = m_orientation.forward * m_verticalInput + m_orientation.right * m_horizontalInput;

//        // on slope
//        if(OnSlope() == true && !exitSlope)
//        {
//            m_rigidbody.AddForce(GetSlopeMoveDirection() * m_moveSpeed * 20f, ForceMode.Force);

//            if(m_rigidbody.velocity.y > 0)
//            {
//                m_rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
//            }
//        }
//        // on ground
//        else if(m_isGround == true)
//        {
//            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f, ForceMode.Force);
//        }
//        // in air
//        else if(m_isGround == false)
//        {
//            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f * m_airMultiplier, ForceMode.Force);
//        }

//        // turn gravity off while on slope
//        m_rigidbody.useGravity = !OnSlope();
//    }

//    private void ControlSpeed()
//    {
//        // limiting speed on slope
//        if (OnSlope() && !exitSlope)
//        {
//            if (m_rigidbody.velocity.magnitude > m_moveSpeed)
//            {
//                m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_moveSpeed;
//            }
//        }
//        // limiting speed on ground or in air
//        else
//        {
//            Vector3 flatVelocity = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);

//            if (flatVelocity.magnitude > m_moveSpeed)
//            {
//                Vector3 limitVelocity = flatVelocity.normalized * m_moveSpeed;
//                m_rigidbody.velocity = new Vector3(limitVelocity.x, m_rigidbody.velocity.y, limitVelocity.z);
//            }
//        }
//    }

//    private void Jump()
//    {
//        exitSlope = true;

//        // reset y velocity
//        m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);

//        m_rigidbody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
//    }

//    private bool OnSlope()
//    {
//        // if you on slope
//        if (Physics.Raycast(transform.position, Vector3.down, out m_slopeHit, m_playerHeight * 0.5f + 0.3f))
//        {
//            float angle = Vector3.Angle(Vector3.up, m_slopeHit.normal);
//            return angle < m_maxSlopeAngle && angle != 0;
//        }

//        return false;
//    }

//    private Vector3 GetSlopeMoveDirection()
//    {
//        return Vector3.ProjectOnPlane(m_moveDir, m_slopeHit.normal).normalized;
//    }

//    private void ResetJump()
//    {
//        m_readyJump = true;

//        exitSlope = false;
//    }
//    #endregion
//}
