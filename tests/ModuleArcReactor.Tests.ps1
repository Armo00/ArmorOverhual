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

function Assert-Near {
    param(
        [double]$Actual,
        [double]$Expected,
        [double]$Tolerance,
        [string]$Message
    )

    if ([Math]::Abs($Actual - $Expected) -gt $Tolerance) {
        throw "$Message Expected $Expected, got $Actual."
    }
}

$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$managed = [System.IO.Path]::GetFullPath((Join-Path $root '..\..\KSP_x64_Data\Managed'))
$plugin = Join-Path $root 'Plugins\ArmorOverhaul.dll'
$source = Join-Path $root 'Source\ArmorOverhaul\ModuleArcReactor.cs'
$phoenixReactor = Join-Path $root 'Mods\Phoenix Industry\ArcReactor.cfg'

Assert-True (Test-Path -LiteralPath $plugin) 'ArmorOverhaul.dll was not built.'
Assert-True (Test-Path -LiteralPath $source) 'ModuleArcReactor source file is missing.'
Assert-True (Test-Path -LiteralPath $phoenixReactor) 'Phoenix Arc Reactor patch is missing.'

foreach ($dependency in @(
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.UI.dll',
    'Assembly-CSharp.dll'
)) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}

$assembly = [Reflection.Assembly]::LoadFrom($plugin)
$type = $assembly.GetType('ArmorOverhaul.ModuleArcReactor')
Assert-True ($null -ne $type) 'ModuleArcReactor type was not exported.'
Assert-True ($type.BaseType.Name -eq 'ModuleResourceConverter') 'ModuleArcReactor does not inherit ModuleResourceConverter.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains 'NearFutureElectrical')) 'Plugin has a Near Future Electrical dependency.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains 'SystemHeat')) 'Plugin has a SystemHeat dependency.'

$efficiencyOverride = $type.GetMethod('GetEfficiencyMultiplier', [Reflection.BindingFlags]'Public,Instance,DeclaredOnly')
Assert-True ($null -ne $efficiencyOverride) 'GetEfficiencyMultiplier override is missing.'
Assert-True ($efficiencyOverride.GetBaseDefinition().DeclaringType.Name -eq 'BaseConverter') 'Power control does not override the stock converter efficiency hook.'
$prepareRecipeOverride = $type.GetMethod('PrepareRecipe', [Reflection.BindingFlags]'NonPublic,Instance,DeclaredOnly')
Assert-True ($null -ne $prepareRecipeOverride) 'Load-following PrepareRecipe override is missing.'
Assert-True ($prepareRecipeOverride.GetBaseDefinition().DeclaringType.Name -eq 'BaseConverter') 'Recipe timing is not captured through the stock converter hook.'
$postProcessOverride = $type.GetMethod('PostProcess', [Reflection.BindingFlags]'NonPublic,Instance,DeclaredOnly')
Assert-True ($null -ne $postProcessOverride) 'Status and power-output PostProcess override is missing.'
Assert-True ($postProcessOverride.GetBaseDefinition().DeclaringType.Name -eq 'BaseConverter') 'Status and power output do not use the stock converter result hook.'
Assert-True ($null -eq $type.GetMethod('FixedUpdate', [Reflection.BindingFlags]'Public,Instance,DeclaredOnly')) 'Legacy pause-state FixedUpdate override remains.'

$powerField = $type.GetField('powerPercentage')
Assert-True ($null -ne $powerField) 'Persistent powerPercentage field is missing.'
$kspField = $powerField.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'KSPField' } | Select-Object -First 1
$range = $powerField.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'UI_FloatRange' } | Select-Object -First 1
Assert-True ($null -ne $kspField -and $kspField.isPersistant) 'powerPercentage is not persistent.'
Assert-True ($kspField.guiActive -and $kspField.guiActiveEditor) 'Power slider is not available in both flight and editor.'
Assert-True ($null -ne $range) 'powerPercentage has no UI_FloatRange.'
Assert-Near $range.minValue 0 0.0001 'Power slider minimum is incorrect.'
Assert-Near $range.maxValue 100 0.0001 'Power slider maximum is incorrect.'

