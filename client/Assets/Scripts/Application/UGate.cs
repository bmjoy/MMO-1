using System;
using EngineCore.Simulater;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Collections.Concurrent;

public abstract class UGate : MonoBehaviour
{
	protected virtual void JoinGate() { }
	protected virtual void ExitGate() { }
	protected virtual void Tick() { }

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		JoinGate();
	}

	private void OnDisable() => ExitGate();

	private void Update()
    {
		while (updateCall.Count > 0)
		{
			if(updateCall.TryDequeue(out Action c ))
                c?.Invoke();
        }
		Tick();
    }

	private readonly ConcurrentQueue<Action> updateCall = new ConcurrentQueue<Action>();

	public void Invoke(Action call)
	{
		updateCall.Enqueue(call);
    }
}

