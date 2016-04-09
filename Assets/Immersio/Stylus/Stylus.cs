using UnityEngine;
using System.Collections;
using Vectrosity;
using Immersio.Utility;


public abstract class Stylus : Singleton<Stylus>
{

    #region Abstract Methods

    abstract protected bool StylusRayIsAvailable { get; }
    abstract protected Ray GetStylusRay();

    // These methods are the stylus equivalent of their "GetMouseButton..." counterparts
    abstract public bool GetStylusButtonUp(int idx = 0);
    abstract public bool GetStylusButtonDown(int idx = 0);
    abstract public bool GetStylusButton(int idx = 0);

    #endregion


    public const float RayCastLength = 99999;


	public enum StylusEvent 
    {
		OnStylusOver,
		OnStylusEnter,
		OnStylusExit,
		OnStylusDown,
		OnStylusUp,
		OnStylusUpAsButton,
		OnStylusDrag
	}


    public Ray StylusRay { get; protected set; }
    public Ray PrevStylusRay { get; private set; }
    public RaycastHit StylusRayHit { get; private set; }
    public RaycastHit PrevStylusRayHit { get; private set; }

    public GameObject HitObject { get; protected set; }
    public GameObject PrevHitObject { get; private set; }
    public GameObject ObjStylusBtnWasPressedOver { get; private set; }


	void Start () 
    {
        InitRenderedRay();
	}


    #region Update

    virtual protected void Update () 
    {
        PrevStylusRay = StylusRay;
		PrevStylusRayHit = StylusRayHit;
		PrevHitObject = HitObject;

        StylusRay = GetStylusRay();
        PerformRayCast();

		DispatchStylusEvents();

        if (doRender) { RenderRay(); }
	}

    void PerformRayCast()
    {
        if (!StylusRayIsAvailable)
        {
            StylusRayHit = new RaycastHit();
            HitObject = null;
            return;
        }

        RaycastHit hit;
        bool didHit = Physics.Raycast(StylusRay, out hit);
        StylusRayHit = hit;
        HitObject = didHit ? StylusRayHit.collider.gameObject : null;
    }

	// These events are the Stylus equivalent of their "OnMouse..." counterparts)
	void DispatchStylusEvents () 
    {
		if(HitObject != null) 
        {
			SendStylusEventToObject(StylusEvent.OnStylusOver, HitObject);
			if(HitObject != PrevHitObject) 
            {
				SendStylusEventToObject(StylusEvent.OnStylusEnter, HitObject);
			}
		}

		if(PrevHitObject != null && PrevHitObject != HitObject)
        {
			SendStylusEventToObject(StylusEvent.OnStylusExit, PrevHitObject);
		}

		if(GetStylusButtonDown()) 
        {
            ObjStylusBtnWasPressedOver = HitObject;
            if (HitObject != null)
            {
                SendStylusEventToObject(StylusEvent.OnStylusDown, HitObject);
            }
		}
		else if(GetStylusButtonUp()) 
        {
			if(HitObject != null) 
            {
				SendStylusEventToObject(StylusEvent.OnStylusUp, HitObject);
			}

            if (ObjStylusBtnWasPressedOver != null)
            {
                SendStylusEventToObject(StylusEvent.OnStylusUpAsButton, ObjStylusBtnWasPressedOver);
            }
		}

		if(GetStylusButton()) 
        {
			if(ObjStylusBtnWasPressedOver != null) 
            {
                if (StylusMoved()) 
                {
					SendStylusEventToObject(StylusEvent.OnStylusDrag, ObjStylusBtnWasPressedOver);
				}
			}
		}
	}

    bool StylusMoved()
    {
        return (
            StylusRay.direction != PrevStylusRay.direction ||
            StylusRay.origin != PrevStylusRay.origin);
    }

	void SendStylusEventToObject (StylusEvent stylusEvent, GameObject receiver, bool doPrint = false) 
    {
		if(doPrint) { print(stylusEvent.ToString() + " sent to " + receiver.name); }

        receiver.SendMessage(stylusEvent.ToString(), SendMessageOptions.DontRequireReceiver);
    }

    #endregion


    #region Rendering

    public bool doRender = true;
    public Color rayColor = Color.green;

    protected const float LineWidth = 2.0f;
    protected VectorLine _renderedRay;
    protected Vector3[] laserPoints;

    virtual protected void InitRenderedRay()
    {
        laserPoints = new Vector3[] { Vector3.zero, Vector3.zero };
        _renderedRay = new VectorLine("renderedRay", laserPoints, rayColor, null, LineWidth);
    }

    virtual protected void RenderRay()
    {
        if (_renderedRay == null) { InitRenderedRay(); }

        float rayLength = 0;
        if (StylusRayIsAvailable)
        {
            rayLength = (HitObject == null) ? RayCastLength : StylusRayHit.distance;
        }

        laserPoints[0] = StylusRay.origin;
        laserPoints[1] = StylusRay.origin + StylusRay.direction * rayLength;

        _renderedRay.Draw3D();

        //print("RenderRay --- start: " + startPos + ", end: " + endPos);
    }

    #endregion

}
