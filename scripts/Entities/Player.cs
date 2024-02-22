using Godot;
using System;

public partial class Player : CharacterBody3D {
	private const float SPEED = 3f;
	private const float LERP_VALUE = 0.15f;
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private Camera Camera;
	private Node3D Armature;
	private AnimationTree AnimTree;
	private Node3D LastInteractedObject;
	private CollisionShape3D Collision;
	private bool onBoard = false; 


	public override void _Ready(){
		Camera = GetNode<Camera>("CameraPivotH");
		Armature = GetNode<Node3D>("Armature");
		AnimTree = GetNode<AnimationTree>("AnimationTree");
		Collision = GetNode<CollisionShape3D>("Collision");
		Camera.InteractedWith += InteractWith;
	}

	public override void _PhysicsProcess(double delta){
		if (!onBoard){
			Vector3 velocity = Velocity;

			// Add the gravity.
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

			AnimTree.Set("parameters/BlendSpace1D/blend_position", velocity.Length() /SPEED);

			Velocity = velocity;
			MoveAndSlide();
		} else {
			Node3D LongboardMarker = (Node3D)LastInteractedObject.GetChild(0);
			this.GlobalTransform = LongboardMarker.GlobalTransform;
		}
	}

	private void InteractWith(Node3D Target){
		LastInteractedObject = Target;
		if (LastInteractedObject.GetType() == typeof(Longboard)){
			onBoard = true;
			Collision.Disabled = true;
		}
	}
}
