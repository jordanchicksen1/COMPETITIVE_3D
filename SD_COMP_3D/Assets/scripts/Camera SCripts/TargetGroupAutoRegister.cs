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

    private void OnDestroy()
    {
        if ( manager != null )
        {
            manager.UnRegisterTarget(transform);
        }
    }
}
