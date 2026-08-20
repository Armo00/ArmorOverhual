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
$source = Join-Path $root 'Source\ArmorOverhaul\ModuleVariableIspThrust.cs'

Assert-True (Test-Path -LiteralPath $plugin) 'ArmorOverhaul.dll was not built.'
Assert-True (Test-Path -LiteralPath $source) 'Variable ISP/thrust source file is missing.'

foreach ($dependency in @(
    'UnityEngine.CoreModule.dll',
    'UnityEngine.dll',
    'UnityEngine.UI.dll',
    'Assembly-CSharp.dll'
)) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $dependency))
}

$assembly = [Reflection.Assembly]::LoadFrom($plugin)
$type = $assembly.GetType('ArmorOverhaul.ModuleVariableIspThrust')
Assert-True ($null -ne $type) 'ModuleVariableIspThrust type was not exported.'
Assert-True ($type.BaseType.Name -eq 'PartModule') 'ModuleVariableIspThrust does not inherit PartModule.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains 'RealFuels')) 'Plugin has a hard RealFuels reference.'
Assert-True (-not ($assembly.GetReferencedAssemblies().Name -contains 'SolverEngines')) 'Plugin has a hard SolverEngines reference.'

$methods = $type.GetMethods([Reflection.BindingFlags]'Public,Instance,DeclaredOnly')
foreach ($methodName in @(
    'SetPerformance0Action',
    'SetPerformance20Action',
    'SetPerformance40Action',
    'SetPerformance60Action',
    'SetPerformance80Action',
    'SetPerformance100Action'
)) {
    $method = $methods | Where-Object Name -eq $methodName | Select-Object -First 1
    Assert-True ($null -ne $method) "Missing action-group control: $methodName"
}

$settingField = $type.GetField('performanceSetting')
Assert-True ($null -ne $settingField) 'Persistent performanceSetting field is missing.'
$kspField = $settingField.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'KSPField' } | Select-Object -First 1
$range = $settingField.GetCustomAttributes($false) | Where-Object { $_.GetType().Name -eq 'UI_FloatRange' } | Select-Object -First 1
Assert-True ($null -ne $kspField -and $kspField.isPersistant) 'performanceSetting is not persistent.'
Assert-True ($null -ne $range) 'performanceSetting has no UI_FloatRange.'
Assert-Near $range.minValue 0 0.0001 'Slider minimum is incorrect.'
Assert-Near $range.maxValue 100 0.0001 'Slider maximum is incorrect.'

$sourceText = Get-Content -LiteralPath $source -Raw -Encoding UTF8
Assert-True ($sourceText.Contains('candidate.GetType() != typeof(ModuleEnginesFX)')) 'Exact ModuleEnginesFX type guard is missing.'
Assert-True ($sourceText.Contains('engine.SetupPropellant()')) 'Fuel-flow recalculation is missing.'
Assert-True ($sourceText.Contains('PERFORMANCE_POINT')) 'Performance-point parser is missing.'

$flags = [Reflection.BindingFlags]'NonPublic,Static'
$constantPower = $type.GetMethod('InterpolateConstantPowerThrust', $flags)
Assert-True ($null -ne $constantPower) 'Constant-power interpolation helper is missing.'
$constantPowerResult = $constantPower.Invoke($null, @(
    [single]1000,
    [single]300,
    [single]600,
    [single]500,
    [single]400,
    [single]0.5
))
Assert-Near $constantPowerResult 750 0.001 'Constant-power interpolation is incorrect.'

$calculateFuelFlow = $type.GetMethod('CalculateFuelFlow', $flags)
Assert-True ($null -ne $calculateFuelFlow) 'Fuel-flow recalculation helper is missing.'
$gravity = [single]9.80665
$fuelFlow = $calculateFuelFlow.Invoke($null, @(
    [single]883,
    [single]500,
    $gravity
))
Assert-Near $fuelFlow (883 / (500 * $gravity)) 0.000001 'Maximum fuel flow is not derived from the selected thrust and vacuum ISP.'
$seaLevelThrust = $fuelFlow * 400 * $gravity
Assert-Near $seaLevelThrust 706.4 0.001 'The 883 kN / 500 s operating point does not produce 706.4 kN at 400 s sea-level ISP.'

$scaleCurve = $type.GetMethod('ScaleCurve', $flags)
$blendCurves = $type.GetMethod('BlendCurves', $flags)
Assert-True ($null -ne $scaleCurve -and $null -ne $blendCurves) 'Atmosphere-curve helpers are missing.'
Assert-True ($scaleCurve.ReturnType.Name -eq 'FloatCurve') 'ScaleCurve does not return a FloatCurve.'
Assert-True ($blendCurves.ReturnType.Name -eq 'FloatCurve') 'BlendCurves does not return a FloatCurve.'

Write-Output 'ModuleVariableIspThrust tests passed.'
