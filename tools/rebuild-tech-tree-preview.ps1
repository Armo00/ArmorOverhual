param(
    [string]$Path = (Join-Path $PSScriptRoot '..\docs\KSP Tech Tree Overhaul.drawio')
)

$resolvedPath = [System.IO.Path]::GetFullPath($Path)
[xml]$xml = Get-Content -LiteralPath $resolvedPath -Raw -Encoding UTF8
$model = $xml.mxfile.diagram.mxGraphModel
$root = $model.root

$model.SetAttribute('dx', '3000')
$model.SetAttribute('dy', '2400')
$model.SetAttribute('pageWidth', '3000')
$model.SetAttribute('pageHeight', '2400')
$xml.mxfile.diagram.SetAttribute('id', 't0-t3-multidirectional-v6')
$xml.mxfile.diagram.SetAttribute('name', 'T0-T3 Multi-Directional v6 Orthogonal')

$removePrefixes = @('band_', 'header_', 'axis_', 'legend', 'note', 'title', 'subtitle', 'sector_', 'tier_', 'future_')
@($root.mxCell) | Where-Object {
    $id = [string]$_.id
    $remove = $false
    foreach ($prefix in $removePrefixes) {
        if ($id.StartsWith($prefix)) {
            $remove = $true
            break
        }
    }
    $remove
} | ForEach-Object {
    [void]$root.RemoveChild($_)
}

$positions = @{
    start                               = @(1280, 1000, 130, 90)

    chemicalRocketry                   = @(1240, 780, 210, 64)
    liquidRocketEngines                = @(950, 600, 220, 64)
    solidRocketMotors                  = @(1270, 590, 220, 64)
    pressureFedEngines                 = @(640, 420, 230, 70)
    gasGeneratorEngines                = @(950, 400, 230, 70)
    improvedSolidMotors                = @(1270, 410, 240, 70)
    advancedPressureFedEngines         = @(350, 220, 250, 70)
    expanderCycleEngines               = @(640, 180, 235, 70)
    improvedGasGeneratorEngines        = @(930, 160, 245, 70)
    tapOffCycleEngines                 = @(1220, 160, 250, 70)
    compositeSolidPropellants          = @(1510, 360, 250, 70)

    materials101                       = @(1500, 900, 205, 64)
    engineering101                     = @(1500, 1080, 205, 64)
    propellantStorage                  = @(1750, 720, 240, 70)
    fuelSystems                        = @(2050, 620, 230, 70)
    cryogenicPropellantManagement      = @(2350, 520, 255, 76)
    basicConstruction                  = @(1760, 900, 220, 70)
    structuralEngineering             = @(2050, 850, 225, 70)
    improvedConstruction              = @(2350, 790, 240, 70)
    improvedMaterials                 = @(2350, 890, 230, 70)
    aerodynamics                       = @(1760, 1040, 215, 64)
    subsonicFlight                     = @(2050, 1040, 225, 70)
    supersonicFlight                   = @(2350, 1040, 230, 70)
    flightControl                      = @(1760, 1180, 215, 64)
    landingSystems                     = @(2050, 1230, 225, 70)
    advancedLandingSystems             = @(2350, 1230, 245, 70)
    thermalManagement                  = @(1510, 1360, 230, 70)
    improvedThermalManagement          = @(1780, 1360, 250, 70)

    basicElectrics                     = @(1800, 1500, 215, 64)
    earlyAvionics                      = @(2600, 1500, 225, 70)
    electricalEngineering             = @(1450, 1720, 235, 70)
    powerGeneration                    = @(1750, 1720, 230, 70)
    basicCommunications                = @(2050, 1720, 240, 70)
    basicActuators                     = @(2350, 1720, 225, 70)
    basicComputerSystems               = @(2600, 1850, 235, 70)
    solarPower                         = @(1600, 1950, 245, 70)
    fuelCellTechnology                 = @(1850, 2070, 230, 70)
    improvedCommunications             = @(2100, 2000, 250, 70)
    advancedFlightControl              = @(2350, 1950, 240, 70)
    basicRobotics                      = @(2650, 2100, 225, 70)

    scientificMethod                   = @(1000, 1150, 215, 64)
    scientificInstrumentation          = @(750, 1350, 245, 70)
    basicSpaceScience                  = @(500, 1550, 225, 70)
    resourceProspecting                = @(750, 1650, 235, 70)
    advancedScientificInstrumentation = @(180, 1770, 270, 76)
    orbitalScience                     = @(460, 1900, 225, 70)
    resourceSurveying                  = @(740, 2000, 235, 70)

    survivability                      = @(1250, 1360, 215, 70)
    humanSpaceflight                   = @(1200, 1580, 225, 70)
    basicLifeSupport                   = @(950, 1800, 225, 70)
    simpleCommandModules               = @(1200, 1900, 240, 70)
    evaSystems                         = @(1320, 1800, 215, 70)
}

