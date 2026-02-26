using MindustrySaveEditor.Core.Model;

namespace MindustrySaveEditor.Core;

public class RichSettingsData
{

    public SettingsData SettingsData { get; init; }
    
    public Dictionary<string, Dictionary<Resource, int>> ResearchEntries { get; init; } = new();
    public Dictionary<string, bool> UnlockedElements { get; init; } = new();
    public Dictionary<Sector, SectorInfo> SectorInfos { get; init; } = new();

    public RichSettingsData(SettingsData settingsData)
    {
        SettingsData = settingsData;
        
        foreach (var (key, value) in settingsData.Values)
        {
            if (key.StartsWith("req-"))
                ProcessResearchLine(key, (int)value);
            if (key.EndsWith("-info"))
                ProcessSectorInfo(key, settingsData.GetJson<SectorInfo>(key));
            if (key.EndsWith("-unlocked"))
                UnlockedElements[key[..^"-unlocked".Length]] = (bool)value;
        }
    }

    #region Processing

    private void ProcessResearchLine(string rawLine, int value)
    {
        rawLine = rawLine.Substring("req-".Length);
        
        // The usual format is '<content>-<resource id>',
        // however some resource id contains dashes, so we
        // cannot just split. We have to parse the resource id first, by first
        // trying with the last element of the split result, then if not found,
        // the last and the second last.
        
        var split = rawLine.Split('-');
        
        var resource = Resources.ById.GetValueOrDefault(split.Last()) 
                       ?? Resources.ById.GetValueOrDefault(string.Join('-', split.Skip(split.Length - 2)));

        if (resource == null)
            // throw new Exception($"Unknown resource id in research line: {rawLine}");
            return;
        
        var contentId = rawLine.Substring(0, rawLine.Length - resource.Id.Length - 1);
        
        if (!ResearchEntries.ContainsKey(contentId))
            ResearchEntries[contentId] = new Dictionary<Resource, int>();
        
        ResearchEntries[contentId][resource] = value;
    }

    private void ProcessSectorInfo(string rawLine, SectorInfo? sectorInfo)
    {
        if (sectorInfo == null)
            return;
        
        var line = rawLine.Substring(0, rawLine.Length - "-info".Length);
        var split = line.Split('-');

        var planet = split[0];
        if (planet == "serpulo")
        {
            // index 1 is a "s" to say "sector", then comes its ID
            var sectorId = int.Parse(split[2]);
            SectorInfos[Sectors.GetSector(sectorId.ToString())] = sectorInfo;
        }
        else
        {
            SectorInfos[Sectors.GetSector(line)] = sectorInfo;
        }
    }

    #endregion
    
    public void SaveToSettings()
    {
        foreach (var (contentId, requirements) in ResearchEntries)
        {
            foreach (var (resource, amount) in requirements)
            {
                SettingsData.Values[$"req-{contentId}-{resource.Id}"] = amount;
            }
        }

        foreach (var (sector, info) in SectorInfos)
        {
            SettingsData.PutJson($"{sector.Id}-info", info);
        }

        foreach (var (elementId, unlocked) in UnlockedElements)
        {
            SettingsData.Values[$"{elementId}-unlocked"] = unlocked;
        }
    }
    
}