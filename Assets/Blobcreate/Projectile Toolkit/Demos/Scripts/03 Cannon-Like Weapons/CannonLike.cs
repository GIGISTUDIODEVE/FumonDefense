using Blobcreate.Universal;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
	public class CannonLike : MonoBehaviour
	{
		public LaunchType type;
		public Rigidbody shell;
		public Transform barrel;
		public Transform launchPoint;
		public Transform uiRange;

		// ForceByAngle
		public float launchAngle;

		// AnglesBySpeed and ForcesBySpeed
		public float launchSpeed;
		public bool useHighAngle;
		public bool useLowAngle;

		public DynamicUI ui;
		public bool controlledByAnotherScript;

		bool hasTarget;
		bool outOfReach;
		Vector3 lookPoint;
		Vector3 targetPoint;

		void Update()
		{
			if (!controlledByAnotherScript)
				// Target
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var mouseHitInfo, 200f))
				{
					lookPoint = mouseHitInfo.point;
					targetPoint = lookPoint;
					lookPoint.y = transform.position.y;

					if (Input.GetMouseButtonDown(0))
						hasTarget = true;
				}

			outOfReach = false;
			var launchVelocity = default(Vector3);

			// Rotate and launch
			if (type == LaunchType.VelocityByAngle)
			{
				launchVelocity = Projectile.VelocityByAngle(launchPoint.position, targetPoint, launchAngle);
				transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);
				barrel.localRotation = Quaternion.AngleAxis(-launchAngle, Vector3.right);
			}
			else if (type == LaunchType.AnglesBySpeed)
			{
				if (Projectile.AnglesBySpeed(launchPoint.position, targetPoint, launchSpeed,
					out var lowA, out var highA))
				{
					transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);

					// Rotates along local x.
					if (useLowAngle)
						barrel.localRotation = Quaternion.AngleAxis(-lowA, Vector3.right);
					else if (useHighAngle)
						barrel.localRotation = Quaternion.AngleAxis(-highA, Vector3.right);

					launchVelocity = barrel.forward * launchSpeed;
				}
				else
				{
					outOfReach = true;
				}
			}
			else if (type == LaunchType.VelocitiesBySpeed)
			{
				if (Projectile.VelocitiesBySpeed(launchPoint.position, targetPoint, launchSpeed,
					out var lowV, out var highV))
				{
					// VelocitiesBySpeed is an extended version of AnglesBySpeed.
					// It is more convenient than AnglesBySpeed when the rotation
					// is not separated into y axis and x axis.

					transform.rotation = Quaternion.LookRotation(lookPoint - transform.position);

					if (useLowAngle)
					{
						barrel.rotation = Quaternion.LookRotation(lowV);
						launchVelocity = lowV;
					}
					else if (useHighAngle)
					{
						barrel.rotation = Quaternion.LookRotation(highV);
						launchVelocity = highV;
					}
				}
				else
				{
					outOfReach = true;
				}
			}

			if (outOfReach)
			{
				// Out of reach, so we find the max range and aim at it.

				var range = Projectile.ElevationalReach(launchPoint.position, 0f, launchSpeed, out var angle);
				ui?.SetRangeText(range);

				if (range > 0f)
				{
					var ro = Quaternion.LookRotation(lookPoint - transform.position);
					transform.rotation = ro;
					barrel.localRotation = Quaternion.AngleAxis(-angle, Vector3.right);
					launchVelocity = barrel.forward * launchSpeed;

					// Range Indicator's activation and animation.
					if (!uiRange.gameObject.activeSelf)
					{
						uiRange.localScale = Vector3.zero;
						uiRange.gameObject.SetActive(true);
					}

					uiRange.rotation = ro;
					uiRange.localScale = Vector3.Lerp(uiRange.localScale, range * Vector3.one, 0.3f);
				}
			}
			else
			{
				ui?.HideRangeText();

				// Range Indicator's animation and deactivation.
				if (uiRange.gameObject.activeSelf)
				{
					uiRange.localScale = Vector3.Lerp(uiRange.localScale, Vector3.zero, 0.3f);
					if (uiRange.localScale.x < 1f)
						uiRange.gameObject.SetActive(false);
				}
			}

			if (!hasTarget)
				return;

			if (launchVelocity != default)
			{
				var bullet = Instantiate(shell, launchPoint.position, Quaternion.identity);
				// Don't forget to call Launch(...) (or use your own explosion logic instead).
				bullet.GetComponent<ProjectileBehaviour>().Launch(Vector3.one);
				bullet.AddForce(launchVelocity, ForceMode.VelocityChange);

				// Shows the time of flight stats.
				FlightTestMode mode;
				if (outOfReach)
					mode = FlightTestMode.VerticalB;
				else
					mode = FlightTestMode.Both;

				if (Projectile.FlightTest(bullet.position, targetPoint, launchVelocity,
						mode, out var t))
					ui?.SetTOFText(t);
			}
				
			hasTarget = false;
		}

// ----------

		#region Curated control methods
		// Turn on controlledByAnotherScript to use the following methods.

		public void Aim(Vector3 value)
		{
			targetPoint = value;
			lookPoint = value;
			lookPoint.y = transform.position.y;
		}

		public void Launch()
		{
			hasTarget = true;
		}

		#endregion

		void OnDisable()
		{
			if (uiRange != null)
				uiRange.gameObject.SetActive(false);
		}


		public enum LaunchType
		{
			VelocityByAngle,
			AnglesBySpeed,
			VelocitiesBySpeed
		}
	}
}