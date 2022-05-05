using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "LoadEventsChannel", menuName = "Events/Load Event Channel")]
public class GravitySourceEventsChannelSO : ScriptableObject
{
	public UnityAction<GravitySource> OnAddGravitySourceRequested;
	public void RaiseAddEvent(GravitySource gravitySource)
	{
		if (OnAddGravitySourceRequested != null)
		{
			OnAddGravitySourceRequested.Invoke(gravitySource);
		}
		else
		{
			Debug.LogWarning("Adding a Gravity Source was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
		}
	}
	
	public UnityAction<GravitySource> OnRemoveGravitySourceRequested;
	public void RaiseRemoveEvent(GravitySource gravitySource)
	{
		if (OnRemoveGravitySourceRequested != null)
		{
			OnRemoveGravitySourceRequested.Invoke(gravitySource);
		}
		else
		{
			/*
			Debug.LogWarning("Removing a Gravity Source was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
			*/
		}
	}
}
