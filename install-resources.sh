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

# Piper platform configuration

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

        if ! brew list espeak-ng >/dev/null 2>&1; then
            echo "Installing espeak-ng dependency via Homebrew..."
            brew install espeak-ng
        fi
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

# Piper installation

PIPER_DIR="$BASE/piper"
PIPER_BINARY="$BASE/$PIPER_EXECUTABLE"

echo ""
echo "Checking Piper installation..."

PIPER_NEEDS_INSTALL=false

# Check executable

if [ ! -x "$PIPER_BINARY" ]; then
    echo "  ❌ Piper executable is missing."
    PIPER_NEEDS_INSTALL=true
elif [ "$OS" = "Linux" ] && ! file "$PIPER_BINARY" 2>/dev/null | grep -q "ELF"; then
    echo "  ❌ Piper executable is not a valid Linux ELF binary."
    PIPER_NEEDS_INSTALL=true
else
    echo "  ✓ Piper executable found."
fi

# Check shared libraries

if [ "$OS" = "Linux" ]; then

    if [ ! -f "$PIPER_DIR/libpiper_phonemize.so.1" ]; then
        echo "  ❌ libpiper_phonemize.so.1 is missing."
        PIPER_NEEDS_INSTALL=true
    else
        echo "  ✓ libpiper_phonemize.so.1 found."
    fi

    if [ ! -f "$PIPER_DIR/libonnxruntime.so.1.14.1" ]; then
        echo "  ❌ libonnxruntime.so.1.14.1 is missing."
        PIPER_NEEDS_INSTALL=true
    else
        echo "  ✓ libonnxruntime.so.1.14.1 found."
    fi

fi

# Reinstall Piper if anything is missing

if [ "$PIPER_NEEDS_INSTALL" = true ]; then

    echo ""
    echo "Piper installation is incomplete."
    echo "Installing Piper $PIPER_VERSION..."

    # Remove broken/old installation.
    rm -rf "$PIPER_DIR"

    # Remove old archive if it exists.
    rm -f "$PACKAGE"

    echo ""
    echo "Downloading Piper engine..."

    download -o "$PACKAGE" \
        "https://github.com/rhasspy/piper/releases/download/$PIPER_VERSION/$PACKAGE"

    echo ""
    echo "Extracting Piper..."

    if [ "$ARCHIVE_TYPE" = "tar" ]; then
        tar -xzf "$PACKAGE"
    else
        unzip -q "$PACKAGE"
    fi

    rm -f "$PACKAGE"

    # Check executable after extraction

    if [ ! -f "$PIPER_BINARY" ]; then
        echo ""
        echo "ERROR: Piper executable was not found after extraction."
        echo "Expected:"
        echo "  $PIPER_BINARY"
        exit 1
    fi

    chmod +x "$PIPER_BINARY"

    if [ "$OS" = "Linux" ]; then

        if ! file "$PIPER_BINARY" | grep -q "ELF"; then
            echo ""
            echo "ERROR: Downloaded Piper is not a valid Linux ELF executable."
            file "$PIPER_BINARY"
            exit 1
        fi

        # Check required libraries after extraction

        if [ ! -f "$PIPER_DIR/libpiper_phonemize.so.1" ]; then
            echo ""
            echo "ERROR: Piper package does not contain:"
            echo "  libpiper_phonemize.so.1"
            echo ""
            echo "Contents of $PIPER_DIR:"
            find "$PIPER_DIR" -maxdepth 2 -type f -print
            exit 1
        fi

        if [ ! -f "$PIPER_DIR/libonnxruntime.so.1.14.1" ]; then
            echo ""
            echo "ERROR: Piper package does not contain:"
            echo "  libonnxruntime.so.1.14.1"
            echo ""
            echo "Contents of $PIPER_DIR:"
            find "$PIPER_DIR" -maxdepth 2 -type f -print
            exit 1
        fi

    fi

    echo ""
    echo "Piper files installed successfully."

else

    echo ""
    echo "✓ Piper installation is complete."

fi

# Verify Piper dependencies

