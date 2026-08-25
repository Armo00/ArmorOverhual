$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$source = Get-Content -LiteralPath (Join-Path $root 'Source\RoboticsKJRCompat\RoboticsKJRCompatAddon.cs') -Raw
$project = Get-Content -LiteralPath (Join-Path $root 'Source\RoboticsKJRCompat\RoboticsKJRCompat.csproj') -Raw
$config = Get-Content -LiteralPath (Join-Path $root 'RoboticsKJRCompat.cfg') -Raw

function Assert-Contains {
    param([string]$Text, [string]$Expected, [string]$Message)
    if (-not $Text.Contains($Expected)) { throw $Message }
}

Assert-Contains $source 'SuspendedVessels' 'Suspension must be tracked per vessel.'
Assert-Contains $source 'part.ReleaseAutoStruts();' 'Every existing physical Auto Strut must be released.'
Assert-Contains $source 'VesselUpdateAutoStrutPatch' 'Auto Strut rebuilding must be blocked while suspended.'
Assert-Contains $source 'VesselAutoStrutAnchorPatch' 'Anchor lookup must be blocked while suspended.'
Assert-Contains $source 'vessel.CycleAllAutoStrut();' 'Preserved Auto Strut modes must be rebuilt after all servos lock.'
Assert-Contains $source 'HasUnlockedServo(vessel)' 'Restoration must wait for the last unlocked servo.'
Assert-Contains $source 'GameEvents.onRoboticPartLockChanged.Fire' 'KJR must be notified for craft loaded unlocked.'
Assert-Contains $source '"UpdatePartJoint"' 'KJR direct robotic joint rewriting must remain intercepted.'
Assert-Contains $source 'PrepareServoLockTransition(__instance)' 'Suspension must begin before stock unlock callbacks.'
Assert-Contains $source 'Robotics / Auto Strut Manager' 'Every servo must expose the manager window event.'
Assert-Contains $source 'Lock All' 'The window must provide one-click locking.'
Assert-Contains $source 'Unlock All' 'The window must provide one-click unlocking.'
Assert-Contains $source 'GetPhysicalAutoStrutCount' 'The window must report physical Auto Strut state.'
Assert-Contains $source 'ServoIsLockedField.SetValue(servo, locked)' 'Window commands must update the stock PAW field.'
Assert-Contains $source 'ModifyServoLockedMethod.Invoke' 'Window commands must use the full stock PAW callback route.'
Assert-Contains $source 'uiControlFlight.onFieldChanged += pawLockCallback' 'Each servo manager must subscribe directly to the PAW field callback.'
Assert-Contains $source 'uiControlFlight.onFieldChanged -= pawLockCallback' 'The direct PAW callback must be removed with its module.'
Assert-Contains $source 'RecoverMissingPawControlCallback' 'The direct PAW callback must recover missing stock lock processing.'
Assert-Contains $source 'lastProcessedState == currentState' 'Working stock PAW callbacks must not be duplicated.'
Assert-Contains $source 'private static void OnRoboticPartLockChanged' 'Global event handlers must not retain destroyed controllers.'
Assert-Contains $source 'private void OnDisable()' 'Event subscriptions must be removed when the flight controller is disabled.'
Assert-Contains $source 'KSPAssemblyDependency("HarmonyKSP", 1, 0)' 'Harmony must use the KSP loader assembly.'
Assert-Contains $project '<Version>1.4.3</Version>' 'The assembly version must be 1.4.3.'
Assert-Contains $project '<Private>False</Private>' 'Game references must not be copied into Plugins.'
Assert-Contains $config '@MODULE[ModuleRoboticServo*]' 'The manager module must be added to every servo part.'
Assert-Contains $config '!MODULE[ModuleRoboticsConstraintManager]' 'The manager module must not be duplicated.'
Assert-Contains $config 'name = ModuleRoboticsConstraintManager' 'The config must add the correct PartModule.'

Write-Host 'RoboticsKJRCompat static tests passed.'
