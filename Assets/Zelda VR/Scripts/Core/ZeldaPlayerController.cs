using UnityEngine;

public class ZeldaPlayerController : OVRPlayerController
{
    const float FLY_SPEED = 0.2f;


    public bool gravityEnabled = true;
    public bool airJumpingEnabled = false;
    public bool flyingEnabled = false;
    public float RunMultiplier = 1.0f;


    [SerializeField]
    ZVRAvatar _avatar;

    [SerializeField]
    Transform _weaponContainerLeft;
    public Transform WeaponContainerLeft {
        get {
            if (ZeldaInput.AreAnyTouchControllersActive())
            {
                return _avatar.WeaponContainerLeft;
            }
            else
            {
                return _weaponContainerLeft;
            }
        }
    }

    [SerializeField]
    Transform _weaponContainerRight;
    public Transform WeaponContainerRight {
        get {
            // TODO
            if (ZeldaInput.AreAnyTouchControllersActive())      
            {
                return _avatar.WeaponContainerRight;
            }
            else
            {
                return _weaponContainerRight;
            }
        }
    }

    [SerializeField]
    Transform _shieldContainer;
    public Transform ShieldContainer { get { return _shieldContainer; } }


    public Vector3 ForwardDirection { get { return transform.forward; } }
    public Vector3 LineOfSight
    {
        get
        {
            return CameraRig.centerEyeAnchor.forward;
        }
    }
    public Vector3 LastAttemptedMotion { get; private set; }

    public float Height { get { return Controller.height; } }

    /*protected override void Update()
    {
        //Transform crt = CameraRig.transform;
        if (useProfileData)
        {
            if (InitialPose == null)
            {
                // Save the initial pose so it can be recovered if useProfileData
                // is turned off later.
                InitialPose = new OVRPose()
                {
                    position = CameraRig.transform.localPosition,
                    orientation = CameraRig.transform.localRotation
                };
            }

            var p = CameraRig.transform.localPosition;
            if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
            {
                p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
            }
            else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
            {
                p.y = -(0.5f * Controller.height) + Controller.center.y;
            }
            CameraRig.transform.localPosition = p;
        }
        else if (InitialPose != null)
        {
            // Return to the initial pose if useProfileData was turned off at runtime
            CameraRig.transform.localPosition = InitialPose.Value.position;
            CameraRig.transform.localRotation = InitialPose.Value.orientation;
            InitialPose = null;
        }

        UpdateMovement();

        Vector3 motion = Vector3.zero;

        float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

        MoveThrottle.x /= motorDamp;
        MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
        MoveThrottle.z /= motorDamp;

        motion += MoveThrottle * SimulationRate * Time.deltaTime;

        // Flying
        if (flyingEnabled)
        {
            float triggersAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.Triggers);
            if (Mathf.Abs(triggersAxis) > 0.1f)
            {
                motion.y += triggersAxis * FLY_SPEED;
            }

            FallSpeed = 0;
        }
        else
        {
            // Gravity
            if (gravityEnabled)
            {
                if (IsGrounded && FallSpeed <= 0)
                    FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
                else
                    FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);
            }

            motion.y += FallSpeed * SimulationRate * Time.deltaTime;

            // Offset correction for uneven ground
            float bumpUpOffset = 0.0f;

            if (IsGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
            {
                bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(motion.x, 0, motion.z).magnitude);
                motion -= bumpUpOffset * Vector3.up;
            }
        }

        Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + motion), new Vector3(1, 0, 1));

        // Move contoller
        Controller.Move(motion);

        LastAttemptedMotion = motion;

        Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

        if (predictedXZ != actualXZ)
            MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
    }*/
    public bool IsGrounded { get { return Controller.isGrounded; } }

    /*public override void UpdateMovement()
    {
        if (HaltUpdateMovement)
            return;

        float moveHorzAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveHorizontal);
        float moveVertAxis = ZeldaInput.GetAxis(ZeldaInput.Axis.MoveVertical);
        bool moveForward = moveVertAxis > 0;
        bool moveLeft = moveHorzAxis < 0;
        bool moveBack = moveVertAxis < 0;
        bool moveRight = moveHorzAxis > 0;

        MoveScale = 1.0f;

        if ((moveForward && moveLeft) || (moveForward && moveRight) || (moveBack && moveLeft) || (moveBack && moveRight))
        {
            MoveScale = 0.70710678f;
        }

        MoveScale *= SimulationRate * Time.deltaTime;

        // Compute this for key movement
        float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

        // Run
        if (ZeldaInput.GetButton(ZeldaInput.Button.Run))
        {
            moveInfluence *= RunMultiplier;
        }


        Quaternion ort = transform.rotation;
        Vector3 ortEuler = ort.eulerAngles;
        ortEuler.z = ortEuler.x = 0f;
        ort = Quaternion.Euler(ortEuler);

        if (moveForward)
            MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
        if (moveBack)
            MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
        if (moveLeft)
            MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
        if (moveRight)
            MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);

        // Jump
        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.Jump))
        {
            Jump();
        }

        // Rotation
        Vector3 euler = transform.rotation.eulerAngles;

        float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

        //if (!SkipMouseRotation)
        //{
        //    euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
        //}

        moveInfluence = SimulationRate * Time.deltaTime * Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

        float deltaRotation = ZeldaInput.GetAxis(ZeldaInput.Axis.LookHorizontal) * rotateInfluence * 3.25f;
        euler.y += deltaRotation;

        transform.rotation = Quaternion.Euler(euler);
    }*/

    new public bool Jump()
    {
        if (airJumpingEnabled)
        {
            FallSpeed = 0;
        }
        else if (!IsGrounded)
        {
            return false;
        }

        MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

        return true;
    }
}