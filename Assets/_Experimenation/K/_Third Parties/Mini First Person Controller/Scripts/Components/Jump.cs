using UnityEngine;

public class Jump : MonoBehaviour
{
    Rigidbody rigidbody;
    private float BaseJumpStrength = 2;
    public float CurrentJumpStrength = 2;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;


    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
        CurrentJumpStrength = BaseJumpStrength;
    }

    void Awake()
    {
        // Get rigidbody.
        rigidbody = GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        // Jump when the Jump button is pressed and we are on the ground.
        if (Input.GetButtonDown("Jump") && (!groundCheck || groundCheck.isGrounded))
        {
            rigidbody.AddForce(Vector3.up * 100 * CurrentJumpStrength);
            Jumped?.Invoke();
        }
    }
    public float GetJumpStrength()
    {
        return BaseJumpStrength;
    }

    public void SetJumpStrength(float newStrength)
    {
        CurrentJumpStrength = newStrength;
    }
    public void ResetJumpStrength()
    {
        CurrentJumpStrength = BaseJumpStrength;
    }
}
