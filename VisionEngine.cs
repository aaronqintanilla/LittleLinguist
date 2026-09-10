using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using Llama.Grammar.Service;

// Stores the object name and its short description.
public class Character
{
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
}

// Handles image analysis and object identification using the SmolVLM vision model.
public class VisionEngine : IDisposable
{
    public static VisionEngine Instance { get; } =
        new VisionEngine();

    private LLamaWeights? _model;
    private LLamaContext? _context;
    private MtmdWeights? _visionModel;
    private InteractiveExecutor? _executor;
    private string _mediaMarker = "<media>";
    private readonly SemaphoreSlim _lock = new(1, 1);

    // Forces a specific structure for the generated text: name of object & description
    public string GetCharacterGrammar()
    {
        var grammar = new GbnfGrammar();
        var gbnf = grammar.ConvertTypeToGbnf<Character>();

        gbnf = gbnf.Replace("(root-Name)?", "root-Name");
        gbnf = gbnf.Replace("(root-Description)?", "root-Description");

        return gbnf;
    }

    private VisionEngine()
    {
    }

    // Loads the SmolVLM vision model and initializes the image processing context.
    public async Task LoadModel()
    {
        if (_model is not null)
        {
            return;
        }

        string modelsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "models"
        );

        string modelPath = Path.Combine(
            modelsFolder,
            "SmolVLM-256M-Instruct-Q8_0.gguf"
        );

        string mmprojPath = Path.Combine(
            modelsFolder,
            "mmproj-SmolVLM-256M-Instruct-Q8_0.gguf"
        );

        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException(
                "SmolVLM model not found.",
                modelPath
            );
        }

        if (!File.Exists(mmprojPath))
        {
            throw new FileNotFoundException(
                "SmolVLM mmproj not found.",
                mmprojPath
            );
        }

        Console.WriteLine("Loading SmolVLM vision model...");

        var modelParameters = new ModelParams(modelPath)
        {
            ContextSize = 4096,
            GpuLayerCount = 0
        };

        var mtmdParameters =
            MtmdContextParams.Default();

        mtmdParameters.UseGpu = false;

        try
        {
            Console.WriteLine($"Model path: {modelPath}");
            Console.WriteLine($"Model size: {new FileInfo(modelPath).Length} bytes");

            _model = await LLamaWeights.LoadFromFileAsync(
                modelParameters
            );

            Console.WriteLine("Model loaded successfully.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("MODEL LOAD ERROR:");
            Console.Error.WriteLine(ex.ToString());
            throw;
        }
        _context =
            _model.CreateContext(modelParameters);

        _visionModel =
            await MtmdWeights.LoadFromFileAsync(
                mmprojPath,
                _model,
                mtmdParameters
            );

        if (!_visionModel.SupportsVision)
        {
            throw new InvalidOperationException(
                "The loaded model does not support images."
            );
        }
        else
        {
            Console.WriteLine(
                "Vision model supports images."
            );
        }

        _executor = new InteractiveExecutor(
            _context,
            _visionModel
        );

        _mediaMarker =
            mtmdParameters.MediaMarker
            ?? NativeApi.MtmdDefaultMarker()
            ?? "<media>";

        Console.WriteLine("SmolVLM loaded successfully.");
    }

    // Analyzes an image and identifies its main physical object.
    public async Task<string> IdentifyObject(
        byte[] imageData)
    {

        if (
            _model is null ||
            _context is null ||
            _visionModel is null ||
            _executor is null
        )
        {
            throw new InvalidOperationException(
                "Call LoadModel() before IdentifyObject()."
            );
        }

        await _lock.WaitAsync();

        try
        {
            Console.WriteLine(
                $"Embeds before reset: {_executor.Embeds.Count}"
            );

            ClearPreviousImage();

            Console.WriteLine(
                $"Embeds after reset: {_executor.Embeds.Count}"
            );

            var imageEmbed =
                _visionModel.LoadMedia(imageData);

            _executor.Embeds.Add(imageEmbed);
            
            string userMessage =
                $"{_mediaMarker}\n" +
                "Identify the main physical object. Then return a JSON schema with the common English name of the object and a short description with no more than 10 simple words.";

            string prompt =
                CreatePrompt(userMessage);

            var inferenceParameters =
                new InferenceParams
                {
                    MaxTokens = 64,

                    SamplingPipeline =
                        new DefaultSamplingPipeline
                        {
                            Grammar = new Grammar(GetCharacterGrammar(), "root"),
                            Temperature = 0.0f
                        },
                };

            var answer = new StringBuilder();

            await foreach (
                string token in _executor.InferAsync(
                    prompt,
                    inferenceParameters
                )
            )
            {
                answer.Append(token);
            }

            string completeAnswer =
                answer.ToString().Trim();

            Console.WriteLine(
                $"Vision raw response: {completeAnswer}"
            );

            return completeAnswer;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Creates the prompt using the format expected by the SmolVLM model.
    private string CreatePrompt(string userMessage)
    {
        if (_model is null)
        {
            throw new InvalidOperationException(
                "The model is not loaded."
            );
        }

        var history = new ChatHistory();

        history.AddMessage(
            AuthorRole.User,
            userMessage
        );

        var template =
            new LLamaTemplate(_model.NativeHandle)
            {
                AddAssistant = true
            };

        foreach (var message in history.Messages)
        {
            template.Add(
                message.AuthorRole
                    .ToString()
                    .ToLowerInvariant(),

                message.Content
            );
        }

        return LLamaTemplate.Encoding.GetString(
            template.Apply()
        );
    }

    // Clears the previous image data and its embeddings before processing a new image.
    private void ClearPreviousImage()
    {
        if (
            _context is null ||
            _visionModel is null ||
            _executor is null
        )
        {
            return;
        }

        _context.NativeHandle.MemoryClear();

        foreach (var embed in _executor.Embeds)
        {
            embed.Dispose();
        }

        _executor.Embeds.Clear();

        _executor = new InteractiveExecutor(_context, _visionModel);

        _visionModel.ClearMedia();
    }

    // Releases the vision model, context, embeddings, and synchronization resources.
    public void Dispose()
    {
        ClearPreviousImage();

        _visionModel?.Dispose();
        _context?.Dispose();
        _model?.Dispose();

        _lock.Dispose();
    }

}