using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
[RequireComponent(typeof(LineRenderer), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    //----- MOVEMENT SETTINGS -----
    [Header("Movement Settings")]
    public float moveSpeed = 5f;       //How fast the player moves on the ground
    public float maxVelocity = 7f;     //Maximum speed to prevent moving too fast

    //----- CAMERA SETTINGS -----
    [Header("Camera Settings")]
    public Transform cameraTransform;  //The player's camera (for looking around)
    public float lookSensitivity = 2f; //How sensitive mouse movement is

    //----- SHOOTING SETTINGS -----
    [Header("Shoot Settings")]
    public float shootDistance = 50f;    //How far the player can "shoot" to toggle gravity
    public LineRenderer lr;              //LineRenderer component to show a visible laser/shot
    public Transform muzzleTransform;    //The point where the laser starts (usually at the gun or camera)
    public float laserDuration = 0.05f;  //How long the laser appears on screen

    //----- Sounds -----
    [Header("Sounds")]
    private AudioSource audioSource;    //The AudioSource attached to the player
    public AudioClip shootSfx;          //The audio clip for the shooting sound effect

    //----- INTERNAL VARIABLES -----
    private Rigidbody rb;             //The Rigidbody attached to the player
    private Vector2 moveInput;        //Stores WASD movement input
    private Vector2 lookInput;        //Stores mouse look input
    private float pitch = 0f;         //Tracks vertical camera rotation (up/down)
    private float laserTimer;         //Tracks how long the laser should stay visible

    // ----- INITIAL SETUP -----
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); //Get the Rigidbody component attached to this GameObject
        rb.constraints = RigidbodyConstraints.FreezeRotation; //Prevent player from tipping over

        //Lock the mouse cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Make sure the laser is off at the start
        if (lr != null)
            lr.enabled = false;
    }

    //----- INPUT CALLBACKS -----
    //Called automatically by the Input System when the player moves
    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>(); //Store input as a 2D vector (x = sideways, y = forward/back)

    //Called automatically by the Input System when the player moves the mouse/look stick
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>(); //Store input as a 2D vector (x = horizontal, y = vertical)

    //Called automatically when the player presses the Fire button
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) //Only do this when the button is first pressed (not released)
            Shoot();           //Call the Shoot function
    }

    //----- MOVEMENT HANDLING -----
    private void FixedUpdate()
    {
        //Calculate movement direction relative to the camera
        Vector3 forward = cameraTransform.forward;  //Forward direction of the camera
        Vector3 right = cameraTransform.right;      //Right direction of the camera

        //Remove vertical component so movement is always horizontal
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize(); //Normalize makes the length of the vector 1
        right.Normalize();

        //Combine WASD input with camera directions
        Vector3 moveDir = forward * moveInput.y + right * moveInput.x; //Forward/back + left/right
        Vector3 targetVelocity = moveDir * moveSpeed; //Multiply by speed to get final velocity

        //Keep existing vertical velocity so gravity works naturally
        targetVelocity.y = rb.linearVelocity.y;

        //Smoothly blend current velocity to target velocity
        //Lerp makes movement feel less jerky
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 0.2f);

        //Clamp horizontal speed so player can't move too fast
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVel.magnitude > maxVelocity)
        {
            Vector3 limitedVel = horizontalVel.normalized * maxVelocity;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    // ----- CAMERA LOOK HANDLING -----
    private void LateUpdate()
    {
        HandleLook(); //Update camera rotation after all other updates
                      //We use LateUpdate so camera movement is smooth and matches the frame

        //--- LASER TIMER LOGIC ---
        if (lr != null && lr.enabled) //Only run if the laser is currently visible
        {
            laserTimer -= Time.deltaTime; //Reduce timer by the time since last frame
            if (laserTimer <= 0f)
            {
                lr.enabled = false; //Turn off the laser
            }
        }
    }

    private void HandleLook()
    {
        //Horizontal rotation (yaw) rotates the player left/right
        float yaw = lookInput.x * lookSensitivity;
        transform.Rotate(Vector3.up * yaw);

        //Vertical rotation (pitch) rotates only the camera up/down
        pitch -= lookInput.y * lookSensitivity;      //Invert mouse for natural feel
        pitch = Mathf.Clamp(pitch, -90f, 90f);       //Prevent camera from flipping over

        //Apply pitch rotation to the camera only
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    //----- SHOOTING FUNCTION -----
    private void Shoot()
    {
        audioSource.PlayOneShot(shootSfx); //Play the shoot sound effect

        //Create a ray from the camera forward
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Vector3 endPoint; //Where the laser will end (hit point or max distance)

        //Check if the ray hits something
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
        {
            endPoint = hit.point; //Laser ends at the hit point

            //Check if the object hit has a GravityBlock script
            GravityBlock block = hit.collider.GetComponent<GravityBlock>();
            if (block != null)
                block.ToggleGravity(); //Flip gravity up or down
        }
        else
        {
            //If nothing is hit, laser just goes straight forward
            endPoint = ray.origin + ray.direction * shootDistance;
        }

        // Show the laser visually
        if (lr != null && muzzleTransform != null)
        {
            lr.SetPosition(0, muzzleTransform.position); //Start at the gun/muzzle
            lr.SetPosition(1, endPoint);                 //End at the hit point or max distance
            lr.enabled = true;                           //Turn on the line
            laserTimer = laserDuration;                  //Reset timer so it disappears after short time
        }
    }
}