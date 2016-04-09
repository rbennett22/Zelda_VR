using UnityEngine;

public interface IFreezable {
	bool CanFreeze (Freezable f, ref float duration);
	bool CanUnfreeze (Freezable f);
	void OnFreeze (Freezable f, float duration);
	void OnUnfreeze (Freezable f);
}

public class Freezable : TemporaryState, ITemporaryState {

	public IFreezable freezableDelegate;


	void Awake () {
		stateDelegate = this;
	}


	bool ITemporaryState.CanActivate (TemporaryState state, ref float duration) {
		return freezableDelegate.CanFreeze(this, ref duration);
	}
	bool ITemporaryState.CanDeactivate (TemporaryState state) {
		return freezableDelegate.CanUnfreeze(this);
	}
	void ITemporaryState.OnActivation (TemporaryState state, float duration) {
		freezableDelegate.OnFreeze(this, duration);
	}
	void ITemporaryState.OnDeactivation (TemporaryState state) {
		freezableDelegate.OnUnfreeze(this);
	}
}
