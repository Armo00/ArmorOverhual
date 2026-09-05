# Controlled Corridor Link

Exposes the existing KAS `StartLinkContextMenuAction` and
`BreakLinkContextMenuAction` events in the part action window when a Flexible
Corridor belongs to the active, unpacked, controllable vessel. KAS can inject
the break action into the target endpoint; the plugin recognizes that event as
well, allowing either available endpoint to disconnect the corridor.

The plugin does not create links itself. Target selection, geometry limits,
collision checks, state transitions, coupling, sounds, and cancellation remain
inside KAS. The original unfocused/EVA event remains unchanged.
