namespace TinyChef
{
    public enum CounterType
    {
        Basic,          // Regular counter for placing items
        IngredientSupply, // Source of ingredients
        CuttingBoard,   // For chopping/processing
        Stove,          // For cooking (boiling, baking, frying)
        Dishwasher,     // For washing plates
        ServingStation, // For serving completed dishes
        WasteBin,       // For disposing unwanted items
        Portal          // Portal for teleporting between counter groups
    }

    public enum PortalColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange
    }
}
