using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using System;
using MEC;
using System.Collections.Generic;
using Exiled.Events.EventArgs.Map;
using UnityEngine;

namespace С4grenade.CustomC4grenade;

[CustomItem(ItemType.Radio)]
internal class C4Radio : CustomItem
{
    public override uint Id { get; set; } = 108;
    public override string Name { get; set; } = "<i><color=black>Набір Підривника</i></color>";
    public override string Description { get; set; } = "<size=0>...</size=0>";
    public override ItemType Type { get; set; } = ItemType.Radio;
    public override float Weight { get; set; } = 5f;
    public override SpawnProperties SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 10,
                Location = SpawnLocationType.Inside079Armory,
            },
            new()
            {
                Chance = 10,
                Location = SpawnLocationType.Inside173Armory,
            },
        },
        StaticSpawnPoints = new List<StaticSpawnPoint>
        {
            new()
            {
                Name = "Escape Hall Building",
                Chance = 10,
                Position = new Vector3(133, 289, 24), 
            },
        },
    };
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded += OnPickUpAdded;
        
        Exiled.Events.Handlers.Player.DroppingItem += OnPlayerDropping;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnCleaning;
        Exiled.Events.Handlers.Player.ChangingRadioPreset += OnChangingRadioPreset;

        base.SubscribeEvents();
    }
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Map.PickupAdded -= OnPickUpAdded;
        
        Exiled.Events.Handlers.Player.DroppingItem -= OnPlayerDropping;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnCleaning;
        Exiled.Events.Handlers.Player.ChangingRadioPreset -= OnChangingRadioPreset;
        base.UnsubscribeEvents();
    }
    
    private void OnPickUpAdded(PickupAddedEventArgs ev)
    {
        if (!Check(ev.Pickup) || ev.Pickup == null) return;
        SchematicObject schematic = ObjectSpawner.SpawnSchematic(
            "CFHealthRad",
            ev.Pickup.Position,
            Quaternion.identity,
            Vector3.one
        );
        if (schematic == null) 
        {
            Log.Error("C4 Detonator schematic == null");
            return;
        }
        schematic.transform.SetParent(ev.Pickup.GameObject.transform);
        schematic.transform.localPosition = Vector3.zero;
        schematic.transform.localScale = Vector3.one;
        schematic.transform.localRotation = Quaternion.identity;
    }
   
    public static Dictionary<ushort, System.Action> Detonators = new Dictionary<ushort, System.Action>();

    private Dictionary<ushort, int> _c4Left = new Dictionary<ushort, int>();
    private void OnChangingRadioPreset(ChangingRadioPresetEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        ev.IsAllowed = false;
        CreateForPlayer(ev.Player, "activated");
        if (Detonators.ContainsKey(ev.Item.Serial))
        {
            Detonators[ev.Item.Serial].Invoke();
            Detonators.Remove(ev.Item.Serial); 
        }
    }
    private void OnPlayerDropping(DroppingItemEventArgs ev)
    {
        if (!Check(ev.Item)) return;
        if (!ev.IsThrown)
        {
            ev.IsAllowed = true;
            return;
        }
        ev.IsAllowed = false;
        ushort radioSerial = ev.Item.Serial;

        if (!_c4Left.ContainsKey(radioSerial))
            _c4Left[radioSerial] = CFGrenade.MainPlugin.Instance.Config.AvailableCharges;

        if (_c4Left[radioSerial] > 0)
        {
            Throwable grenade = ev.Player.ThrowGrenade(ProjectileType.FragGrenade, false);
            if (grenade.Projectile is not TimeGrenadeProjectile timeGrenade) return;
            timeGrenade.FuseTime = 10000f;
            SchematicObject schematic = ObjectSpawner.SpawnSchematic(
                "CFHealth",
                timeGrenade.Position,
                Quaternion.identity,
                Vector3.one
            );
            if (schematic == null) 
            {
                Log.Error("C4 schematic == null");
                return;
            }

            schematic.transform.SetParent(timeGrenade.GameObject.transform);
            schematic.transform.localPosition = Vector3.zero;
            schematic.transform.localScale = Vector3.one;
            schematic.transform.localRotation = Quaternion.identity;

            ImpactDetect customScript = timeGrenade.GameObject.AddComponent<ImpactDetect>();
            customScript.Init(timeGrenade, schematic, radioSerial);
                
            if (!Detonators.ContainsKey(radioSerial))
                Detonators[radioSerial] = null;
            Detonators[radioSerial] += customScript.Detonate;
            _c4Left[radioSerial] -= 1;
            
                ev.Player.ShowHint( $"<b><color=white>[{_c4Left[radioSerial]}/3]</b></color>",2f);
        }
        else
            ev.IsAllowed = true;
    }
    private static void CreateForPlayer(Exiled.API.Features.Player player, string file)
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

    private void OnCleaning()
    {
        Detonators.Clear();
        _c4Left.Clear();
    }

    public static SchematicObject SpawnSchematic(SerializableSchematic serializableSchematic)
    {
        GameObject? gameObject = serializableSchematic.SpawnOrUpdateObject();
        if (gameObject == null)
            return null!;

        return gameObject.GetComponent<SchematicObject>();
    }
}
