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

echo ""
echo "Setup completed successfully."