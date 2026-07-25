using Unity.Cinemachine;
using UnityEngine;

public class TargetedGroupCameraManager : MonoBehaviour
{
    [SerializeField]
    private CinemachineTargetGroup targetGroup;

    [SerializeField]
    private float defaultweight;

    [SerializeField]
    private float defaultRadious;

    private void Start()
    {
        
    }

    private void Awake()
    {
        if (targetGroup == null)
        {
            targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();

        }
    }

    public void RegisterTarget(Transform target)
    {
        if (target == null && targetGroup == null)
        {
            return;
        }

        targetGroup.AddMember(target, defaultweight, defaultRadious);

    }

    public void UnRegisterTarget(Transform target)
    {
        if (target == null && targetGroup == null)
        {
            return;
        }

        targetGroup.RemoveMember(target);

    }
}
