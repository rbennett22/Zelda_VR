#pragma strict

// switches between fast and slow movement when "r" is pressed

public var motor : CharacterMotor;
private var speedOn : boolean;

function Update () {
	if (Input.GetKeyDown ("r")) {
		if (speedOn) {
			motor.movement.maxForwardSpeed = 4;
			motor.movement.maxBackwardsSpeed = 4;
			motor.movement.maxSidewaysSpeed = 4;
			motor.jumping.baseHeight = 0.5;
			
			speedOn = false;
		}
		else {
			motor.movement.maxForwardSpeed = 15;
			motor.movement.maxBackwardsSpeed = 15;
			motor.movement.maxSidewaysSpeed = 15;
			motor.jumping.baseHeight = 1.5;
			
			speedOn = true;
		}
		
	}
}