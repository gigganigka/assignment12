using UnityEngine;
public sealed class DataExit : MonoBehaviour
{
    public DataLevelController owner;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) owner.TryComplete();
    }
}
