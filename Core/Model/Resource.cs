namespace MindustrySaveEditor.Core.Model;

public record Resource(string Id, string Name, string IconPath, bool IsItem = true) : IComparable<Resource>
{

    public override string ToString() => Name;

    public string FullIconPath => $"Images/Icons/{(IsItem ? "Items" : "Fluids")}/{IconPath}.png";
    public string ImgTag => $"<img src=\"{FullIconPath}\" alt=\"{Name}\" />";

    public int CompareTo(Resource? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var idComparison = string.Compare(Id, other.Id, StringComparison.Ordinal);
        if (idComparison != 0) return idComparison;
        var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0) return nameComparison;
        var iconPathComparison = string.Compare(IconPath, other.IconPath, StringComparison.Ordinal);
        if (iconPathComparison != 0) return iconPathComparison;
        return IsItem.CompareTo(other.IsItem);
    }
}

public static class Resources
{
    // Items
    public static Resource Beryllium = new("beryllium", "Beryllium", "item-beryllium");
    public static Resource BlastCompound = new("blast-compound", "Blast Compound", "item-blast-compound");
    public static Resource Carbide = new("carbide", "Carbide", "item-carbide");
    public static Resource Coal = new("coal", "Coal", "item-coal");
    public static Resource Copper = new("copper", "Copper", "item-copper");
    public static Resource DormantCyst = new("dormant-cyst", "Dormant Cyst", "item-dormant-cyst");
    public static Resource FissileMatter = new("fissile-matter", "Fissile Matter", "item-fissile-matter");
    public static Resource Graphite = new("graphite", "Graphite", "item-graphite");
    public static Resource Lead = new("lead", "Lead", "item-lead");
    public static Resource Metaglass = new("metaglass", "Metaglass", "item-metaglass");
    public static Resource Oxide = new("oxide", "Oxide", "item-oxide");
    public static Resource PhaseFabric = new("phase-fabric", "Phase Fabric", "item-phase-fabric");
    public static Resource Plastanium = new("plastanium", "Plastanium", "item-plastanium");
    public static Resource Pyratite = new("pyratite", "Pyratite", "item-pyratite");
    public static Resource Sand = new("sand", "Sand", "item-sand");
    public static Resource Scrap = new("scrap", "Scrap", "item-scrap");
    public static Resource Silicon = new("silicon", "Silicon", "item-silicon");
    public static Resource SporePod = new("spore-pod", "Spore Pod", "item-spore-pod");
    public static Resource SurgeAlloy = new("surge-alloy", "Surge Alloy", "item-surge-alloy");
    public static Resource Thorium = new("thorium", "Thorium", "item-thorium");
    public static Resource Titanium = new("titanium", "Titanium", "item-titanium");
    public static Resource Tungsten = new("tungsten", "Tungsten", "item-tungsten");
    
    // Fluids
    public static Resource Arkycite = new Resource("arkycite", "Arkycite", "liquid-arkycite", false);
    public static Resource Cryofluid = new Resource("cryofluid", "Cryofluid", "liquid-cryofluid", false);
    public static Resource Cyanogen = new Resource("cyanogen", "Cyanogen", "liquid-cyanogen", false);
    public static Resource Gallium = new Resource("gallium", "Gallium", "liquid-gallium", false);
    public static Resource Hydrogen = new Resource("hydrogen", "Hydrogen", "liquid-hydrogen", false);
    public static Resource Neoplasm = new Resource("neoplasm", "Neoplasm", "liquid-neoplasm", false);
    public static Resource Nitrogen = new Resource("nitrogen", "Nitrogen", "liquid-nitrogen", false);
    public static Resource Oil = new Resource("oil", "Oil", "liquid-oil", false);
    public static Resource Ozone = new Resource("ozone", "Ozone", "liquid-ozone", false);
    public static Resource Slag = new Resource("slag", "Slag", "liquid-slag", false);
    public static Resource Water = new Resource("water", "Water", "liquid-water", false);
    
    public static List<Resource> All =
    [
        // Items
        Beryllium, BlastCompound, Carbide, Coal, Copper, DormantCyst, FissileMatter, Graphite, Lead,
        Metaglass, Oxide, PhaseFabric, Plastanium, Pyratite, Sand, Scrap, Silicon, SporePod,
        SurgeAlloy, Thorium, Titanium, Tungsten,
        
        // Fluids
        Arkycite, Cryofluid, Cyanogen, Gallium, Hydrogen, Neoplasm, Nitrogen, Oil, Ozone, Slag, Water
    ];
    
    public static Dictionary<string, Resource> ById = All.ToDictionary(r => r.Id);
}