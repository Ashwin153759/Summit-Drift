using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public int sectorNumber;
    private SectorManager sectorManager;

    private void Start()
    {
        sectorManager = GetComponentInParent<SectorManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Player"))
        {
            sectorManager.RecordSectorTime(sectorNumber);
        }
    }
}
