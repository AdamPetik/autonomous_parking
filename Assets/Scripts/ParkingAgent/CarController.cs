using UnityEngine;

/// <summary>
/// Component which controls the car by motor torque, break torqe and steer angle.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    /// <summary>
    /// Motor torque to be applied.  
    /// </summary>
    [HideInInspector]
    public float motorTorque;

    /// <summary>
    /// Break torque to be applied if brake is not handled by motor torque.  
    /// </summary>
    [HideInInspector] 
    public float breakTorque;

    /// <summary>
    /// Steer angle to be applied.
    /// </summary>
    [HideInInspector]
    public float steerAngle;

    /// <summary>
    /// Break is handled by motor torque. That means if motor torque is
    /// oposite to the car moving direction, break is applyed. 
    /// </summary>
    public bool handleBreakByMotorTorque;

    private Rigidbody rigidbodyComponent;
    
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;


    private void Awake()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Stops the car immediately.  
    /// </summary>
    public void StopCar()
    {
        rigidbodyComponent.velocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;
        motorTorque = 0f;
        steerAngle = 0f;
    }

    private void FixedUpdate()
    {
        // applies the break of 2 times of motor torque if the motor torque would
        // spin the wheels in oposite direction as the car is actually going.
        if (handleBreakByMotorTorque && ( GetSpeed() >= 1 && motorTorque < 0 || GetSpeed() <= -1 && motorTorque > 0 ))
        {
            ApplyBreaking(Mathf.Abs(motorTorque*2));
            HandleMotorTorque(0); // this was added
        }
        else
        {
            HandleMotor();
        }
        HandleSteering();
        UpdateWheels();
    }

    private void HandleMotor()
    {
        HandleMotorTorque(motorTorque);

        if (handleBreakByMotorTorque)
        {
            ApplyBreaking(0);
        }
        else
        {
            ApplyBreaking(breakTorque);
        }
    }

    private void HandleMotorTorque(float value)
    {
        frontLeftWheelCollider.motorTorque = value;
        frontRightWheelCollider.motorTorque = value;    
    }

    private void ApplyBreaking(float torque)
    {
        frontLeftWheelCollider.brakeTorque = torque;
        frontRightWheelCollider.brakeTorque = torque;
        rearLeftWheelCollider.brakeTorque = torque;
        rearRightWheelCollider.brakeTorque = torque;
    }

    private void HandleSteering() {
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    /// <summary>
    /// Update orientation and rotation of wheel transform based on wheel collider.
    /// </summary>
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.rotation = rotation;
        wheelTransform.position = position;
    }

    public float GetSpeed()
    {
        Vector3 velocity = rigidbodyComponent.velocity;
        double speed = System.Math.Round(velocity.magnitude*3.6, 1);

        var localVel = transform.InverseTransformDirection(velocity);
        
        // if reversing, negative value is returned
        if(localVel.z < 0)
        {
            speed = -speed;
        }
        return (float) speed;
    }

    public bool Reverse 
    {
        get 
        { 
            return GetSpeed() < 0 ? true : false;
        } 
    }

}
