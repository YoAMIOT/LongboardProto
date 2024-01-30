using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const int MAX_STEER = 10;
	private const float ENGINE_POWER = 3;
	private Node3D CameraPivot;
	private Camera3D Camera;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivot = GetNode<Node3D>("CameraPivot");
		Camera = GetNode<Camera3D>("CameraPivot/Camera");
	}

	public override void _PhysicsProcess(double delta){
		this.Steering = (float)Mathf.MoveToward(this.Steering, Input.GetAxis("Right","Left") * MAX_STEER, delta * 2.5);
		this.EngineForce = Input.GetAxis("Backward", "Forward") * ENGINE_POWER;
		CameraPivot.GlobalPosition = CameraPivot.GlobalPosition.Lerp(this.GlobalPosition, (float)delta * 20);
	}
}
