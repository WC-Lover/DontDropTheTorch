using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleWeaponController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.TryGetComponent<EnemyController>(out var ec))
        {
            ec.DealDamageToEnemy(1000, new Vector2(0, 0));
        }

    }
}
