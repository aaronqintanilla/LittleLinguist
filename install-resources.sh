#!/bin/bash
set -e

# Check required system dependencies
if ! command -v curl >/dev/null 2>&1; then
    echo "curl is not installed."

    if command -v apt-get >/dev/null 2>&1; then
        sudo apt-get update
        sudo apt-get install -y curl
    elif command -v brew >/dev/null 2>&1; then
        brew install curl
    elif command -v winget >/dev/null 2>&1; then
        winget install --id curl.curl --exact \
            --accept-package-agreements \
            --accept-source-agreements
    else
        echo "Install curl manually, then run this script again."
        exit 1
    fi
fi

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BASE="$PROJECT_DIR/resources"
PIPER_VERSION="2023.11.14-2"
VOICE_NAME="en_US-lessac-medium"
VOICE_URL="https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium"

OS=$(uname -s)
ARCH=$(uname -m)

download() {
    curl --fail --location --progress-bar "$@"
}

echo "Detected platform: $OS $ARCH"

# Download the Piper engine
case "$OS/$ARCH" in
    Linux/x86_64)
        PACKAGE="piper_linux_x86_64.tar.gz"
        PIPER_EXECUTABLE="piper/piper"
        ARCHIVE_TYPE="tar"
        ;;
    Linux/aarch64)
        PACKAGE="piper_linux_aarch64.tar.gz"
        PIPER_EXECUTABLE="piper/piper"
        ARCHIVE_TYPE="tar"
        ;;
    Linux/armv7l)
        PACKAGE="piper_linux_armv7l.tar.gz"
        PIPER_EXECUTABLE="piper/piper"
        ARCHIVE_TYPE="tar"
        ;;
    Darwin/arm64)
        PACKAGE="piper_macos_aarch64.tar.gz"
        PIPER_EXECUTABLE="piper/piper"
        ARCHIVE_TYPE="tar"
        ;;
    MINGW*/x86_64|MSYS*/x86_64|CYGWIN*/x86_64)
        PACKAGE="piper_windows_amd64.zip"
        PIPER_EXECUTABLE="piper/piper.exe"
        ARCHIVE_TYPE="zip"
        ;;
    *)
        echo "Unsupported platform: $OS $ARCH"
        exit 1
        ;;
esac

mkdir -p "$BASE/voices"
cd "$BASE"

# Check that Piper is really installed and is an executable binary.
if [ ! -x "$PIPER_EXECUTABLE" ] || ! file "$PIPER_EXECUTABLE" 2>/dev/null | grep -q "ELF"; then
    echo "Piper engine not installed correctly. Installing it..."

    # Elimina cualquier instalación/resto anterior.
    rm -rf "piper"

    echo "Downloading Piper engine..."
    download -o "$PACKAGE" \
        "https://github.com/rhasspy/piper/releases/download/$PIPER_VERSION/$PACKAGE"

    if [ "$ARCHIVE_TYPE" = "tar" ]; then
        tar -xzf "$PACKAGE"
    else
        unzip -q "$PACKAGE"
    fi

    rm "$PACKAGE"

    chmod +x "$PIPER_EXECUTABLE"

    # Verify installation
    if ! file "$PIPER_EXECUTABLE" | grep -q "ELF"; then
        echo "ERROR: Downloaded Piper is not a valid Linux executable."
        file "$PIPER_EXECUTABLE"
        exit 1
    fi

    echo "Piper engine installed successfully."
else
    echo "Piper engine already installed: $PIPER_EXECUTABLE"
fi

# Download the voice model
cd "$BASE/voices"

if [ ! -f "$VOICE_NAME.onnx" ]; then
    echo "Downloading English voice model..."
    download -o "$VOICE_NAME.onnx" "$VOICE_URL/$VOICE_NAME.onnx"
    download -o "$VOICE_NAME.onnx.json" "$VOICE_URL/$VOICE_NAME.onnx.json"
else
    echo "Voice model already downloaded."
fi

