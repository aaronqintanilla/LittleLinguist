using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;

public class VisionEngine : IDisposable
{
    public static VisionEngine Instance { get; } =
        new VisionEngine();

    private LLamaWeights? _model;
    private LLamaContext? _context;
    private MtmdWeights? _visionModel;
    private InteractiveExecutor? _executor;

    private string _mediaMarker = "<media>";

    // Evita analizar dos imágenes simultáneamente.
    private readonly SemaphoreSlim _lock = new(1, 1);

    private VisionEngine()
    {
    }

    // ---------------------------------------------------------
    // CARGAR EL MODELO
    // ---------------------------------------------------------

    public async Task LoadModel()
    {
        // Evita cargarlo dos veces.
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

        // Utilizamos CPU.
        mtmdParameters.UseGpu = false;


        //Console.WriteLine("Loaded model...");

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

    // ---------------------------------------------------------
    // IDENTIFICAR EL OBJETO
    // ---------------------------------------------------------

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
            ResetInference();

            Console.WriteLine(
                $"Embeds after reset: {_executor.Embeds.Count}"
            );

            // Convierte la imagen en información que entiende
            // el modelo multimodal.
            var imageEmbed =
                _visionModel.LoadMedia(imageData);

            _executor.Embeds.Add(imageEmbed);

            string userMessage =
                $"{_mediaMarker}\n" +
                "Identify the main physical object in this image. " +
                "Answer with only one simple English noun. " +
                "Output nothing else.";

            string prompt =
                CreatePrompt(userMessage);

            var inferenceParameters =
                new InferenceParams
                {
                    MaxTokens = 12,

                    SamplingPipeline =
                        new DefaultSamplingPipeline
                        {
                            Temperature = 0.4f
                        }
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

            Match wordMatch = Regex.Match(
                completeAnswer,
                @"[A-Za-z]+"
            );

            if (!wordMatch.Success)
            {
                throw new InvalidOperationException(
                    "The model did not return a valid object name."
                );
            }

            return wordMatch.Value.ToLowerInvariant();
        }
        finally
        {
            _lock.Release();
        }
    }

    // ---------------------------------------------------------
    // CREAR EL PROMPT CORRECTO PARA SMOLVLM
    // ---------------------------------------------------------

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

    // ---------------------------------------------------------
    // LIMPIAR LA IMAGEN ANTERIOR
    // ---------------------------------------------------------

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

        // Limpia la memoria de la conversación anterior.
        _context.NativeHandle.MemoryClear();

        // Libera los embeddings de la imagen anterior.
        foreach (var embed in _executor.Embeds)
        {
            embed.Dispose();
        }

        _executor.Embeds.Clear();

        // Limpia los datos multimedia anteriores.
        _visionModel.ClearMedia();
    }

    private void ResetInference()
    {
        if (_context is null || _visionModel is null)
            return;

        _context.NativeHandle.MemoryClear();
        _executor = new InteractiveExecutor(_context, _visionModel);
        _visionModel.ClearMedia();
    }

    // ---------------------------------------------------------
    // LIBERAR RECURSOS
    // ---------------------------------------------------------

    public void Dispose()
    {
        ClearPreviousImage();

        _visionModel?.Dispose();
        _context?.Dispose();
        _model?.Dispose();

        _lock.Dispose();
    }

}