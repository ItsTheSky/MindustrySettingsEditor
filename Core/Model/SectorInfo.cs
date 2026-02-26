namespace MindustrySaveEditor.Core.Model;

using System.Text.Json.Serialization;

/// <summary>
/// Miroir C# de mindustry.game.SectorInfo.
/// Seuls les champs sérialisés (non-transient) sont inclus.
/// </summary>
public class SectorInfo
{
    [JsonPropertyName("production")]
    public Dictionary<string, ExportStat> Production { get; set; } = new();

    [JsonPropertyName("rawProduction")]
    public Dictionary<string, ExportStat> RawProduction { get; set; } = new();

    [JsonPropertyName("export")]
    public Dictionary<string, ExportStat> Export { get; set; } = new();

    [JsonPropertyName("imports")]
    public Dictionary<string, ExportStat> Imports { get; set; } = new();

    /// <summary>Items stockés dans tous les cores. Clé = nom de l'item, Valeur = quantité.</summary>
    [JsonPropertyName("items")]
    public Dictionary<string, int> RawItems { get; set; } = new();
    
    [JsonIgnore]
    public Dictionary<Resource, int> Items => RawItems.ToDictionary(kv => Model.Resources.ById[kv.Key], kv => kv.Value);

    /// <summary>Meilleur type de core disponible (nom du bloc).</summary>
    [JsonPropertyName("bestCoreType")]
    public string BestCoreType { get; set; } = "core-shard";

    [JsonPropertyName("storageCapacity")]
    public int StorageCapacity { get; set; }

    [JsonPropertyName("hasCore")]
    public bool HasCore { get; set; } = true;

    [JsonPropertyName("lastPresetName")]
    public string? LastPresetName { get; set; }

    [JsonPropertyName("lastWidth")]
    public int LastWidth { get; set; }

    [JsonPropertyName("lastHeight")]
    public int LastHeight { get; set; }

    [JsonPropertyName("wasCaptured")]
    public bool WasCaptured { get; set; }

    /// <summary>Secteur d'origine (référence par ID ou null).</summary>
    [JsonPropertyName("origin")]
    public object? Origin { get; set; }

    /// <summary>Destination de lancement (référence par ID ou null).</summary>
    [JsonPropertyName("destination")]
    public object? Destination { get; set; }

    /// <summary>Ressources connues dans ce secteur (noms de contenu).</summary>
    [JsonPropertyName("resources")]
    public List<string> RawResources { get; set; } = new();
    
    [JsonIgnore]
    public List<Resource> Resources => RawResources.Select(r => Model.Resources.ById[r]).ToList();

    [JsonPropertyName("waves")]
    public bool Waves { get; set; } = true;

    [JsonPropertyName("attack")]
    public bool Attack { get; set; }

    [JsonPropertyName("hasSpawns")]
    public bool HasSpawns { get; set; } = true;

    [JsonPropertyName("attempts")]
    public int Attempts { get; set; }

    [JsonPropertyName("wave")]
    public int Wave { get; set; } = 1;

    [JsonPropertyName("winWave")]
    public int WinWave { get; set; } = -1;

    /// <summary>Temps entre les vagues (en ticks, 60 ticks = 1 sec).</summary>
    [JsonPropertyName("waveSpacing")]
    public float WaveSpacing { get; set; } = 2f * 60f * 60f; // 2 minutes en ticks

    /// <summary>Position de spawn du core (packed x/y).</summary>
    [JsonPropertyName("spawnPosition")]
    public int SpawnPosition { get; set; }

    [JsonPropertyName("minutesCaptured")]
    public float MinutesCaptured { get; set; }

    [JsonPropertyName("lightCoverage")]
    public float LightCoverage { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>Icône en tant que contenu (nom du contenu).</summary>
    [JsonPropertyName("contentIcon")]
    public string? ContentIcon { get; set; }

    [JsonPropertyName("waveVersion")]
    public int WaveVersion { get; set; } = -1;

    [JsonPropertyName("shown")]
    public bool Shown { get; set; }

    [JsonPropertyName("importCooldownTimers")]
    public Dictionary<string, float> ImportCooldownTimers { get; set; } = new();
}

/// <summary>
/// Miroir C# de SectorInfo.ExportStat.
/// Seul "mean" est sérialisé (les autres champs sont transient en Java).
/// </summary>
public class ExportStat
{
    [JsonPropertyName("mean")]
    public float Mean { get; set; }
}