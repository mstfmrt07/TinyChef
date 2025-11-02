namespace TinyChef
{
    public enum CounterType
    {
        Basic,          // Regular counter for placing items
        IngredientSupply, // Source of ingredients
        CuttingBoard,   // For chopping/processing
        Stove,          // For cooking (boiling, baking, frying)
        Dishwasher,     // For washing plates
        ServingStation  // For serving completed dishes
    }
}
