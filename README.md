# NuGet Packages Downloader

NuGet Packages Downloader is a console application that allows you to download all available versions of a specified NuGet package. It is developed using .NET 8.

## Features

- 📦 Downloads all versions of a specified NuGet package.
- 💾 Saves the packages to a local directory.
- 🖥️ Provides a CLI (Command Line Interface) for easy usage.

## Requirements

- 🛠️ .NET 8 SDK ([Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- 🌐 Internet connection

## Installation

Clone the repository:

```sh
git clone https://github.com/candemiroguz/NugetPackagesDownloader.git
cd NugetPackagesDownloader
```

## Usage

Run the application with the following command:

```sh
dotnet run -- <package-name>
```

Example usage:

```sh
dotnet run -- Newtonsoft.Json
```

This command will download all versions of the `Newtonsoft.Json` package.

Alternatively, you can run the compiled application:

```sh
dotnet publish -c Release -r win-x64 --self-contained true
./bin/Release/net8.0/win-x64/NuGetDownloader.exe Newtonsoft.Json
```

## To-Do

- ✅ Ensure downloaded packages are saved in a specific folder
- ⏳ Display download progress
- ❗ Add more error handling

## Contributing

If you would like to contribute, feel free to submit a pull request or open an issue.

## License

📜 Licensed under the [MIT License](LICENSE).

