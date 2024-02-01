using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER = 0.21f;
	private const float MAX_BOARD_ANGLE = MAX_STEER * 2;
	private const float ENGINE_POWER = 10;
	private const float BRAKE_POWER = 0.08f;
	private Node3D CameraPivot;
	private Camera3D Camera;
	private VehicleWheel3D FrontRightWheel;
	private VehicleWheel3D FrontLeftWheel;
	private VehicleWheel3D BackRightWheel;
	private VehicleWheel3D BackLeftWheel;
	private MeshInstance3D Board;
	private Timer ThrustCooldown;
	private Vector3 CameraLookAt;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivot = GetNode<Node3D>("CameraPivot");
		Camera = GetNode<Camera3D>("CameraPivot/Camera3D");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
		CameraLookAt = this.GlobalPosition;
	}

	public override void _PhysicsProcess(double delta){
		float STEERING_SPEED = (float)delta * 0.8f;
		float currentSteering = (float)Mathf.MoveToward(FrontRightWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER, STEERING_SPEED);
		FrontLeftWheel.Steering = currentSteering;
		FrontRightWheel.Steering = currentSteering;
		BackLeftWheel.Steering = currentSteering * -1;
		BackRightWheel.Steering = currentSteering * -1;
		float BoardRotation =  (float)Mathf.MoveToward(Board.Rotation.X, Input.GetAxis("Left","Right") * MAX_BOARD_ANGLE, STEERING_SPEED * 2);
		Board.Rotation = new Vector3(BoardRotation, 0, 0);
		
		//RacingGame camera (Does not support reverse)
		CameraPivot.GlobalPosition = CameraPivot.GlobalPosition.Lerp(this.GlobalPosition, (float)delta * 20);
		CameraPivot.Transform = CameraPivot.Transform.InterpolateWith(this.Transform, (float)delta * 6);
		CameraLookAt = CameraLookAt.Lerp(this.GlobalPosition + this.LinearVelocity, (float)delta * 6);
		Camera.LookAt(CameraLookAt);

		//Thrust management
		if (ThrustCooldown.TimeLeft > 1f && Input.IsActionPressed("Forward")){
			this.EngineForce = ENGINE_POWER;
			GetNode<MeshInstance3D>("Cap").Visible = true;
		} else {
			this.EngineForce = 0;
			GetNode<MeshInstance3D>("Cap").Visible = false;
		}
		
		if (Input.IsActionPressed("Backward") && !Input.IsActionPressed("Forward")){
			this.Brake = BRAKE_POWER;
		} else {
			this.Brake = 0;
		}
	}
}
