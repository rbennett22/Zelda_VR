using UnityEngine;

public interface ITemporaryState {
	bool CanActivate (TemporaryState state, ref float duration);
	bool CanDeactivate (TemporaryState state);
	void OnActivation (TemporaryState state, float duration);
	void OnDeactivation (TemporaryState state);
}

public class TemporaryState : MonoBehaviour {

	public ITemporaryState stateDelegate;

	public bool IsActive { get; private set; }

	float _timer = 0;
	public float TimeRemaining { get { return Mathf.Max(0, _timer); } }


	void Update () {
		if(IsActive) {
			_timer -= Time.deltaTime;
			if(_timer <= 0)
				Deactivate();
		}
	}


	public void Activate (float duration = float.PositiveInfinity) {
		if(stateDelegate != null && !stateDelegate.CanActivate(this, ref duration))
			return;

		_timer = duration;
		IsActive = true;

		if(stateDelegate != null)
			stateDelegate.OnActivation(this, duration);
	}

	public void Deactivate () {
		if(!IsActive)
			return;
		if(stateDelegate != null && !stateDelegate.CanDeactivate(this))
			return;

		IsActive = false;

		if(stateDelegate != null)
			stateDelegate.OnDeactivation(this);
	}
}
