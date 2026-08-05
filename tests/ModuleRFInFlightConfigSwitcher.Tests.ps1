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
$managed = [System.IO.Path]::GetFullPath((Join-Path $root '..\..\KSP_x64_Data\Managed'))
$plugin = Join-Path $root 'Plugins\ArmorOverhaul.dll'
$source = Join-Path $root 'Source\ArmorOverhaul\ModuleRFInFlightConfigSwitcher.cs'
$la150 = Join-Path $root 'SquadPartsOverhaul\Engine\LA-150.cfg'

Assert-True (Test-Path -LiteralPath $plugin) 'ArmorOverhaul.dll was not built.'
Assert-True (Test-Path -LiteralPath $source) 'Switcher source file is missing.'

foreach ($dependency in @(
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.UI.dll',
    'Assembly-CSharp.dll'
)) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}

$assembly = [Reflection.Assembly]::LoadFrom($plugin)
$type = $assembly.GetType('ArmorOverhaul.ModuleRFInFlightConfigSwitcher')
Assert-True ($null -ne $type) 'Switcher PartModule type was not exported.'
Assert-True ($type.BaseType.Name -eq 'PartModule') 'Switcher does not inherit PartModule.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains 'RealFuels')) 'Plugin has a hard RealFuels reference.'

$methods = $type.GetMethods([Reflection.BindingFlags]'Public,Instance,DeclaredOnly')
foreach ($methodName in @(
    'ToggleConfigurationWindow',
    'NextConfigurationAction',
    'PreviousConfigurationAction'
)) {
    $method = $methods | Where-Object Name -eq $methodName | Select-Object -First 1
    Assert-True ($null -ne $method) "Missing public control: $methodName"
}

$sourceText = Get-Content -LiteralPath $source -Raw -Encoding UTF8
Assert-True ($sourceText.Contains('new DialogGUIButton("Close"')) 'Selector window has no explicit Close button.'
Assert-True ($sourceText.Contains('SetConfiguration')) 'RF SetConfiguration API call is missing.'
Assert-True ($sourceText.Contains('TemporarilyRemoveSpoolUp')) 'Spool-up continuity handling is missing.'

$cfg = Get-Content -LiteralPath $la150 -Raw -Encoding UTF8
$firstLine = Get-Content -LiteralPath $la150 -Encoding UTF8 -TotalCount 1
Assert-True ($firstLine -eq '// Modified 2026-07-28') 'LA-150 modification date is incorrect.'
Assert-True ([regex]::Matches($cfg, '(?m)^\s*CONFIG\s*$').Count -eq 3) 'LA-150 must have exactly three RF configurations.'
foreach ($configuration in @('LA-150-500', 'LA-150-900', 'LA-150-1200')) {
    Assert-True ($cfg.Contains("name = $configuration")) "Missing LA-150 configuration: $configuration"
}
Assert-True ([regex]::Matches($cfg, 'engineID\s*=\s*LA150').Count -eq 4) 'LA-150 engine and RSE engineID wiring is incomplete.'
Assert-True ($cfg.Contains('name = ModuleRFInFlightConfigSwitcher')) 'LA-150 switcher module is missing.'
Assert-True ($cfg.Contains('@PART[AO_LA150]:HAS[@MODULE[RSE_Engines]]:NEEDS[RocketSoundEnhancement]:FINAL')) 'LA-150 RSE compatibility patch is missing.'
Assert-True ($cfg.Contains('@SOUNDLAYERGROUP[LH2]')) 'LA-150 inherited RSE sound group is not targeted.'
Assert-True ($cfg.Contains('@name = LA150')) 'LA-150 RSE sound group is not wired to the RF engine ID.'

Write-Output 'ModuleRFInFlightConfigSwitcher static tests passed.'
