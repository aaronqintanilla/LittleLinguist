using System;
using System.Collections.Generic;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using LittleLinguist;
using System.Linq;
using LLama.Transformers;

/*
TO DO:
1. Increase story variety. The generator currently tends to reuse the same
   characters, locations, and actions. Possible solutions include using a
   small database of predefined elements or testing a more powerful AI model,
   while considering the Arduino's memory limitations.

2. Improve story generation. Sometimes the model generates incoherent content
   or includes parts of the prompt in the story. Possible solutions include
   improving the prompts or testing another AI model, while considering the
   available memory and hardware limitations.

3. Adapt the story to the user's learning level. Use the results of the
   learning activities to adjust the difficulty over time. For example,
   generate shorter stories with simpler words for users who need more
   practice, or longer stories with more advanced vocabulary as they improve.
*/

// Generates children's stories and reading comprehension questions using a language model.
public class StoryGenerator
{
    public static StoryGenerator Instance {get;} = new StoryGenerator();
    public event Action<List<string>>? SentencesChanged;
    ChatSession? session;
    private LLamaContext? context;
    private readonly StringBuilder _currentSentence = new();
    private readonly List<string> _sentences = new();
    private LLamaWeights? model;
    private ModelParams? parameters;
    private CancellationTokenSource? _generation;
    InteractiveExecutor? executor;
    
    // Initializes a new chat session for story generation.
    void InitSession()
    {
        if (executor is null) throw new NullReferenceException("Interactive Executor is null");
        if (model is null) throw new NullReferenceException("Model is null");
        session = new ChatSession(executor);
        session.WithHistoryTransform(
            new PromptTemplateTransformer(model, withAssistant: true)
        );
        context?.NativeHandle.MemoryClear();
        if(model is not null && parameters is not null)
        {
            context = model.CreateContext(parameters);
            executor = new InteractiveExecutor(context);
        }
    }

    // Loads the language model and initializes the inference context.
    public async Task LoadModel()
    {
        // Downloads and saves the model in the models folder.
        string modelPath = $"{System.IO.Directory.GetCurrentDirectory()}/models/gemma-3-1b-it-q4_0.gguf";

        parameters = new ModelParams(modelPath)
        {
            ContextSize = 8192,
            GpuLayerCount = 0
        };

        Console.WriteLine("Loading model...");
        model = LLamaWeights.LoadFromFile(parameters);

        context = model.CreateContext(parameters);
        executor = new InteractiveExecutor(context);
        InitSession();
    }

    // Returns a GBNF grammar that forces the model to output only three paragraphs
    public string GetStoryGrammar()
    {
        return """
            root ::= paragraph "\n\n" paragraph "\n\n" paragraph "\n"*

            paragraph ::= [^\r\n]+
            """;
    }

