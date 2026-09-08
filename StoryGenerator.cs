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
using System.Text.Json;
using LittleLinguist;
using System.Linq;

/*
FALTA:
1. base de datos para que cambie la historia, siempre es repetitiva
2. que genere diferentes momentos de la historia segun en que fase estamos: inicio, nudo, desenlace? -> mejorarlos NO VA AGHHHH !!!
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



    InteractiveExecutor? executor;

    private LLamaWeights? _model;
    private ModelParams? _modelParams;
    
    void InitSession()
    {
        if (executor is null) throw new NullReferenceException("Interactive Executor is null");
        session = new ChatSession(executor);
    }

    public async Task LoadModel()
    {
        //DESCARGAR y guardar en models
        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/gemma-3-1b-it-q4_0.gguf";

        _modelParams = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 0
        };

        Console.WriteLine("Loading model...");

        _model = LLamaWeights.LoadFromFile(
            _modelParams
        );

        var context =
            _model.CreateContext(_modelParams);

        executor =
            new InteractiveExecutor(context);

        InitSession();

        inferenceParams = new InferenceParams
        {
            MaxTokens = 400,
            AntiPrompts = new List<string> { "User:", "<|user|>" },
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 1.0f, MinP = 0.05f, RepeatPenalty = 1.3f }
        };
    }

    // Starts a new story.
    public async Task WriteIntroduction(string? objectDescription)
    {
        if (objectDescription is null)
        {
            await Generate(
                "Write exactly the first THREE paragraphs of a children's story. " +
                "Output only the story. The response must begin with Once upon a time. " +
                "Do not include introductions, explanations or comments. " +
                "End your response immediately after the third paragraph.");
        }
        else
        {
            await Generate(
                "Write exactly the first THREE paragraphs of a children's story. " +
                $"The protagonist of the story must be this object: {objectDescription}\n" +
                "Output only the story. The response must begin with Once upon a time. " +
                "Do not include introductions, explanations or comments. " +
                "And do not mention that the story was generated from an object description. " +
                "End your response immediately after the third paragraph. " +
                "Do not use Markdown or code blocks.");
        }
    }

    // Continues the story without closing it. Can be called many times.
    public async Task WriteMiddle()
    {
        await Generate(
            "Continue the previous story with exactly THREE more paragraphs. " +
            "Do not end the story: leave it open for more to happen. " +
            "Output only the story. Do not repeat what you already wrote. " +
            "Do not include introductions, explanations or comments. " +
            "End your response immediately after the third paragraph.");
    }

    // Brings the story to an end.
    public async Task WriteEnding()
    {
        await Generate(
            "Write the final THREE paragraphs of the story. " +
            "Bring it to a happy and satisfying ending. " +
            "Output only the story. Do not repeat what you already wrote. " +
            "Do not include introductions, explanations or comments. " +
            "End your response immediately after the third paragraph.");
    }

    // Generates a story and writes it on screen token by token,
    // sending each finished sentence to be read aloud.
    private async Task Generate(string prompt)
    {
        if (session is null) return;

        // Cancel the previous story and stop its audio.
        StopStory();
        InitSession();

        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;

        string generatedText = "";  //Control

        Console.WriteLine("----- PROMPT -----");
        Console.WriteLine(prompt);

        try
        {
            var message = new ChatHistory.Message(AuthorRole.User, prompt);

            await foreach (var chunk in session.ChatAsync(message, inferenceParams, token))
            {
                token.ThrowIfCancellationRequested();

                generatedText += chunk;
                if (generatedText.Contains("```"))
                    break;

                OnTokenReceived(chunk);
            }
            OnGenerationCompleted();

        }
        catch (OperationCanceledException)
        {
            // The user moved on to another story.
        }
    }

    public async Task WriteStoryFromCharacter(string objectDescription)
    {
        if (session is null) return;

        // Cancel the previous story and stop its audio.
        StopStory();
        InitSession();

        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;

        _currentSentence.Clear();

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

                OnTokenReceived(chunk);
            }

            OnGenerationCompleted();
        }
        catch (OperationCanceledException)
        {
            // The user moved on to another story. Nothing to report.
        }
    }

   public async Task<List<ReadingQuestion>> GenerateReadingQuestions(
    string story)
    {
        var questions = new List<ReadingQuestion>();

        if (executor is null)
        {
            Console.Error.WriteLine("Language model is not loaded.");
            return questions;
        }

        if (string.IsNullOrWhiteSpace(story))
        {
            Console.Error.WriteLine("Story is empty.");
            return questions;
        }

        Console.WriteLine("----- STORY FOR QUESTIONS -----");
        Console.WriteLine(story);

        if (_model is null ||
        _modelParams is null)
        {
            Console.Error.WriteLine(
                "Language model is not loaded."
            );

            return questions;
        }

        string prompt =
            "Read this children's story:\n\n" +
            story +
            "\n\n" +

            "Create ONE very easy multiple-choice reading comprehension question.\n" +
            "Use only information explicitly written in the story.\n" +
            "Give exactly 3 short answer options.\n" +
            "The FIRST option must be the correct answer.\n" +
            "The SECOND and THIRD options must be clearly WRONG.\n" +
            "Wrong answers must NOT be paraphrases of the correct answer.\n" +
            "Wrong answers must refer to clearly different places, objects, people or actions.\n" +
            "Do not repeat the same fact using different words.\n" +
            "Keep all answers short.\n\n" +

            "Return ONLY ONE line using exactly this format:\n" +
            "question|correct answer|wrong answer|wrong answer\n\n" +

            "Good example:\n" +
            "Where did Mia live?|In the forest|At the beach|In a castle\n\n" +

            "BAD example - do NOT do this:\n" +
            "Where did Mia live?|In the forest|The Green Forest|She lived in the forest\n";

        var parameters = new InferenceParams
        {
            MaxTokens = 70,

            AntiPrompts = new List<string>
            {
                "User:",
                "<|user|>"
            },

            SamplingPipeline =
                new DefaultSamplingPipeline
                {
                    Temperature = 0.1f,
                    MinP = 0.05f,
                    RepeatPenalty = 1.1f
                }
        };

        var generated =
    new StringBuilder();

    try
    {
        // Para las preguntas usamos una inferencia
        // independiente de la historia.
        var questionExecutor =
            new StatelessExecutor(
                _model,
                _modelParams)
            {
                ApplyTemplate = true
            };

        await foreach (
            string chunk
            in questionExecutor.InferAsync(
                prompt,
                parameters))
        {
            generated.Append(chunk);
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(
            "QUESTION GENERATION ERROR:"
        );

        Console.Error.WriteLine(ex);

        return questions;
    }
        

        string text = generated
            .ToString()
            .Trim();

        Console.WriteLine(
            "----- GENERATED QUESTIONS -----"
        );

        Console.WriteLine(text);

        // Cada pregunta debe tener:
        //
        // pregunta | opción1 | opción2 | opción3 | respuesta

        string[] lines = text.Split(
    '\n',
    StringSplitOptions.RemoveEmptyEntries
);

    foreach (string rawLine in lines)
    {
        string line = rawLine.Trim();

        string[] parts =
            line.Split('|');

        // Ahora esperamos 4 partes:
        // pregunta | correcta | incorrecta | incorrecta
        if (parts.Length != 4)
            continue;

        string question =
            parts[0].Trim();

        string correct =
            parts[1].Trim();

        string wrong1 =
            parts[2].Trim();

        string wrong2 =
            parts[3].Trim();

        if (string.IsNullOrWhiteSpace(question) ||
            string.IsNullOrWhiteSpace(correct) ||
            string.IsNullOrWhiteSpace(wrong1) ||
            string.IsNullOrWhiteSpace(wrong2))
        {
            continue;
        }

        // Guardamos las opciones junto con si son correctas.
        var options = new List<(string Text, bool Correct)>
        {
            (correct, true),
            (wrong1, false),
            (wrong2, false)
        };

        // Las mezclamos para que la correcta
        // no aparezca siempre la primera.
        options = options
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        int correctIndex =
            options.FindIndex(x => x.Correct);

        questions.Add(
            new ReadingQuestion
            {
                Question = question,

                Options = options
                    .Select(x => x.Text)
                    .ToList(),

                CorrectAnswer = correctIndex
            }
        );
        break;
    }

        Console.WriteLine(
            $"Parsed questions: {questions.Count}"
        );

        return questions;
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

    public void StopStoryKeepingText()
    {
        _generation?.Cancel();
        _generation?.Dispose();
        _generation = null;

        SpeechReader.Instance.Stop();

        _currentSentence.Clear();

        // IMPORTANTE:
        // NO borramos _sentences
        // NO avisamos a la interfaz con una lista vacía
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
        string raw = _currentSentence.ToString();
        _currentSentence.Clear();

        string sentence = raw.Trim();

        if (sentence.Length == 0)
        {
            if (raw.Contains('\n') && _sentences.Count > 0)
            {
                _sentences[^1] += "\n";
                SentencesChanged?.Invoke(new List<string>(_sentences));
            }

            return;
        }

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



