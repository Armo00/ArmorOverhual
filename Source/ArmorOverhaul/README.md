# ArmorOverhaul plugin

## Arc reactor resource converter

`ModuleArcReactor` extends the stock `ModuleResourceConverter` with a
persistent 0-100 percent PAW power slider. Input and output rates are the rates
at 100 percent power; the slider scales the complete recipe linearly. At zero
percent the converter remains activated but consumes and produces nothing.

In flight, the PAW also shows the actual electrical power produced from the
converter's measured load (`1 EC/s = 3.6 kW`). The display selects W, kW, MW,
or GW automatically. Stock percentage-load readouts remain intact, while the
ambiguous zero-output states are reported as `Offline`, `Output Disabled`,
`Standby`, or `Starting` as appropriate.

Status: compiled and covered by static/assembly tests; in-game PAW and resource
flow testing is pending.

It retains the stock start, stop, and toggle events/actions. It intentionally
adds no power-preset actions.

```cfg
MODULE
{
    name = ModuleArcReactor

    ConverterName = Arc Reactor
    StartActionName = Start Arc Reactor
    StopActionName = Stop Arc Reactor
    ToggleActionName = Toggle Arc Reactor

    powerPercentage = 100
    sliderStep = 1
    FillAmount = 0.95

    AutoShutdown = false
    GeneratesHeat = false
    UseSpecialistBonus = false

    INPUT_RESOURCE
    {
        ResourceName = ArcElement
        Ratio = 0.00002296296
        FlowMode = NO_FLOW
    }

    OUTPUT_RESOURCE
    {
        ResourceName = ElectricCharge
        Ratio = 3000000
        FlowMode = ALL_VESSEL
        blocked_when_full = true
    }

    OUTPUT_RESOURCE
    {
        ResourceName = LqdHelium
        Ratio = 0.00009589698
        // blocked_when_full defaults to false
    }
}
```

For each `OUTPUT_RESOURCE`:

- `blocked_when_full = true` maps to stock `DumpExcess = false` and makes that
  output regulate the complete recipe. As its storage approaches
  `FillAmount`, actual input consumption and every output rate are reduced to
  exactly the rate that fits in the remaining target headroom.
- `blocked_when_full = false`, or an omitted field, maps to stock
  `DumpExcess = true`. Production continues and output that cannot be stored
  is discarded.

The slider is a maximum-power setting, not a forced output. Below the
`FillAmount` target, the converter remains at the requested maximum whenever
the remaining target headroom can contain a complete physics step of output.
Only when the next full-power step would cross the target does it reduce that
step to the exact amount needed. It then follows the observed resource deficit
near the target. It stays activated and does not use start/stop water levels.
If demand exceeds its configured maximum, it runs at that maximum and storage
is allowed to fall.

KSP reports output-direction resource totals as spare capacity. The regulator
therefore subtracts the capacity reserved above `FillAmount` from that spare
capacity; it does not interpret the value as the currently stored amount.

If several outputs use `blocked_when_full = true`, the output with the least
available target headroom limits the complete recipe. The calculation uses
the stock physics-step duration only to convert configured rates into resource
amounts; it has no countdown, cooldown, coroutine, or timer-driven state.
Input shortages retain the stock converter behavior.

Set `FlowMode` explicitly for every regulated output. Stock `ResourceRatio`
defaults an omitted flow mode to `NO_FLOW`, which limits both production and
fill-level measurement to the converter's own part. Vessel-wide electrical
generation should use `FlowMode = ALL_VESSEL`.

## Variable ISP/thrust for stock ModuleEnginesFX

`ModuleVariableIspThrust` continuously adjusts the ISP curve and maximum
thrust of one stock `ModuleEnginesFX`. It deliberately rejects derived engine
types, including RealFuels engines. `engineID` may be omitted only when the
part contains exactly one stock `ModuleEnginesFX`.

Status: compiled and covered by static/assembly tests; in-game PAW testing is
pending.

