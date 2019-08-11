using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField]
    private Vector2 WaitTimeRange;
    [SerializeField]
    private Vector2 WalkRandomTimeRange;
    
    private enum ENpcState
    {
        Wait,
        WalkRandom,
        WalkDirection,
        WalkLocation
    }
    
    private void Update()
    {
        
    }

    private void Move()
    {
        
    }

    private void Emote()
    {
        
    }

    private void ScanDisguise()
    {
        
    }

    public void Kill()
    {
        
    }
}
