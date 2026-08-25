# Robotics / KJR Compatibility

## Current implementation

- Plug-in: `Plugins/RoboticsKJRCompat.dll`
- Source: `Source/RoboticsKJRCompat/`
- Current development version: 1.4.3 (uncommitted as of 2026-08-23)
- Target: KSP 1.12.x Breaking Ground `BaseServo` parts with Kerbal Joint Reinforcement Continued.
- No timer or per-frame polling is accepted for lock-state synchronization.

When any Servo on a vessel is unlocked, the plug-in suspends every physical
stock Auto Strut on that vessel while preserving each part's selected
`autoStrutMode`. Auto Struts are restored from those modes after the final Servo
is locked. KJR direct rewriting of a robotic part's native attachment joint is
also bypassed.

Every Servo part receives a `Robotics / Auto Strut Manager` PAW event. The
window shows vessel suspension state, Servo lock states, selected Auto Strut
modes and physical joint counts, and provides individual plus Lock All/Unlock
All controls.

## Confirmed working paths

- The custom manager window changes Servo lock state and vessel Auto Strut
  ACTIVE/SUSPENDED state together.
- Stock action-group Servo lock/unlock also synchronizes Auto Struts correctly.
- These two paths are the accepted workaround for the deferred PAW issue.

## Deferred issue: stock PAW Locked toggle

Status: intentionally deferred on 2026-08-23 because the plug-in remains usable
through the manager window and action groups.

Observed controlled test:

- Start with manager `Lock All`: Servo LOCKED, vessel ACTIVE.
- Change the stock part right-click `Locked` field to No: manager reads Servo
  UNLOCKED, but vessel remains ACTIVE and physical Heaviest Auto Strut joints
  remain present.
- Change the same field back to Yes: manager reads Servo LOCKED and vessel
  remains ACTIVE.

This proves the stock PAW changes `servoIsLocked` but does not execute the lock
processing route observed by this plug-in in the current mod environment.
Action groups work because `EngageServoLock` / `DisengageServoLock` explicitly
call `ModifyLocked`.

Attempts that did not solve the stock PAW synchronization:

1. v1.4.1 patched `BaseField<KSPField>.SetValue`. KSPCommunityFixes
   `BaseFieldListUseFieldHost` applies an `Override` to that method after this
   plug-in, so the fallback did not run.
2. v1.4.2 patched stock `UIPartActionFieldItem.SetFieldValue`; no synchronization
   occurred.
3. v1.4.3 subscribed the manager module directly to
   `UI_Control.onFieldChanged`; the controlled PAW test still remained
   ACTIVE/ACTIVE.

Do not resume this investigation unless Dr Armor explicitly brings the issue
back. If resumed, add direct diagnostic state to the manager window before
trying another fix: callback registration status, callback invocation count,
`servoIsLocked`, `prevServoIsLocked`, and `IsJointUnlocked()`.

## Process safety

Never close or terminate KSP without Dr Armor's explicit approval. Starting KSP
is allowed only when it is not already running.
