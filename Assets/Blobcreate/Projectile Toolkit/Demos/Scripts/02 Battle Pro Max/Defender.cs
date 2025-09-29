using Blobcreate.Universal;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
	public class Defender : MonoBehaviour
	{
		public Transform attackTarget;
		public float perceptionRadius = 20f;
		public float perceptionInterval = 1f;
		public float timerOffset;
		public Transform bombLaunchPoint;
		public Rigidbody bombPrefab;
		public float bombFlyTime = 1f;

		[Header("Position Prediction of Target")]
		public bool predict = true;
		[Tooltip("finalPos = (1 - weight) * currentPos + weight * predictedPos")]
		public float weight = 1;

		[Tooltip("This is used for position prediction of target. " +
		         "Smaller values bring higher accuracy, but more likely to be tricked by tiny movements.")]
		public float recordInterval = 0.1f;

		float timer;
		float estimationTimer;
		float lastRecordTime;
		Vector3 lastTargetPosition;

		void Start()
		{
			lastTargetPosition = attackTarget.position;
			timer = timerOffset;
		}

		void Update()
		{
			if (attackTarget == null)
				return;

			timer += Time.deltaTime;

			if (timer > perceptionInterval)
			{
				timer -= perceptionInterval;
				var currentPos = attackTarget.position;

				if (Vector3.SqrMagnitude(currentPos - transform.position) <
					perceptionRadius * perceptionRadius)
				{
					// Predict the position of the target after time bombFlyTime.
					var predictedPos = currentPos + bombFlyTime * EstimateVelocity();
					var finalPos = (1 - weight) * currentPos + weight * predictedPos;

					var b = Instantiate(bombPrefab, bombLaunchPoint.position, Quaternion.identity);
					var f = Projectile.VelocityByTime(b.position, finalPos, bombFlyTime);
					b.AddForce(f, ForceMode.VelocityChange);

					// Initialize the ProjectileBehaviour.
					b.GetComponent<ProjectileBehaviour>().Launch(finalPos);
				}
			}

			estimationTimer += Time.deltaTime;

			// Record the position of the target every recordInterval second.
			if (estimationTimer > recordInterval)
			{
				estimationTimer -= recordInterval;
				lastRecordTime = Time.time;
				lastTargetPosition = attackTarget.position;
			}

			var lookPoint = attackTarget.position;
			lookPoint.y = transform.position.y;
			transform.LookAt(lookPoint);
		}

		Vector3 EstimateVelocity()
		{
			var v = (attackTarget.position - lastTargetPosition) / (Time.time - lastRecordTime);
			v.y = 0f;
			return v;
		}
	}
}