foreach ($entry in $positions.GetEnumerator()) {
    $cell = @($root.mxCell) | Where-Object { $_.id -eq $entry.Key } | Select-Object -First 1
    if (-not $cell) {
        throw "Missing node: $($entry.Key)"
    }
    $cell.SetAttribute('parent', '1')
    $geometry = $cell.mxGeometry
    $geometry.SetAttribute('x', [string]$entry.Value[0])
    $geometry.SetAttribute('y', [string]$entry.Value[1])
    $geometry.SetAttribute('width', [string]$entry.Value[2])
    $geometry.SetAttribute('height', [string]$entry.Value[3])
}

$domainColors = @{
    propulsion = '#82b366'
    vehicle    = '#d79b00'
    electrical = '#d6b656'
    science    = '#9673a6'
    crew       = '#b85450'
}

$domainFills = @{
    propulsion = '#eaf4e8'
    vehicle    = '#fff2df'
    electrical = '#fffbe6'
    science    = '#f3edf7'
    crew       = '#fbeaea'
}

$domainNodes = @{
    propulsion = @('chemicalRocketry', 'liquidRocketEngines', 'solidRocketMotors', 'pressureFedEngines',
        'gasGeneratorEngines', 'improvedSolidMotors', 'advancedPressureFedEngines', 'expanderCycleEngines',
        'improvedGasGeneratorEngines', 'tapOffCycleEngines', 'compositeSolidPropellants')
    vehicle = @('materials101', 'engineering101', 'propellantStorage', 'fuelSystems',
        'cryogenicPropellantManagement', 'basicConstruction', 'structuralEngineering', 'improvedConstruction',
        'improvedMaterials', 'aerodynamics', 'subsonicFlight', 'supersonicFlight', 'flightControl',
        'landingSystems', 'advancedLandingSystems', 'thermalManagement', 'improvedThermalManagement')
    electrical = @('basicElectrics', 'earlyAvionics', 'electricalEngineering', 'powerGeneration',
        'basicCommunications', 'basicActuators', 'basicComputerSystems', 'solarPower', 'fuelCellTechnology',
        'improvedCommunications', 'advancedFlightControl', 'basicRobotics')
    science = @('scientificMethod', 'scientificInstrumentation', 'basicSpaceScience', 'resourceProspecting',
        'advancedScientificInstrumentation', 'orbitalScience', 'resourceSurveying')
    crew = @('survivability', 'humanSpaceflight', 'basicLifeSupport', 'simpleCommandModules', 'evaSystems')
}

foreach ($domain in $domainNodes.Keys) {
    foreach ($nodeId in $domainNodes[$domain]) {
        $cell = @($root.mxCell) | Where-Object { $_.id -eq $nodeId } | Select-Object -First 1
        $style = [string]$cell.style
        $style = [regex]::Replace($style, 'strokeColor=#[0-9A-Fa-f]{6};', "strokeColor=$($domainColors[$domain]);")
        $style = [regex]::Replace($style, 'fillColor=#[0-9A-Fa-f]{6};', "fillColor=$($domainFills[$domain]);")
        $cell.SetAttribute('style', $style)
    }
}

