using System;
using System.Collections;
using System.Linq;
using CharacterBehaviors;
using Movement;
using Status;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

namespace Enemy
{
	public class BasicEnemyController : CharacterMovement
	{
		[SerializeField] private NavMeshAgent agent;
		[SerializeField] private CharacterAnimation characterAnimation;
		[SerializeField] private CharacterVfx characterVfx;
		[SerializeField] private CharacterStatus status;
		[SerializeField] private float playerSearchFrequencySeconds = 1f;
		[SerializeField] private float basicAttackFrequencySeconds = .75f;
		[SerializeField] private float minDistanceToPursue = 10f;
		
		[Header("Attack Config")]
		[SerializeField] private bool meleeAtacker;

		private Squad.Squad _playerSquad;
		private float _previousBasicAttackTime;
		private float _previousPlayerSearchTime;
		
		private bool _pursuingPlayer;
	
		public event Action<BasicEnemyController> Died;
	
		[field: SerializeField] public CharacterAbilityActivator AbilityActivator { get; private set; }
		[field: SerializeField] public CharacterType Type { get; private set; }

		public void SetDestination(Vector3 destination) => agent.SetDestination(destination);
		public void SetIsMelee(bool isMelee) => meleeAtacker = isMelee;

		private void Awake()
		{
			Assert.IsNotNull(agent, "Agent is null and BasicEnemyControl depends on it.");
			Assert.IsNotNull(characterAnimation, "CharacterAnimation is null and BasicEnemyControl depends on it.");
			Assert.IsNotNull(status, "CharacterStatus is null and BasicEnemyControl depends on it.");
			Assert.IsNotNull(AbilityActivator, "CharacterAbilityActivator is null and BasicEnemyControl depends on it.");

			status.HealthDepleted += OnHealthDepleted;
		}

		private void Start()
		{
			_playerSquad = GameObject.FindWithTag("Squad").GetComponent<Squad.Squad>();
			agent.isStopped = true;
			StartCoroutine(DoSpawn());

			IEnumerator DoSpawn()
			{
				characterAnimation.PlaySpawn();
				yield return new WaitForSeconds(2f);
				agent.isStopped = false;
			}
		}
	
		private void OnDestroy() { status.HealthDepleted -= OnHealthDepleted; }

		protected void Update()
		{
			if (!status.IsAlive)
			{
				AbilityActivator.ClearIndicators();
				return;
			}
		
			var isCloseEnoughToAttack = IsCloseEnoughToAttack();
		
			// Make sure agent destination is always the player
			if (_playerSquad != null && IsCloseEnoughToPursue())
			{
				agent.SetDestination(_playerSquad.transform.position);
				_pursuingPlayer = true;
			}
			// If the enemy is close enough to attack, make sure they are facing the player
			if (isCloseEnoughToAttack && !AbilityActivator.IndicatingAttack)
			{
				transform.LookAt(_playerSquad.transform);
			}

			// Attack if close enough and not stunned
			if (!Stunned
			    && isCloseEnoughToAttack
			    && Time.time - _previousBasicAttackTime > basicAttackFrequencySeconds
			    && _playerSquad != null)
			{
				DoAttack(meleeAtacker);
			}
		
			// Stop agent if close enough to attack or if stunned
			agent.isStopped = AbilityActivator.IndicatingAttack || isCloseEnoughToAttack || Stunned;
			if(agent.isStopped) characterAnimation.SetRunBlend(0f);
		
			if (agent.isStopped || Time.time - _previousPlayerSearchTime < playerSearchFrequencySeconds) return;

			characterAnimation.SetRunBlend(Mathf.InverseLerp(0f, agent.speed, agent.velocity.magnitude));

			_previousPlayerSearchTime = Time.time;
		}

		protected override void HandleMove() { }

		private bool IsCloseEnoughToAttack()
		{
			var abilityData = AbilityActivator.Abilities.ElementAt(meleeAtacker ? 0 : 1);
			return IsCloseEnoughToPursue() && Vector3.Distance(transform.position, agent.destination) <= abilityData.MinRange;
		}

		private bool IsCloseEnoughToPursue()
		{
			return _pursuingPlayer || 
			       Vector3.Distance(transform.position, _playerSquad.transform.position) <= minDistanceToPursue;
		}
	
		private void DoAttack(bool melee)
		{
			_previousBasicAttackTime = Time.time;
			characterVfx.SetProjectileDirection(transform.forward);

			if (melee)
			{
				AbilityActivator.ActivateAbilityAtIndex(0, true);
			}
			else
			{
				var position = _playerSquad.GetRandomMember().transform.position;
				position.y += 0.2f;
				
				AbilityActivator.ActivateAbilityAtIndex(1, true, position);
			}
		}

		private void OnHealthDepleted()
		{
			agent.isStopped = true;
			characterAnimation.PlayDie();
			status.ToggleDisplay(false);
			Died?.Invoke(this);
		}
	}
}