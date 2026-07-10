namespace WeightliftingApi.Models;

public class Entry
{
    public int Id { get; set; }
    public string MovementName { get; set; } = "";
    public double Weight { get; set; }
    public int Reps { get; set; }
    // yyyy-MM-dd — as string to avoid timezone headaches
    public string Date { get; set; } = "";
}

public class Movement
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
