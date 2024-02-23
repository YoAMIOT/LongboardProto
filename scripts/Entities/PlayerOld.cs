using Godot;
using System;

public partial class PlayerOld : CharacterBody3D {
	private const float SPEED = 3f;
	private const float LERP_VALUE = 0.15f;
	private Camera Camera;
	private Node3D Armature;
	private AnimationTree AnimTree;
	private Node3D LastInteractedObject;
	private CollisionShape3D Collision;
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public bool onBoard = false;
	private bool canInteract = true;

	public override void _Ready(){
		Camera = GetNode<Camera>("CameraPivotH");
		Armature = GetNode<Node3D>("Armature");
		AnimTree = GetNode<AnimationTree>("AnimationTree");
		Collision = GetNode<CollisionShape3D>("Collision");
	}

	public override void _PhysicsProcess(double delta){
		Vector3 velocity = Velocity;
		if (!onBoard){
			if (!IsOnFloor()){
				velocity.Y -= gravity * (float)delta;
			}

			Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			direction = direction.Rotated(Vector3.Up, Mathf.DegToRad(Camera.RotationDegrees.Y));
			if (direction != Vector3.Zero){
				velocity.X = Mathf.Lerp(velocity.X, direction.X * SPEED, LERP_VALUE);
				velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * SPEED, LERP_VALUE);
				Armature.Rotation = new Vector3(0, Mathf.LerpAngle(Armature.Rotation.Y, Mathf.Atan2(-velocity.X, -velocity.Z), LERP_VALUE), 0);
			} else {
				velocity.X = Mathf.Lerp(velocity.X, 0f, LERP_VALUE);
				velocity.Z = Mathf.Lerp(velocity.Z, 0f, LERP_VALUE);
			}
		} else {
			velocity = new Vector3();
			Node3D LongboardMarker = (Node3D)LastInteractedObject.GetChild(0);
			Armature.Rotation = new Vector3(LongboardMarker.Rotation.X, Mathf.LerpAngle(Armature.Rotation.Y, LongboardMarker.Rotation.Y - Mathf.DegToRad(90f), LERP_VALUE), LongboardMarker.Rotation.Z);
			this.GlobalTransform = LongboardMarker.GlobalTransform;
			if (Input.IsActionJustPressed("Interact") && canInteract){
				InteractWith(LastInteractedObject);
			}
		}
		AnimTree.Set("parameters/BlendSpace1D/blend_position", velocity.Length() /SPEED);
		Velocity = velocity;
		MoveAndSlide();
	}

	public async void InteractWith(Node3D Target){
		LastInteractedObject = Target;
		if (LastInteractedObject.GetType() == typeof(Longboard)){
			if(!onBoard){
				canInteract = false;
				Collision.Disabled = true;
				Target.Call("SetInUse", true);
				onBoard = true;
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				canInteract = true;
			} else {
				Collision.Disabled = false;
				Target.Call("SetInUse", false);
				this.GlobalRotation = new Vector3();
				onBoard = false;
			}
		}
	}
}
