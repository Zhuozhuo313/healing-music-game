using UnityEngine;

public class FollowParent : MonoBehaviour
{
    // 主物体
    public Transform mainObject;
    void Start()
    {
        
    }
    void Update()
    {
        if (mainObject != null)
        {
            // 更新子物体的位置，使其与主物体的位置一致
            transform.position = mainObject.position;

            // 更新子物体的自转，使其与主物体的自转一致
            transform.rotation = mainObject.rotation;
        }
    }
}
