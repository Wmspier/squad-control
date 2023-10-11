using System;
using CharacterBehaviors;
using Data;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace Movement
{
	public class FollowCharacterMovement : CharacterMovement
	{
		[SerializeField] private NavMeshAgent agent;
		[SerializeField] private CharacterAnimation characterAnimation;
		[SerializeField] private Transform defaultTarget;
		[SerializeField] private MovementConfig config;

		private Vector3 targetPositon;
		private float previousTargetSearchTime;

		public event Action TargetReached;

		public void SetTargetPosition(Vector3 t)
		{
			targetPositon = t;
			if(agent.enabled) agent.destination = t;
		}
		public void ClearTargetPosition() => SetTargetPosition(Vector3.negativeInfinity);
		public void SetAgentStoppingDistance(float distance) => agent.stoppingDistance = distance;

		private void OnDisable() => agent.enabled = false;

		private void OnEnable()=> agent.enabled = true;
		
		private void Awake()
		{
			Assert.IsNotNull(agent, "Agent is null and FollowCharacterMovement depends on it.");
			Assert.IsNotNull(characterAnimation, "CharacterAnimation is null and FollowCharacterMovement depends on it.");

			if (defaultTarget != null) targetPositon = defaultTarget.position;
		}

		protected override void HandleMove()
		{
			if (!enabled || !agent.enabled) return;
			
			// Stop agent if stunned
			agent.isStopped = Stunned;
			if(agent.isStopped) characterAnimation.SetRunBlend(0f);
		
			if (agent.isStopped || Time.time - previousTargetSearchTime < config.TargetSearchFrequencySeconds) return;

			characterAnimation.SetRunBlend(Mathf.InverseLerp(0f, agent.speed, agent.velocity.magnitude));

			previousTargetSearchTime = Time.time;

			if (Vector3.Distance(agent.transform.position, targetPositon) <= config.ReachedTargetRange + agent.radius/2)
			{
				TargetReached?.Invoke();
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(agent.destination, .5f);
		}
	}
}