using UnityEngine;

public class ShootingTrailController : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * 150);
        Destroy(gameObject, 0.25f);
    }
}