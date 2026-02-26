namespace MindustrySaveEditor.Core.Model;

public record Sector(string Id, int? NumericId, string? DisplayName, string? Planet)
{
    public string FullDisplayName => DisplayName ?? Id;
    public string? FullImagePath => DisplayName != null ? $"Images/Icons/Sectors/sector-{Id}.png" : null;
    
    public override string ToString() => FullDisplayName;
    
    public bool IsNamed => DisplayName != null;
}

public static class Sectors
{
    // Named sectors (Serpulo - story sectors with icons)
    public static readonly Sector GroundZero = new("groundZero", 15, "Ground Zero", "serpulo");
    public static readonly Sector FrozenForest = new("frozenForest", 86, "Frozen Forest", "serpulo");
    public static readonly Sector TheCraters = new("craters", 18, "The Craters", "serpulo");
    public static readonly Sector BiomassFacility = new("biomassFacility", 81, "Biomass Synthesis Facility", "serpulo");
    public static readonly Sector RuinousShores = new("ruinousShores", 213, "Ruinous Shores", "serpulo");
    public static readonly Sector StainedMountains = new("stainedMountains", 20, "Stained Mountains", "serpulo");
    public static readonly Sector Facility32M = new("facility32m", 64, "Facility 32M", "serpulo");
    public static readonly Sector FungalPass = new("fungalPass", 21, "Fungal Pass", "serpulo");
    public static readonly Sector InfestedCanyons = new("infestedCanyons", 210, "Infested Canyons", "serpulo");
    public static readonly Sector WindsweptIslands = new("windsweptIslands", 246, "Windswept Islands", "serpulo");
    public static readonly Sector Frontier = new("frontier", 50, "Frontier", "serpulo");
    public static readonly Sector Overgrowth = new("overgrowth", 134, "Overgrowth", "serpulo");
    public static readonly Sector TarFields = new("tarFields", 23, "Tar Fields", "serpulo");
    public static readonly Sector TestingGrounds = new("testingGrounds", null, "Testing Grounds", "serpulo");
    public static readonly Sector NuclearComplex = new("nuclearComplex", 130, "Nuclear Production Complex", "serpulo");
    public static readonly Sector ExtractionOutpost = new("extractionOutpost", 165, "Extraction Outpost", "serpulo");
    public static readonly Sector SaltFlats = new("saltFlats", 101, "Salt Flats", "serpulo");
    public static readonly Sector Atolls = new("atolls", null, "Atolls", "serpulo");
    public static readonly Sector TaintedWoods = new("taintedWoods", 221, "Tainted Woods", "serpulo");
    public static readonly Sector Coastline = new("coastline", 108, "Coastline", "serpulo");
    public static readonly Sector NavalFortress = new("navalFortress", 216, "Naval Fortress", "serpulo");
    public static readonly Sector MycelialBastion = new("mycelialBastion", 260, "Mycelial Bastion", "serpulo");
    public static readonly Sector Impact0078 = new("impact0078", 227, "Impact 0078", "serpulo");
    public static readonly Sector DesolateRift = new("desolateRift", 123, "Desolate Rift", "serpulo");
    public static readonly Sector WeatheredChannels = new("weatheredChannels", 39, "Weathered Channels", "serpulo");

    public static readonly Sector PlanetaryTerminal =
        new("planetaryTerminal", 93, "Planetary Launch Terminal", "serpulo");

    public static readonly Sector GeothermalStronghold =
        new("geothermalStronghold", 264, "Geothermal Stronghold", "serpulo");

    public static readonly Sector Cruxscape = new("cruxscape", 54, "Cruxscape", "serpulo");

    // Named sectors (Erekir)
    public static readonly Sector TheOnset = new("onset", null, "The Onset", "erekir");
    public static readonly Sector Aegis = new("aegis", null, "Aegis", "erekir");
    public static readonly Sector Lake = new("lake", null, "Lake", "erekir");
    public static readonly Sector Intersect = new("intersect", null, "Intersect", "erekir");
    public static readonly Sector Atlas = new("atlas", null, "Atlas", "erekir");
    public static readonly Sector Split = new("split", null, "Split", "erekir");
    public static readonly Sector Basin = new("basin", null, "Basin", "erekir");
    public static readonly Sector Marsh = new("marsh", null, "Marsh", "erekir");
    public static readonly Sector Peaks = new("peaks", null, "Peaks", "erekir");
    public static readonly Sector Ravine = new("ravine", null, "Ravine", "erekir");
    public static readonly Sector Caldera = new("caldera-erekir", null, "Caldera", "erekir");
    public static readonly Sector Stronghold = new("stronghold", null, "Stronghold", "erekir");
    public static readonly Sector Crevice = new("crevice", null, "Crevice", "erekir");
    public static readonly Sector Siege = new("siege", null, "Siege", "erekir");
    public static readonly Sector Crossroads = new("crossroads", null, "Crossroads", "erekir");
    public static readonly Sector Karst = new("karst", null, "Karst", "erekir");
    public static readonly Sector Origin = new("origin", null, "Origin", "erekir");

    public static readonly Sector[] All = new[]
    {
        GroundZero, FrozenForest, TheCraters, BiomassFacility, RuinousShores, StainedMountains, Facility32M, FungalPass,
        InfestedCanyons, WindsweptIslands,
        Frontier, Overgrowth, TarFields, TestingGrounds, NuclearComplex, ExtractionOutpost, SaltFlats, Atolls,
        TaintedWoods, Coastline,
        NavalFortress, MycelialBastion, Impact0078, DesolateRift, WeatheredChannels, PlanetaryTerminal,
        GeothermalStronghold, Cruxscape,

        TheOnset, Aegis, Lake, Intersect, Atlas, Split, Basin, Marsh, Peaks, Ravine,
        Caldera, Stronghold, Crevice, Siege, Crossroads, Karst, Origin
    };
    
    public static Dictionary<int, Sector> ByNumericId => All.Where(s => s.NumericId.HasValue)
        .ToDictionary(s => s.NumericId!.Value, s => s);

    public static Sector GetSector(string id)
    {
        Console.WriteLine($"Searching for sector with id: {id}");
        if (int.TryParse(id, out var numId) && ByNumericId.TryGetValue(numId, out var sectorByNumId))
            return sectorByNumId;

        var stringSearch = All.FirstOrDefault(s => s.Id == id);

        if (stringSearch != null) 
            return stringSearch;
        
        var numericId = int.TryParse(id, out var parsedId) ? parsedId : (int?) null;
        return new Sector(id, numericId, null, null);
    }
}