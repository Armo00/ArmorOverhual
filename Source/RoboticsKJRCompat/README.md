# RoboticsKJRCompat

Compatibility plug-in for KSP 1.12.x Breaking Ground robotics and Kerbal Joint
Reinforcement Continued.

When any servo on a vessel is unlocked, the plug-in suspends every physical
stock Auto Strut on that vessel. It preserves each part's selected Auto Strut
mode, blocks rebuilding while a servo remains unlocked, and recreates the Auto
Struts from those modes after the last servo is locked. This prevents descendant
Auto Struts from bypassing a moving robotic joint without changing the saved
craft configuration.

The plug-in also:

- prevents KJR from directly rewriting a robotic part's native attachment joint;
- repairs stale stock servo lock transitions loaded from the editor;
- notifies KJR about servos that enter physics already unlocked;
- subscribes directly to the servo PAW `UI_Control.onFieldChanged` callback and
  recovers a missing stock `ModifyLocked` call without depending on replaced
  PAW/BaseField methods or duplicating working action-group callbacks;
- adds a `Robotics / Auto Strut Manager` event to every servo PAW in flight;
- shows the vessel-wide suspension state, every servo lock state, every part's
  selected Auto Strut mode and physical joint count;
- supports individual and one-click lock/unlock commands from that window;
- does not alter motor engagement, target position, damping, or Auto Strut modes.

The built assembly is `Plugins/RoboticsKJRCompat.dll`.
