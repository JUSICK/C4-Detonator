using System.Collections;
using UnityEngine;
using ProjectMER.Features.Objects;
using Exiled.API.Features.Toys;
using Light = Exiled.API.Features.Toys.Light;

namespace С4grenade.CustomC4grenade;

public class ImpactDetect : MonoBehaviour
{
    public bool IsManualDetonation = false;
    public SchematicObject _C4schemmatic;
    public System.Action _OnCollisionEnter;
    
    private Exiled.API.Features.Toys.Light _beepLight;
    private float _timer = 0f;
    private bool _isLightOn = true;
    
    public Rigidbody rg;
    public Exiled.API.Features.Pickups.Projectiles.TimeGrenadeProjectile _projectile;

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
        CreateForObject(_projectile.GameObject, "bombCollision", 2f);
        
        if (_C4schemmatic == null) return;
        _C4schemmatic.transform.SetParent(null);
        AlignSchematic(collision);
        SpawnBlinkingLight();
    }

    public void Detonate()
    {
        if (_projectile == null && _projectile.GameObject == null) return;
        CreateForObject(_projectile.GameObject, "beep", 3f);
        IsManualDetonation = true;
        _projectile.FuseTime = 2f;
        if (_C4schemmatic != null) Destroy(_C4schemmatic, 3f);
        if (_beepLight != null) _beepLight.Destroy();

    }
    public void CreateForObject(GameObject soundObject, string file, float volume)
    {
        string uniqueId = $"{soundObject.GetInstanceID()}";
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet(uniqueId, onIntialCreation: (p) =>
        {
            p.transform.parent = soundObject.transform;
            Speaker speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 3f, maxDistance: 10f);
            speaker.transform.parent = soundObject.transform;
            speaker.transform.localPosition = Vector3.zero;
        });
        audioPlayer.AddClip(file, volume);
    }
    private void AlignSchematic(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 wallNormal = contact.normal;
        Vector3 desiredUp = Vector3.up;
        _C4schemmatic.transform.rotation = Quaternion.LookRotation(wallNormal, desiredUp) * Quaternion.Euler(0, 90, 90);
    }
    private void SpawnBlinkingLight()
    {
        _beepLight = Light.Create(_C4schemmatic.transform.position);
        _beepLight.Color = new Color(1f, 1f, 1f); 
        _beepLight.Range = 1.0f;   
        _beepLight.Intensity = 0f;
        _beepLight.ShadowType = LightShadows.None; 
        
        _beepLight.Transform.SetParent(_C4schemmatic.transform);
        _beepLight.Transform.localPosition = new Vector3(0, 0, 0); 
    }
    private void Update()
    {
        if (_beepLight == null) return;
        _timer += Time.deltaTime;
        if (_isLightOn)
        {
            if (_timer >= 0.1f)
            {
                _beepLight.Intensity = 0f;
                CreateForObject(_projectile.GameObject, "beeping", 1f);
                _isLightOn = false;
                _timer = 0f;
            }
        }
        else
        {
            if (_timer >= 1.9f)
            {
                _beepLight.Intensity = 2.0f;
                _isLightOn = true;
                _timer = 0f;
            }
        }
    }
    private void OnDestroy()
    {
        if (_C4schemmatic != null) Destroy(_C4schemmatic);
        if (_beepLight != null) _beepLight.Destroy();
    }
}

