using System;
using System.Threading.Tasks;

/*
FALTA:
1. usar el finished para que ya no lleve a storyPage, si no a homePage y reiniciar todo:

        if (Session.Instance.IsFinished)
        {
            // La historia ha terminado: volvemos al inicio.
            await Navigation.PopAsync();
            return;
        }
*/

// Keeps track of how far along the story is, so no page needs
// to know which part comes next.
public class Session
{
    public static Session Instance { get; } = new Session();

    // How many middle parts to generate before the ending.
    public int MiddleCount { get; set; } = 3;

    // Parts of the story already generated (counter)
    private int _partsWritten;

    private Session() { }

    // Starts a new session from the beginning.
    public void Reset()
    {
        _partsWritten = 0;
    }

    // Generates whichever part comes next.
    public async Task WriteNextPart(string? objectDescription)
    {
        if (_partsWritten == 0)
        {
            await StoryGenerator.Instance.WriteIntroduction(objectDescription);
        }
        else if (_partsWritten <= MiddleCount)
        {
            await StoryGenerator.Instance.WriteMiddle();
        }
        else
        {
            await StoryGenerator.Instance.WriteEnding();
        }

        ++_partsWritten;
    }

    // True when the story has been closed.
    public bool IsFinished => _partsWritten > MiddleCount + 1;
}