using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER_ANGLE= 0.08f;
	private const float MAX_BOARD_ANGLE = MAX_STEER_ANGLE * -2;
	private const float ENGINE_POWER = 5;
	private const float BRAKE_POWER = 0.05f;
	private const int MAX_THRUSTING_SPEED = 20;
	private const int MAX_THRUST_STAMINA = 8;
	private const int FOV_MIN = 103;
	private const int FOV_MAX = 120;
	private const int MAX_BALANCING_TIME = 20;
	private Node3D CameraPivotH;
	private Node3D CameraPivotV;
	private Camera3D Camera;
	private VehicleWheel3D FrontRightWheel;
	private VehicleWheel3D FrontLeftWheel;
	private VehicleWheel3D BackRightWheel;
	private VehicleWheel3D BackLeftWheel;
	private MeshInstance3D Board;
	private Timer ThrustCooldown;
	private Vector3 CameraLookAt;
	private Timer RecenterCameraTimer;
	private Label Speedometer;
	private Timer StaminaCooldown;
	private Label StaminaLabel;
	private float MouseSensivity = 0.05f;
	private int minPitch = -50;
	private int maxPitch = 30;
	private bool recenterCamera = false;
	private int thrustStamina = MAX_THRUST_STAMINA;
	private bool canThrust = true;
	private RayCast3D RayCast;
	private Timer BackBalancingCooldown;
	private int timeBalancing = 0;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		BackBalancingCooldown = GetNode<Timer>("BackBalancingCooldown");
		CameraPivotH = GetNode<Node3D>("CameraPivotH");
		CameraPivotV = GetNode<Node3D>("CameraPivotH/CameraPivotV/");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
		ThrustCooldown.Timeout += ResetThrustCapability;
		Speedometer = GetNode<Label>("HUD/Speedometer");
		RecenterCameraTimer = GetNode<Timer>("ReCenterCamera");
		StaminaCooldown = GetNode<Timer>("StaminaCooldown");
		StaminaLabel = GetNode<Label>("HUD/StaminaLabel");
		StaminaCooldown.Timeout += StaminaReload;
		RecenterCameraTimer.Timeout += TiggerRecenterCamera;
		CameraLookAt = this.GlobalPosition;
		RayCast = GetNode<RayCast3D>("RayCast3D");
		Camera = GetNode<Camera3D>("CameraPivotH/CameraPivotV/CameraSpringArm/Camera3D");
	}

    public override void _Input(InputEvent @event){
		if(@event is InputEventMouseMotion mouseMotion){
			CameraPivotH.RotateY(-Mathf.DegToRad(mouseMotion.Relative.X) * MouseSensivity);
			CameraPivotV.RotateZ(-Mathf.DegToRad(mouseMotion.Relative.Y) * MouseSensivity);
			CameraPivotV.RotationDegrees = new Vector3(0, 0, Mathf.Clamp(CameraPivotV.RotationDegrees.Z, minPitch, maxPitch));
			RecenterCameraTimer.WaitTime = 0.7f;
			RecenterCameraTimer.Start();
		}
    }

    public override void _PhysicsProcess(double delta){
		bool isFrontWheelsTouchingGround = FrontLeftWheel.IsInContact() || FrontRightWheel.IsInContact();
		bool isBackWheelsTouchingGround = BackLeftWheel.IsInContact() || BackRightWheel.IsInContact();
		float steeringAxis = Input.GetAxis("Right","Left");
		float steeringSpeed = (float)delta * 0.4f;
		int speed = GetCurrentSpeed();

		//Weigh Distribution
		ManageWeighDistribution(isFrontWheelsTouchingGround, isBackWheelsTouchingGround, delta);

		//Thrust management
		if (Input.IsActionJustPressed("Forward") && speed < MAX_THRUSTING_SPEED && canThrust && thrustStamina > 0 && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			ThrustCooldown.Start();
			DecreaseStamina();
			canThrust = false;
		} else {
			this.EngineForce = 0;
		}
		if (ThrustCooldown.TimeLeft > 1 && speed < MAX_THRUSTING_SPEED && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			this.EngineForce = ENGINE_POWER;
			Camera.Fov = (float)Mathf.Lerp(Camera.Fov, FOV_MAX, delta * 4);
		} else {
			Camera.Fov = (float)Mathf.Lerp(Camera.Fov, FOV_MIN, delta * 1);
		}

		//Brake management
		if (Input.IsActionPressed("Backward") && !Input.IsActionPressed("Forward") && isFrontWheelsTouchingGround && isBackWheelsTouchingGround){
			this.Brake = BRAKE_POWER;
		} else {
			this.Brake = 0;
		}

		//Steering
		if (!isFrontWheelsTouchingGround || !isBackWheelsTouchingGround){
			steeringAxis = 0;
		}
		ManageSteering(steeringAxis, steeringSpeed);

		//Camera recenter
		if(recenterCamera){
			RecenterCamera(delta);
		}
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

	private void ManageWeighDistribution(bool isFrontWheelsTouchingGround, bool isBackWheelsTouchingGround, double delta){
		if (isFrontWheelsTouchingGround){
			if (timeBalancing > 5){
				BackBalancingCooldown.Start();
			}
			timeBalancing = 0;
		}

		if (Input.IsActionPressed("Ctrl") && isBackWheelsTouchingGround && !(BackBalancingCooldown.TimeLeft > 0)){
			timeBalancing += 1;
			if (Input.IsActionPressed("Right")){
				this.RotateObjectLocal(new Vector3(0,1,0), -0.03f);
			} else if (Input.IsActionPressed("Left")){
				this.RotateObjectLocal(new Vector3(0,1,0), 0.03f);
			}
		}

		if (Input.IsActionPressed("Ctrl") && GetDistanceFromGround() < 0.25f && timeBalancing < MAX_BALANCING_TIME && isBackWheelsTouchingGround && timeBalancing < MAX_BALANCING_TIME && !(BackBalancingCooldown.TimeLeft > 0)){
			this.CenterOfMass = new Vector3(-0.2f,0,0);
		} else {
			if (this.CenterOfMass != new Vector3(0.4f,0,0)){
				this.CenterOfMass = new Vector3(0.4f,0,0);
			}
		}
	}

	//Manage recentering
	private void RecenterCamera(double delta){
		if(CameraPivotH.Rotation.Y != 0){
				float HorizontalRotation = (float)Mathf.MoveToward(CameraPivotH.Rotation.Y, 0, delta * 6);
				CameraPivotH.Rotation = new Vector3(0, HorizontalRotation, 0);
			}
			if (CameraPivotV.Rotation.Z != 0){
				float VerticalRotation = (float)Mathf.MoveToward(CameraPivotV.Rotation.Z, 0, delta * 6);
				CameraPivotV.Rotation = new Vector3(0, 0, VerticalRotation);
			}
			if (CameraPivotH.Rotation.Y == 0 && CameraPivotV.Rotation.Z == 0){
				recenterCamera = false;
			}
	}

	private float GetDistanceFromGround(){
		float distanceFromGround = 0f;
		if (RayCast.IsColliding()){
			Vector3 collisionPoint = RayCast.GetCollisionPoint();
			distanceFromGround = RayCast.GlobalTransform.Origin.DistanceTo(collisionPoint);
		}
		return distanceFromGround;
	}

	private int GetCurrentSpeed(){
		Vector3 currentVelocity = this.LinearVelocity * this.Transform.Basis;
		int speed = (int)(currentVelocity.Length() * 3.6);
		Speedometer.Text = (speed).ToString() + " Km/h";
		return speed;
	}

	private void StaminaReload(){
		if (thrustStamina < MAX_THRUST_STAMINA){
			thrustStamina += 1;
			UpdateStaminaLabel();
			if (thrustStamina < MAX_THRUST_STAMINA){
				StaminaCooldown.Start();
			}
		}
	}

	private void DecreaseStamina(){
		if (thrustStamina > 0){
			thrustStamina -= 1;
			UpdateStaminaLabel();
			StaminaCooldown.Start();
		}
	}

	private void UpdateStaminaLabel(){
		StaminaLabel.Text = thrustStamina.ToString();
	}

	private void TiggerRecenterCamera(){
		recenterCamera = true;
	}

	private void ResetThrustCapability(){
		canThrust = true;
	}
}
