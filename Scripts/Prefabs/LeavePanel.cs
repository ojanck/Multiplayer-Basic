using UnityEngine;
using UnityEngine.Events;

public class LeavePanel : BasePanel
{
	public UnityEvent onLeaveGameConfirm;

	/**
	 * Dispatch a custom event to leave th ecurrent game Room.
	 */
	public void OnLeaveGameConfirmClick()
	{
		// Dispatch event
		onLeaveGameConfirm.Invoke();

		// Hide panel
		Hide();
	}
}
