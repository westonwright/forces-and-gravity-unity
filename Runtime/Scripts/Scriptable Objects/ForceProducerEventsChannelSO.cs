using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[CreateAssetMenu(fileName = "LoadEventsChannel", menuName = "Events/Load Event Channel")]
public class ForceProducerEventsChannelSO : ScriptableObject
{
	public UnityAction<ForceProducer> OnAddForceProducerRequested;
	public void RaiseAddEvent(ForceProducer forceProducer)
	{
		if (OnAddForceProducerRequested != null)
		{
			OnAddForceProducerRequested.Invoke(forceProducer);
		}
		else
		{
			Debug.LogWarning("Adding a Force Producer was requested but nobody picked it up. " +
				"Check why there is no GravityManager already present, " +
				"and make sure it's listening on this Add Gravity Source Channel.");
		}
	}
	
	public UnityAction<ForceProducer> OnRemoveForceProducerRequested;
	public void RaiseRemoveEvent(ForceProducer forceProducer)
	{
		if (OnRemoveForceProducerRequested != null)
		{
			OnRemoveForceProducerRequested.Invoke(forceProducer);
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