$edgeDomains = @{
    propulsion = @('e_start_chemical', 'e_chemical_liquid', 'e_chemical_solid', 'e_liquid_pressure',
        'e_liquid_gg', 'e_solid_improved', 'e_pressure_advancedpressure', 'e_pressure_expander',
        'e_gg_improvedgg', 'e_gg_tapoff', 'e_solid_composite')
    vehicle = @('e_start_materials', 'e_start_engineering', 'e_material_storage',
        'e_engineering_construction', 'e_engineering_aero', 'e_engineering_control', 'e_storage_fuelsystems',
        'e_construction_structural', 'e_aero_subsonic', 'e_construction_landing', 'e_material_thermal',
        'e_fuels_cryo', 'e_struct_improvedconstruction', 'e_struct_improvedmaterials',
        'e_subsonic_supersonic', 'e_landing_advancedlanding', 'e_thermal_improvedthermal')
    electrical = @('e_engineering_electrics', 'e_control_avionics', 'e_electrics_engineering',
        'e_electrics_power', 'e_electrics_comms', 'e_electrics_actuators', 'e_avionics_computers',
        'e_actuators_flightcontrol', 'e_power_solar', 'e_power_fuelcells', 'e_comms_improvedcomms',
        'e_actuators_robotics')
    science = @('e_start_science', 'e_science_instruments', 'e_instruments_spacescience',
        'e_spacescience_prospecting', 'e_spacescience_advancedinstruments', 'e_spacescience_orbitalscience',
        'e_prospecting_surveying')
    crew = @('e_engineering_survival', 'e_survival_human', 'e_human_lifesupport',
        'e_human_command', 'e_human_eva')
}

foreach ($domain in $edgeDomains.Keys) {
    foreach ($edgeId in $edgeDomains[$domain]) {
        $edge = @($root.mxCell) | Where-Object { $_.id -eq $edgeId } | Select-Object -First 1
        if (-not $edge) {
            throw "Missing edge: $edgeId"
        }
        $edge.SetAttribute('parent', '1')
        $edge.SetAttribute(
            'style',
            "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=18;html=1;" +
            "strokeColor=$($domainColors[$domain]);strokeWidth=2;opacity=65;endArrow=block;endFill=1;"
        )
        if ($edge.mxGeometry) {
            [void]$edge.RemoveChild($edge.mxGeometry)
        }
        $edgeGeometry = $xml.CreateElement('mxGeometry')
        $edgeGeometry.SetAttribute('relative', '1')
        $edgeGeometry.SetAttribute('as', 'geometry')
        [void]$edge.AppendChild($edgeGeometry)
    }
}

$thermalEdge = @($root.mxCell) | Where-Object { $_.id -eq 'e_material_thermal' } | Select-Object -First 1
$thermalEdge.SetAttribute('source', 'survivability')
$thermalNode = @($root.mxCell) | Where-Object { $_.id -eq 'thermalManagement' } | Select-Object -First 1
$thermalNode.SetAttribute('value', ([string]$thermalNode.value -replace '\+ Survivability', '+ Materials 101'))

function Set-OrthogonalEdgeRoute {
    param(
        [string]$EdgeId,
        [string]$Color,
        [array]$Points
    )

    $edge = @($root.mxCell) | Where-Object { $_.id -eq $EdgeId } | Select-Object -First 1
    $edge.SetAttribute(
        'style',
        "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=18;html=1;" +
        "strokeColor=$Color;strokeWidth=2;opacity=65;endArrow=block;endFill=1;"
    )
    $pointArray = $xml.CreateElement('Array')
    $pointArray.SetAttribute('as', 'points')
    foreach ($point in $Points) {
        $mxPoint = $xml.CreateElement('mxPoint')
        $mxPoint.SetAttribute('x', [string]$point[0])
        $mxPoint.SetAttribute('y', [string]$point[1])
        [void]$pointArray.AppendChild($mxPoint)
    }
    [void]$edge.mxGeometry.AppendChild($pointArray)
}

