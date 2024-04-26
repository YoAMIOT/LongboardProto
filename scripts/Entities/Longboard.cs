using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER_ANGLE= 0.08f;
	private const float MAX_BOARD_ANGLE = MAX_STEER_ANGLE * -2;
	private const float ENGINE_POWER = 5;
	private const float BRAKE_POWER = 0.1f;
	private const int MAX_THRUSTING_SPEED = 20;
	private const int MIN_FOV = 45;
	private const int MAX_FOV = 70;
	private const int MAX_SPEED = 60;	
	private VehicleWheel3D FrontRightWheel;
	private VehicleWheel3D FrontLeftWheel;
	private VehicleWheel3D BackRightWheel;
	private VehicleWheel3D BackLeftWheel;
	private MeshInstance3D Board;
	private Timer ThrustCooldown;
	private Label Speedometer;
	private ProgressBar StaminaBar;
	private Control Hud;
	private Camera3D Camera;
	private Marker3D CameraMarker;
	private bool canThrust = true;
	private bool hasStamina = true;

	public override void _Ready(){
		Camera = GetNode<Camera3D>("Camera");
		CameraMarker = GetNode<Marker3D>("CameraMarker");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
		ThrustCooldown.Timeout += ResetThrustCapability;
		Hud = GetNode<Control>("HUD");
		Speedometer = GetNode<Label>("HUD/Speedometer");
		StaminaBar = GetNode<ProgressBar>("HUD/StaminaBar");
		StaminaBar.MaxValue = ThrustCooldown.WaitTime;
	}

    public override void _PhysicsProcess(double delta){
		bool isFrontWheelsTouchingGround = FrontLeftWheel.IsInContact() || FrontRightWheel.IsInContact();
		bool isBackWheelsTouchingGround = BackLeftWheel.IsInContact() || BackRightWheel.IsInContact();
		float steeringAxis = Input.GetAxis("Right","Left");
		float steeringSpeed = (float)delta * 0.4f;
		int speed = GetCurrentSpeed();

		//Thrust management
		if (Input.IsActionPressed("Forward") && speed < MAX_THRUSTING_SPEED && canThrust && hasStamina && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			ThrustCooldown.Start();
			canThrust = false;
		} else {
			this.EngineForce = 0;
		}
		if (ThrustCooldown.TimeLeft > 1 && speed < MAX_THRUSTING_SPEED && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			this.EngineForce = ENGINE_POWER;
		}

		//Brake management
		if (Input.IsActionPressed("Backward") && !Input.IsActionPressed("Forward") && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			this.Brake = BRAKE_POWER;
		} else if (speed >= MAX_SPEED){
			this.Brake = 0.06f;
		} else {
			this.Brake = 0;
		}

		//Steering
		if (!isFrontWheelsTouchingGround || !isBackWheelsTouchingGround){
			steeringAxis = 0;
		}
		ManageSteering(steeringAxis, steeringSpeed);

		ManageCameraMovements();

		//Update stamina bar
		UpdateStaminaBar();
	}

	private void ManageSteering(float steeringAxis, float steeringSpeed){
		float currentSteering = (float)Mathf.MoveToward(FrontRightWheel.Steering, steeringAxis * MAX_STEER_ANGLE, steeringSpeed);
		FrontLeftWheel.Steering = currentSteering;
		FrontRightWheel.Steering = currentSteering;
		BackLeftWheel.Steering = currentSteering * -1;
		BackRightWheel.Steering = currentSteering * -1;
		float BoardRotation =  (float)Mathf.MoveToward(Board.Rotation.X, steeringAxis * MAX_BOARD_ANGLE, steeringSpeed * 2);
		Board.Rotation = new Vector3(BoardRotation, 0, 0);
	}

	private void ManageCameraMovements(){
		Camera.GlobalPosition = Camera.GlobalPosition.Lerp(CameraMarker.GlobalPosition, 0.2f);
		Camera.GlobalRotation = CameraMarker.GlobalRotation.Lerp(CameraMarker.GlobalRotation, 0.05f);
	}

	private int GetCurrentSpeed(){
		Vector3 currentVelocity = this.LinearVelocity * this.Transform.Basis;
		int speed = (int)(currentVelocity.Length() * 3.6);
		Speedometer.Text = (speed).ToString() + " Km/h";
		if (speed > 0){
			int fovDifference = MAX_FOV - MIN_FOV;
			float targetFov = (float)(((double)speed / (double)MAX_SPEED) * (double)fovDifference) + MIN_FOV;
			if (targetFov > MAX_FOV){
				targetFov = MAX_FOV;
			}
			Camera.Fov = Mathf.Lerp(Camera.Fov, targetFov, 0.03f);
		}
		return speed;
	}

	private void ResetThrustCapability(){
		canThrust = true;
	}

	private void UpdateStaminaBar(){
		StaminaBar.Value = ThrustCooldown.TimeLeft;
	}
}
