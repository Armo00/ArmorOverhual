$ErrorActionPreference = 'Stop'

function Assert-True {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$gameData = [System.IO.Path]::GetFullPath((Join-Path $root '..'))
$managed = [System.IO.Path]::GetFullPath((Join-Path $root '..\..\KSP_x64_Data\Managed'))
$plugin = Join-Path $root 'Plugins\CrewListSorter.dll'
$source = Join-Path $root 'Source\CrewListSorter\CrewListSorterAddon.cs'

Assert-True (Test-Path -LiteralPath $plugin) 'CrewListSorter.dll was not built.'
Assert-True (Test-Path -LiteralPath $source) 'CrewListSorter source file is missing.'

foreach ($dependency in @(
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'UnityEngine.TextRenderingModule.dll',
    'Assembly-CSharp.dll'
)) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}

$assembly = [Reflection.Assembly]::LoadFrom($plugin)
$type = $assembly.GetType('ArmorOverhaul.CrewSorting.CrewListSorterAddon')
Assert-True ($null -ne $type) 'CrewListSorterAddon type was not exported.'
Assert-True ($type.BaseType.Name -eq 'MonoBehaviour') 'CrewListSorterAddon is not a Unity MonoBehaviour.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains '0Harmony')) 'CrewListSorter unexpectedly depends on Harmony.'

$addon = $type.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'KSPAddon' } | Select-Object -First 1
Assert-True ($null -ne $addon) 'CrewListSorterAddon has no KSPAddon attribute.'
$editorAnyValue = [int][Enum]::Parse($addon.startup.GetType(), 'EditorAny')
Assert-True ([int]$addon.startup -eq $editorAnyValue) 'CrewListSorterAddon does not cover both VAB and SPH.'
Assert-True (-not $addon.once) 'CrewListSorterAddon should be created for each editor scene.'

$sortModeType = $type.GetNestedType('SortMode', [Reflection.BindingFlags]'NonPublic')
$compare = $type.GetMethod('CompareValues', [Reflection.BindingFlags]'NonPublic,Static')
Assert-True ($null -ne $sortModeType) 'SortMode enum is missing.'
Assert-True ($null -ne $compare) 'Pure sorting comparison helper is missing.'

$original = [Enum]::Parse($sortModeType, 'Original')
$name = [Enum]::Parse($sortModeType, 'Name')

function Compare-CrewKeys {
    param(
        [object]$Mode,
        [bool]$Reverse,
        [string]$LeftName,
        [int]$LeftType,
        [int]$LeftIndex,
        [string]$RightName,
        [int]$RightType,
        [int]$RightIndex
    )

    return [int]$compare.Invoke($null, @(
        $Mode,
        $Reverse,
        $LeftName,
        $LeftType,
        $LeftIndex,
        $RightName,
        $RightType,
        $RightIndex
    ))
}

Assert-True ((Compare-CrewKeys $original $false 'Zulu' 0 2 'Alpha' 0 5) -lt 0) 'Original ascending does not follow roster index.'
Assert-True ((Compare-CrewKeys $original $true 'Zulu' 0 2 'Alpha' 0 5) -gt 0) 'Original descending does not reverse roster index.'
Assert-True ((Compare-CrewKeys $original $false 'Tourist' 1 0 'Crew' 0 50) -gt 0) 'Original ascending does not preserve the stock Crew-before-Tourist grouping.'
Assert-True ((Compare-CrewKeys $name $false 'alpha kerman' 0 50 'Zulu Kerman' 0 1) -lt 0) 'Name ascending is incorrect.'
Assert-True ((Compare-CrewKeys $name $true 'alpha kerman' 0 50 'Zulu Kerman' 0 1) -gt 0) 'Name descending is incorrect.'
Assert-True ((Compare-CrewKeys $name $false 'Jeb Kerman' 0 1 'jeb kerman' 0 2) -lt 0) 'Equal names do not use original order as a stable tie-breaker.'

$sourceText = Get-Content -LiteralPath $source -Raw -Encoding UTF8
Assert-True ($sourceText.Contains('CrewAssignmentDialog.Instance')) 'Stock crew dialog is not used.'
Assert-True ($sourceText.Contains('scrollListAvail')) 'Available Crew UIList is not targeted.'
Assert-True ($sourceText.Contains('GetUiListItems()')) 'Existing stock list items are not reused.'
Assert-True ($sourceText.Contains('SwapItems(')) 'Stock UIList swapping is not used.'
Assert-True ($sourceText -match 'SwapItems\(\s*currentItem,\s*desiredItem,\s*true,\s*true\)') 'UIList swapping does not preserve world transforms and can accumulate item scaling.'
Assert-True ($sourceText.Contains('GetWorldCorners(listWorldCorners)')) 'Control width is not isolated from crew item child bounds.'
Assert-True (-not $sourceText.Contains('CalculateRelativeRectTransformBounds')) 'Control width still includes scaled crew item descendants.'
Assert-True ($sourceText.Contains('PluginConfiguration.CreateForType')) 'Sort preference persistence is missing.'
Assert-True (-not ($sourceText -match 'Harmony|KerbalRoster\.Remove|KerbalRoster\.Add')) 'Sorter contains an invasive roster or Harmony modification.'

$projectCopies = @(Get-ChildItem -LiteralPath $root -Recurse -Filter 'CrewListSorter.dll')
Assert-True ($projectCopies.Count -eq 1) "Expected exactly one CrewListSorter.dll below the project, found $($projectCopies.Count)."

$gameDataCopies = @(Get-ChildItem -LiteralPath $gameData -Recurse -Filter 'CrewListSorter.dll')
Assert-True ($gameDataCopies.Count -eq 1) "Expected exactly one CrewListSorter.dll below GameData, found $($gameDataCopies.Count)."
Assert-True ($gameDataCopies[0].FullName -eq $plugin) 'The only CrewListSorter.dll is not in the project Plugins directory.'

Write-Output 'CrewListSorter tests passed.'
