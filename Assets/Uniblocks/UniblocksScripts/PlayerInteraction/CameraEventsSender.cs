using UnityEngine;
using System.Collections;

// sends VoxelEvents such as OnLook, OnMouseDown, etc.

namespace Uniblocks {

public class CameraEventsSender : MonoBehaviour {
	
	public float Range;
	private GameObject SelectedBlockGraphics;
	
	public void Awake () {
		if (Range <= 0) {
			Debug.LogWarning ("Uniblocks: CameraEventSender.Range must be greater than 0. Setting Range to 5.");
			Range = 5.0f;
		}
		
		SelectedBlockGraphics = GameObject.Find ("selected block graphics");
	}
	
	public void Update () {
	
		if (Engine.SendCameraLookEvents) {
			CameraLookEvents();
		}
		if (Engine.SendCursorEvents) {
			MouseCursorEvents();
		}		
	}
	
	private void MouseCursorEvents () { // cursor position
	
		//Vector3 pos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10.0f);
		VoxelInfo raycast = Engine.VoxelRaycast ( Camera.main.ScreenPointToRay(Input.mousePosition), 9999.9f, false );
		
		if (raycast != null) {
			
			// create a local copy of the hit voxel so we can call functions on it
			GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (raycast.GetVoxel()) ) as GameObject;
			
			// only execute this if the voxel actually has any events (either VoxelEvents component, or any component that inherits from it)
			if (voxelObject.GetComponent<VoxelEvents>() != null) {
				
				voxelObject.GetComponent<VoxelEvents>().OnLook(raycast);
				
				// for all mouse buttons, send events
				for (int i=0; i<3; i++) {
					if (Input.GetMouseButtonDown(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseDown(i, raycast);
					}
					if (Input.GetMouseButtonUp(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseUp(i, raycast);
					}		
					if (Input.GetMouseButton(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseHold(i, raycast);
					}
				}
			}
			
			Destroy(voxelObject);
		
		}
		
		else {
			// disable selected block ui when no block is hit
			
			if (SelectedBlockGraphics != null) {
				SelectedBlockGraphics.GetComponent<Renderer>().enabled = false;
			}		
		}
	
	}
	
	private void CameraLookEvents () { // first person camera
			
		VoxelInfo raycast = Engine.VoxelRaycast ( Camera.main.transform.position, Camera.main.transform.forward, Range, false );
		
		if (raycast != null) {
			
			// create a local copy of the hit voxel so we can call functions on it
			GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (raycast.GetVoxel()) ) as GameObject;
			
			// only execute this if the voxel actually has any events (either VoxelEvents component, or any component that inherits from it)
			if (voxelObject.GetComponent<VoxelEvents>() != null) {
				
				voxelObject.GetComponent<VoxelEvents>().OnLook(raycast);			
				
				// for all mouse buttons, send events
				for (int i=0; i<3; i++) {
					if (Input.GetMouseButtonDown(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseDown(i, raycast);
					}
					if (Input.GetMouseButtonUp(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseUp(i, raycast);
					}		
					if (Input.GetMouseButton(i)) {
						voxelObject.GetComponent<VoxelEvents>().OnMouseHold(i, raycast);
					}
				}
			}
			
			Destroy (voxelObject);
				
		}
		
		else {
			// disable selected block ui when no block is hit
			if (SelectedBlockGraphics != null) {
				SelectedBlockGraphics.GetComponent<Renderer>().enabled = false;
			}		
		}
	}



}

}