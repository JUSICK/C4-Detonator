using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using static CFGrenade.CustomC4grenade.DefuseManager;
using Server = Exiled.Events.Handlers.Server;

namespace CFGrenade;

public class MainPlugin : Plugin<Config>
{
    public static MainPlugin Instance { get; private set; }
    public override string Name => "C4";
    public override string Prefix => "C4";
    public override string Author => "JUICE";
    public override Version Version => new(1, 0, 2);
    public override void OnEnabled()
    {
        CustomItem.RegisterItems(overrideClass: Config);
        Server.WaitingForPlayers += OnPluginLoad;
        Server.RoundStarted += ClearingDictionaries;
        Instance = this;
        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        CustomItem.UnregisterItems();
        Server.WaitingForPlayers -= OnPluginLoad;
        Server.RoundStarted -= ClearingDictionaries;
        base.OnDisabled();
        Instance = null;
    }
    
    private void OnPluginLoad()
    {
        LoadAllSchematic();
        LoadAllSounds();
        NotifyLog();
    }
    private void LoadAllSchematic()
    {
        LoadSchematic("CFHealth");
        LoadSchematic("CFHealthRad");
    }
    private void LoadAllSounds()
    {
        var sounds = new Dictionary<string, string>
        {
            { "beep.ogg", "beep" },
            { "bombCollision.ogg", "bombCollision" },
            { "activated.ogg", "activated" },
            { "beeping.ogg", "beeping" }
        };
        foreach (var sound in sounds)
        {
            string path = SetupSoundFile(sound.Key);
            AudioClipStorage.LoadClip(path, sound.Value);
        }
    }
    private void ClearingDictionaries()=> ActiveSessions.Clear();
    private void NotifyLog() => Log.Info("C4 plugin was loaded successfully. Thank You!");
    private string SetupSoundFile(string filename)
    {
        string folderPath = Path.Combine(Paths.Plugins, "C4Sounds");
        string filePath = Path.Combine(folderPath, filename);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (File.Exists(filePath))
            return filePath;

        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = $"CFGrenade.CustomC4grenade.ogg_json.{filename}";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Log.Error($"Failed to find embedded resource: {resourceName}");
                foreach (var name in assembly.GetManifestResourceNames()) Log.Info($"Found: {name}");
                return null;
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        Log.Info($"The {filename} was created in: {filePath}");
        return filePath;
    }

    private void LoadSchematic(string schematicName)
    {
        string specificFolder = Path.Combine(ProjectMER.ProjectMER.SchematicsDir, schematicName);
        string destinationPath = Path.Combine(specificFolder, schematicName + ".json");
        if (!Directory.Exists(specificFolder))
        {
            Directory.CreateDirectory(specificFolder);
            Log.Info($"Created ProjectMER directory: {specificFolder}");
        }
        
        if (!File.Exists(destinationPath))
        {
            SaveResource($"{schematicName}.json", destinationPath);
            Log.Info($"Saved schematic to LabAPI folder: {destinationPath}");
        }
    }
    private void SaveResource(string fileName, string savePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        string resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith(fileName));

        if (resourceName == null)
        {
            Log.Error($"Could not find embedded resource: '{fileName}'.");
            return;
        }
        
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        {
            if (stream != null) stream.CopyTo(fileStream);
        }
    }
}
