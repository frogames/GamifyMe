$sourceDir = "d:\GamifyMe\GamifyMe.UI.Shared\Resources\Icons"
$outputFile = "d:\GamifyMe\GamifyMe.UI.Shared\Resources\CustomIcons.cs"

$sb = new-object System.Text.StringBuilder
[void]$sb.AppendLine("namespace GamifyMe.UI.Shared.Resources")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    public class CustomIconData")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        public string Path { get; set; }")
[void]$sb.AppendLine("        public string ViewBox { get; set; }")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("    public static class CustomIcons")
[void]$sb.AppendLine("    {")

$files = Get-ChildItem "$sourceDir\*.svg"
foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    
    $path = ""
    $viewBox = "0 0 24 24" # Default for Material icons

    # Match d attribute (case insensitive, allowing newlines)
    if ($content -match '(?si)\bd\s*=\s*(["''])(.*?)\1') {
        $path = $matches[2] -replace '\s+', ' '
    }

    # Match viewBox attribute
    if ($content -match '(?si)viewBox\s*=\s*(["''])(.*?)\1') {
        $viewBox = $matches[2]
    }

    if ($path -ne "") {
        # Cleanup name
        $base = $f.BaseName -replace '[^a-zA-Z0-9]', ' '
        $name = (Get-Culture).TextInfo.ToTitleCase($base) -replace '\s+', ''
        if ($name -match '^\d') { $name = "Icon" + $name }
        if ([string]::IsNullOrWhiteSpace($name)) { $name = "Icon" + [Math]::Abs($f.Name.GetHashCode()) }

        [void]$sb.AppendLine("        public static readonly CustomIconData $name = new CustomIconData")
        [void]$sb.AppendLine("        {")
        [void]$sb.AppendLine("            Path = `"$path`",")
        [void]$sb.AppendLine("            ViewBox = `"$viewBox`"")
        [void]$sb.AppendLine("        };")
    }
}

[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("}")

$sb.ToString() | Out-File $outputFile -Encoding UTF8
Write-Host "Generated $outputFile"
