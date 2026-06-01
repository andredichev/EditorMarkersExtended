# EditorMarkersExtended

A Kerbal Space Program mod that adds toggleable extension lines to the Center of Mass (CoM), Center of Thrust (CoT), and Center of Lift (CoL) markers in the KSP vehicle editor (`VAB` or `SPH`).

## Installation
1. Download the latest release `.zip` archive.
2. Extract the contents of the archive into your KSP installation directory.

## Usage
1. Enable the standard CoM, CoT, or CoL markers by left-clicking their respective buttons in the editor UI.
2. **Right-click** any of these UI buttons to independently toggle the extended lines on or off for that specific marker.
3. UI tooltips will display the right-click instruction upon hovering over the marker buttons.

## Build
Create a `EditorMarkersExtended.csproj.user` file next to the `.csproj` and point it at your KSP install:

```xml
<Project>
  <PropertyGroup>
    <KSPBT_GameRoot>C:\Your\KSP\Install</KSPBT_GameRoot>
  </PropertyGroup>
</Project>
```

## Legal
This project is licensed under the MIT License, see the [LICENSE](LICENSE) file for details.
Developed with AI assistance.
