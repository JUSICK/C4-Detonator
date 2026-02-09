
using System.Collections.Generic;
using UnityEngine;
using ProjectMER.Features.Objects;
using MEC;
using UnityEngine.Serialization;
using Light = Exiled.API.Features.Toys.Light;

namespace С4grenade.CustomC4grenade;

public class ImpactDetect : MonoBehaviour
{
    public SchematicObject c4Schematic;
    public Rigidbody rg;
    private Exiled.API.Features.Pickups.Projectiles.TimeGrenadeProjectile _projectile;
    
    private CoroutineHandle _blinkCoroutine;
    private Exiled.API.Features.Toys.Light _beepLight;
    
    private ushort _radioSerial;
    [FormerlySerializedAs("IsDefusalLocked")] public bool isDefusalLocked = false;

    public void Init(Exiled.API.Features.Pickups.Projectiles.TimeGrenadeProjectile projectile, SchematicObject schematic, ushort serial)
    {
        c4Schematic = schematic;
        _projectile = projectile;
        rg = projectile.GameObject.GetComponent<Rigidbody>();
        SpawnBlinkingLight();
        _radioSerial = serial;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_projectile == null && rg == null) return;
        rg.isKinematic = true;
        CreateForObject(_projectile.GameObject, "bombCollision", 2f);
        if (c4Schematic == null) return;
        AlignSchematic(collision);
        if (_beepLight != null)
        {
            _beepLight.Transform.position = collision.contacts[0].point + (collision.contacts[0].normal * 0.2f);
            
            _beepLight.Transform.SetParent(c4Schematic.transform); 
        }
    }

    public void Detonate()
    {
        if (_projectile == null) return;
        if (_projectile.GameObject == null) return;
        CreateForObject(_projectile.GameObject, "beep", 3f);

        KillBlink();
        _projectile.FuseTime = 2f;
        if (c4Schematic != null) Destroy(c4Schematic, 3f);
        if (_beepLight != null) _beepLight.Destroy();

    }
    public void Defuse()
    {
        if (_projectile == null) return;
        if (_projectile.GameObject == null) return;
        UnsubscribeFromRadio();
        CreateForObject(_projectile.GameObject, "beep", 3f);

        KillBlink();
        Destroy(_projectile.GameObject, 2f);
        if (c4Schematic != null) Destroy(c4Schematic, 3f);
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
        c4Schematic.transform.rotation = Quaternion.LookRotation(wallNormal, desiredUp) * Quaternion.Euler(180, 90, 90);
        if (c4Schematic.gameObject.GetComponent<BoxCollider>() == null)
        {
            BoxCollider hitbox = c4Schematic.gameObject.AddComponent<BoxCollider>();
            hitbox.size = new Vector3(0.3f, 0.15f, 0.08f); 
            hitbox.center = Vector3.zero;
        }
        c4Schematic.gameObject.layer = 0;
    }
    private void SpawnBlinkingLight()
    {
        if (_beepLight != null) 
        {
            _beepLight.Destroy();
            _beepLight = null;
        }
        _beepLight = Light.Create(c4Schematic.transform.position);
        _beepLight.Color = new Color(0.7f, 0f, 0f); 
        _beepLight.Range = 0.3f;   
        _beepLight.Intensity = 0f;
        _beepLight.ShadowType = LightShadows.None; 
        
        _beepLight.Transform.SetParent(c4Schematic.transform);
        _beepLight.Transform.localPosition = new Vector3(0, 0, 0); 
        
        _blinkCoroutine = Timing.RunCoroutine(BlinkSequence());
    }
    private IEnumerator<float> BlinkSequence()
    {
        while (true)
        {
            if (_beepLight == null) yield break;
            _beepLight.Intensity = 0.4f;
            CreateForObject(_projectile.GameObject, "beeping", 1f);
            yield return Timing.WaitForSeconds(0.1f); 
            if (_beepLight == null) yield break;
            _beepLight.Intensity = 0.0f;
            yield return Timing.WaitForSeconds(2f); 
        }
    }
    private void OnDestroy()
    {
        UnsubscribeFromRadio();
        KillBlink();
        if (c4Schematic != null) Destroy(c4Schematic);
        if (_beepLight != null) _beepLight.Destroy();
    }
    private void KillBlink() => Timing.KillCoroutines(_blinkCoroutine);
    private void UnsubscribeFromRadio()
    {
        if (!C4Radio.Detonators.ContainsKey(_radioSerial)) return;
            C4Radio.Detonators[_radioSerial] -= Detonate;
    }
}

