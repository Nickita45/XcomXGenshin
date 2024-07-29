using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public enum Element
{
    Pyro,
    Cryo,
    Hydro,
    Electro,
    Anemo,
    Geo,
    Dendro,
    Physical, // for simplicity, we treat physical damage as an element
    Default = Physical 
}