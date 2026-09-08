using System.Collections.Generic;

namespace LittleLinguist;

public class ReadingQuestion
{
    public string Question { get; set; } = "";

    public List<string> Options { get; set; } = new();

    public int CorrectAnswer { get; set; }
}