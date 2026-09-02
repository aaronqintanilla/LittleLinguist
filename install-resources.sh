#!/bin/bash
set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BASE="$PROJECT_DIR/resources"
PIPER_VERSION="2023.11.14-2"
VOICE_NAME="en_US-lessac-medium"
VOICE_URL="https://huggingface.co/rhasspy/piper-voices/resolve/main/en/en_US/lessac/medium"

# Detect the CPU architecture of this machine
ARCH=$(uname -m)

case "$ARCH" in
  x86_64)  PACKAGE="piper_linux_x86_64.tar.gz" ;;
  aarch64) PACKAGE="piper_linux_aarch64.tar.gz" ;;
  armv7l)  PACKAGE="piper_linux_armv7l.tar.gz" ;;
  *)
    echo "Unsupported architecture: $ARCH"
    exit 1
    ;;
esac

echo "Detected architecture: $ARCH"

# Download the Piper engine
mkdir -p "$BASE/voices"
cd "$BASE"

if [ ! -f "piper/piper" ]; then
    echo "Downloading Piper engine..."
    wget -q --show-progress "https://github.com/rhasspy/piper/releases/download/$PIPER_VERSION/$PACKAGE"
    tar -xzf "$PACKAGE"
    rm "$PACKAGE"
    chmod +x "$BASE/piper/piper"
else
    echo "Piper engine already installed."
fi

# Download the voice model
cd "$BASE/voices"

if [ ! -f "$VOICE_NAME.onnx" ]; then
    echo "Downloading English voice model..."
    wget -q --show-progress "$VOICE_URL/$VOICE_NAME.onnx"
    wget -q --show-progress "$VOICE_URL/$VOICE_NAME.onnx.json"
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
        echo "3. Run:  export HF_TOKEN=your_token_here ./install-resources.sh"
        echo ""
        exit 1
    fi

    wget -q --show-progress \
        --header="Authorization: Bearer $HF_TOKEN" \
        -O "$MODELS_DIR/$GGUF_FILE" \
        "$GGUF_URL"
else
    echo "Language model already downloaded."
fi

# ---------------------------------------------------------
# Download the vision model
# ---------------------------------------------------------

VISION_MODEL="SmolVLM-256M-Instruct-Q8_0.gguf"
VISION_MMPROJ="mmproj-SmolVLM-256M-Instruct-Q8_0.gguf"

VISION_BASE_URL="https://huggingface.co/ggml-org/SmolVLM-256M-Instruct-GGUF/resolve/main"

if [ ! -f "$MODELS_DIR/$VISION_MODEL" ]; then
    echo "Downloading SmolVLM vision model..."

    wget -q --show-progress \
        -O "$MODELS_DIR/$VISION_MODEL" \
        "$VISION_BASE_URL/$VISION_MODEL"
else
    echo "Vision model already downloaded."
fi

if [ ! -f "$MODELS_DIR/$VISION_MMPROJ" ]; then
    echo "Downloading SmolVLM vision projector..."

    wget -q --show-progress \
        -O "$MODELS_DIR/$VISION_MMPROJ" \
        "$VISION_BASE_URL/$VISION_MMPROJ"
else
    echo "Vision projector already downloaded."
fi

# ---------------------------------------------------------
# Download Vosk speech recognition model
# ---------------------------------------------------------

VOSK_MODEL="vosk-model-small-en-us-0.15"
VOSK_ZIP="$VOSK_MODEL.zip"
VOSK_URL="https://alphacephei.com/vosk/models/$VOSK_ZIP"

mkdir -p "$MODELS_DIR"

if [ ! -d "$MODELS_DIR/$VOSK_MODEL" ]; then
    echo "Downloading Vosk English speech recognition model..."

    wget -q --show-progress \
        -O "$MODELS_DIR/$VOSK_ZIP" \
        "$VOSK_URL"

    echo "Extracting Vosk model..."

    unzip -q \
        "$MODELS_DIR/$VOSK_ZIP" \
        -d "$MODELS_DIR"

    rm "$MODELS_DIR/$VOSK_ZIP"

    echo "Vosk model installed."
else
    echo "Vosk model already downloaded."
fi

# ---------------------------------------------------------
# Check system dependencies
# ---------------------------------------------------------

if ! command -v ffmpeg >/dev/null 2>&1; then
    echo "FFmpeg is not installed."

    if command -v apt-get >/dev/null 2>&1; then
        echo "Installing FFmpeg..."
        sudo apt-get update
        sudo apt-get install -y ffmpeg
    else
        echo "Please install FFmpeg manually and run this script again."
        exit 1
    fi
else
    echo "FFmpeg already installed: $(command -v ffmpeg)"
fi

# ---------------------------------------------------------
# Check system dependencies
# ---------------------------------------------------------

for PACKAGE in wget unzip ffmpeg
do
    if ! command -v "$PACKAGE" >/dev/null 2>&1; then
        echo "$PACKAGE is not installed."

        if command -v apt-get >/dev/null 2>&1; then
            echo "Installing $PACKAGE..."
            sudo apt-get update
            sudo apt-get install -y "$PACKAGE"
        else
            echo "Please install $PACKAGE manually."
            exit 1
        fi
    else
        echo "$PACKAGE already installed."
    fi
done

echo ""
echo "Setup completed successfully."