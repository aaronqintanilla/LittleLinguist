using Avalonia.Controls;
using System;
using System.Collections.Generic;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;
using System.Text;
using Avalonia.Threading;
using System.Threading;

public class StoryGenerator
{
    public static StoryGenerator Instance {get;} = new StoryGenerator();

    ChatSession? session;
    InferenceParams? inferenceParams;
    private readonly StringBuilder _currentSentence = new();

    // Cancels the story currently being generated.
    private CancellationTokenSource? _generation;

    InteractiveExecutor? executor;
    
    void InitSession()
    {
        if (executor is null) throw new NullReferenceException("Interactive Executor is null");
        session = new ChatSession(executor);
    }

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
        executor = new InteractiveExecutor(context);
        InitSession();

        inferenceParams = new InferenceParams
        {
            MaxTokens = 400,
            AntiPrompts = new List<string> {"User:", "<|user|>"},
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 1.0f, MinP = 0.05f, RepeatPenalty = 1.3f }
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
        InitSession();

        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;

        _currentSentence.Clear();
        textBlock.Text = "";

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

                OnTokenReceived(chunk, textBlock);
            }

            OnGenerationCompleted();
        }
        catch (OperationCanceledException)
        {
            // The user moved on to another story. Nothing to report.
        }
    }

    public async Task WriteStoryFromCharacter(TextBlock textBlock, string objectDescription)
    {
        if (session is null) return;

        // Cancel the previous story and stop its audio.
        StopStory();
        InitSession();

        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;

        _currentSentence.Clear();
        textBlock.Text = "";

        string input =
            $"Write a short children's story about this object: {objectDescription}\n\n" +
            "The object must be the main protagonist.\n" +
            "Do not write the object description or any kind of introduction.\n" +
            "The story must be friendly, imaginative and suitable for young children.\n" +
            "Use exactly 3 paragraphs.\n" +
            "Use simple language.\n" +
            "Do not mention that the story was generated from an object description.\n" +
            "Output only the story. Do not use Markdown or code blocks.";

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

                OnTokenReceived(chunk, textBlock);
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
    }

    // Called for every token produced by the language model.
    private void OnTokenReceived(string token, TextBlock textBlock)
    {
        // Show the token on screen straight away.
        Dispatcher.UIThread.Post(() => textBlock.Text += token);

        // Accumulate it until a full sentence is formed.
        _currentSentence.Append(token);

        if (EndsSentence(token))
            FlushSentence();
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

        if (sentence.Length > 0)
            SpeechReader.Instance.Speak(sentence);
    }

    // Called when the model has finished generating.
    private void OnGenerationCompleted()
    {
        FlushSentence();
    }

}



