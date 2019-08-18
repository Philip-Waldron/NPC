using UnityEngine;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Alternative Path")]
	[System.Serializable]
	/// <summary>
	/// Randomise path a bit.
	/// </summary>
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_alternative_path.php")]
	public class RandomPath : MonoModifier {
	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Random Path Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(RandomPath));
		}
	#endif
		
		public override int Order { get { return 10; } }
		
		[SerializeField]
		private bool _randomizeAllPenalties;
		[SerializeField]
		private bool _randomizeAdjacentPenalties;
		
		/// <summary>How much penalty (weight) to apply to nodes</summary>
		public int penalty = 10000;
		/// <summary>Max number of nodes to skip in a row</summary>
		public int randomStep = 10;
		/// <summary>A random object</summary>
		readonly System.Random rnd = new System.Random();
		private bool _destroyed;

		public override void Apply (Path p)
		{
			if (this == null)
			{
				return;
			}

			ApplyNow(p);
		}

		protected void OnDestroy()
		{
			_destroyed = true;
		}

		void ApplyNow (Path path)
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
					int rndStart = rnd.Next(randomStep);
					for (int i = rndStart; i < path.path.Count; i += rnd.Next(1, randomStep))
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
					
					path.BlockUntilCalculated();
				}
			}
		}
	}
}
