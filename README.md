# fps-parkour
Recreation of source-engine style movement with Unity Game Engine

Play in your browser: https://xacer.dev/pages/parkour/

## Strafe Calculation
```cs
private void ComputeStrafe() {
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
}
```
