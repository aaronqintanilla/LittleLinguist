using System;
using System.Threading.Tasks;

// Keeps track of the current story session and its progress.
public class Session
{
    public static Session Instance { get; } = new Session();
    private int _partsWritten;
    private Session() { }

    // Starts a new session from the beginning.
    public void Reset()
    {
        _partsWritten = 0;
    }

    // Generates the next part of the story.
    public async Task WriteNextPart(string? objectDescription)
    {
        Console.WriteLine($"Parte {_partsWritten}");
        switch (_partsWritten)
        {
            case 0:
                await StoryGenerator.Instance.WriteIntroduction(objectDescription);
                break;
            case 1:
                await StoryGenerator.Instance.WriteMiddle();
                break;
            case 2:
                await StoryGenerator.Instance.WriteEnding();
                break;

            default:
                return;
        }

        _partsWritten++;
    }

    // True when the story has been closed.
    public bool IsFinished => _partsWritten >= 3;
}