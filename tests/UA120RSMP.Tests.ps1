[CmdletBinding()]
param([string]$OutputPath)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
foreach ($file in @('UnityEngine.CoreModule.dll','UnityEngine.dll','Assembly-CSharp.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $root "KSP_x64_Data/Managed/$file"))
}
[void][Reflection.Assembly]::LoadFrom((Join-Path $root 'GameData/ModuleManager.4.2.3.dll'))
function Assert-True([bool]$value, [string]$message) { if (-not $value) { throw $message } }
function Read-Config([string]$relative) { [ConfigNode]::Parse([IO.File]::ReadAllText((Join-Path $root $relative))) }
$cache = [IO.File]::ReadAllText((Join-Path $root 'GameData/ModuleManager.ConfigCache'))
$marker = $cache.IndexOf('name = bluedog_UA120' + "`r`n")
Assert-True ($marker -ge 0) 'Merged UA120 not found in current ConfigCache'
$start = $cache.LastIndexOf('UrlConfig', $marker)
$end = $cache.IndexOf('UrlConfig', $marker)
$part = [ConfigNode]::Parse($cache.Substring($start, $end-$start)).GetNode('UrlConfig').GetNode('PART')
$original = $part.CreateCopy()
$stream = New-Object IO.MemoryStream
$logger = New-Object ModuleManager.Logging.StreamLogger($stream)
$progress = New-Object ModuleManager.Progress.PatchProgress($logger)
$database = [ModuleManager.IProtoUrlConfig[]]@()
$context = New-Object ModuleManager.PatchContext($null,$database,$logger,$progress)
function Apply-Patch([ConfigNode]$node) {
    $stack = New-Object 'ModuleManager.Collections.ImmutableStack[ConfigNode]'($script:part)
    $script:part = [ModuleManager.MMPatchLoader]::ModifyNode($stack,$node,$script:context)
}
$patches = (Read-Config 'GameData/zzzArmorOverhual/Mods/Bluedog_DB/zz_UA120_RSMP.cfg').nodes
# Emulate the relevant ordered passes using the actual installed MM patch engine.
Apply-Patch $patches[0]
$generator = (Read-Config 'GameData/RSMP/Templates/RP/katniss rp.cfg').nodes[0]
Apply-Patch $generator
Apply-Patch $patches[1]
Apply-Patch $patches[2]
$logger.Dispose()
Write-Verbose ([Text.Encoding]::UTF8.GetString($stream.ToArray()))
Write-Verbose ($progress.Counter | Out-String)
Assert-True ($progress.Counter.errors.ToString() -eq '0') 'ModuleManager reported patch errors'
Assert-True ($progress.Counter.warnings.ToString() -eq '0') 'ModuleManager reported patch warnings'
Assert-True ($progress.Counter.exceptions.ToString() -eq '0') 'ModuleManager reported patch exceptions'
$modules = @($part.GetNodes('MODULE'))
$wf = @($modules | Where-Object { $_.GetValue('name') -eq 'ModuleWaterfallFX' })
Assert-True ($wf.Count -eq 2) 'Expected exactly main and abort Waterfall modules'
foreach ($spec in @(@('UA120XFX','S1SRB','thrustTransform2'),@('AOUA120AbortFX','S1SRB_Abort','fxAbortTransform2'))) {
    $module = @($wf | Where-Object { $_.GetValue('moduleID') -eq $spec[0] })[0]
    Assert-True ($null -ne $module) "Missing Waterfall $($spec[0])"
    foreach ($controller in $module.GetNodes('CONTROLLER')) {
        if ($controller.GetValue('linkedTo') -in @('throttle','engineEvent')) {
            Assert-True ($controller.GetValue('engineID') -eq $spec[1]) 'Controller bound to wrong engine'
        }
    }
    foreach ($template in $module.GetNodes('TEMPLATE')) {
        Assert-True ($template.GetValue('overrideParentTransform') -eq $spec[2]) 'Old model transform survived'
    }
}
# Every B9 Waterfall reference must resolve to one module, including Radial reset.
$switch = @($modules | Where-Object { $_.GetValue('moduleID') -eq 'meshSwitchType' })[0]
foreach ($subtype in $switch.GetNodes('SUBTYPE')) {
    $references = @($subtype.GetNodes('MODULE') | Where-Object { $_.GetNode('IDENTIFIER').GetValue('name') -eq 'ModuleWaterfallFX' })
    Assert-True ($references.Count -eq 1) 'Expected one plume modifier for each installation type'
    $id = $references[0].GetNode('IDENTIFIER').GetValue('moduleID')
    Assert-True (@($wf | Where-Object { $_.GetValue('moduleID') -eq $id }).Count -eq 1) "Dangling B9 reference $id"
}
$effects = $part.GetNode('EFFECTS')
foreach ($spec in @(@('AOUA120Main','fxTransform2'),@('AOUA120Abort','fxAbortTransform2'))) {
    $effect = $effects.GetNode($spec[0])
    Assert-True ($null -ne $effect) "Missing effect $($spec[0])"
    $smoke = $effect.GetNodes('MODEL_MULTI_SHURIKEN_PERSIST')
    Assert-True ($smoke.Count -eq 1) 'Missing or duplicated RSMP smoke'
    Assert-True ($smoke[0].GetValue('transformName') -eq $spec[1]) "Smoke attached to wrong nozzle: $($spec[0]) -> $($smoke[0].GetValue('transformName'))"
}
foreach ($old in @('running_solid','running_abort','Solid-Lower','solid_abort','katniss-wfss-solid-lower')) {
    Assert-True (-not $effects.HasNode($old)) "Duplicate legacy effect $old"
}
Assert-True ($part.GetNodes('PLUME').Count -eq 0) 'Temporary or legacy PLUME retained'
foreach ($engine in @($modules | Where-Object { $_.GetValue('name') -like 'ModuleEngines*' })) {
    $expected = if ($engine.GetValue('engineID') -eq 'S1SRB') { 'AOUA120Main' } else { 'AOUA120Abort' }
    Assert-True ($engine.GetValue('powerEffectName') -eq $expected) 'Engine still drives a legacy effect'
}
$mec = @($modules | Where-Object { $_.GetValue('name') -eq 'ModuleEngineConfigs' -and $_.GetValue('engineID') -eq 'S1SRB' })[0]
Assert-True ($mec.GetNodes('CONFIG').Count -eq 8) 'Missing RF variants'
foreach ($config in $mec.GetNodes('CONFIG')) {
    Assert-True ($config.GetValue('powerEffectName') -eq 'AOUA120Main') 'RF switch can restore legacy flames'
}
foreach ($asset in @('RSMP/Templates/Waterfall/srb-waterfall.cfg','RSMP/Templates/Waterfall/lemon-SRB-core.cfg','RSMP/Sounds/srb-loop-3.ogg','RealPlume/MP_Nazari_FX/smokelaunch.mu')) {
    Assert-True (Test-Path -LiteralPath (Join-Path $root "GameData/$asset")) "Missing installed asset $asset"
}
# Rendering changes must not alter geometry, propellant, thrust or MEC links.
foreach ($name in @('MODEL','RESOURCE')) {
    Assert-True ((@($part.GetNodes($name) | ForEach-Object ToString) -join '') -eq (@($original.GetNodes($name) | ForEach-Object ToString) -join '')) "Changed $name"
}
foreach ($before in $original.GetNodes('MODULE')) {
    $name = $before.GetValue('name')
    if ($name -notin @('ModuleEnginesRF','ModuleEnginesFX','ModuleFuelTanks','ModuleEngineConfigs','ModuleB9PartSwitch')) { continue }
    $idKey = if ($name -eq 'ModuleB9PartSwitch') { 'moduleID' } else { 'engineID' }
    $after = @($modules | Where-Object { $_.GetValue('name') -eq $name -and $_.GetValue($idKey) -eq $before.GetValue($idKey) })[0]
    $copy = $after.CreateCopy()
    $oldCopy = $before.CreateCopy()
    if ($name -eq 'ModuleB9PartSwitch' -and $before.GetValue('moduleID') -eq 'meshSwitchType') {
        foreach ($item in @($copy,$oldCopy)) { foreach ($subtype in $item.GetNodes('SUBTYPE')) { $subtype.RemoveNodes('MODULE') } }
    }
    foreach ($item in @($copy,$oldCopy)) {
        $item.RemoveValues('powerEffectName'); $item.RemoveValues('runningEffectName')
        foreach ($config in $item.GetNodes('CONFIG')) { $config.RemoveValues('powerEffectName'); $config.RemoveValues('runningEffectName') }
    }
    Assert-True ($copy.ToString() -eq $oldCopy.ToString()) "Changed physical configuration: $name $($before.GetValue($idKey))"
}
if ($OutputPath) { [IO.File]::WriteAllText($OutputPath,$part.ToString()) }
# Optional SmokeScreen path: Waterfall/audio must also work without generated smoke.
$part = $original.CreateCopy()
Apply-Patch $patches[1]
foreach ($id in @('UA120XFX','AOUA120AbortFX')) {
    Assert-True (@($part.GetNodes('MODULE') | Where-Object { $_.GetValue('moduleID') -eq $id }).Count -eq 1) "No-smoke path missing $id"
}
'PASS: actual ModuleManager patching, resolved B9 links, separate engines/nozzles, RSMP smoke, no legacy flames, unchanged physical configurations.'
