# README.md
# InfoTools

InfoTools is a Windows desktop application (WPF, .NET 8) that provides tools such as favicon identification using a local database of known favicon hashes, and website header analysis with integrated favicon detection.

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
  dotnet build

## Run

- **Using Visual Studio:**  
- Press `F5` to run with debugging, or `Ctrl+F5` to run without debugging.

- **Using .NET CLI:**  
  dotnet run --project InfoTools

## Usage

- The application opens with a navigation pane.
- Select "Favicon Identifier" to use the favicon hash lookup tool.
- Click "Browse" to select a `.ico` file, then click "Analyze" to identify the favicon using the local database.
- Alternatively, drag and drop a `.ico` file directly onto the "Favicon Identifier" page to automatically populate the file selection and enable analysis.
- Select "Get Site Headers" to retrieve HTTP headers from a website. Enter a URL and click "Check Site Headers" to view the headers.
- **NEW in v0.4.0:** The "Get Site Headers" page now also automatically downloads and analyzes the website's favicon, displaying both the favicon image and its identification result above the headers section.

## Features

### Favicon Identifier
- Analyze local `.ico` files by computing MD5 hashes
- Look up favicon hashes in a local database of known frameworks
- Drag and drop support for easy file selection
- Comprehensive error handling and user feedback

### Get Site Headers
- Retrieve and display HTTP response headers from any website
- **NEW:** Automatic favicon detection and analysis
- **NEW:** Visual favicon display with identification results
- **NEW in v0.5.1:** Intelligent caching system to prevent excessive requests to the same host
- URL validation with support for localhost and IP addresses
- Comprehensive error handling for network requests

### Caching System (v0.5.1)
- **Smart Caching**: Results are cached for 5 minutes per unique host to prevent excessive traffic
- **Complete Data Storage**: Both HTTP headers and favicon data are cached together
- **Cache Indicators**: Cached results are clearly marked with "[CACHED COPY]" and timestamp
- **Automatic Expiration**: Cache automatically expires after 5 minutes, ensuring fresh data when needed
- **Host-Based Keying**: Cache keys are normalized by host to handle URL variations efficiently

### Architecture
- **NEW:** Refactored favicon functionality into a reusable `FaviconService` class
- Comprehensive XML documentation for IntelliSense support
- Modular design allowing favicon analysis to be used across multiple pages

## Alert Functionality

The alert bar functionality has been refactored into a reusable user control called `AlertBarControl`, allowing it to be used across any page in the application.

### Features

- **Dynamic Content**: Messages support placeholders that are automatically replaced with current values:
  - `$$DAY$$` - Current day of the week (e.g., "Monday")
  - `$$MONTH$$` - Current month name (e.g., "January")
  - `$$DATE$$` - Current day of the month (e.g., "15")
  - `$$YEAR$$` - Current year (e.g., "2024")
  - `$$TIME$$` - Current time in 12-hour format with AM/PM (e.g., "02:30:45 PM")
- **Scrolling Display**: Messages scroll from right to left across the screen for better visibility.
- **Periodic Updates**: The alert bar checks for content changes every 60 seconds and updates automatically with current date/time values.
- **Time updates**: if \$$TIME$$ is used, it will update every second to reflect the current time.
- **Conditional Display**: The alert bar only appears when `alertBarText.txt` exists and contains content.
- **Reusable Control**: The alert bar is now a standalone control that can be included in any WPF page.

### Usage

To add the alert bar to any page, follow these steps:

1. **Include the Control in XAML**: Add the following namespace declaration at the top of your XAML file:
   ```xml
   xmlns:local="clr-namespace:InfoTools"
	```
2. Add the Control to Your Page: Place the AlertBarControl in your page layout, typically at the top:
   ```xml
   <Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    <local:AlertBarControl x:Name="AlertBar" Grid.Row="0" />
    <!-- Rest of your page content -->
    </Grid>
   ```
3. Configuration: The alert bar automatically uses the settings from resources/config.json and reads alert text from resources/alertBarText.txt. No additional configuration is required.
 

### Configuration

To enable alerts, simply create or modify the `alertBarText.txt` file in the `resources` folder with your message content. The file is automatically copied to the output directory on build.

Example content:
Welcome to InfoTools! Today is \$$DAY$$, \$$DATE$$/\$$MONTH$$/\$$YEAR$$ at \$$TIME$$.

This will display: "Welcome to InfoTools! Today is Monday, 15/January/2024 at 02:30:45 PM."

## Settings Functionality

InfoTools now supports user-configurable settings for UI customization:

- **Navigation Bar Color:**  
  Change the color of the navigation bar to match your preferences.
- **Alert Bar Color:**  
  Set the background color of the alert bar displayed on the Home page.
- **Alert Bar Font Face:**  
  Choose the font family used for the alert bar text.
- **Alert Bar Scale X / Y:**  
  Adjust the horizontal and vertical scale (size) of the alert bar text independently.

### How to Use

1. **Open the Settings Page:**  
   Click the "Settings" button in the navigation panel.

2. **Modify Settings:**  
   - Enter your desired hex color codes for the Navigation Bar and Alert Bar.
   - Select a font face from the dropdown for the alert bar.
   - Enter positive numeric values for the alert bar's horizontal (Scale X) and vertical (Scale Y) scaling.
   - Example: `#2D2D30` for dark gray, `#FF0000` for red, font face `Consolas`, scale `2.5`.

3. **Apply Changes:**  
   - Click "Apply Changes" to save and immediately apply your settings.
   - The changes are persisted in `resources/config.json` and will be loaded on next startup.

4. **Immediate Effect:**  
   - Navigation bar color updates instantly.
   - If the Home page is active, the alert bar color, font, and scale update immediately as well.

### Configuration File

Settings are stored in `resources/config.json` in the application directory.  
If the file does not exist, it is created with default values on first run.

Example:
```json
{
  "NavigationColor": "#2D2D30",
  "AlertBarColor": "#FF0000",
  "AlertBarFontFace": "Consolas",
  "AlertBarScaleX": 1.0,
  "AlertBarScaleY": 1.0
}
```

For more details, see the in-app Settings page or review the App.xaml.cs and SettingsPage.xaml.cs source files.

Update these as needed for new releases.

## Versioning
- Current Version: 0.10.0
- Assembly Version: 0.10.0.1
- File Version: 0.10.0.1
- Informational Version: 0.10.0

Versioning follows semantic versioning for features and bugfixes. Assembly and file versions are incremented with each release.

### Notes
- If you encounter issues with missing resources, ensure the resources directory and its files are present in the output directory after build.
- The application is intended for Windows only.
- Favicon downloads have a 10-second timeout to prevent hanging on unresponsive servers.
- The favicon analysis uses MD5 hashing for compatibility with the OWASP favicon database format.
- Site scanning results are cached for 5 minutes to reduce server load and improve performance.
- Added `$$TIME$$` placeholder support with 12-hour format including AM/PM
- Fixed `$$DATE$$` to return only the day number instead of the full date string
- Updated version numbers to 0.9.0 for the new functionality
- Updated README documentation to reflect the new time placeholder and corrected date behavior
- All date values are dynamically updated every 60 seconds to ensure current information.
- All time values are dynamically updated every second to ensure current information.
- **NEW:** The alert bar is now a reusable control (AlertBarControl) that can be added to any page by including <local:AlertBarControl /> in your XAML (with proper namespace). This refactoring enhances modularity and allows the alert functionality to be used throughout the application.
