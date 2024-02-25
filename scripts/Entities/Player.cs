using Godot;
using System;

public partial class Player : RigidBody3D {
	private const float SPEED = 4f;
	private const float LERP_VALUE = 0.15f;
	private Node3D Armature;
	private Camera Camera;
	private AnimationTree AnimTree;
	private CollisionShape3D Collision;
	private Node3D LastInteractedObject;
	public bool onBoard = false;
	private bool canInteract = true;


	public override void _Ready(){
		Armature = GetNode<Node3D>("Armature");
		Camera = GetNode<Camera>("CameraPivotH");
		AnimTree = GetNode<AnimationTree>("AnimationTree");
		Collision = GetNode<CollisionShape3D>("Collision");
	}

    public override void _Process(double delta){
        if (onBoard){
			Node3D LongboardMarker = (Node3D)LastInteractedObject.GetChild(0);
			Armature.Rotation = new Vector3(LongboardMarker.Rotation.X, Mathf.LerpAngle(Armature.Rotation.Y, LongboardMarker.Rotation.Y - Mathf.DegToRad(90f), LERP_VALUE), LongboardMarker.Rotation.Z);
			this.GlobalTransform = LongboardMarker.GlobalTransform;

		}
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state){
        Vector3 velocity = LinearVelocity;
		if (!onBoard){
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
			if (Input.IsActionJustPressed("Interact") && canInteract){
				InteractWith(LastInteractedObject);
			}
		}
		AnimTree.Set("parameters/BlendSpace1D/blend_position", velocity.Length() /SPEED);
		LinearVelocity = velocity;
    }

		public async void InteractWith(Node3D Target){
		LastInteractedObject = Target;
		if (Target.GetType() == typeof(Longboard)){
			if(!onBoard){
				canInteract = false;
				Collision.Disabled = true;
				Target.Call("SetInUse", true);
				onBoard = true;
				await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
				canInteract = true;
			} else {
				Collision.SetDeferred("disabled", false);
				Target.Call("SetInUse", false);
				Node3D LongboardExitMarker = (Node3D)Target.GetChild(1);
				this.GlobalPosition = LongboardExitMarker.GlobalPosition;
				this.GlobalRotation = new Vector3();
				onBoard = false;
			}
		}
	}
}
