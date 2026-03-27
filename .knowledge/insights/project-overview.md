# Project Onboarding: Plagiarism Detection Software

## Project Purpose
The Plagiarism Detection Software is designed to identify potential plagiarism in C# source code. It achieves this by:
1. Parsing C# files using Microsoft Roslyn.
2. Generating Control Flow Dependency Graphs (CFDGs) for each file, capturing structural patterns like loops, conditionals, and method declarations.
3. Comparing these graphs using distance algorithms (e.g., symmetric difference of node sets) to calculate similarity scores between codebases.

## Architecture & Layout
The project follows a **Clean Architecture** pattern:

```
PlagiarismDetector.sln
├── PlagiarismDetector.Domain/          # Core entities, enums, and business rules.
├── PlagiarismDetector.Application/     # Use cases, DTOs, and core logic (graph building/comparison).
├── PlagiarismDetector.Infrastructure/  # External integrations: Local Storage, MongoDB, Minio.
└── PlagiarismDetector.WebAPI/          # Entry point, controllers, and API configuration.
```

**Key Technologies:**
- **Runtime:** .NET 9.0
- **Static Analysis:** Microsoft.CodeAnalysis (Roslyn)
- **Data Storage:** MongoDB (configured), Local File System (active default)
- **Object Storage:** Minio (configured)
- **API Documentation:** Scalar (integrated for modern OpenAPI docs)

## Workflow & Standards
- **Build:** `dotnet build`
- **Run:** `dotnet run --project PlagiarismDetector.WebAPI`
- **Test:** No automated tests currently exist (standard for the current prototype phase).
- **Style:** Adheres to standard C# naming conventions and Clean Architecture separation of concerns.

## Current State
- **Active Work:** Recent focus on Linux compatibility, project upload functionality, and ranking endpoints.
- **Recent Activity:**
  - `fde8cd7` feat: ranking endpoint
  - `d6115d4` fix: linux availability
  - `6de8c1c` feature: project uploads
- **Configured Storage:** Currently uses `LocalStorage` pointing to `../database` relative to the WebAPI project.

## First Steps
1. **Explore Core Logic:** Read `PlagiarismDetector.Application/Services/DependencyWalker/DependencyControlFlowWalker.cs` to understand how code is analyzed.
2. **Review Comparison Algorithm:** Examine `PlagiarismDetector.Application/Services/DistanceCalculator/NaiveGraphComparator.cs`.
3. **Run API:** Execute `dotnet run --project PlagiarismDetector.WebAPI` and visit the Scalar documentation (likely at `/scalar/v1`) to test endpoints.
4. **Identify Gaps:** Consider adding unit tests for the comparison logic or improving the `NaiveGraphComparator`.
