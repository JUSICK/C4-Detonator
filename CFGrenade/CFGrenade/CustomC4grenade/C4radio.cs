using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features.Toys;
using AdminToys;
using UnityEngine;
using ProjectMER.Features.Serializable;
//using LabApi.Features.Wrappers;

namespace С4grenade.CustomC4grenade;

[CustomItem(ItemType.Radio)]
internal class C4radio : CustomItem
{
    public override uint Id { get; set; } = 108;
    public override string Name { get; set; } = "<i><color=black>C4-Detonator</i></color>";
    public override string Description { get; set; } = "<size=0>...</size=0>";
    public override ItemType Type { get; set; } = ItemType.Radio;
    public override float Weight { get; set; } = 3f;
    public override SpawnProperties SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        RoleSpawnPoints =
        [
            new RoleSpawnPoint
        {
            Chance = 100,
            Role = RoleTypeId.ChaosRepressor
        }
        ]
    };
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.DroppingItem += OnDroppping;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnCleaning;
        Exiled.Events.Handlers.Player.ChangingRadioPreset += OnChangingRadioPreset;

        base.SubscribeEvents();
    }
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.DroppingItem -= OnDroppping;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnCleaning;
        Exiled.Events.Handlers.Player.ChangingRadioPreset -= OnChangingRadioPreset;
        base.UnsubscribeEvents();
    }
    private Dictionary<ushort, System.Action> _detonators = new Dictionary<ushort, System.Action>();

    private Dictionary<ushort, int> _c4left = new Dictionary<ushort, int>();
    private void OnChangingRadioPreset(ChangingRadioPresetEventArgs ev)
    {
        if (_detonators.ContainsKey(ev.Item.Serial))
        {
            ev.IsAllowed = false;
            _detonators[ev.Item.Serial].Invoke();
            _detonators.Remove(ev.Item.Serial); // _detonators dictionary cleaning 
            CreateForPlayer(ev.Player, "activated");
        }
    }
    private void OnDroppping(DroppingItemEventArgs ev)
    {
        if (!Check(ev.Item)) return;//our radio?
        ev.IsAllowed = false;
        ushort radioSerial = ev.Item.Serial;

        if (!_c4left.ContainsKey(radioSerial)) //if radio new - create dictionary with available explosives
            _c4left[radioSerial] = 3;

        if (_c4left[radioSerial] > 0) //as long as radio includes ammo
        {
            Throwable grenade = ev.Player.ThrowGrenade(ProjectileType.FragGrenade, false); //drop c4
            if (grenade.Projectile is not TimeGrenadeProjectile timeGrenade) return; //check what we drop
            timeGrenade.FuseTime = 200f;
            SchematicObject schematic = ObjectSpawner.SpawnSchematic(
                "CFHealth",
                timeGrenade.Position,
                Quaternion.identity,
                Vector3.one
            );
            if (schematic == null) 
            {
                Log.Error("schematic == null");
                return;
            }

            schematic.transform.SetParent(timeGrenade.GameObject.transform);
            schematic.transform.localPosition = Vector3.zero;
            schematic.transform.localScale = Vector3.one;
            schematic.transform.localRotation = Quaternion.identity;

                ImpactDetect customScript = timeGrenade.GameObject.AddComponent<ImpactDetect>(); //add grenade scipt
                customScript.Init(timeGrenade, schematic); //start the script manually
                

            if (!_detonators.ContainsKey(radioSerial))
                _detonators[radioSerial] = null;
            _detonators[radioSerial] += customScript.Detonate;
            _c4left[radioSerial] -= 1;//one c4 must have been dropped
            
                ev.Player.Broadcast(2, $"<b><color=yellow><i>{_c4left[radioSerial]}/3</b></i></color>", shouldClearPrevious: true);
            
        }
        else //radio doesn't cointain more c$4 charges
            ev.IsAllowed = true;
    }
    private void CreateForPlayer(Exiled.API.Features.Player player, string file)
    {
        AudioPlayer audioPlayer = AudioPlayer.CreateOrGet($"Player {player.Nickname}", onIntialCreation: (p) =>
        {
            p.transform.parent = player.GameObject.transform;
            Speaker speaker = p.AddSpeaker("Main", isSpatial: true, minDistance: 5f, maxDistance: 15f);
            speaker.transform.parent = player.GameObject.transform;
            speaker.transform.localPosition = Vector3.zero;
        });
        audioPlayer.AddClip(file, 1f);
    }
    private void OnCleaning() => _c4left.Clear();
    public static SchematicObject SpawnSchematic(SerializableSchematic serializableSchematic)
    {
        GameObject? gameObject = serializableSchematic.SpawnOrUpdateObject();
        if (gameObject == null)
            return null!;

        return gameObject.GetComponent<SchematicObject>();
    }
}