$powerOutputField = $type.GetField('currentPowerOutput')
Assert-True ($null -ne $powerOutputField) 'Current power-output display field is missing.'
$powerOutputKspField = $powerOutputField.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'KSPField' } | Select-Object -First 1
Assert-True ($null -ne $powerOutputKspField -and $powerOutputKspField.guiActive) 'Current power output is not visible in flight.'
Assert-True (-not $powerOutputKspField.guiActiveEditor) 'Current power output should not be shown in the editor.'

$declaredMethods = $type.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
$declaredPowerActions = foreach ($method in $declaredMethods) {
    $method.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'KSPAction' }
}
Assert-True (@($declaredPowerActions).Count -eq 0) 'ModuleArcReactor unexpectedly declares power-preset actions.'

$flags = [Reflection.BindingFlags]'NonPublic,Static'
$applyPower = $type.GetMethod('ApplyPowerPercentage', $flags)
Assert-True ($null -ne $applyPower) 'Power scaling helper is missing.'
Assert-Near ($applyPower.Invoke($null, @([double]2.0, [single]100))) 2.0 0.000001 '100% power scaling is incorrect.'
Assert-Near ($applyPower.Invoke($null, @([double]2.0, [single]40))) 0.8 0.000001 '40% power scaling is incorrect.'
Assert-Near ($applyPower.Invoke($null, @([double]2.0, [single]0))) 0.0 0.000001 '0% power scaling is incorrect.'

$resolveDump = $type.GetMethod('ResolveDumpExcess', $flags)
Assert-True ($null -ne $resolveDump) 'blocked_when_full translation helper is missing.'

[object[]]$arguments = @('true', $false)
$dumpExcess = $resolveDump.Invoke($null, $arguments)
Assert-True (-not $dumpExcess -and $arguments[1]) 'blocked_when_full=true must disable dumping.'

$arguments = @('false', $false)
$dumpExcess = $resolveDump.Invoke($null, $arguments)
Assert-True ($dumpExcess -and $arguments[1]) 'blocked_when_full=false must enable dumping.'

$arguments = @($null, $false)
$dumpExcess = $resolveDump.Invoke($null, $arguments)
Assert-True ($dumpExcess -and $arguments[1]) 'Omitted blocked_when_full must enable dumping.'

$headroomCheck = $type.GetMethod('CalculateTargetHeadroomFromSpareCapacity', $flags)
Assert-True ($null -ne $headroomCheck) 'Load-following target-headroom helper is missing.'
Assert-Near ($headroomCheck.Invoke($null, @([double]950000, [double]1000000, [single]0.95))) 900000 0.02 'Low stored charge does not expose the full target headroom.'
Assert-Near ($headroomCheck.Invoke($null, @([double]51000, [double]1000000, [single]0.95))) 1000 0.02 'Headroom immediately below the 95% target is incorrect.'
Assert-Near ($headroomCheck.Invoke($null, @([double]50000, [double]1000000, [single]0.95))) 0 0.02 'Headroom at the 95% target is not zero.'
Assert-Near ($headroomCheck.Invoke($null, @([double]0, [double]1000000, [single]0.95))) 0 0.02 'Storage above the 95% target incorrectly permits output.'

$loadLimitCheck = $type.GetMethod('LimitEfficiencyToHeadroom', $flags)
Assert-True ($null -ne $loadLimitCheck) 'Load-following efficiency limiter is missing.'
Assert-Near ($loadLimitCheck.Invoke($null, @([double]1, [double]60000, [double]3000000, [double]0.02))) 1 0.000001 'Full available headroom incorrectly reduces power.'
Assert-Near ($loadLimitCheck.Invoke($null, @([double]1, [double]30000, [double]3000000, [double]0.02))) 0.5 0.000001 'Partial headroom is not converted into proportional power.'
Assert-Near ($loadLimitCheck.Invoke($null, @([double]1, [double]0, [double]3000000, [double]0.02))) 0 0.000001 'A full target still permits output.'

