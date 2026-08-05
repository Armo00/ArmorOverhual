# ArmorOverhaul plugin

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
