using UnityEngine;

public class MazeGoal : MonoBehaviour
{
    [HideInInspector]
    public MazeGenerator mazeGenerator;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the overlapping object is the player
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (mazeGenerator != null)
            {
                mazeGenerator.OnGoalReached();
            }
        }
    }
}