# Download the language model
MODELS_DIR="$PROJECT_DIR/models"
GGUF_FILE="gemma-3-1b-it-q4_0.gguf"
GGUF_URL="https://huggingface.co/google/gemma-3-1b-it-qat-q4_0-gguf/resolve/main/$GGUF_FILE"

mkdir -p "$MODELS_DIR"

if [ ! -f "$MODELS_DIR/$GGUF_FILE" ]; then
    echo "Downloading language model..."

    if [ -z "$HF_TOKEN" ]; then
        echo ""
        echo "LLM model requires a Hugging Face access token."
        echo "1. Accept the license at https://huggingface.co/google/gemma-3-1b-it-qat-q4_0-gguf"
        echo "2. Create a token at https://huggingface.co/settings/tokens"
        echo "3. Run: export HF_TOKEN=your_token_here ./install-resources.sh"
        echo ""
        exit 1
    fi

    download \
        -H "Authorization: Bearer $HF_TOKEN" \
        -o "$MODELS_DIR/$GGUF_FILE" \
        "$GGUF_URL"
else
    echo "Language model already downloaded."
fi

# Download the vision model
VISION_MODEL="SmolVLM-256M-Instruct-Q8_0.gguf"
VISION_MMPROJ="mmproj-SmolVLM-256M-Instruct-Q8_0.gguf"
VISION_BASE_URL="https://huggingface.co/ggml-org/SmolVLM-256M-Instruct-GGUF/resolve/main"

if [ ! -f "$MODELS_DIR/$VISION_MODEL" ]; then
    echo "Downloading SmolVLM vision model..."
    download -o "$MODELS_DIR/$VISION_MODEL" \
        "$VISION_BASE_URL/$VISION_MODEL"
else
    echo "Vision model already downloaded."
fi

if [ ! -f "$MODELS_DIR/$VISION_MMPROJ" ]; then
    echo "Downloading SmolVLM vision projector..."
    download -o "$MODELS_DIR/$VISION_MMPROJ" \
        "$VISION_BASE_URL/$VISION_MMPROJ"
else
    echo "Vision projector already downloaded."
fi

# Download Vosk speech recognition model
VOSK_MODEL="vosk-model-small-en-us-0.15"
VOSK_ZIP="$VOSK_MODEL.zip"
VOSK_URL="https://alphacephei.com/vosk/models/$VOSK_ZIP"

if [ ! -d "$MODELS_DIR/$VOSK_MODEL" ]; then
    echo "Downloading Vosk English speech recognition model..."
    download -o "$MODELS_DIR/$VOSK_ZIP" "$VOSK_URL"

    echo "Extracting Vosk model..."
    unzip -q "$MODELS_DIR/$VOSK_ZIP" -d "$MODELS_DIR"
    rm "$MODELS_DIR/$VOSK_ZIP"

    echo "Vosk model installed."
else
    echo "Vosk model already downloaded."
fi

# Check required system dependencies
if ! command -v ffmpeg >/dev/null 2>&1; then
    echo "FFmpeg is not installed."

    if command -v apt-get >/dev/null 2>&1; then
        sudo apt-get update
        sudo apt-get install -y ffmpeg
    elif command -v brew >/dev/null 2>&1; then
        brew install ffmpeg
    elif command -v winget >/dev/null 2>&1; then
        winget install --id Gyan.FFmpeg --exact \
            --accept-package-agreements \
            --accept-source-agreements
    else
        echo "Install FFmpeg manually, then run this script again."
        exit 1
    fi
else
    echo "FFmpeg already installed: $(command -v ffmpeg)"
fi

if ! command -v unzip >/dev/null 2>&1; then
    echo "unzip is not installed."

    if command -v apt-get >/dev/null 2>&1; then
        sudo apt-get update
        sudo apt-get install -y unzip
    elif command -v brew >/dev/null 2>&1; then
        brew install unzip
    else
        echo "Install unzip manually, then run this script again."
        exit 1
    fi
else
    echo "unzip already installed."
fi

echo ""
echo "Setup completed successfully."