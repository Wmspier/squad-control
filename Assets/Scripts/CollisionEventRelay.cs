using System;
using UnityEngine;

public class CollisionEventRelay : MonoBehaviour
{
	public Action<Collider> TriggerEnter;
	public GameObject Caster;

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject != Caster) TriggerEnter?.Invoke(collision.collider);
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject != Caster) TriggerEnter?.Invoke(other);
	}
}