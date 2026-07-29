using Avalonia.Controls;
using System;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;
using System.Text;
using Avalonia.Threading;
using System.Threading;
using System.Collections.Generic;

/*
FALTA:
1. base de datos para que cambie la historia, siempre es repetitiva
2. que genere diferentes momentos de la historia segun en que fase estamos: inicio, nudo, desenlace?
*/

public class StoryGenerator
{
    public static StoryGenerator Instance {get;} = new StoryGenerator();

    // Raised whenever the list of sentences changes, so the
    // interface can redraw the story.
    public event Action<List<string>>? SentencesChanged;

    ChatSession? session;
    InferenceParams? inferenceParams;
    private readonly StringBuilder _currentSentence = new();

    // All sentences generated so far, in order.
    private readonly List<string> _sentences = new();

    // Cancels the story currently being generated.
    private CancellationTokenSource? _generation;

    private StoryGenerator(){}

    public async Task LoadModel()
    {
        //DESCARGAR y guardar en models
        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/gemma-3-1b-it-q4_0.gguf";

        var parameters = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 0
        };

        Console.WriteLine("Loading model...");
        var model = LLamaWeights.LoadFromFile(parameters);

        var context = model.CreateContext(parameters);
        var executor = new InteractiveExecutor(context);
        session = new ChatSession(executor);

        inferenceParams = new InferenceParams
        {
            MaxTokens = 400,
            //AntiPrompts = new List<string> { "<|im_end|>", "<|im_start|>" },
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 1.2f, MinP = 0.05f }
        };
    }

    // Generates a story and writes it on screen token by token,
    // sending each finished sentence to be read aloud.
    // Calling it again cancels any story still being generated.
    public async Task WriteStory(TextBlock textBlock)
    {
        if (session is null) return;

        // Cancel the previous story and stop its audio.
        StopStory();

        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;

        _currentSentence.Clear();

        string input = "Write exactly the first THREE paragraphs of a children's story. Output only the story. The response must begin with Once upon a time. Do not include introductions, explanations or comments. Write exactly the first three paragraphs of the story. End your response immediately after the third paragraph.";

        string generatedText = "";  //Control

        try
        {
            var message = new ChatHistory.Message(AuthorRole.User, input);

            await foreach (var chunk in session.ChatAsync(message, inferenceParams, token))
            {
                token.ThrowIfCancellationRequested();

                //Control
                generatedText += chunk;
                if (generatedText.Contains("```")) break;

                OnTokenReceived(chunk);
            }

            OnGenerationCompleted();
        }
        catch (OperationCanceledException)
        {
            // The user moved on to another story. Nothing to report.
        }
    }

    // Cancels the story being generated and stops any audio.
    public void StopStory()
    {
        _generation?.Cancel();
        _generation?.Dispose();
        _generation = null;

        SpeechReader.Instance.Stop();

        _currentSentence.Clear();
        _sentences.Clear();

        SentencesChanged?.Invoke(new List<string>());
    }

    // Called for every token produced by the language model.
    private void OnTokenReceived(string token)
    {
        // Accumulate it until a full sentence is formed.
        _currentSentence.Append(token);

        if (EndsSentence(token))
        {
            FlushSentence();
        }
        else
        {
            // Show the sentence being written, still incomplete.
            var preview = new List<string>(_sentences)
            {
                _currentSentence.ToString()
            };

            SentencesChanged?.Invoke(preview);
        }
    }

    // Returns true when the token closes a sentence.
    private bool EndsSentence(string token)
    {
        return token.Contains('.')
            || token.Contains('!')
            || token.Contains('?')
            || token.Contains('\n');
    }

    // Sends the accumulated sentence to be spoken and starts a new one.
    private void FlushSentence()
    {
        string sentence = _currentSentence.ToString().Trim();
        _currentSentence.Clear();

        if (sentence.Length == 0) return;

        _sentences.Add(sentence);
        SpeechReader.Instance.Speak(sentence);

        SentencesChanged?.Invoke(new List<string>(_sentences));
    }

    // Called when the model has finished generating.
    private void OnGenerationCompleted()
    {
        FlushSentence();
    }
}