The persistent PAW slider is available in the editor and, by default, in
flight while the engine is running. Six action-group actions select 0, 20,
40, 60, 80, or 100 percent directly. The PAW also reports vacuum ISP, vacuum
maximum thrust, and current-environment ISP.

```cfg
MODULE
{
    name = ModuleVariableIspThrust
    engineID = MainEngine

    performanceSetting = 50
    sliderStep = 1
    allowInFlight = true
    allowWhileRunning = true
    scaleMinThrust = true
    thrustInterpolation = linear // linear or constantPower

    INPUT_RESOURCE
    {
        name = ArcElement
        ratio = 0.00002296296
        flowMode = NO_FLOW
    }

    OUTPUT_RESOURCE
    {
        name = LqdHelium
        ratio = 0.00009589698
        flowMode = NO_FLOW
        dumpExcess = true
    }

    PERFORMANCE_POINT
    {
        percent = 0
        maxThrust = 1000

        atmosphereCurve
        {
            key = 0 300
            key = 1 260
        }
    }

    PERFORMANCE_POINT
    {
        percent = 100
        maxThrust = 600

        atmosphereCurve
        {
            key = 0 450
            key = 1 320
        }
    }
}
```

`INPUT_RESOURCE` and `OUTPUT_RESOURCE` are optional and may each appear any
number of times. Their `ratio` is an absolute resource rate in units per
second at 100 percent engine throttle; it is independent of propellant mass
flow, ISP, and the performance slider. The actual rate is
`ratio * currentThrottle`, integrated over physics time. `flowMode` is
optional and otherwise uses the resource definition's default flow mode.

All inputs form one recipe. When an input is short, the module processes the
available fraction for the final physics tick and shuts the engine down.
Outputs default to `dumpExcess = true`, so unavailable storage does not
throttle or stop the engine. Set `dumpExcess = false` when a full output must
shut the engine down. These resources are processed only in flight while the
target engine is ignited and operational. Use this mechanism, rather than an
engine `PROPELLANT` with `ignoreForIsp`, for fixed companion rates such as
reactor consumables and byproducts.

At least the 0 and 100 percent points are required. Intermediate points are
optional. All explicitly supplied `atmosphereCurve` nodes must use identical
pressure keys. At intermediate slider positions, the module blends every ISP
key and its tangents.

A point may use `vacuumIsp` instead of `atmosphereCurve`. In that shorthand,
the engine's original atmosphere curve is scaled by the ratio between the
point's vacuum ISP and the original vacuum ISP:

```cfg
PERFORMANCE_POINT
{
    percent = 0
    vacuumIsp = 300
    maxThrust = 1000
}
```

`maxThrust` is vacuum maximum thrust in kN. With `linear`, maximum thrust is
linearly interpolated between adjacent points. With `constantPower`, the
module interpolates `maxThrust * vacuumIsp` and derives thrust from the current
vacuum ISP. An optional `minThrust` may be supplied per point; otherwise the
original minimum/maximum thrust ratio is preserved when `scaleMinThrust` is
true.

Do not attach this module to `ModuleEngines`, `ModuleEnginesRF`, or another
class derived from `ModuleEnginesFX`.

`ModuleRFInFlightConfigSwitcher` switches configurations on one live
`ModuleEnginesRF` instance by calling RealFuels' public
`ModuleEngineConfigs.SetConfiguration(string, bool)` API.

The module discovers RF configurations automatically. Set
`allowedConfigurations` to a comma-, semicolon-, or pipe-separated list only
when a part should expose a subset or a custom order.

```cfg
MODULE
{
    name = ModuleRFInFlightConfigSwitcher
    engineID = ExampleEngine
    allowedConfigurations = Config-A, Config-B, Config-C
    allowSwitchWhileRunning = true
    keepWindowOpen = true
    showScreenMessage = true
    switchCooldown = 0.25
}
```

The targeted `ModuleEnginesRF` and `ModuleEngineConfigs` must use the same
`engineID`. In flight, the PAW opens or closes the selector window. Action
groups can invoke `Next Engine Configuration` and
`Previous Engine Configuration`.
