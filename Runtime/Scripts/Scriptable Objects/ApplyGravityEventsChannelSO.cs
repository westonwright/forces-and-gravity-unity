using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "LoadEventsChannel", menuName = "Events/Apply Gravity Event Channel")]
public class ApplyGravityEventsChannelSO : ScriptableObject
{
	public UnityAction<ApplyGravityObject> OnAddApplyObjectRequested;
	public void RaiseAddEvent(ApplyGravityObject applyGravityObject)
	{
		if (OnAddApplyObjectRequested != null)
		{
			OnAddApplyObjectRequested.Invoke(applyGravityObject);
		}
		else
		{
			Debug.LogWarning("Adding an Apply Gravity Object was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
		}
	}
	
	public UnityAction<ApplyGravityObject> OnRemoveApplyObjectRequested;
	public void RaiseRemoveEvent(ApplyGravityObject applyGravityObject)
	{
		if (OnRemoveApplyObjectRequested != null)
		{
			OnRemoveApplyObjectRequested.Invoke(applyGravityObject);
		}
		else
		{
			/*
			Debug.LogWarning("Removing an Apply Gravity Object was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
			*/
		}
	}
}