    // Creates the parameters used for story generation.
    private InferenceParams CreateInferenceParams()
    {
        return new InferenceParams
        {
            MaxTokens = 400,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.7f,
                MinP = 0.05f,
                RepeatPenalty = 1.1f,
                Grammar = new Grammar(GetStoryGrammar(), "root")
            }
        };
    }

    // Prints the current chat history to the console for debugging.
    public void DebugPrintChatHistory()
    {
        if (session is null)
        {
            Console.WriteLine("[DEBUG] Session is null.");
            return;
        }

        Console.WriteLine("\n=================== CHAT SESSION HISTORY DEBUG ===================");
        
        int messageIndex = 0;
        foreach (var message in session.History.Messages)
        {
            Console.WriteLine($"\n--- [Msg #{messageIndex++}] Role: {message.AuthorRole} ---");
            Console.WriteLine(message.Content);
        }

        Console.WriteLine("\n==================================================================\n");
    }

    // Starts a new story and generates its introduction.
    public async Task WriteIntroduction(string? objectDescription)
    {
        InitSession();

        if (objectDescription is null)
        {
            await Generate(
                "Write exactly the first THREE paragraphs of a children's story. " +
                "Output only the story. The response must begin with Once upon a time. " +
                "Do not include introductions, explanations or comments. " +
                "End your response immediately after the third paragraph. ");
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
        InitSession();
        string storyContext = string.Join("\n\n", _sentences);

        await Generate(
            "Here's the story so far:\n\n" +
            storyContext +
            "\n\n" +
            "Continue the previous story with exactly THREE more paragraphs. " +
            "Do not end the story: leave it open for more to happen. " +
            "Output only the story. Do not repeat what you already wrote. " +
            "Do not include introductions, explanations or comments. " +
            "End your response immediately after the third paragraph.");
    }

    // Brings the story to an end.
    public async Task WriteEnding()
    {
        InitSession();
        string storyContext = string.Join("\n\n", _sentences);

        await Generate(
            "Here's the story so far:\n\n" +
            storyContext +
            "\n\n" +
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

        PauseStory();
        _generation = new CancellationTokenSource();
        CancellationToken token = _generation.Token;
        string generatedText = "";

        int tokens = 0;
        foreach (var message in session.History.Messages)
        {
            foreach (var chunk in message.Content)
            {
                tokens++;
            }
        }

        try
        {
            var message = new ChatHistory.Message(AuthorRole.User, prompt);
            var requestParams = CreateInferenceParams();

            await foreach (var chunk in session.ChatAsync(message, requestParams, token))
            {
                token.ThrowIfCancellationRequested();

                generatedText += chunk;

                OnTokenReceived(chunk);
            }

            OnGenerationCompleted();

        }
        catch (OperationCanceledException)
        {
        }

        DebugPrintChatHistory();
        Console.WriteLine("CURRENT TOKEN COUNT: " + tokens);
    }

    // Generates a simple reading comprehension question from the story.
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

        if (model is null || parameters is null)
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

        var questionInfParams = new InferenceParams
        {
            MaxTokens = 70,

            AntiPrompts = new List<string>
            {
                "<end_of_turn>",
                "</end_of_turn>"
            },

            SamplingPipeline =
                new DefaultSamplingPipeline
                {
                    Temperature = 0.1f,
                    MinP = 0.05f,
                    RepeatPenalty = 1.1f
                }
        };

        var generated = new StringBuilder();

        try
        {
            var questionExecutor =
                new StatelessExecutor(
                    model,
                    parameters)
                {
                    ApplyTemplate = true
                };

            await foreach (
                string chunk
                in questionExecutor.InferAsync(
                    prompt,
                    questionInfParams))
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

        string[] lines = text.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            string[] parts =
                line.Split('|');

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

            var options = new List<(string Text, bool Correct)>
            {
                (correct, true),
                (wrong1, false),
                (wrong2, false)
            };

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

    // Pauses story generation and clears the current story content.
    public void PauseStory()
    {
        _generation?.Cancel();
        _generation?.Dispose();
        _generation = null;

        SpeechReader.Instance.Stop();

        _currentSentence.Clear();
        _sentences.Clear();
    }

    // Cancels the story being generated and stops any audio.
    public void StopStory()
    {
        PauseStory();
        _sentences.Clear();
        //context?.NativeHandle.MemoryClear();
        //context?.Dispose();
        /*if(model is not null && parameters is not null)
        {
            context = model.CreateContext(parameters);
            executor = new InteractiveExecutor(context);
        }
        InitSession();*/
        Session.Instance.Reset();
        SentencesChanged?.Invoke(new List<string>());
    }

    // Stops story generation and audio without clearing the generated text.
    public void StopStoryKeepingText()
    {
        _generation?.Cancel();
        _generation?.Dispose();
        _generation = null;

        SpeechReader.Instance.Stop();

        _currentSentence.Clear();

        // IMPORTANT:
        // DO NOT clear _sentences.
        // DO NOT notify the interface with an empty list.
    }

    // Called for every token produced by the language model.
    private void OnTokenReceived(string token)
    {
        _currentSentence.Append(token);

        if (EndsSentence(token))
        {
            FlushSentence();
        }
        else
        {
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