if [ "$OS" = "Linux" ]; then

    echo ""
    echo "Checking Piper shared library dependencies..."

    # Piper looks for libraries in its own directory.
    export LD_LIBRARY_PATH="$PIPER_DIR${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"

    MISSING_LIBS=$(ldd "$PIPER_BINARY" 2>&1 | grep "not found" || true)

    if [ -n "$MISSING_LIBS" ]; then

        echo ""
        echo "ERROR: Piper still has missing shared libraries:"
        echo ""
        echo "$MISSING_LIBS"
        echo ""
        echo "Piper installation cannot continue."
        exit 1

    fi

    echo "✓ All Piper shared libraries found."

    echo ""
    echo "Testing Piper executable..."

    if ! "$PIPER_BINARY" --help >/dev/null 2>&1; then
        echo ""
        echo "ERROR: Piper executable could not be started."
        exit 1
    fi

    echo "✓ Piper executable works."

fi

# Download voice model

cd "$BASE/voices"

if [ ! -f "$VOICE_NAME.onnx" ]; then

    echo ""
    echo "Downloading English voice model..."

    download \
        -o "$VOICE_NAME.onnx" \
        "$VOICE_URL/$VOICE_NAME.onnx"

    download \
        -o "$VOICE_NAME.onnx.json" \
        "$VOICE_URL/$VOICE_NAME.onnx.json"

else

    echo "Voice model already downloaded."

fi

# Download language model

MODELS_DIR="$PROJECT_DIR/models"

GGUF_FILE="gemma-3-1b-it-q4_0.gguf"

GGUF_URL="https://huggingface.co/google/gemma-3-1b-it-qat-q4_0-gguf/resolve/main/$GGUF_FILE"

mkdir -p "$MODELS_DIR"

if [ ! -f "$MODELS_DIR/$GGUF_FILE" ]; then

    echo ""
    echo "Downloading language model..."

    if [ -z "$HF_TOKEN" ]; then

        echo ""
        echo "LLM model requires a Hugging Face access token."
        echo "1. Accept the license at:"
        echo "   https://huggingface.co/google/gemma-3-1b-it-qat-q4_0-gguf"
        echo ""
        echo "2. Create a token at:"
        echo "   https://huggingface.co/settings/tokens"
        echo ""
        echo "3. Run:"
        echo "   export HF_TOKEN=your_token_here ./install-resources.sh"
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

# Download vision model

VISION_MODEL="SmolVLM-256M-Instruct-Q8_0.gguf"
VISION_MMPROJ="mmproj-SmolVLM-256M-Instruct-Q8_0.gguf"

VISION_BASE_URL="https://huggingface.co/ggml-org/SmolVLM-256M-Instruct-GGUF/resolve/main"

if [ ! -f "$MODELS_DIR/$VISION_MODEL" ]; then

    echo ""
    echo "Downloading SmolVLM vision model..."

    download \
        -o "$MODELS_DIR/$VISION_MODEL" \
        "$VISION_BASE_URL/$VISION_MODEL"

else

    echo "Vision model already downloaded."

fi

if [ ! -f "$MODELS_DIR/$VISION_MMPROJ" ]; then

    echo ""
    echo "Downloading SmolVLM vision projector..."

    download \
        -o "$MODELS_DIR/$VISION_MMPROJ" \
        "$VISION_BASE_URL/$VISION_MMPROJ"

else

    echo "Vision projector already downloaded."

fi

# Download Vosk speech recognition model

VOSK_MODEL="vosk-model-small-en-us-0.15"
VOSK_ZIP="$VOSK_MODEL.zip"

VOSK_URL="https://alphacephei.com/vosk/models/$VOSK_ZIP"

if [ ! -d "$MODELS_DIR/$VOSK_MODEL" ]; then

    echo ""
    echo "Downloading Vosk English speech recognition model..."

    download \
        -o "$MODELS_DIR/$VOSK_ZIP" \
        "$VOSK_URL"

    echo "Extracting Vosk model..."

    unzip -q "$MODELS_DIR/$VOSK_ZIP" \
        -d "$MODELS_DIR"

    rm "$MODELS_DIR/$VOSK_ZIP"

    echo "Vosk model installed."

else

    echo "Vosk model already downloaded."

fi

# Check FFmpeg

if ! command -v ffmpeg >/dev/null 2>&1; then

    echo ""
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

# Check unzip

if ! command -v unzip >/dev/null 2>&1; then

    echo ""
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

# Finished

echo ""
echo "=============================================="
echo " Setup completed successfully!"
echo "=============================================="
echo ""
echo "Piper:"
echo "  $PIPER_BINARY"

if [ "$OS" = "Linux" ]; then
    echo ""
    echo "Piper libraries:"
    echo "  $PIPER_DIR/libpiper_phonemize.so.1"
    echo "  $PIPER_DIR/libonnxruntime.so.1.14.1"
fi

echo ""