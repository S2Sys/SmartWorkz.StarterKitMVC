# Phase 4 Refinements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete Biometric Service test coverage, implement Bluetooth Connect/Disconnect with device pairing and connection state tracking, and implement NFC read operations for Android/iOS.

**Architecture:** Biometric tests validate existing production-ready implementations across platforms. Bluetooth refinements replace stubs with actual device connection management using platform-native APIs (BluetoothSocket on Android, CBPeripheral on iOS) plus connection state tracking via System.Reactive. NFC refinements implement read operations using intent interception (Android) and NFCNDEFReaderSession (iOS). All follow existing Phase 3/4 patterns: partial classes with #if conditional compilation, permission gating via IPermissionService, Result<T> error handling, and comprehensive XML documentation.

**Tech Stack:** .NET 9 MAUI, xUnit, Moq, SmartWorkz.Shared (Result<T>, Guard), System.Reactive, platform APIs (Android BluetoothAdapter/BluetoothSocket/NfcManager, iOS CBCentralManager/CBPeripheral/NFCNDEFReaderSession).

---

## BIOMETRIC SERVICE TESTS (Tasks B1-B2)

### Task B1: BiometricService.AuthenticateAsync Tests

**Files:**
- Create: `tests/SmartWorkz.Core.Mobile.Tests/Services/BiometricServiceTests.cs`

**Task content:** Write 4 unit tests covering BiometricService.AuthenticateAsync success/failure paths, availability checks, and biometric type detection. Tests validate existing production implementation across Android/iOS platforms using mocked IPermissionService.

---

### Task B2: BiometricService Platform-Specific Tests

**Files:**
- Modify: `tests/SmartWorkz.Core.Mobile.Tests/Services/BiometricServiceTests.cs`

**Task content:** Add 2 platform-specific tests validating Face/Fingerprint detection on Android and FaceID/TouchID detection on iOS. Verifies platform APIs return correct BiometricType enum values.

---

## PHASE 4.5: BLUETOOTH REFINEMENTS (Tasks BT1-BT6)

### Task BT1: BluetoothConnectionState Model

**Files:**
- Create: `src/SmartWorkz.Core.Mobile/Models/BluetoothConnectionState.cs`

**Task content:** Create BluetoothConnectionState record with device address, connection status, timestamp, RSSI signal strength tracking, and ServiceUuids. Include SignalStrength enum (Excellent/Good/Fair/Weak/Poor) and ConnectionDuration property computing elapsed time since connection.

---

### Task BT2: Update IBluetoothService for Connection State Tracking

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Services/IBluetoothService.cs`

**Task content:** Add three new members to IBluetoothService: GetConnectionStateAsync() returning nullable BluetoothConnectionState, OnConnectionStateChanged() observable stream, ConnectedDeviceAddress property. These enable connection state tracking across Android/iOS platforms.

---

### Task BT3: Implement BluetoothService Connect/Disconnect with Android Platform

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Services/Implementations/BluetoothService.cs`
- Modify: `src/SmartWorkz.Core.Mobile/Platforms/Android/BluetoothService.Android.cs`

**Task content:** Update BluetoothService main class to track connection states in dictionary and emit via Subject<BluetoothConnectionState>. Implement Android platform ConnectAsyncPlatform using BluetoothSocket and UUID for standard SPP profile. Track RSSI and emit connection state changes.

---

### Task BT4: Implement BluetoothService Connect/Disconnect with iOS Platform

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Platforms/iOS/BluetoothService.iOS.cs`

**Task content:** Implement iOS platform ConnectAsyncPlatform and DisconnectAsyncPlatform using CBCentralManager and CBPeripheral. Store connected peripheral reference, implement 30-second connection timeout, check CBPeripheralState.Connected status.

---

### Task BT5: Device Pairing Workflow

**Files:**
- Create: `src/SmartWorkz.Core.Mobile/Services/IBluetoothPairingService.cs`
- Create: `src/SmartWorkz.Core.Mobile/Services/Implementations/BluetoothPairingService.cs`

**Task content:** Create IBluetoothPairingService interface with PairAsync, UnpairAsync, GetPairedDevicesAsync, OnPairingStateChanged methods. Implement BluetoothPairingService checking permissions, managing pairing state via Subject<>, with platform-specific stubs for Android/iOS implementation. Error handling with Result<T>.

---

### Task BT6: Bluetooth Tests for Connection State and Pairing

**Files:**
- Create: `tests/SmartWorkz.Core.Mobile.Tests/Services/BluetoothConnectionTests.cs`
- Create: `tests/SmartWorkz.Core.Mobile.Tests/Services/BluetoothPairingTests.cs`

**Task content:** Write 3 connection state tests (successful connect tracking, state retrieval, observable stream) and 3 pairing tests (permission denied failure, paired devices list, observable stream). Mock IPermissionService, verify Result<T> status codes.

---

## PHASE 4.6: NFC REFINEMENTS (Tasks NFC1-NFC3)

### Task NFC1: Implement NFC Read Operation (Android)

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Platforms/Android/NfcService.Android.cs`

**Task content:** Implement Android ReadAsyncPlatform using NfcManager/NfcAdapter. Check NFC availability/enabled status. Get Tag from Activity intent, open Ndef connection, extract NDEF message payload, return NfcMessage with type/payload/timestamp/URI. Handle missing tag, not NDEF, disabled NFC errors.

---

### Task NFC2: Implement NFC Read Operation (iOS)

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Platforms/iOS/NfcService.iOS.cs`

**Task content:** Implement iOS ReadAsyncPlatform using NFCNDEFReaderSession delegate pattern. Check ReadingAvailable, create session, implement DidDetect callback storing detected NDEF message, implement DidInvalidate callback for errors. Handle 30-second timeout, return NfcMessage with extracted payload and URI if parseable.

---

### Task NFC3: NFC Read Operation Tests

**Files:**
- Create: `tests/SmartWorkz.Core.Mobile.Tests/Services/NfcReadTests.cs`

**Task content:** Write 3 NFC read tests: NFC unavailable returns failure, permission denied returns failure, availability check returns boolean. Mock IPermissionService, verify Result<T> error codes and messages.

---

## FINAL VERIFICATION (Task FINAL)

### Task FINAL: Register Refinements + Run Full Test Suite

**Files:**
- Modify: `src/SmartWorkz.Core.Mobile/Extensions/ServiceCollectionExtensions.cs`

**Task content:** Register IBluetoothPairingService in ServiceCollectionExtensions. Build all target frameworks (net9.0-ios/android/maccatalyst/windows). Run full test suite verifying 100+ tests pass across all phases. Final commit with complete implementation summary.

---
