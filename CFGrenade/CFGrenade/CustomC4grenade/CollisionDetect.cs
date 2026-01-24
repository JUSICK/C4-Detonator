using UnityEngine;
using Exiled.API.Features;
using ProjectMER;
using ProjectMER.Events.Handlers;
using ProjectMER.Features;
using ProjectMER.Features.Objects;

namespace С4grenade.CustomC4grenade;

public class ImpactDetect : MonoBehaviour
{
    public bool IsManualDetonation = false;
    public SchematicObject _C4schemmatic;
    public System.Action _OnCollisionEnter;
    public Rigidbody rg;
    public Exiled.API.Features.Pickups.Projectiles.TimeGrenadeProjectile _projectile;
    public float creationTime;

    public void Init(Exiled.API.Features.Pickups.Projectiles.TimeGrenadeProjectile projectile, SchematicObject schematic)
    {
        _C4schemmatic = schematic;
        _projectile = projectile;
        rg = projectile.GameObject.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_projectile == null && rg == null) return;
        rg.isKinematic = true;
        _OnCollisionEnter?.Invoke();
        CreateForObject(_projectile.GameObject, "bombCollision");
    }

    public void Detonate()
    {
        if (_projectile == null && _projectile.GameObject == null) return;
        CreateForObject(_projectile.GameObject, "beep");
        IsManualDetonation = true;
        _projectile.FuseTime = 1f;

    }
    public void CreateForObject(GameObject soundObject, string file)
    {
        string uniqueId = $"{soundObject.GetInstanceID()}";
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet(uniqueId, onIntialCreation: (p) =>
        {
            p.transform.parent = soundObject.transform;
            Speaker speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 3f, maxDistance: 6f);
            speaker.transform.parent = soundObject.transform;
            speaker.transform.localPosition = Vector3.zero;
        });
        audioPlayer.AddClip(file, 3f);
    }
}

