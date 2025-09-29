using UnityEngine;

namespace Blobcreate.ProjectileToolkit.Demo
{
	public class TurnOffGodMode : MonoBehaviour
	{
		public SimpleHealth healthComponent;
		public int limitHealth = 100;

		bool hasTriggered;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.G))
			{
				if (hasTriggered)
					return;

				healthComponent.MaxHealth = limitHealth;
				hasTriggered = true;
			}
		}
	}
}
