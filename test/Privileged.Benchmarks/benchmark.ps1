# build-and-run-benchmarks.ps1
# Ensure the script stops on errors
$ErrorActionPreference = "Stop"

# Define the project path
$projectPath = ".\Privileged.Benchmarks.csproj"

# Build the project in Release mode
Write-Host "Building the Privileged.Benchmarks project in Release mode..."
dotnet build $projectPath -c Release

# Navigate to the output directory
$outputDir = ".\bin\Release\net9.0"
if (-Not (Test-Path $outputDir)) {
    Write-Host "Error: Output directory not found. Build might have failed."
    exit 1
}

# Run the benchmarks
Write-Host "Running the benchmarks..."
dotnet $outputDir\Privileged.Benchmarks.dll
