#!/bin/bash

# Default values for parameters
SecretsFilePath=""
ProjectFilePath=""

# Parse command-line arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --secrets-file) SecretsFilePath="$2"; shift ;;
        --project-file) ProjectFilePath="$2"; shift ;;
        *) echo "Unknown parameter passed: $1"; exit 1 ;;
    esac
    shift
done

# Check if SecretsFilePath is provided
if [ -z "$SecretsFilePath" ]; then
    files=$(find "$(dirname "$0")/.." -name "secrets.*.json" -type f)
    file_count=$(echo "$files" | wc -l)
    echo "Found $file_count secrets files. You have to pass one to the script as --secrets-file parameter."
    echo "$files" | while read -r file; do
        echo "> $file"
    done
    exit 0
fi

# Check if ProjectFilePath is provided
if [ -z "$ProjectFilePath" ]; then
    csprojs=$(find "$(dirname "$0")/.." -name "*.csproj" -type f -exec grep -l '<UserSecretsId>' {} +)
    csproj_count=$(echo "$csprojs" | wc -l)
    echo "Found $csproj_count projects with user secrets. You have to pass one to the script as --project-file parameter."
    echo "$csprojs" | while read -r csproj; do
        echo "> $csproj"
    done
    exit 0
fi

# Apply user secrets
if [[ "$(uname)" == "Linux" ]]; then
    cat "$SecretsFilePath" | dotnet user-secrets set --project "$ProjectFilePath"
else
    jsonText=$(cat "$SecretsFilePath")
    jsonWithoutComments=$(echo "$jsonText" | sed -E 's:(?m)(?<=^([^"]|"[^"]*")*)//.*::; s:(?ms)/\*.*?\*/::')
    echo "$jsonWithoutComments" | jq -r 'to_entries | .[] | "dotnet user-secrets set \(.key) \(.value) --project '"$ProjectFilePath"'"' | bash
fi