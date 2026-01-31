using Exiled.API.Features;
using Exiled.CreditTags;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using SharpCompress.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CFGrenade;

public class MainPlugin : Plugin<Config>
{
    public static MainPlugin Instance { get; private set; }
    public override string Name => "CF_Detonator";
    public override string Prefix => "CFGrenade";
    public override string Author => "JUICE";
    public override Version Version => new(0, 5, 0);
    public string schematicName = "CFHealth";
    public override void OnEnabled()
    {
        CustomItem.RegisterItems(overrideClass: Config);
        Exiled.Events.Handlers.Server.WaitingForPlayers += LoadSchematic;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnPluginLoad;
        Exiled.Events.Handlers.Server.WaitingForPlayers += NotifyLog;
        Instance = this;
        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        CustomItem.UnregisterItems();
        Exiled.Events.Handlers.Server.WaitingForPlayers -= LoadSchematic;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnPluginLoad;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= NotifyLog;
        base.OnDisabled();
        Instance = null;
    }
    private void NotifyLog() => Log.Info("CFGrenade plugin was loaded successfully. Thank You!");
    public void OnPluginLoad()
    {
        string path1 = SetupSoundFile("beep.ogg");
        AudioClipStorage.LoadClip(path1, "beep");
        string path2 = SetupSoundFile("bombCollision.ogg");
        AudioClipStorage.LoadClip(path2, "bombCollision");
        string path3 = SetupSoundFile("activated.ogg");
        AudioClipStorage.LoadClip(path3, "activated");
        string path4 = SetupSoundFile("beeping.ogg");
        AudioClipStorage.LoadClip(path4, "beeping");
    }
    public string SetupSoundFile(string filename)
    {
        string folderPath = Path.Combine(Paths.Plugins, "C4Sounds");
        string filePath = Path.Combine(folderPath, filename);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        if (File.Exists(filePath))
            return filePath;

        Assembly assembly = Assembly.GetExecutingAssembly();

        string resourceName = $"CFGrenade.CustomC4grenade.{filename}";

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




    public void LoadSchematic()
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
            stream.CopyTo(fileStream);
        }
    }
}
