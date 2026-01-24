using UnityEngine;

public class C4StableFollow : MonoBehaviour
{
    public Transform TargetGrenade; // The green grenade
    private Quaternion _fixedRotation; // The direction we look while flying
    private bool _stuckToWall = false;

    // Called when we first spawn
    public void Init(Transform grenade, Vector3 lookAtPos)
    {
        TargetGrenade = grenade;

        // Calculate rotation: Face the player who threw it
        Vector3 direction = lookAtPos - grenade.position;
        direction.y = 0; // Keep it flat

        if (direction != Vector3.zero)
            _fixedRotation = Quaternion.LookRotation(direction);
        else
            _fixedRotation = Quaternion.identity;
    }

    void Update()
    {
        // Safety: If grenade is destroyed (exploded), destroy the C4
        if (TargetGrenade == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!_stuckToWall)
        {
            // 1. Teleport to grenade position
            transform.position = TargetGrenade.position;

            // 2. Force our stable rotation (Ignore grenade spin)
            transform.rotation = _fixedRotation;
        }
    }

    public void Freeze()
    {
        _stuckToWall = true;
    }
}