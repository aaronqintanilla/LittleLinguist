using System.Collections.Generic;

namespace LittleLinguist;

// Represents a reading comprehension question and its answer options.
public class ReadingQuestion
{
    public string Question { get; set; } = "";

    public List<string> Options { get; set; } = new();

    public int CorrectAnswer { get; set; }
}