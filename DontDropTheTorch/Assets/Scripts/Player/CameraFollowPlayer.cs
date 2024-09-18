using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public static Transform LocalPlayerTransform = null;
    public static Transform PlayerToFollowTransform = null;

    void Update()
    {
        if (PlayerToFollowTransform != null) transform.position = PlayerToFollowTransform.position + new Vector3(0, 0, -10);
    }
}