$loadFractionCheck = $type.GetMethod('CalculateLoadFraction', $flags)
Assert-True ($null -ne $loadFractionCheck) 'Actual converter-load helper is missing.'
Assert-Near ($loadFractionCheck.Invoke($null, @([double]0.0068, [double]2.0))) 0.0034 0.000001 '0.34% converter load is not preserved for the power display.'
Assert-Near ($loadFractionCheck.Invoke($null, @([double]0, [double]0.02))) 0 0.000001 'Zero converter load is not displayed as zero power.'

$formatPowerCheck = $type.GetMethod('FormatPowerOutput', $flags)
Assert-True ($null -ne $formatPowerCheck) 'Power-unit formatting helper is missing.'
Assert-True (($formatPowerCheck.Invoke($null, @([double]10800000))) -eq '10.80 GW') 'Full Arc Reactor output is not formatted as 10.80 GW.'
Assert-True (($formatPowerCheck.Invoke($null, @([double]36720))) -eq '36.72 MW') '0.34% Arc Reactor output is not formatted as 36.72 MW.'
Assert-True (($formatPowerCheck.Invoke($null, @([double]0))) -eq '0 W') 'Zero Arc Reactor output is not formatted as 0 W.'

$sourceText = Get-Content -LiteralPath $source -Raw -Encoding UTF8
Assert-True ($sourceText.Contains('base.GetEfficiencyMultiplier()')) 'Stock converter efficiency is not preserved.'
Assert-True ($sourceText.Contains('GetNodes("OUTPUT_RESOURCE")')) 'OUTPUT_RESOURCE translation is missing.'
Assert-True ($sourceText.Contains('outputNode.AddValue("DumpExcess", dumpExcess)')) 'Stock DumpExcess mapping is missing.'
Assert-True ($sourceText.Contains('protected override ConversionRecipe PrepareRecipe(double deltaTime)')) 'Stock recipe duration is not captured for load following.'
Assert-True ($sourceText.Contains('LimitEfficiencyToHeadroom')) 'Continuous load-following calculation is missing.'
Assert-True ($sourceText.Contains('status = "Standby"')) 'Charge-target standby status is missing.'
Assert-True ($sourceText.Contains('status = "Output Disabled"')) 'Zero-slider status is missing.'
Assert-True ($sourceText.Contains('status = "Offline"')) 'Manual shutdown status is missing.'
Assert-True (-not ($sourceText -match 'capacityPaused|resume_when_below|defaultResumeWhenBelow|BuiltInResumeThreshold')) 'Legacy start/stop water-level logic remains.'
Assert-True (-not ($sourceText -match 'WaitForSeconds|StartCoroutine|realtimeSinceStartup|cooldown')) 'A timer-based control mechanism was introduced.'

$cfg = Get-Content -LiteralPath $phoenixReactor -Raw -Encoding UTF8
$firstLine = Get-Content -LiteralPath $phoenixReactor -Encoding UTF8 -TotalCount 1
Assert-True ($firstLine -eq '// Modified 2026-09-06') 'Phoenix Arc Reactor modification date is incorrect.'
Assert-True ($cfg.Contains('@name = phoenixreactor-0625')) 'phoenixreactor-0625 clone is missing.'
Assert-True ($cfg.Contains('name = ModuleArcReactor')) 'phoenixreactor-0625 does not use ModuleArcReactor.'
Assert-True (-not $cfg.Contains('name = ModuleResourceConverter')) 'Legacy ModuleResourceConverter remains on phoenixreactor-0625.'
Assert-True ($cfg.Contains('powerPercentage = 100')) 'phoenixreactor-0625 initial power is not 100%.'
Assert-True ($cfg -match 'FillAmount\s*=\s*0\.95') 'phoenixreactor-0625 regulation target is not 95%.'
Assert-True (-not ($cfg -match 'resume_when_below|defaultResumeWhenBelow')) 'phoenixreactor-0625 still contains legacy start/stop water levels.'

