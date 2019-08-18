using UnityEngine;

namespace Pathfinding
{
	/// <summary>
	/// Randomise path a bit. Based on Alternative Path
	/// </summary>
	[AddComponentMenu("Pathfinding/Modifiers/Randomise Path")]
	[System.Serializable]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_alternative_path.php")]
	public class RandomisePath : MonoModifier
	{
	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Random Path Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command)
		{
			(command.context as Component).gameObject.AddComponent(typeof(RandomisePath));
		}
	#endif
		
		[SerializeField]
		private bool _RecalculatePathAfterRandomise;
		[SerializeField]
		private bool _randomizeAllPenalties;
		[SerializeField]
		private bool _randomizeAdjacentPenalties;
		
		// How much penalty (weight) to apply to nodes.
		public int penalty = 10000;
		// Max number of nodes to skip in a row.
		public int randomStep = 10;
		
		public override int Order { get { return 10; } }
		private readonly System.Random _random = new System.Random();
		private bool _destroyed;

		public override void Apply (Path p)
		{
			if (this == null)
			{
				return;
			}

			ApplyNow(p);
		}

		private void ApplyNow (Path path)
		{
			if (_destroyed)
			{
				return;
			}
			
			if (path.path != null)
			{
				if (_randomizeAllPenalties)
				{
					foreach (var node in path.path)
					{
						node.Penalty = (uint)Random.Range(0, penalty);
						if (_randomizeAdjacentPenalties)
						{
							node.GetConnections(otherNode =>
							{
								otherNode.Penalty = (uint)Random.Range(0, penalty);
							});
						}
					}
				}
				else
				{
					int rndStart = _random.Next(randomStep);
					for (int i = rndStart; i < path.path.Count; i += _random.Next(1, randomStep))
					{
						path.path[i].Penalty += (uint)penalty;
						if (_randomizeAdjacentPenalties)
						{
							path.path[i].GetConnections(otherNode =>
							{
								otherNode.Penalty = (uint)Random.Range(0, penalty);
							});
						}
					}
				}

				if (_RecalculatePathAfterRandomise)
				{
					path.BlockUntilCalculated();
				}
			}
		}
		
		protected void OnDestroy()
		{
			_destroyed = true;
		}
	}
}
