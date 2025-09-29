using System;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Aerodynamics
{
    [HelpURL("https://blobcreate.github.io/projectile-toolkit/docs/aerodynamic-movement.html#--inspector-properties")]
    public class AeroProjectileLauncher : MonoBehaviour
    {
        public Rigidbody projectilePrefab;
        public Transform launchPoint;
        public float heightAboveTarget = 12f;
        [SerializeField] Vector3 maxOffset = new Vector3(25f, 0f, 10f);
        [SerializeField, HideInInspector] float projectileRadius;
        [SerializeField, HideInInspector] Transform flag;
        public bool useCallback;
        [SerializeField] bool usePrediction;
        public PEBTrajectoryPredictor predictor;
        public bool controlledByAnotherScript;

        Vector3 targetPoint;
        Rigidbody currentBall;
        AeroSolver aeroSolver;
        Camera cam;
        float aeroTimer;
        bool isLaunching;

        #region Control Methods

        // Turn on controlledByAnotherScript to use the following methods.

        public Action<Rigidbody> OnFinished
        {
            get => aeroSolver.OnFinished;
            set => aeroSolver.OnFinished = value;
        }

        public bool UsePrediction
        {
            get => usePrediction;
            set
            {
                usePrediction = value;
                predictor.gameObject.SetActive(usePrediction);
            }
        }

        public Vector3 MaxOffset
        {
            get => maxOffset;
            set => maxOffset = value;
        }

        public void AimAt(Vector3 point)
        {
            targetPoint = point;
        }

        public void Launch()
        {
            isLaunching = true;
        }

        #endregion

        void Start()
        {
            cam = Camera.main;

            // Step 1: Create a solver
            aeroSolver = new AeroSolver();
            // (optional):
            // Add a custom callback that will be called when the projectile reaches target.
            aeroSolver.OnFinished = (ball) =>
            {
                if (!useCallback)
                    return;
                ball.angularVelocity = Vector3.zero;
                ball.linearVelocity = new Vector3(0, 10, 0);
            };

            // Prediction:
            // Create a new instance of the projectile at the launch point for the predictor to use.
            var b = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation);

            if (!usePrediction)
                return;
            predictor.Simulatee = b;
            predictor.AeroSolverInstance = aeroSolver;
            b.gameObject.SetActive(false);
        }

        void Update()
        {
            var startPoint = launchPoint.position;
            var hit = default(RaycastHit);

            if (!controlledByAnotherScript)
                if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 200f))
                    targetPoint = hit.point + hit.normal * projectileRadius;

            // Step 2: Solve
            // (you can use any method in the Projectile class, not just VelocityByHeight)
            var offY = new Vector3(0, maxOffset.y, 0);
            var vRaw = Projectile.VelocityByHeight(startPoint, targetPoint + offY, heightAboveTarget);
            var vReal = aeroSolver.Solve(startPoint, targetPoint, maxOffset, vRaw);
            aeroTimer = 0f;  // <-- Only needed if you use ^method 2

            // Prediction:
            // Set up and run PEBTrajectoryPredictor.
            if (usePrediction)
            {
                predictor.LaunchVelocity = vReal + Projectile.VelocityCompensation;
                predictor.SimulateAndRender();
            }

            isLaunching = isLaunching || (!controlledByAnotherScript && Input.GetMouseButtonDown(0));

            if (isLaunching)
            {
                if (!controlledByAnotherScript && flag)
                {
                    flag.position = targetPoint;
                    flag.rotation = Quaternion.LookRotation(hit.normal);
                }

                // Launch
                currentBall = Instantiate(projectilePrefab, startPoint, launchPoint.rotation);
                currentBall.AddForce(vReal + Projectile.VelocityCompensation, ForceMode.VelocityChange);

                // Step 3: Run
                //
                // This is ^method 1, which adds AeroRuntime to each ball,
                // allowing unlimited number of objects to do Projectile Aerodynamics simultaneously.
                currentBall.TryGetComponent<AeroRuntime>(out var rt);
                rt ??= currentBall.gameObject.AddComponent<AeroRuntime>();
                rt.Configure(aeroSolver, currentBall);
                rt.Run();

                isLaunching = false;
            }
        }

        // // This is ^method 2, it is more lightweight than ^method 1 and is
        // // easier to embed into custom logic (PEBTrajectoryPredictor uses this).
        // // Uncomment this and comment out ^method 1 to use.
        // void FixedUpdate()
        // {
        //     if (currentBall != null)
        //     {
        //         aeroSolver.ApplyAcceleration(currentBall, ref aeroTimer);
        //     }
        // }

    }
}