$outputBlocks = [regex]::Matches($cfg, '(?s)OUTPUT_RESOURCE\s*\{(.*?)\}')
$electricChargeOutput = $outputBlocks | Where-Object { $_.Groups[1].Value -match 'ResourceName\s*=\s*ElectricCharge' } | Select-Object -First 1
$heliumOutput = $outputBlocks | Where-Object { $_.Groups[1].Value -match 'ResourceName\s*=\s*LqdHelium' } | Select-Object -First 1
Assert-True ($null -ne $electricChargeOutput) 'ElectricCharge output is missing from phoenixreactor-0625.'
Assert-True ($null -ne $heliumOutput) 'LqdHelium output is missing from phoenixreactor-0625.'
Assert-True ($electricChargeOutput.Groups[1].Value -match 'blocked_when_full\s*=\s*true') 'ElectricCharge must regulate reactor output near the fill target.'
Assert-True ($electricChargeOutput.Groups[1].Value -match 'FlowMode\s*=\s*ALL_VESSEL') 'ElectricCharge output does not reach vessel-wide storage.'
Assert-True ($heliumOutput.Groups[1].Value -match 'blocked_when_full\s*=\s*false') 'LqdHelium must vent excess output.'
Assert-True (-not ($cfg -match 'DumpExcess\s*=')) 'phoenixreactor-0625 still uses the legacy DumpExcess setting.'

$reactorVariants = @(
    @{ Id = 'phoenixreactor-0625'; Mass = 0.04; Power = 3000000; Fuel = 100; EntryCost = 3000000 },
    @{ Id = 'phoenixreactor-125-v2'; Mass = 0.32; Power = 24000000; Fuel = 800; EntryCost = 3150000 },
    @{ Id = 'phoenixreactor-1875'; Mass = 1.08; Power = 81000000; Fuel = 2700; EntryCost = 3300000 },
    @{ Id = 'phoenixreactor-250'; Mass = 2.56; Power = 192000000; Fuel = 6400; EntryCost = 3600000 },
    @{ Id = 'phoenixreactor-375'; Mass = 8.64; Power = 648000000; Fuel = 21600; EntryCost = 4050000 },
    @{ Id = 'phoenixreactor-500'; Mass = 20.48; Power = 1536000000; Fuel = 51200; EntryCost = 4500000 }
)

foreach ($variant in $reactorVariants) {
    Assert-True ($cfg.Contains("@name = $($variant.Id)")) "Missing Arc Reactor variant: $($variant.Id)"
    Assert-True ($cfg -match "(?s)@name\s*=\s*$([regex]::Escape($variant.Id)).*?@mass\s*=\s*$($variant.Mass)") "Incorrect mass for $($variant.Id)."
    Assert-True ($cfg -match "(?s)@name\s*=\s*$([regex]::Escape($variant.Id)).*?@entryCost\s*=\s*$($variant.EntryCost)") "Incorrect entry cost for $($variant.Id)."
}

Assert-True ($cfg.Contains('@Ratio = 1536000000')) '5m Arc Reactor electrical output is incorrect.'
Assert-True ($cfg.Contains('@maxAmount = 51200')) '5m Arc Reactor fuel or helium capacity is incorrect.'
Assert-True (($reactorVariants[-1].EntryCost / $reactorVariants[0].EntryCost) -le 1.5) 'Largest Arc Reactor entry cost exceeds 1.5x baseline.'
Assert-True ($cfg.Contains('@PART[phoenixreactor-*]:NEEDS[VABOrganizer]:Final')) 'Arc Reactor family VABO patch is missing.'

$pluginCopies = @(Get-ChildItem -LiteralPath $root -Recurse -Filter 'ArmorOverhaul.dll')
Assert-True ($pluginCopies.Count -eq 1) 'ArmorOverhaul.dll exists more than once below GameData and may be loaded twice by KSP.'

Write-Output 'ModuleArcReactor tests passed.'
