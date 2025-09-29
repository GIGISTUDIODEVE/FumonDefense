using UnityEngine;

namespace Blobcreate.ProjectileToolkit
{
	public class TrajectoryPredictor : MonoBehaviour
	{
		[SerializeField] LineRenderer line;

		Vector3[] positions;
		int validCount;
		Vector3 gravity;
		float gAcceleration;

		public LineRenderer Line { get => line; set => line = value; }
		public Vector3[] Positions => positions;
		public int PositionCount => validCount;

		/// <summary>
		/// The gravity that used to simulate trajectories. Currently does not support custom gravity direction.
		/// </summary>
		public Vector3 Gravity
		{
			get => gravity;
			set
			{
				gravity = value;
				gAcceleration = -gravity.magnitude;
			}
		}


		/// <summary>
		/// Computes and renders the trajectory.
		/// </summary>
		/// <param name="origin">Launch position, or current position of the projectile if it is moving.</param>
		/// <param name="originVelocity">The velocity of the projectile when it is at origin.</param>
		/// <param name="distance">To calculate the trajectory to how far. Measured from origin and
		/// ignoring vertical displacement of the trajectory.</param>
		/// <param name="count">How many positions (points) to calculate, including the origin and end points.</param>
		public virtual void Render(Vector3 origin, Vector3 originVelocity, float distance, int count = 16)
		{
			if (count > positions.Length)
				positions = new Vector3[count];

			Projectile.Positions(origin, originVelocity, distance, count, gAcceleration, positions);
			validCount = count;
			line.positionCount = count;
			for (int i = 0; i < count; i++)
			{
				line.SetPosition(i, positions[i]);
			}
		}

		/// <param name="end">The end point of the trajectory.</param>
		public virtual void Render(Vector3 origin, Vector3 originVelocity, Vector3 end, int count = 16)
		{
			var xz = end - origin;
			xz.y = 0f;
			Render(origin, originVelocity, xz.magnitude, count);
		}


		protected virtual void Awake()
		{
			positions = new Vector3[16];
			Gravity = Physics.gravity;
		}

		protected virtual void OnEnable()
		{
			if (line != null)
				line.enabled = true;
		}

		protected virtual void OnDisable()
		{
			if (line != null)
				line.enabled = false;
		}
	}
}