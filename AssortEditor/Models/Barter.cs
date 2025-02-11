


namespace AssortEditor.Models;

public class Barter
{
    public string _tpl { get; set; } // Template ID for the barter item
    public int Count { get; set; }    // Count of the barter item
    
    public int? Level { get; set; }
    
    public string? Side { get; set; }
}