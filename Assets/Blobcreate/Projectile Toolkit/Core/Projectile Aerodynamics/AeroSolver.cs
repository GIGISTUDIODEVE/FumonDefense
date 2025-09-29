using System;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Aerodynamics
{
    public class AeroSolver
    {
        private Vector3 offsetVector;
        private Vector3 vReal;
        private Vector3 acc;
        private float timeOfFlight;
        private bool isAccelerating;
        private Vector3 vStore;

        /// <summary>
        /// Custom callback that is triggered when the projectile reaches its target.<br/>
        /// Args: Rigidbody — Projectile's Rigidbody.
        /// <remarks>It is triggered after a pre-calculated time, not by collision.</remarks>
        /// </summary>
        public Action<Rigidbody> OnFinished { get; set; }

        /// <summary>
        /// The returned value of Solve(...) — the computed launch velocity that takes projectile
        /// aerodynamics into account. Only valid after Solve(...) has been called.
        /// </summary>
        public Vector3 SolvedVelocity => vReal;

        /// <summary>
        /// Computed continuous acceleration that is used to conduct projectile aerodynamics.
        /// </summary>
        public Vector3 Acceleration => acc;

        /// <summary>
        /// World space version of maxOffset (one parameter you passed to method Solve(...)).
        /// It is used internally to get offset target point (= target + OffsetVector).
        /// </summary>
        public Vector3 OffsetVector => offsetVector;

        /// <summary>
        /// Time needed for projectile to reach endpoint. The acceleration (force) is applied
        /// during this period of time.
        /// </summary>
        public float TimeOfFlight => timeOfFlight;

        /// <summary>
        /// Computes the data required for projectile aerodynamics.
        /// </summary>
        /// <param name="start">Launch point.</param>
        /// <param name="end">Target point.</param>
        /// <param name="maxOffset">Maximum displacement along local space of a curved trajectory.
        /// The local space is formed by using "end - start" vector (ignoring y) as forward vector.</param>
        /// <param name="v">The original launch velocity that makes the projectile move from start point
        /// to end point without taking projectile aerodynamics into account.</param>
        /// <returns>The modified launch velocity that takes projectile aerodynamics into account.</returns>
        public Vector3 Solve(Vector3 start, Vector3 end, Vector3 maxOffset, Vector3 v)
        {
            // Gets the duration that the projectile will fly.
            Projectile.FlightTest(start, end, v, FlightTestMode.Horizontal, out timeOfFlight);

            var f = end - start;
            f.y = 0f;
            offsetVector = Quaternion.LookRotation(f) * maxOffset;

            // Computes the continuous acceleration that will be applied to the Rigidbody.
            acc = AccelerationByTime(end + offsetVector, Vector3.zero, end, timeOfFlight);
            vReal = Projectile.VelocityByTime(start, end + offsetVector, timeOfFlight);
            return vReal;
        }

        // Much more complicated than I thought, several
        // tricks are involved to make the movement accurate.
        /// <summary>
        /// Executes one physics update of projectile aerodynamics.
        /// </summary>
        /// <param name="rBody">The Rigidbody of the object to which the acceleration is applied.</param>
        /// <param name="timer">This is used as a runtime context, it records how much time has been passed
        /// since the projectile launch.</param>
        /// <returns>Whether the acceleration is applied. False means the procedure is finished.</returns>
        public bool ApplyAcceleration(Rigidbody rBody, ref float timer)
        {
            if (timer <= timeOfFlight && timer + Time.fixedDeltaTime > timeOfFlight)
            {
                var t = timeOfFlight - timer;
                vStore = rBody.linearVelocity;
                rBody.linearVelocity = vStore / Time.fixedDeltaTime * t;
                timer += Time.fixedDeltaTime;
                return true;
            }

            if (timer > timeOfFlight)
            {
                if (isAccelerating)
                {
                    rBody.linearVelocity = vStore;
                    OnFinished?.Invoke(rBody);
                }

                isAccelerating = false;
                return false;
            }

            if (timer > 0f)
                rBody.AddForce(acc, ForceMode.Acceleration);
            else
                rBody.AddForce(0.5f * acc, ForceMode.Acceleration);

            timer += Time.fixedDeltaTime;
            isAccelerating = true;
            return true;
        }

        private Vector3 AccelerationByTime(Vector3 pointA, Vector3 velocityA, Vector3 pointB, float time)
        {
            var inverseTime = 1f / time;
            var dS = pointB - pointA;
            var vB = dS * (2f * inverseTime) - velocityA;

            return (vB - velocityA) * inverseTime;
        }
    }
}