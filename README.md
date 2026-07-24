# Recallr

Recallr is an open-source desktop app that turns your existing study materials into structured learning sheets, summaries, and spaced-repetition quizzes — helping you learn more efficiently and see exactly what you know and what you don't.

> ⚠️ **Project status:** Recallr is currently in active development. Features, UI, and structure may change frequently. Development happens on the [`dev` branch](https://github.com/001Sarper/Recallr/tree/dev).

## Features

- **Import from multiple sources** — Turn PDFs, GoodNotes files, and other documents into learning material.
- **Automatic learning sheets & summaries** — Generate concise, structured summaries from your imported files.
- **Active Recall quizzing** — Get tested on the material instead of just re-reading it.
- **Spaced Repetition** — Get quizzed at the optimal intervals to strengthen long-term memory.
- **Progress visualization** — Clear graphs show which topics you've mastered, which need more work, and which you haven't reviewed yet.

## Tech Stack

- **Framework:** [Avalonia UI](https://avaloniaui.net/)
- **Language / Runtime:** C# / .NET 10
- **Libraries:**
    - [OpenAI SDK](https://github.com/openai/openai-dotnet)
    - [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia)
- **Data storage:** No database — configuration and data are stored locally in `.json` files.

## Platforms

Recallr is currently built and tested for desktop:

- Windows
- macOS
- Linux

Since it's built with Avalonia UI, it could technically also run on Android and iOS in the future, though this is not currently a focus.

## Getting Started

There are no pre-built releases yet, so you'll need to build the project from source.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or newer

### Build & Run

```bash
# Clone the repository (dev branch)
git clone -b dev https://github.com/001Sarper/Recallr.git
cd Recallr

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the app
dotnet run
```

## License

This project is licensed under the [MIT License](LICENSE).

## Author

Made by **Dipsy**.
