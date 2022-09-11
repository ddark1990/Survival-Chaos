using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
	public class ObjectSpawnerEditor : MonoBehaviour
	{
		[SerializeField] GameObject[] objectsToSpawn;
		public float radius = 1;
		public Vector2 regionSize = Vector2.one;
		public int rejectionSamples = 30;
		public float displayRadius = 1;

		List<Vector2> points;

		void OnValidate()
		{
			points = PoissonDiscSampling.GeneratePoints(radius, regionSize, rejectionSamples);
		}

		void OnDrawGizmos()
		{
			//Gizmos.DrawWireCube(transform.position + new Vector3((regionSize / 2).x, (regionSize / 2).y, 0), regionSize);
			if (points != null)
			{
				foreach (Vector2 point in points)
				{
					Gizmos.DrawSphere(transform.position + new Vector3(point.x, 0, point.y), displayRadius);
				}
			}
		}

		[ContextMenu("Spawn Objects")]
		void Plant()
        {
            foreach (var point in points)
            {
				var randomNum = Random.Range(0, objectsToSpawn.Length);

				Instantiate(objectsToSpawn[randomNum], transform.position + new Vector3(point.x, 0, point.y), Quaternion.identity);
            }

			print($"Spawning {points.Count} points.");
        }
	}
}