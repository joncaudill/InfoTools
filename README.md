# InfoTools

InfoTools is a Windows desktop application (WPF, .NET 8) that provides tools such as favicon identification using a local database of known favicon hashes.

NOTE: for testing purposes, the application includes:
- A sample favicon.ico file
- A sample favicon.ico database in csv format based on the OWASP favicon hash database, 
but with a new entry for the favicon.ico file included in the project.

## Prerequisites

- **.NET 8 SDK**  
  Download and install from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)

- **Visual Studio 2022 or later**  
  Make sure the ".NET Desktop Development" workload is installed.

- **Windows OS**  
  WPF applications require Windows.

## Setup

1. **Extract the Zip File**  
   Unzip the distributed archive to a directory of your choice.

2. **Open the Project in Visual Studio**  
   - Open Visual Studio.
   - Use __File > Open > Project/Solution__ and select the `.sln` file, or open the folder containing the project.

3. **Verify Resources**  
   Ensure the `resources` directory contains:
   - `favicons-database.csv` (the favicon hash database)
   - Any `.ico` files you wish to use for testing

   These files are configured to be copied to the output directory automatically on build.

## Build

You can build the project using Visual Studio or the .NET CLI:

- **Using Visual Studio:**  
  - Press `Ctrl+Shift+B` or select __Build > Build Solution__.

- **Using .NET CLI:**  
  Open a terminal in the project directory and run:

  ```
  dotnet build
  ```

  
## Run

- **Using Visual Studio:**  
  - Press `F5` to run with debugging, or `Ctrl+F5` to run without debugging.

- **Using .NET CLI:** 

```
dotnet run --project InfoTools
```

## Usage

- The application opens with a navigation pane.
- Select "Favicon Identifier" to use the favicon hash lookup tool.
- Click "Browse" to select a `.ico` file, then click "Analyze" to identify the favicon using the local database.
- Alternatively, drag and drop a `.ico` file directly onto the "Favicon Identifier" page to automatically populate the file selection and enable analysis.

## Versioning

Version information is managed in the `.csproj` file using these properties:
- `<Version>`
- `<AssemblyVersion>`
- `<FileVersion>`
- `<InformationalVersion>`

Update these as needed for new releases.

## Notes

- If you encounter issues with missing resources, ensure the `resources` directory and its files are present in the output directory after build.
- The application is intended for Windows only.

---