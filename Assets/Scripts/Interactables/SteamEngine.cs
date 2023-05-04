using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SteamEngine : MonoBehaviour {
    [SerializeField] private List<HingeJoint> joints;

    [SerializeField] private float maxSpeed;
    [SerializeField] private float turnMultiplier;

    private float _currentSpeed;
    private float _currentTurn;
    private Rigidbody _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        joints.ForEach(joint => {
            joint.motor = new JointMotor() { targetVelocity = -_currentSpeed, force = joint.motor.force, freeSpin = false };
        });

        _rb.AddTorque(_currentTurn * turnMultiplier * transform.up, ForceMode.Impulse);
    }

    public void SetSpeedPercentage(float percentage) {
        _currentSpeed = (maxSpeed / 100) * percentage;
    }

    public void SetTurn(float turn) {
        _currentTurn = turn;
    }
}
