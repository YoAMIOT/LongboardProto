using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER_ANGLE= 0.08f;
	private const float MAX_BOARD_ANGLE = MAX_STEER_ANGLE * -2;
	private const float ENGINE_POWER = 5;
	private const float BRAKE_POWER = 0.1f;
	private const int MAX_THRUSTING_SPEED = 20;
	private const int MAX_BALANCING_TIME = 20;
	private VehicleWheel3D FrontRightWheel;
	private VehicleWheel3D FrontLeftWheel;
	private VehicleWheel3D BackRightWheel;
	private VehicleWheel3D BackLeftWheel;
	private MeshInstance3D Board;
	private Timer ThrustCooldown;
	private Label Speedometer;
	private RayCast3D RayCast;
	private ProgressBar StaminaBar;
	private Timer BackBalancingCooldown;
	private Control Hud;
	private float MouseSensivity = 0.05f;
	private bool canThrust = true;
	private bool hasStamina = true;
	private int timeBalancing = 0;
	private bool isInUse = false;

	public override void _Ready(){
		BackBalancingCooldown = GetNode<Timer>("BackBalancingCooldown");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
		ThrustCooldown.Timeout += ResetThrustCapability;
		RayCast = GetNode<RayCast3D>("RayCast3D");
		Hud = GetNode<Control>("HUD");
		Speedometer = GetNode<Label>("HUD/Speedometer");
		StaminaBar = GetNode<ProgressBar>("HUD/StaminaBar");
		StaminaBar.MaxValue = ThrustCooldown.WaitTime;
	}

    public override void _PhysicsProcess(double delta){
		if (isInUse){
			bool isFrontWheelsTouchingGround = FrontLeftWheel.IsInContact() || FrontRightWheel.IsInContact();
			bool isBackWheelsTouchingGround = BackLeftWheel.IsInContact() || BackRightWheel.IsInContact();
			float steeringAxis = Input.GetAxis("Right","Left");
			float steeringSpeed = (float)delta * 0.4f;
			int speed = GetCurrentSpeed();

			//Weigh Distribution
			ManageWeighDistribution(isFrontWheelsTouchingGround, isBackWheelsTouchingGround, delta);

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
			} else if (speed >= 60){
				this.Brake = 0.06f;
			} else {
				this.Brake = 0;
			}

			//Steering
			if (!isFrontWheelsTouchingGround || !isBackWheelsTouchingGround){
				steeringAxis = 0;
			}
			ManageSteering(steeringAxis, steeringSpeed);

			//Update stamina bar
			UpdateStaminaBar();
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

	private void ResetThrustCapability(){
		canThrust = true;
	}

	private void UpdateStaminaBar(){
		StaminaBar.Value = ThrustCooldown.TimeLeft;
	}

	public void SetInUse(bool inUse){
		this.isInUse = inUse;
		Hud.Visible = inUse;
	}
}
