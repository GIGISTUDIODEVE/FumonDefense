using System;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Aerodynamics
{
    public class AeroRuntime : MonoBehaviour
    {
        private Vector3 acc;
        private bool isRunning;
        private float timeOfFlight;
        private float timer;
        private Rigidbody ball;
        private bool isAccelerating;
        private Vector3 vStore;

        private Action<Rigidbody> onFinished;

        /// <summary>
        /// Time needed for projectile to reach endpoint. The acceleration
        /// (force) is applied during this period of time.
        /// </summary>
        public float TimeOfFlight => timeOfFlight;

        /// <summary>
        /// How much time has been passed since the projectile launched.
        /// </summary>
        public float Timer => timer;

        /// <summary>
        /// Read the data from the AeroSolver instance. A valid instance
        /// should have been called Solve(...) beforehand.
        /// </summary>
        /// <param name="solver">AeroSolver instance to get data from.</param>
        /// <param name="rBody">Rigidbody you want to control</param>
        public void Configure(AeroSolver solver, Rigidbody rBody)
        {
            acc = solver.Acceleration;
            timeOfFlight = solver.TimeOfFlight;
            onFinished = solver.OnFinished;
            ball = rBody;
        }

        /// <summary>
        /// Starts the internal timer and logic (applying Acceleration, etc.).
        /// </summary>
        public void Run()
        {
            if (isRunning)
                timer = 0f;
            isRunning = true;
        }

        /// <summary>
        /// Stops applying Acceleration, and resets the internal timer. Useful for replay,
        /// handling collisions, etc.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            timer = 0f;
        }

        protected virtual void FixedUpdate()
        {
            if (isRunning)
                ApplyAcceleration();
        }

        private bool ApplyAcceleration()
        {
            if (timer <= timeOfFlight && timer + Time.fixedDeltaTime > timeOfFlight)
            {
                var t = timeOfFlight - timer;
                vStore = ball.linearVelocity;
                ball.linearVelocity = vStore / Time.fixedDeltaTime * t;
                timer += Time.fixedDeltaTime;
                return true;
            }

            if (timer > timeOfFlight)
            {
                if (isAccelerating)
                {
                    ball.linearVelocity = vStore;
                    onFinished?.Invoke(ball);
                }

                isAccelerating = false;
                return false;
            }

            if (timer > 0f)
                ball.AddForce(acc, ForceMode.Acceleration);
            else
                ball.AddForce(0.5f * acc, ForceMode.Acceleration);

            timer += Time.fixedDeltaTime;
            isAccelerating = true;
            return true;
        }
    }
}