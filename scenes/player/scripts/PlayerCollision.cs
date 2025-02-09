using Godot;

public partial class PlayerCollision : CharacterBody3D
{
    // The collision normal of the player on the ground. Or null if the player is not touching the ground.
    public Vector3? floorCollisionNormal = null;

    // The ball 3D model.
    private Ball ball;

    private float rotateLerpT = 0;
    private Vector3 prevInput = Vector3.Zero;

    private PrintEvery printer;

    private Vector3 GRAVITY = new Vector3(0, -9.8f, 0);

    public override void _Ready()
    {
        this.ball = GetNode<Ball>("ball");
        this.printer = new PrintEvery(10);
    }

    public override void _Process(double delta)
    {
		// Open ball
		if (Input.IsActionPressed("open_ball") && this.floorCollisionNormal == null && !this.ball.isOpen) {
			this.ball.Open();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        //this.Sleeping = false;

		var forwardStrength = Input.GetActionStrength("player_forward") - Input.GetActionStrength("player_backward");
		var leftStrength = Input.GetActionStrength("player_left") - Input.GetActionStrength("player_right");

        var currentInput = new Vector3(leftStrength, 0, forwardStrength);

        // Movement equations
        var vPrev = Velocity;
        var accel = new Vector3();

        // ... Contact based movement
        var lastColl = GetLastSlideCollision();
        
        if (lastColl != null && lastColl.GetCollisionCount() > 0) {
            // When contacting
            if (this.ball.isOpen) {
                // When open: face planted on something
                // Don't move
                printer.Print("Colliding + Open = Face Planted");
            } else {
                // When closed: Rolling
                // Account for user input
                accel += currentInput * new Vector3(1, 1, 4);

                // Roll up and down slopes + gravity
                var rollDirection = (lastColl.GetNormal().Normalized() + GRAVITY.Normalized()).Normalized();
                accel += GRAVITY * rollDirection;
                printer.Print("Colliding + Closed = Rolling");
            }
        } else {
            // When not contacting
            floorCollisionNormal = null;

            if (this.ball.isOpen) {
                // When open: Flying
                printer.Print("Not Colliding + Open = Flying");
            } else {
                // When closed: In free fall mode
                accel += GRAVITY;

                printer.Print("Not Colliding + Closed = Free Falling");
            }
        }

        // Update new velocity
        Velocity = vPrev + (accel * (float)delta);

        MoveAndSlide();

        printer.Print("accel=" + accel);

        prevInput = currentInput;
    }

   /*  public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        // Detect collision with floor
        if (state.GetContactCount() > 0)
        {
            this.floorCollisionNormal = state.GetContactLocalNormal(0);
        } else {
			this.floorCollisionNormal = null;
		}
    } */
}
