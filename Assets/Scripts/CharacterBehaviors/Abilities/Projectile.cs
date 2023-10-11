using UnityEngine;

namespace CharacterBehaviors.Abilities
{
	public class Projectile : MonoBehaviour
	{
		[SerializeField] private float _volleyMaxHeight = 2f;
		
		private bool _isVolley;
		private bool _launched;
		private float _speed;
		private Vector3 _direction;
		private Vector3 _destination;
		private Vector3 _origin;
		private Vector3 _center;

		private float _launchTimestamp;

		private void FixedUpdate()
		{
			if (!_launched) return;
			
			if(_isVolley) HandleVolleyUpdate();
			else HandleBulletUpdate();
		}

		private void HandleBulletUpdate()
		{
			var t = transform;
			var position = t.position;
			position += _direction * _speed * Time.fixedDeltaTime;
			t.position = position;
		}

		private void HandleVolleyUpdate()
		{
			var ratio = (Time.time - _launchTimestamp) / _speed;
			
			var position = Vector3.Lerp(_origin, _destination, ratio);
			var center = Vector3.Lerp(_origin, _destination, .5f);
			var xDif = _origin.x - _destination.x;

			position.y += Mathf.SmoothStep(0, _volleyMaxHeight, 1 - Mathf.Pow((center.x - position.x) * 2 / xDif, 2));
			transform.position = position;
		}

		public void LaunchBullet(float speed, Vector3 direction)
		{
			_launchTimestamp = Time.time;
			_speed = speed;
			
			_direction = direction;
			
			_isVolley = false;
			_launched = true;
		}

		public void LaunchVolley(float speed, Vector3 destination)
		{
			_launchTimestamp = Time.time;
			_speed = speed;
			
			_origin = transform.position;
			_destination = destination;
			_center = Vector3.Lerp(_origin, _destination, .5f);
			
			_isVolley = true;
			_launched = true;
		}
	}
}