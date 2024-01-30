using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER = 0.25f;
	private const float ENGINE_POWER = 3;
	private Node3D CameraPivot;
	private Camera3D Camera;
	private VehicleWheel3D FrontRightWheel;
	private VehicleWheel3D FrontLeftWheel;
	private VehicleWheel3D BackRightWheel;
	private VehicleWheel3D BackLeftWheel;
	private MeshInstance3D Board;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivot = GetNode<Node3D>("CameraPivot");
		Camera = GetNode<Camera3D>("CameraPivot/Camera");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
	}

	public override void _PhysicsProcess(double delta){
		float coef = (float)delta * 1.2f;
		FrontRightWheel.Steering = (float)Mathf.MoveToward(FrontRightWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER, coef);
		FrontLeftWheel.Steering = (float)Mathf.MoveToward(FrontLeftWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER, coef);
		BackRightWheel.Steering = (float)Mathf.MoveToward(BackRightWheel.Steering, Input.GetAxis("Left","Right") * MAX_STEER, coef);
		BackLeftWheel.Steering = (float)Mathf.MoveToward(BackLeftWheel.Steering, Input.GetAxis("Left","Right") * MAX_STEER, coef);
		float BoardRotation =  (float)Mathf.MoveToward(Board.Rotation.X, Input.GetAxis("Left","Right") * MAX_STEER, coef);
		Board.Rotation = new Vector3(BoardRotation, 0, 0);
		this.EngineForce = Input.GetAxis("Backward", "Forward") * ENGINE_POWER;
		CameraPivot.GlobalPosition = CameraPivot.GlobalPosition.Lerp(this.GlobalPosition, (float)delta * 20);

	}
}
