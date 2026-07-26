using Avalonia.Controls;
using System;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Threading.Tasks;

public class StoryGenerator
{
    public static StoryGenerator Instance {get;} = new StoryGenerator();

    ChatSession? session;
    InferenceParams? inferenceParams;

    private StoryGenerator(){}

    public async Task LoadModel()
    {
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
            SamplingPipeline = new DefaultSamplingPipeline { Temperature = 1.0f, MinP = 0.0f }
        };
    }

    public async Task WriteStory(TextBlock textBlock)
    {
        if(session is null) return;

        string input = "Write exactly the first THREE paragraphs of a children's story. Output only the story. The response must begin with Once upon a time. Do not include introductions, explanations or comments. Write exactly the first three paragraphs of the story. End your response immediately after the third paragraph.";

        string generatedText = "";  //Control

        await foreach (var token in session.ChatAsync(new ChatHistory.Message(AuthorRole.User, input), inferenceParams))
        {
            //Control
            generatedText += token;
            if (generatedText.Contains("```")) break;

            textBlock.Text += token;
        }
    }
}



