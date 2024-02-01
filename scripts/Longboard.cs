using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER = 0.2f;
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

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivot = GetNode<Node3D>("CameraPivot");
		Camera = GetNode<Camera3D>("CameraPivot/Camera");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
	}

	public override void _PhysicsProcess(double delta){
		float STEERING_SPEED = (float)delta * 0.8f;
		FrontRightWheel.Steering = (float)Mathf.MoveToward(FrontRightWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER, STEERING_SPEED);
		FrontLeftWheel.Steering = (float)Mathf.MoveToward(FrontLeftWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER, STEERING_SPEED);
		BackRightWheel.Steering = (float)Mathf.MoveToward(BackRightWheel.Steering, Input.GetAxis("Left","Right") * MAX_STEER, STEERING_SPEED);
		BackLeftWheel.Steering = (float)Mathf.MoveToward(BackLeftWheel.Steering, Input.GetAxis("Left","Right") * MAX_STEER, STEERING_SPEED);
		float BoardRotation =  (float)Mathf.MoveToward(Board.Rotation.X, Input.GetAxis("Left","Right") * MAX_STEER, STEERING_SPEED);
		Board.Rotation = new Vector3(BoardRotation, 0, 0);
		CameraPivot.GlobalPosition = CameraPivot.GlobalPosition.Lerp(this.GlobalPosition, (float)delta * 20);

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
