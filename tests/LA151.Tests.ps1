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
$config = Join-Path $root 'SquadPartsOverhaul\Engine\LA-151.cfg'

Assert-True (Test-Path -LiteralPath $config) 'LA-151 configuration is missing.'
$text = Get-Content -LiteralPath $config -Raw -Encoding UTF8

Assert-True ($text.StartsWith('// Modified 2026-08-20')) 'LA-151 modification date is missing or incorrect.'
Assert-True ($text.Contains('+PART[nuclearEngine]:NEEDS[Squad]:FINAL')) 'LA-151 is not cloned from the stock LV-N with a dependency guard.'
Assert-True ($text.Contains('@name = AO_LA151')) 'LA-151 has no unique part name.'
Assert-True ($text.Contains('@name = ModuleEnginesFX')) 'LA-151 does not restore the exact stock ModuleEnginesFX class.'
Assert-True ($text.Contains('name = ModuleVariableIspThrust')) 'LA-151 does not use the generic variable ISP/thrust module.'
Assert-True ($text.Contains('engineID = LA151')) 'LA-151 engine ID is missing.'
Assert-True ($text.Contains('thrustInterpolation = constantPower')) 'LA-151 does not use constant-power interpolation.'
Assert-True ($text.Contains('!MODULE[ModuleEngineConfigs],*{}')) 'LA-151 does not remove inherited RF engine configurations.'
Assert-True ($text.Contains('!MODULE[ModuleRFInFlightConfigSwitcher],*{}')) 'LA-151 does not remove the RF in-flight switcher.'
Assert-True (-not $text.Contains('name = ModuleEnginesRF')) 'LA-151 still declares ModuleEnginesRF.'
Assert-True (-not $text.Contains('ignoreForIsp')) 'LA-151 still relies on ignoreForIsp for ArcElement consumption.'
Assert-True (-not ($text -match '(?s)PROPELLANT\s*\{[^}]*name\s*=\s*ArcElement')) 'ArcElement is still configured as an engine propellant.'

$gimbalModule = [regex]::Match($text, '(?s)MODULE\s*\{\s*name\s*=\s*ModuleGimbal\s*(?<body>.*?)\}')
Assert-True ($gimbalModule.Success) 'LA-151 has no ModuleGimbal for thrust-vector control.'
$gimbalBody = $gimbalModule.Groups['body'].Value
Assert-True ($gimbalBody -match '(?m)^\s*gimbalTransformName\s*=\s*thrustTransform\s*$') 'LA-151 TVC is not bound to the ReStock thrustTransform.'
Assert-True ($gimbalBody -match '(?m)^\s*gimbalRange\s*=\s*15\s*$') 'LA-151 TVC range is not 15 degrees.'
Assert-True ($gimbalBody -match '(?m)^\s*enablePitch\s*=\s*true\s*$') 'LA-151 TVC pitch control is disabled.'
Assert-True ($gimbalBody -match '(?m)^\s*enableYaw\s*=\s*true\s*$') 'LA-151 TVC yaw control is disabled.'
Assert-True ($gimbalBody -match '(?m)^\s*enableRoll\s*=\s*true\s*$') 'LA-151 TVC roll control is disabled.'
Assert-True ($gimbalBody -match '(?m)^\s*useGimbalResponseSpeed\s*=\s*true\s*$') 'LA-151 TVC does not use an explicit response speed.'
Assert-True ($gimbalBody -match '(?m)^\s*gimbalResponseSpeed\s*=\s*16\s*$') 'LA-151 TVC response speed is incorrect.'

$inputResource = [regex]::Match($text, '(?s)INPUT_RESOURCE\s*\{(?<body>.*?)\}')
Assert-True ($inputResource.Success) 'LA-151 has no throttle-linked INPUT_RESOURCE.'
$inputBody = $inputResource.Groups['body'].Value
Assert-True ($inputBody -match '(?m)^\s*name\s*=\s*ArcElement\s*$') 'LA-151 throttle input is not ArcElement.'
Assert-True ($inputBody -match '(?m)^\s*flowMode\s*=\s*NO_FLOW\s*$') 'LA-151 ArcElement input does not use NO_FLOW.'
$arcRatioMatch = [regex]::Match($inputBody, '(?m)^\s*ratio\s*=\s*(?<ratio>\d+(?:\.\d+)?)')
Assert-True ($arcRatioMatch.Success) 'LA-151 ArcElement input ratio is missing.'
$arcRatio = [double]$arcRatioMatch.Groups['ratio'].Value
Assert-Near $arcRatio 0.00002296296 0.000000000001 'LA-151 ArcElement full-throttle rate is incorrect.'
$arcDurationDays = 100.0 / $arcRatio / 86400.0
Assert-Near $arcDurationDays 50.4032 0.001 'LA-151 does not provide approximately 50 days from 100 units of ArcElement.'