Set-OrthogonalEdgeRoute 'e_construction_landing' $domainColors.vehicle @(
    @(2010, 935),
    @(2010, 1265)
)
Set-OrthogonalEdgeRoute 'e_engineering_electrics' $domainColors.electrical @(
    @(1485, 1112),
    @(1485, 1470),
    @(1907, 1470)
)
Set-OrthogonalEdgeRoute 'e_electrics_engineering' $domainColors.electrical @(
    @(1790, 1620),
    @(1567, 1620),
    @(1567, 1720)
)
Set-OrthogonalEdgeRoute 'e_electrics_comms' $domainColors.electrical @(
    @(2015, 1620),
    @(2170, 1620),
    @(2170, 1720)
)
Set-OrthogonalEdgeRoute 'e_electrics_actuators' $domainColors.electrical @(
    @(2015, 1590),
    @(2462, 1590),
    @(2462, 1720)
)
Set-OrthogonalEdgeRoute 'e_control_avionics' $domainColors.electrical @(
    @(2020, 1212),
    @(2860, 1212),
    @(2860, 1535)
)

function Add-TextCell {
    param(
        [string]$Id,
        [string]$Value,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [int]$Height,
        [string]$Style
    )

    $cell = $xml.CreateElement('mxCell')
    $cell.SetAttribute('id', $Id)
    $cell.SetAttribute('value', $Value)
    $cell.SetAttribute('style', $Style)
    $cell.SetAttribute('parent', '1')
    $cell.SetAttribute('vertex', '1')
    $geometry = $xml.CreateElement('mxGeometry')
    $geometry.SetAttribute('x', [string]$X)
    $geometry.SetAttribute('y', [string]$Y)
    $geometry.SetAttribute('width', [string]$Width)
    $geometry.SetAttribute('height', [string]$Height)
    $geometry.SetAttribute('as', 'geometry')
    [void]$cell.AppendChild($geometry)
    [void]$root.AppendChild($cell)
}

$titleStyle = 'text;html=1;align=left;verticalAlign=middle;fontStyle=1;fontSize=24;fontColor=#1f2937;'
$subtitleStyle = 'text;html=1;align=left;verticalAlign=middle;fontSize=13;fontColor=#6b7280;'
$sectorStyle = 'text;html=1;align=left;verticalAlign=middle;fontStyle=1;fontSize=15;'
$tierStyle = 'text;html=1;align=center;verticalAlign=middle;fontStyle=1;fontSize=12;fontColor=#6b7280;'

Add-TextCell 'title' 'T0-T3 MULTI-DIRECTIONAL TECHNOLOGY TREE' 70 30 900 36 $titleStyle
Add-TextCell 'subtitle' 'Distance from Start along each branch represents tier progression.' 70 66 720 28 $subtitleStyle
Add-TextCell 'sector_propulsion' 'PROPULSION / PROPELLANTS  |  UP' 80 110 420 28 ($sectorStyle + 'fontColor=#3d7f35;')
Add-TextCell 'sector_vehicle' 'VEHICLE / MATERIALS / FLIGHT  |  RIGHT' 2390 700 520 28 ($sectorStyle + 'fontColor=#a06400;')
Add-TextCell 'sector_electrical' 'ELECTRICAL / AVIONICS  |  DOWN-RIGHT' 2600 1600 380 28 ($sectorStyle + 'fontColor=#8a7000;')
Add-TextCell 'sector_science' 'SCIENCE / RESOURCES  |  DOWN-LEFT' 80 1460 460 28 ($sectorStyle + 'fontColor=#6f4f7e;')
Add-TextCell 'sector_crew' 'CREW / SURVIVAL  |  DOWN' 1020 1710 390 28 ($sectorStyle + 'fontColor=#8b3e3b;')

Add-TextCell 'tier_legend' 'T0  5 SCIENCE   |   T1  20   |   T2  45   |   T3  90' 2100 45 720 26 $tierStyle
Add-TextCell 'future_note' 'T4-T7 continue outward along every branch' 2350 2335 520 30 ($subtitleStyle + 'align=right;fontStyle=1;')

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
$settings.IndentChars = '  '
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)
$settings.NewLineChars = "`r`n"
$settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

$writer = [System.Xml.XmlWriter]::Create($resolvedPath, $settings)
try {
    $xml.Save($writer)
}
finally {
    $writer.Dispose()
}

Write-Output "Rebuilt $resolvedPath"
