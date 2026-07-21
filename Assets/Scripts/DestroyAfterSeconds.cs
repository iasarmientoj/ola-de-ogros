using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float seconds = 2f;

    void Start()
    {
        Destroy(gameObject, seconds);
    }
}
