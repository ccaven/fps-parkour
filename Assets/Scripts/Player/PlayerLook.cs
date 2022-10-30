using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Settings")]
    [SerializeField] private float sensitivity;
    [SerializeField] private bool invertControlsX;
    [SerializeField] private bool invertControlsY;

    /// <summary>
    /// A measure of the camera's pitch (up and down)
    /// </summary>
    private float pitch = 0f;
    
    /// <summary>
    /// A measure of the camera's yaw (side to side)
    /// </summary>
    private float yaw = 0f;

    /// <summary>
    /// A measure of the camera's tilt
    /// </summary>
    private float roll = 0f;

    /// <summary>
    /// Where the roll variable is trying to go to.
    /// Used for smooth transitions.
    /// </summary>
    private float targetRoll;
    
    /// <summary>
    /// Public access to the targetRoll variable
    /// </summary>
    public float TargetRoll { set => targetRoll = value; }

    /// <summary>
    /// Lock the mouse at the beginning of the frame
    /// </summary>
    private void Start () {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Make the camera look around when the user moves the mousee
    /// </summary>
    private void Update () {

        float multiplierX = sensitivity * (invertControlsX ? -1 : 1);
        float multiplierY = sensitivity * (invertControlsY ? -1 : 1);

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        pitch -= mouseY * multiplierY;
        yaw += mouseX * multiplierX;

        orientation.rotation = Quaternion.Euler(0, yaw, 0);

        cameraHolder.position = playerHead.position;

        roll += (targetRoll - roll) / 20;

        playerCamera.rotation = Quaternion.Euler(pitch, yaw, roll);

    }

}