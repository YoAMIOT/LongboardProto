using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float SPEED = 5.0f;
	private const float JUMP_VELOCITY = 4.5f;
	private const float LERP_VALUE = 0.15f;
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private Node3D CamPivotH;
	private Node3D Armature;
	private AnimationTree AnimTree;

	public override void _Ready(){
		CamPivotH = GetNode<Node3D>("CameraPivotH");
		Armature = GetNode<Node3D>("Armature");
		AnimTree = GetNode<AnimationTree>("AnimationTree");
	}

	public override void _PhysicsProcess(double delta){
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor()){
			velocity.Y -= gravity * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("Spacebar") && IsOnFloor()){
			velocity.Y = JUMP_VELOCITY;
		}

		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		direction = direction.Rotated(Vector3.Up, Mathf.DegToRad(CamPivotH.RotationDegrees.Y));
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
	}
}