$outputResource = [regex]::Match($text, '(?s)OUTPUT_RESOURCE\s*\{(?<body>.*?)\}')
Assert-True ($outputResource.Success) 'LA-151 has no throttle-linked OUTPUT_RESOURCE.'
$outputBody = $outputResource.Groups['body'].Value
Assert-True ($outputBody -match '(?m)^\s*name\s*=\s*LqdHelium\s*$') 'LA-151 throttle output is not LqdHelium.'
Assert-True ($outputBody -match '(?m)^\s*ratio\s*=\s*0\.00009589698\s*$') 'LA-151 LqdHelium output ratio is incorrect.'
Assert-True ($outputBody -match '(?m)^\s*flowMode\s*=\s*NO_FLOW\s*$') 'LA-151 LqdHelium output does not use NO_FLOW.'
Assert-True ($outputBody -match '(?m)^\s*dumpExcess\s*=\s*true\s*$') 'LA-151 LqdHelium output is not allowed to dump excess.'

$pointPattern = '(?s)PERFORMANCE_POINT\s*\{\s*percent\s*=\s*(?<percent>\d+(?:\.\d+)?)\s*maxThrust\s*=\s*(?<thrust>\d+(?:\.\d+)?).*?key\s*=\s*0\s+(?<vacuum>\d+(?:\.\d+)?).*?key\s*=\s*1\s+(?<seaLevel>\d+(?:\.\d+)?)\s*\}'
$matches = [regex]::Matches($text, $pointPattern)
Assert-True ($matches.Count -eq 3) "Expected three LA-151 performance points, found $($matches.Count)."

$expectedPercents = @(0.0, 50.0, 100.0)
$expectedThrusts = @(3964.66, 1982.33, 1321.55)
$points = @()
for ($index = 0; $index -lt $expectedPercents.Count; $index++) {
    $actual = $matches[$index].Groups
    $point = @{
        Percent = [double]$actual['percent'].Value
        Thrust = [double]$actual['thrust'].Value
        Vacuum = [double]$actual['vacuum'].Value
        SeaLevel = [double]$actual['seaLevel'].Value
    }
    Assert-Near $point.Percent $expectedPercents[$index] 0.001 "Performance point $index percent is incorrect."
    Assert-Near $point.Thrust $expectedThrusts[$index] 0.01 "Performance point $index thrust is incorrect."
    Assert-True ($point.Thrust -gt 0) "Performance point $index thrust must be positive."
    Assert-True ($point.Vacuum -gt 0) "Performance point $index vacuum ISP must be positive."
    Assert-True ($point.SeaLevel -gt 0) "Performance point $index sea-level ISP must be positive."
    $points += $point
}

$seaLevelRatio = $points[0].SeaLevel / $points[0].Vacuum
foreach ($point in $points) {
    Assert-Near ($point.SeaLevel / $point.Vacuum) $seaLevelRatio 0.0001 'LA-151 atmosphere curves do not use a consistent sea-level/vacuum ISP ratio.'
}

$powerFactors = foreach ($point in $points) {
    $point.Thrust * $point.Vacuum
}
$averagePowerFactor = ($powerFactors | Measure-Object -Average).Average
foreach ($powerFactor in $powerFactors) {
    Assert-True ([Math]::Abs($powerFactor - $averagePowerFactor) / $averagePowerFactor -lt 0.005) 'LA-151 performance points deviate by more than 0.5% from constant power.'
}

$standardGravity = 9.80665
foreach ($point in $points) {
    $jetPowerGW = 0.5 * $point.Thrust * 1000.0 * $point.Vacuum * $standardGravity / 1.0e9
    Assert-Near $jetPowerGW 9.72 0.001 'LA-151 performance point does not represent 90% of 10.8 GW.'
}

Write-Output 'LA-151 tests passed.'
