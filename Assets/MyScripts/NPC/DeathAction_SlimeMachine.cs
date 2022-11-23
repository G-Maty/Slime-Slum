using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;

[AddComponentMenu("")]
public class DeathAction_SlimeMachine : ActionBehaviour {
	protected override void OnAwake() {
	}

	protected override void OnStart() {
        Debug.Log("撃破");
    }

    protected override void OnExecute() {
	}

	protected override void OnEnd() {
	}
}
