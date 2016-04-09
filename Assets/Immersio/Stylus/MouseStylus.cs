using UnityEngine;


public class MouseStylus : Stylus
{

    public Camera activeCamera;


    protected override bool StylusRayIsAvailable
    {
        get { return true; }
    }

    protected override Ray GetStylusRay()
    {
        return activeCamera.ScreenPointToRay(Input.mousePosition);
    }


    public override bool GetStylusButtonUp(int idx = 0)
    {
		return Input.GetMouseButtonUp(idx);
	}
    public override bool GetStylusButtonDown(int idx = 0)
    {
		return Input.GetMouseButtonDown(idx);
	}
    public override bool GetStylusButton(int idx = 0)
    {
		return Input.GetMouseButton(idx);
	}


    void Start()
    {
        if (activeCamera == null) { activeCamera = Camera.main; }
    }
}
