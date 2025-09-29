using System.Collections.Generic;
using Blobcreate.ProjectileToolkit.Aerodynamics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blobcreate.ProjectileToolkit
{
    [HelpURL("https://blobcreate.github.io/projectile-toolkit/docs/PEB%20Trajectory%20Predictor.html")]
	public class PEBTrajectoryPredictor : MonoBehaviour
	{
		[SerializeField] LineRenderer line;

		[Tooltip("The rigidbody to be simulated.")]
		[SerializeField] Rigidbody simulatee;

		[Tooltip("All the colliders to be simulated.")]
		[SerializeField] List<Transform> obstacles;

		[Tooltip("Turn this on for auto re-simulation, so that when any of the colliders moves, " +
		         "rotates, or scales, the prediction gets re-simulated.")]
		[SerializeField] bool updateObstacleTransforms;

		[Tooltip("How many physics steps to simulate.")]
		[SerializeField] int totalIterations = 90;

		[Tooltip("The max physics steps to simulate during one frame. This allocates physics " +
		         "simulation into several frames, thus optimize the performance.")]
		[SerializeField] int maxIterationsEveryFrame = 30;

		[Tooltip("The timestep of simulation. To get accurate prediction result, this should " +
		         "be the same as physics timestep value in the project settings.")]
		[SerializeField] float simulationTimestep = 0.02f;

		[SerializeField] Vector3 launchVelocity;

		[Tooltip("Controls how smoothly the line is updated.")]
		[Range(0f, 1f)]
		[SerializeField] float lineLerpFactor = 0.25f;

		[Tooltip("Prevents the line from tearing.")]
		[SerializeField] bool tSync;

		[Header("Projectile Aerodynamics")]
		[Tooltip("Controls whether to predict Projectile Aerodynamics. Note that the AeroSolverInstance property " +
		         "needs to be assigned for this to take effect.")]
		[SerializeField] bool predictAeroMove = true;

		Scene simScene;
		PhysicsScene simPhyScene;
		Rigidbody ghostRigid;
		Dictionary<Transform, Transform> obstaclePairs = new Dictionary<Transform, Transform>();
		int iterationTimer;
		int iterationCounter;
		int requestInQueue;
		float amTimer;

		Vector3[] posCache;
		Vector3[] posCacheL2;

		/// <summary> The launch velocity to be applied to the simulation. </summary>
		public Vector3 LaunchVelocity
		{
			get => launchVelocity;
			set => launchVelocity = value;
		}

		/// <summary> If you have any moving colliders, set this to true for auto re-simulation. </summary>
		public bool UpdateObstacleTransforms
		{
			get => updateObstacleTransforms;
			set => updateObstacleTransforms = value;
		}

		/// <summary> Controls how smoothly the line is updated. The value should be in the range
		/// of 0f (exclusive) to 1f (inclusive). The smaller the value, the smoother the line
		/// updates. </summary>
		public float LineLerpFactor
		{
			get => lineLerpFactor;
			set => lineLerpFactor = value;
		}

		/// <summary> Set to true to prevent the line from tearing. </summary>
		public bool TSync
		{
			get => tSync;
			set => tSync = value;
		}

		/// <summary>
		/// The rigidbody to be simulated.
		/// </summary>
		public Rigidbody Simulatee
		{
			get => simulatee;
			set
			{
				if (ghostRigid != null)
					Destroy(ghostRigid.gameObject);
				simulatee = value;
				if (simulatee != null)
					CreateGhostObject(simulatee.transform, true);
			}
		}

		/// <summary>
		/// Controls whether to predict Projectile Aerodynamics. Note that the AeroSolverInstance property
		/// needs to be assigned for this to take effect.
		/// </summary>
		public bool PredictAeroMove
		{
			get => predictAeroMove;
			set => predictAeroMove = value;
		}

		/// <summary>
		/// The AeroSolver instance that will be used to predict Projectile Aerodynamics.
		/// </summary>
		public AeroSolver AeroSolverInstance { get; set; }

		/// <summary>
		/// Tells the PEB Trajectory Predictor to do one round of simulation and render the line
		/// accordingly.
		/// </summary>
		public void SimulateAndRender()
		{
			requestInQueue += 1;
			if (requestInQueue > 2)
				requestInQueue = 2;
		}

		/// <summary>
		/// Adds a collider to the simulation context.
		/// </summary>
		public void AddObstacle(Transform obstacle)
		{
			if (!obstaclePairs.ContainsKey(obstacle))
			{
				obstacles.Add(obstacle);
				CreateGhostObject(obstacle, false);
			}
		}

		/// <summary>
		/// Removes a collider from the simulation context.
		/// </summary>
		public void RemoveObstacle(Transform obstacle)
		{
			if (obstaclePairs.TryGetValue(obstacle, out var ghost))
			{
				obstacles.Remove(obstacle);
				obstaclePairs.Remove(obstacle);
				Destroy(ghost.gameObject);
			}
		}

		void Awake()
		{
			CreatePhysicsScene();
			Simulatee = simulatee;
			line.positionCount = totalIterations;
			posCache = new Vector3[totalIterations];
			posCacheL2 = new Vector3[totalIterations];
		}

		void CreatePhysicsScene()
		{
			simScene = SceneManager.CreateScene("PhySim", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
			simPhyScene = simScene.GetPhysicsScene();

			foreach (Transform obj in obstacles)
			{
				CreateGhostObject(obj, false);
			}
		}

		void CreateGhostObject(Transform raw, bool isSimulatee)
		{
			var ghostObj = Instantiate(raw.gameObject, raw.position, raw.rotation);
			if (ghostObj.TryGetComponent<Renderer>(out var r))
				r.enabled = false;
			SceneManager.MoveGameObjectToScene(ghostObj, simScene);

			if (isSimulatee)
			{
				ghostRigid = ghostObj.GetComponent<Rigidbody>();
				ghostRigid.isKinematic = false;
			}
			else
			{
				obstaclePairs.Add(raw, ghostObj.transform);
			}
		}

		void Update()
		{
			var cache = tSync ? posCacheL2 : posCache;

			for (var i = 0; i < line.positionCount; i++)
			{
				line.SetPosition(i,
					Vector3.Lerp(line.GetPosition(i), cache[i], lineLerpFactor * Time.deltaTime * 60));
			}
		}

		void FixedUpdate()
		{
			if (updateObstacleTransforms)
			{
				var anyHasChanged = false;

				for (int i = 0; i < obstacles.Count; i++)
				{
					if (obstacles[i].hasChanged)
					{
						var ghost = obstaclePairs[obstacles[i]];
						ghost.position = obstacles[i].position;
						ghost.rotation = obstacles[i].rotation;
						ghost.localScale = obstacles[i].lossyScale;
						obstacles[i].hasChanged = false;
						anyHasChanged = true;
					}
				}

				if (anyHasChanged)
					SimulateAndRender();
			}

			if (requestInQueue == 0)
				return;

			SimulateTrajectory(launchVelocity);
		}

		void SimulateTrajectory(Vector3 velocity)
		{
			if (simulatee == null)
				return;

			if (iterationCounter == 0)
			{
				var t = simulatee.transform;
				ghostRigid.position = t.position;
				ghostRigid.rotation = t.rotation;
				ghostRigid.linearVelocity = velocity;
				ghostRigid.angularVelocity = simulatee.angularVelocity;
				amTimer = 0f;
			}

			for (var i = iterationCounter;
				i - iterationCounter < maxIterationsEveryFrame && i < line.positionCount;
				i++)
			{
				if (i > 0)
				{
					if (predictAeroMove && AeroSolverInstance != null)
						AeroSolverInstance.ApplyAcceleration(ghostRigid, ref amTimer);
					simPhyScene.Simulate(simulationTimestep);
				}

				posCache[i] = ghostRigid.position;
			}

			iterationCounter += maxIterationsEveryFrame;
			if (iterationCounter > totalIterations + 1)
			{
				iterationCounter = 0;
				requestInQueue -= 1;
			}

			if (tSync && iterationCounter == 0)
				for (int i = 0; i < posCache.Length; i++)
					posCacheL2[i] = posCache[i];

		}
	}
}