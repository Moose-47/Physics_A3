using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBlock : MonoBehaviour
{
    private Rigidbody rb;          //Reference to the Rigidbody on this block
    private bool inverted = false; //Tracks whether gravity is flipped (true = floating up)
    public float gravityStrength = 9.81f; //How strong gravity is applied (similar to Earth's gravity)

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); //Grab the Rigidbody attached to this block
        rb.useGravity = false;          //Turn off default Unity gravity
    }

    private void FixedUpdate()
    {
        //Decide which direction gravity should go
        //If inverted = true ? gravity points UP, otherwise DOWN
        Vector3 gravity = (inverted ? Vector3.up : Vector3.down) * gravityStrength;

        //Apply the gravity force to the block
        //ForceMode.Acceleration makes the Rigidbody accelerate without considering its mass
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    //----- TOGGLE GRAVITY -----
    //This function flips the block's gravity state
    //If it's falling down, it will start floating up, and vice versa
    public void ToggleGravity() => inverted = !inverted;
}
