using System.Collections.Generic;
using UnityEngine;

namespace NPC.Scripts.Networking
{
    public interface IMovementQueue
    {
        Vector2 nextMovement { get; }
        List<Vector2> moveList { get; }

        void BroadcastMove(Vector2 moveDirection);
        void MoveComplete();
    }
}
