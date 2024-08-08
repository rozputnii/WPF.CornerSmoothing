# WPF.CornerSmoothing

WPF Border Container with Corner Smoothing (Figma style Corner Smoothing using Squircle)

### Installing WPF.CornerSmoothing

You should install [WPF.CornerSmoothing with NuGet](https://www.nuget.org/packages/WPF.CornerSmoothing):

    Install-Package WPF.CornerSmoothing
    
Or via the .NET Core command line interface:

    dotnet add package WPF.CornerSmoothing

Either commands, from Package Manager Console or .NET Core CLI, will download and install WPF.CornerSmoothing and all required dependencies.


### Instruction
To use add namespace to your control:
```xaml
xmlns:cs="clr-namespace:WPF.CornerSmoothing;assembly=WPF.CornerSmoothing"
```
Then use `SmoothBorder` control:
```xaml
<cs:SmoothBorder Width="200"
                Height="200"
                Padding="15"
                CornerRadius="50"
                CornerSmoothing="1"
                ClipContent="True"
                BorderBrush="#7F000000"
                BorderThickness="10"
                Background="#FFFD74C8"
                >
    <Rectangle Fill="#FF9093FF" />
</cs:SmoothBorder>
```
Where: 
- `CornerSmoothing` (value from `0` to `1`) `0.1=10%`, `1=100%` Figma's Corner smoothing, `0` is equals standart Border Corners
- `ClipContent` (boolean `True`/`False` ) Enable Content Clip to to have corners same as container with Padding and BorderThickness adjustements

Check Examples in the [Demo project](https://github.com/rozputnii/WPF.CornerSmoothing/tree/main/src/Demo).