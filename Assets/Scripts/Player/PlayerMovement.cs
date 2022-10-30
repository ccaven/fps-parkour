using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerLook lookScript;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallrunMask;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private KeyCode crouchKey;

    [Header("Movement variables")]
    [SerializeField] private float maximumGroundSpeed;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float jumpHeight;

    [Header("Air Movement")]
    [SerializeField] private float maximumAirSpeed;
    [SerializeField] private float airAcceleration;

    [Header("Wall Running")]
    [SerializeField] private float wallRunSpeedBoost;
    [SerializeField] private float wallRunMaxSpeed;
    [SerializeField] private float wallRunTilt = 20f;
    [SerializeField] private float maxWallRunTime = 5f;
    [SerializeField] private float wallRunMinSpeed = 1f;

    /// <summary>
    /// A measure of if the player is currently able to move around and jump.
    /// </summary>
    private bool isGrounded;

    /// <summary>
    /// A measure of if the player is currently wall running
    /// </summary>
    private bool isWallRunning;

    /// <summary>
    /// A measure of if the player just jumped
    /// </summary>
    private bool justJumped;

    /// <summary>
    /// Keeps track of how long the wall run is in place
    /// </summary>
    private float wallRunTime;

    /// <summary>
    /// A measure of if the user has a double jump
    /// </summary>
    private bool hasDoubleJump;

    /// <summary>
    /// A measure of if the user has double jumped yet
    /// </summary>
    private bool hasDoubleJumped;

    /// <summary>
    /// A measure of where the player is currently trying to accelerate.
    /// </summary>
    private Vector3 inputMoveDirection;

    /// <summary>
    /// Get the current ground speed of the player
    /// </summary>
    private float GroundSpeed { get => Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z); }

    /// <summary>
    /// Represents how much of a force to apply to the player
    /// </summary>
    private float JumpForce { get => Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight); }

    /// <summary>
    /// Translate the key presses from the user into a useable direction in 3d space.
    /// </summary>
    private void GetInput() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputMoveDirection = Vector3.Normalize(orientation.forward * vertical + orientation.right * horizontal);
    }

    /// <summary>
    /// Called whenever the player hits the spacebar
    /// </summary>
    private void Jump() {
        if (rb.velocity.y < JumpForce) {
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);

            isGrounded = false;

            hasDoubleJump = false;

            justJumped = true;

            print("Jumped");
        }
    }

    /// <summary>
    /// Called when the player performs a double jump
    /// </summary>
    private void DoubleJump() {
        // Cancel downwards y velocity of the player
        if (rb.velocity.y < 0f) {
            rb.AddForce(Vector3.down * rb.velocity.y, ForceMode.Impulse);
        }

        // Add jump force
        rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);

        // Update double jump variable
        hasDoubleJump = false;
        hasDoubleJumped = true;

        print("Double jumped");
    }

    /// <summary>
    /// Determines how the player moves while on the ground.
    /// </summary>
    private void MovePlayerOnGround() {

        hasDoubleJumped = false;
        hasDoubleJump = false;

        // Option to add a jump force
        if (Input.GetKey(jumpKey) && isGrounded && !justJumped) { Jump(); return; }

        // Regular movement
        // Add the movement force
        rb.AddForce(inputMoveDirection * groundAcceleration, ForceMode.Impulse);

        // Add the drag force
        float coefficientOfFriction = groundAcceleration / maximumGroundSpeed;
        rb.AddForce(-rb.velocity * coefficientOfFriction, ForceMode.Impulse);
    }

    /// <summary>
    /// Determines how the player moves while in the air.
    /// </summary>
    private void MovePlayerInAir() {
        // Double jump
        if (hasDoubleJump && !hasDoubleJumped && Input.GetKey(jumpKey)) {
            DoubleJump();
        }

        // Find magnitude of velocity relative to move direction
        Vector3 projectVelocity = Vector3.Project(rb.velocity, inputMoveDirection);

        // Determine if you are trying to move forward or backwards
        bool isAway = Vector3.Dot(inputMoveDirection, projectVelocity) <= 0f;

        // Calculate ideal force
        if (projectVelocity.sqrMagnitude < maximumAirSpeed * maximumAirSpeed || isAway) {
            Vector3 idealForce = inputMoveDirection * airAcceleration;
            idealForce = Vector3.ClampMagnitude(idealForce, maximumAirSpeed + (isAway ? 1 : -1) * projectVelocity.magnitude);
            rb.AddForce(idealForce, ForceMode.Impulse);
        }

        if (!hasDoubleJumped && !Input.GetKey(jumpKey)) {
            hasDoubleJump = true;
        }
    }

    /// <summary>
    /// Called at the beginning of a wall run
    /// </summary>
    /// <param name="wallNormal"></param>
    private void StartWallRun(Vector3 wallNormal) {
        // Check for minimum speed
        float playerSpeed = rb.velocity.magnitude;
        if (playerSpeed < wallRunMinSpeed) {
            isWallRunning = false;

            // Push player off of wall
            // rb.AddForce(wallNormal, ForceMode.Impulse);

            return;
        }

        // Set the wall running variable to true
        isWallRunning = true;

        // Calculate wall tangent
        Vector3 wallTangent = Vector3.Cross(wallNormal, Vector3.up);
        float dotProduct = Vector3.Dot(rb.velocity, wallTangent);
        wallTangent *= Mathf.Sign(dotProduct);

        // Determine which way to rotate camera
        float rightOrLeft = Mathf.Sign(Vector3.Dot(wallNormal, orientation.right));
        lookScript.TargetRoll = -wallRunTilt * rightOrLeft;

        // Direct ground velocity along the wall tangent
        float groundSpeed = GroundSpeed;
        rb.velocity = new Vector3(groundSpeed * wallTangent.x, rb.velocity.y, groundSpeed * wallTangent.z);

        // Add speed boost
        if (groundSpeed < wallRunMaxSpeed) {

            float speedBoost = Mathf.Min(wallRunMaxSpeed - groundSpeed, wallRunSpeedBoost);

            rb.AddForce(wallTangent * speedBoost, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Called during the wall run
    /// </summary>
    /// <param name="wallNormal"></param>
    private void WallRun(Vector3 wallNormal) {

        // Check for minimum speed
        float playerSpeed = rb.velocity.magnitude;
        if (playerSpeed < wallRunMinSpeed) {
            isWallRunning = false;

            // Push player off of wall
            rb.AddForce(wallNormal, ForceMode.Impulse);

            return;
        }

        hasDoubleJump = false;
        hasDoubleJumped = false;

        // Option to jump
        if (Input.GetKey(jumpKey) && !justJumped) {
            Vector3 jumpDirection = JumpForce * Vector3.up + wallNormal;

            if (rb.velocity.y < jumpDirection.y) {
                rb.AddForce(jumpDirection, ForceMode.Impulse);
            }

            justJumped = true;
            isWallRunning = false;
            hasDoubleJump = false;

            print("Jumped off the wall");

            return;
        }

        // Limit y velocity
        if (!justJumped) rb.AddForce(Vector3.down * rb.velocity.y, ForceMode.Impulse);

        // Apply negative gravity
        rb.AddForce(-Physics.gravity, ForceMode.Force);

        // Apply a force towards the wall to keep the player on the wall
        rb.AddForce(-wallNormal, ForceMode.Force);

        // Apply wall run time
        wallRunTime += Time.deltaTime;

        // Slight drag
        if (wallRunTime >= maxWallRunTime) {
            print("Maximum time reached");
        }
    }

    /// <summary>
    /// Get the most-upward-facing surface of the contact
    /// </summary>
    /// <param name="collision">The collision object from OnCollisionEnter or OnCollisionStay</param>
    /// <returns>The most upward facing normal</returns>
    private Vector3 GetBestNormal(Collision collision) {
        Vector3 best = new Vector3(0, -1f, 0);
        foreach (ContactPoint contactPoint in collision.contacts) {
            if (contactPoint.normal.y > best.y) best = contactPoint.normal;
        }
        return best;
    }

    /// <summary>
    /// Determine if object obj is a part of the ground LayerMask.
    /// </summary>
    /// <param name="obj">The object</param>
    /// <returns></returns>
    private bool ObjectInGroundLayer(GameObject obj) {
        int objLayerMask = 1 << obj.layer;
        return (groundMask & objLayerMask) > 0 || ObjectInWallrunLayer(obj);
    }

    private bool ObjectInWallrunLayer(GameObject obj) {
        int objLayerMask = 1 << obj.layer;
        return (wallrunMask & objLayerMask) > 0;
    }

    /// <summary>
    /// Determine what happens when the player hits a surface
    /// </summary>
    /// <param name="collision">The collision object</param>
    private void OnCollisionEnter(Collision collision) {
        Vector3 normal = GetBestNormal(collision);

        // Able to jump
        // if ( normal.y > 0.9f ) isGrounded = true;

        if (Mathf.Abs(normal.y) < 0.01f && ObjectInWallrunLayer(collision.gameObject)) StartWallRun(normal);
    }

    /// <summary>
    /// Determine what happens when the player continues touching something
    /// </summary>
    /// <param name="collision">The collision object</param>
    private void OnCollisionStay(Collision collision) {
        Vector3 normal = GetBestNormal(collision);

        if (normal.y > 0.9f && ObjectInGroundLayer(collision.gameObject)) isGrounded = !justJumped;
        else isGrounded = false;

        if (!isGrounded && Mathf.Abs(normal.y) < 0.01f && ObjectInWallrunLayer(collision.gameObject)) {
            WallRun(normal);
        }
        else {
            isWallRunning = false;
        }
    }

    /// <summary>
    /// Determine what happens when the player leaves the surface
    /// </summary>
    /// <param name="collision">The collision object</param>
    private void OnCollisionExit(Collision collision) {
        isGrounded = false;
        isWallRunning = false;
        justJumped = false;

        lookScript.TargetRoll = 0;

        wallRunTime = 0;
    }

    /// <summary>
    /// Called every time the screen is refreshed
    /// </summary>
    private void Update() {
        GetInput();

        if (transform.position.y < -200f) {
            transform.position = new Vector3(7, 0, -5);
            rb.velocity = new Vector3(0, 0, 0);
        }

        if (!isGrounded) {
            MovePlayerInAir();
        }

        else if (!isWallRunning) {
            MovePlayerOnGround();
        }

        if (!Input.GetKey(jumpKey)) justJumped = false;
    }
}