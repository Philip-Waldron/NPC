using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomPenaltySetter : MonoBehaviour
{
    [SerializeField]
    private float _minPenalty = 0;
    [SerializeField]
    private float _maxPenalty = 100000;
    
    private GridNode[] _gridNodes;

    private void Start()
    {
        _gridNodes = AstarPath.active.data.gridGraph.nodes;
        AstarPath.active.AddWorkItem(() =>
        {
            foreach (var node in _gridNodes)
            {
                node.Penalty = (uint) Random.Range(_minPenalty, _maxPenalty);
            }
        });
    }
}
