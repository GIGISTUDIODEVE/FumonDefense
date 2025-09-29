using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
    public class SimpleLauncher : MonoBehaviour
    {
        [SerializeField] PEBTrajectoryPredictor predictor;

        [SerializeField] float launchSpeed = 20f;
        [SerializeField] float launchAngleY = 0f;
        [SerializeField] float launchAngleX = 45f;
        Vector3 launchV;
        bool tSync;

        public void SetAngleY(float angle)
    	{
            launchAngleY = angle;
            UpdateLaunchVector();
        }

        public void SetAngleX(float angle)
        {
            launchAngleX = angle;
            UpdateLaunchVector();
        }

        public void SetLaunchSpeed(float speed)
    	{
            launchSpeed = speed;
            UpdateLaunchVector();
        }

        public void SetTSync(bool isOn)
        {
            tSync = isOn;
            predictor.TSync = tSync;
            UpdateLaunchVector();
        }

        void UpdateLaunchVector()
    	{
            var q = Quaternion.AngleAxis(-launchAngleX, Vector3.right);
            q = Quaternion.AngleAxis(launchAngleY, Vector3.up) * q;
            launchV = q * Vector3.forward * launchSpeed;
            predictor.LaunchVelocity = launchV;

            // PEB Trajectory Predictor does not re-simulate at every frame automatically, so make sure
            // to call SimulateAndRender() after any changes. As you can see in the Set...() methods
            // above, they all call UpdateLaunchVector() which eventually calls SimulateAndRender()
            // here.
            //
            // (Exception: when updateObstacleTransforms is turned on, whenever any of the obstacles
            // moves, rotates, or scales, the prediction gets re-simulated).
            predictor.SimulateAndRender();
    	}

        void Start()
        {
            UpdateLaunchVector();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                predictor.Simulatee.isKinematic = false;
                predictor.Simulatee.AddForce(launchV, ForceMode.VelocityChange);
            }
        }
    }
}