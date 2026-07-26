using UnityEngine;

public class TargetGroupAutoRegister : MonoBehaviour
{
    private TargetedGroupCameraManager manager;

    private void Start()
    {
        manager = FindFirstObjectByType<TargetedGroupCameraManager>();

        if (manager != null )
        {
            manager.RegisterTarget(transform);
        }
    }

    public void RemovePlayer()
    {
        if (manager != null)
        {
            manager.UnRegisterTarget(transform);
        }
    }

    private void OnDestroy()
    {
        if ( manager != null )
        {
            manager.UnRegisterTarget(transform);
        }
    }
}
