$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..'))
$managed = Join-Path $projectRoot 'KSP_x64_Data\Managed'
$dll = Join-Path $projectRoot 'BuildArtifacts\EditorSearchOptimizer\bin\EditorSearchOptimizer.dll'
function Assert-Condition([bool]$value, [string]$message) { if (-not $value) { throw $message } }
foreach ($name in @('UnityEngine.CoreModule.dll','UnityEngine.dll','UnityEngine.UI.dll','UnityEngine.UIModule.dll','Assembly-CSharp.dll')) {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $managed $name))
}
[void][Reflection.Assembly]::LoadFrom((Join-Path $projectRoot 'GameData\000_Harmony\0Harmony.dll'))
$assembly = [Reflection.Assembly]::LoadFrom($dll)
$type = $assembly.GetType('ArmorOverhaul.EditorSearch.SearchOptimizer', $true)
$flags = [Reflection.BindingFlags]'Static,NonPublic,Public,Instance'
$budget = $type.GetMethod('BatchFinished', $flags)
foreach ($case in @(
    @(0,0.0,$false), @(23,4.99,$false), @(24,0.0,$true), @(1,5.0,$true), @(25,0.0,$true), @(0,20.0,$true)
)) {
    $actual = $budget.Invoke($null, [object[]]@([int]$case[0],[double]$case[1]))
    Assert-Condition ($actual -eq $case[2]) "Batch policy failed: $case"
}
$listType = $type.Assembly.GetReferencedAssemblies() | Where-Object Name -eq 'Assembly-CSharp'
Assert-Condition ($null -ne $listType) 'KSP reference missing'
$ksp = [Reflection.Assembly]::LoadFrom((Join-Path $managed 'Assembly-CSharp.dll'))
$editor = $ksp.GetType('KSP.UI.Screens.EditorPartList', $true)
foreach ($field in @('categoryList','icons','iconCache','state')) {
    Assert-Condition ($null -ne $editor.GetField($field,$flags)) "Missing editor field $field"
}
foreach ($method in @('ClearAllItems','UpdatePartIcon','UpdatePartIcons','OnPartListUpdate','RefreshSearchList','Refresh')) {
    Assert-Condition (@($editor.GetMethods($flags) | Where-Object Name -eq $method).Count -gt 0) "Missing editor method $method"
}
$job = $type.GetNestedType('SearchJob', [Reflection.BindingFlags]'Public')
$cancel = $job.GetMethod('Cancel')
# No MonoBehaviour constructor/native engine is needed to check cancellation generations.
$fakeJob = [Runtime.Serialization.FormatterServices]::GetUninitializedObject($job)
$generation = $job.GetField('generation',$flags)
$cancel.Invoke($fakeJob,@())
$cancel.Invoke($fakeJob,@())
Assert-Condition ($generation.GetValue($fakeJob) -eq 2) 'Cancellation did not invalidate stale batches'
$sourcePath = Join-Path $projectRoot 'GameData\zzzArmorOverhual\Source\EditorSearchOptimizer\EditorSearchOptimizer.cs'
$sourceText = [IO.File]::ReadAllText($sourcePath)
Assert-Condition ($sourceText.Contains('list.ToArray()')) 'Search results must be snapshotted, not truncated'
Assert-Condition ($sourceText.Contains('UpdateIcon.Invoke')) 'Existing icon hooks must be retained'
Assert-Condition ($sourceText.Contains('fallback = true')) 'Stock fallback missing'
Assert-Condition ($sourceText.Contains('ticket == generation')) 'Stale-search protection missing'
Assert-Condition (-not $sourceText.Contains('Task.Run')) 'Unity work must remain on the main thread'
Assert-Condition (-not (Test-Path (Join-Path $projectRoot 'GameData\zzzArmorOverhual\Source\EditorSearchOptimizer\obj'))) 'Build artifacts leaked into GameData'
Write-Output 'PASS: 6 batch-budget cases, cancellation generations, installed editor API, full-result snapshot, mod hooks, fallback, and build isolation.'
