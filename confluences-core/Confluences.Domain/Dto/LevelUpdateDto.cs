public class LevelUpdateDto
{
    public string Field { get; set; } // "oralLevel" ou "writtenLevel"
    public string Value { get; set; } // "A1.1", "B2", etc.
    
    // NOUVELLES PROPRIÉTÉS POUR L'HISTORISATION
    public int Year { get; set; }
    public int Quarter { get; set; } // 1, 2, 3 ou 4
}