using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "LoadEventsChannel", menuName = "Events/Apply Gravity Event Channel")]
public class ForceReceiverEventsChannelSO : ScriptableObject
{
	public UnityAction<ForceReceiver> OnAddForceReceiverRequested;
	public void RaiseAddEvent(ForceReceiver forceReceiver)
	{
		if (OnAddForceReceiverRequested != null)
		{
			OnAddForceReceiverRequested.Invoke(forceReceiver);
		}
		else
		{
			Debug.LogWarning("Adding a Force Receiver was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
		}
	}
	
	public UnityAction<ForceReceiver> OnRemoveForceReceiverRequested;
	public void RaiseRemoveEvent(ForceReceiver forceReceiver)
	{
		if (OnRemoveForceReceiverRequested != null)
		{
			OnRemoveForceReceiverRequested.Invoke(forceReceiver);
		}
		else
		{
			/*
			Debug.LogWarning("Removing a Force Receiver was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
			*/
		}
	}
}
