using UnityEngine;

public class Floater : MonoBehaviour {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float depthBeforeSubmerged = 1f;
    [SerializeField] private float displacementAmount = 3f;
    [SerializeField] private float waterDrag = 0.99f;
    [SerializeField] private float waterAngularDrag = 0.5f;
    [SerializeField] private int floaterCount = 1;

    public bool ApplyGravity = true;
    public bool InWater = false;
    public bool Enable = true;

    private void FixedUpdate() {
        if (!Enable) return;

        if (ApplyGravity)
            rb.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);

        if (WaveManager.Instance == null) return;
        float waveHeight = WaveManager.Instance.GetWaveHeight(transform.position.x, transform.position.z);
        InWater = transform.position.y <= waveHeight;

        if (InWater) {
            float displacementMultiplier = Mathf.Clamp01((waveHeight - transform.position.y) / depthBeforeSubmerged) * displacementAmount;
            rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y / floaterCount) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
            rb.AddForce(displacementMultiplier * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacementMultiplier * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
