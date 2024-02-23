using Godot;
using System;

public partial class Camera : Node3D{
	private const int MIN_PITCH = -50;
	private const int MAX_PITCH = 30;
	private Node3D CameraPivotV;
	private Camera3D Cam;
	private RayCast3D InteractionRayCast;
	private Label InteractionLabel;
	private Player Player;
	private float MouseSensivity = 0.05f;

	public override void _Ready(){
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraPivotV = GetNode<Node3D>("CameraPivotV");
		Cam = GetNode<Camera3D>("CameraPivotV/CameraSpringArm/Camera3D");
		InteractionRayCast = GetNode<RayCast3D>("CameraPivotV/CameraSpringArm/RayCast3D");
		InteractionLabel = GetNode<Label>("HUD/InteractionLabel");
		Player = GetParent<Player>();
	}

	public override void _Input(InputEvent @event){
		if(@event is InputEventMouseMotion mouseMotion){
			this.RotateY(-Mathf.DegToRad(mouseMotion.Relative.X) * MouseSensivity);
			CameraPivotV.RotateX(-Mathf.DegToRad(mouseMotion.Relative.Y) * MouseSensivity);
			CameraPivotV.RotationDegrees = new Vector3(Mathf.Clamp(CameraPivotV.RotationDegrees.X, MIN_PITCH, MAX_PITCH), 0, 0);
		}
    }

	public override void _Process(double delta){
		if (InteractionRayCast.IsColliding() && !Player.onBoard){
			Node3D TargetObject = (Node3D)((Node3D)InteractionRayCast.GetCollider()).GetParent();
			InteractionLabel.Text = TargetObject.Name;
			if (Input.IsActionJustPressed("Interact")){
				Player.InteractWith(TargetObject);
			}
		} else {
			InteractionLabel.Text = "";
		}
	}
}
