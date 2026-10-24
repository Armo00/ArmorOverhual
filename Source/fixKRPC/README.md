# fixKRPC

Runtime compatibility layer for KRPC 0.6.0 and RemoteTech 1.9.x.

The plug-in leaves the original KRPC and RemoteTech assemblies untouched. It
uses the installed Harmony runtime to remove KRPC's duplicate ordinary
FlyByWire callback after RemoteTech registers the same delegate as a sanctioned
pilot. Pending KRPC throttle commands are also copied to the authoritative
RemoteTech `FlightCtrlState` before KRPC consumes its one-shot update flag.

The patch is version-locked to `KRPC.SpaceCenter` 0.6.0.0 and disables itself
if the validated private members cannot be found.
