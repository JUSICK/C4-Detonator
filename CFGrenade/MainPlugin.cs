using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static CFGrenade.CustomC4grenade.DefuseManager;

namespace CFGrenade;

public class MainPlugin : Plugin<Config>
{
    public static MainPlugin Instance { get; private set; }
    public override string Name => "CF_Detonator";
    public override string Prefix => "CFGrenade";
    public override string Author => "JUICE";
    public override Version Version => new(1, 0, 1);
    public override void OnEnabled()
    {
        CustomItem.RegisterItems(overrideClass: Config);
        Exiled.Events.Handlers.Server.WaitingForPlayers += LoadAllSchematic;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnPluginLoad;
        Exiled.Events.Handlers.Server.WaitingForPlayers += NotifyLog;
        Exiled.Events.Handlers.Server.RoundStarted += ClearingDictionaries;
        Instance = this;
        base.OnEnabled();
    }
    public override void OnDisabled()
    {
        CustomItem.UnregisterItems();
        Exiled.Events.Handlers.Server.WaitingForPlayers -= LoadAllSchematic;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnPluginLoad;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= NotifyLog;
        Exiled.Events.Handlers.Server.RoundStarted -= ClearingDictionaries;
        base.OnDisabled();
        Instance = null;
    }

    private void ClearingDictionaries()=> ActiveSessions.Clear();
    private void NotifyLog() => Log.Info("CFGrenade plugin was loaded successfully. Thank You!");
    
    private void OnPluginLoad()
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
    private string SetupSoundFile(string filename)
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


    private void LoadAllSchematic()
    {
        LoadSchematic("CFHealth");
        LoadSchematic("CFHealthRad");
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
