using Blobcreate.ProjectileToolkit.Aerodynamics;
using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
    public class CurvyTestParamsUI : MonoBehaviour
    {
        public AeroProjectileLauncher test;

        public void SetXOffset(float value)
        {
            var ov = test.MaxOffset;
            test.MaxOffset = new Vector3(value, ov.y, ov.z);
        }

        public void SetZOffset(float value)
        {
            var ov = test.MaxOffset;
            test.MaxOffset = new Vector3(ov.x, ov.y, value);
        }

        public void SetCallbackState(bool isOn)
        {
            test.useCallback = isOn;
        }

        public void SetPredictorState(bool isOn)
        {
            test.predictor.gameObject.SetActive(isOn);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                test.controlledByAnotherScript = !test.controlledByAnotherScript;
            }
        }

    }
}