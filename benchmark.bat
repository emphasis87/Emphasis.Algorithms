dotnet clean "src\Emphasis.Algorithms.sln"
dotnet build -c Release "src\Emphasis.Algorithms.sln"
cd "src\Emphasis.Algorithms.Tests.Benchmarks\"
dotnet run -c Release -- --filter *
cd ..\..
