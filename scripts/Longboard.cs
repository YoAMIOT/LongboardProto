using Godot;
using System;

public partial class Longboard : VehicleBody3D{
	private const float MAX_STEER_ANGLE= 0.08f;
	private const float MAX_BOARD_ANGLE = MAX_STEER_ANGLE * 2;
	private const float ENGINE_POWER = 8;
	private const float BRAKE_POWER = 0.05f;
	private const int MAX_THRUSTING_SPEED = 16;
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
	private Label Speedometer;
	private float MouseSensivity = 0.05f;
	private int minPitch = -50;
	private int maxPitch = 30;
	private Timer RecenterCameraTimer;
	private bool recenterCamera = false;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivotH = GetNode<Node3D>("CameraPivotH");
		CameraPivotV = GetNode<Node3D>("CameraPivotH/CameraPivotV/");
		FrontRightWheel = GetNode<VehicleWheel3D>("FrontRight");
		FrontLeftWheel = GetNode<VehicleWheel3D>("FrontLeft");
		BackRightWheel = GetNode<VehicleWheel3D>("BackRight");
		BackLeftWheel = GetNode<VehicleWheel3D>("BackLeft");
		Board = GetNode<MeshInstance3D>("Board");
		ThrustCooldown = GetNode<Timer>("ThrustCooldown");
		Speedometer = GetNode<Label>("HUD/Speedometer");
		RecenterCameraTimer = GetNode<Timer>("ReCenterCamera");
		RecenterCameraTimer.Timeout += TiggerRecenterCamera;
		CameraLookAt = this.GlobalPosition;
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
		//Calculate speed
		Vector3 currentVelocity = this.LinearVelocity * this.Transform.Basis;
		int speed = (int)(currentVelocity.Length() * 3.6);
		Speedometer.Text = (speed).ToString() + " Km/h";

		//Thrust management
		if (ThrustCooldown.TimeLeft > 1f && Input.IsActionPressed("Forward") && speed < MAX_THRUSTING_SPEED){
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

		//Steering
		float STEERING_SPEED = (float)delta * 0.4f;
		float currentSteering = (float)Mathf.MoveToward(FrontRightWheel.Steering, Input.GetAxis("Right","Left") * MAX_STEER_ANGLE, STEERING_SPEED);
		FrontLeftWheel.Steering = currentSteering;
		FrontRightWheel.Steering = currentSteering;
		BackLeftWheel.Steering = currentSteering * -1;
		BackRightWheel.Steering = currentSteering * -1;
		float BoardRotation =  (float)Mathf.MoveToward(Board.Rotation.X, Input.GetAxis("Left","Right") * MAX_BOARD_ANGLE, STEERING_SPEED * 2);
		Board.Rotation = new Vector3(BoardRotation, 0, 0);

		if(recenterCamera){
			RecenterCamera(delta);
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

	private void TiggerRecenterCamera(){
		recenterCamera = true;
	}
}
