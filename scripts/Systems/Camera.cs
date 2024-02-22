using Godot;
using System;

public partial class Camera : Node3D{
	private const int MIN_PITCH = -50;
	private const int MAX_PITCH = 30;
	private float MouseSensivity = 0.05f;
	private bool recenterCamera = false;
	private string parentName = "";
	private Node3D CameraPivotV;
	private Camera3D Cam;
	private Timer RecenterCameraTimer;
	private RayCast3D InteractionRayCast;
	private Label InteractionLabel;

	[Signal]
	public delegate void InteractedWithEventHandler(Node Target);

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivotV = GetNode<Node3D>("CameraPivotV");
		Cam = GetNode<Camera3D>("CameraPivotV/CameraSpringArm/Camera3D");
		RecenterCameraTimer = GetNode<Timer>("ReCenterCamera");
		RecenterCameraTimer.Timeout += TiggerRecenterCamera;
		InteractionRayCast = GetNode<RayCast3D>("CameraPivotV/CameraSpringArm/RayCast3D");
		InteractionLabel = GetNode<Label>("HUD/InteractionLabel");
		parentName = GetParent().Name;
	}

	public override void _Input(InputEvent @event){
		if(@event is InputEventMouseMotion mouseMotion){
			this.RotateY(-Mathf.DegToRad(mouseMotion.Relative.X) * MouseSensivity);
			CameraPivotV.RotateX(-Mathf.DegToRad(mouseMotion.Relative.Y) * MouseSensivity);
			CameraPivotV.RotationDegrees = new Vector3(Mathf.Clamp(CameraPivotV.RotationDegrees.X, MIN_PITCH, MAX_PITCH), 0, 0);
			RecenterCameraTimer.WaitTime = 0.7f;
			RecenterCameraTimer.Start();
		}
    }

	public override void _Process(double delta){
		if(recenterCamera && parentName == "Longboard"){
			RecenterCamera(delta);
		}
		if (InteractionRayCast.IsColliding()){
			Node TargetObject = ((Node3D)InteractionRayCast.GetCollider()).GetParent();
			InteractionLabel.Text = TargetObject.Name;
			if (Input.IsActionJustPressed("Interact")){
				EmitSignal(nameof(this.InteractedWith), TargetObject);
			}
		} else {
			InteractionLabel.Text = "";
		}
	}

	private void TiggerRecenterCamera(){
		recenterCamera = true;
	}

	//Manage recentering
	private void RecenterCamera(double delta){
		if(this.RotationDegrees.Y != 0){
			float HorizontalRotation = (float)Mathf.MoveToward(this.RotationDegrees.Y, 0, delta * 120);
			this.RotationDegrees = new Vector3(0, HorizontalRotation, 0);
		}
		if (CameraPivotV.RotationDegrees.Z != 0){
			float VerticalRotation = (float)Mathf.MoveToward(CameraPivotV.RotationDegrees.Z, 0, delta * 120);
			CameraPivotV.RotationDegrees = new Vector3(0, 0, VerticalRotation);
		}
		if (this.RotationDegrees.Y == 0 && CameraPivotV.RotationDegrees.Z == 0){
			recenterCamera = false;
		}
	}
}
