# Image Management Features: Exhaustive Specification

This document defines the "Feature Complete" state for the Image Management property sheet panel. It is a 1:1 mapping of every control and logic branch seen in the Rufus 4.11 screenshots.

## 🚀 Drive Properties
- [x] **Device Selector**: 
    - [x] Dropdown listing all removable/CD drives.
    - [x] **Save Icon**: Button to save the current drive list or selection.
- [x] **Boot Selection**:
    - [x] Dropdown for bootable image selection (Disk or ISO image).
    - [x] **Checkmark Icon**: Status indicator for image verification.
    - [x] **SELECT Button**: Full file picker for source image.
- [x] **Partition Scheme**: Dropdown containing `MBR` and `GPT`.
- [x] **Target System**: Dropdown containing `BIOS (or UEFI-CSM)` and `UEFI (non CSM)`.
- [x] **Advanced Drive Properties (Expandable/Toggleable)**:
    - [x] **List USB Hard Drives**: Checkbox to show/hide fixed USB disks.
    - [x] **Add fixes for old BIOSes**: Checkbox for legacy partition alignment/fixes.
    - [x] **Enable runtime UEFI media validation**: Checkbox for post-write validation.

## 🛠️ Format Options
- [x] **Volume Label**: Text input field.
- [x] **File System**: Dropdown containing `FAT32`, `NTFS`, `exFAT`, and `UDF`.
- [x] **Cluster Size**: Dropdown for allocation unit size selection.
- [x] **Advanced Format Options (Expandable/Toggleable)**:
    - [x] **Quick Format**: Checkbox (Default ON).
    - [x] **Create extended label and icon files**: Checkbox (Default ON).
    - [x] **Check device for bad blocks**: Checkbox + **Pass Selection Dropdown** (1, 2, 3, or 4 passes).

## 📊 Status & Interaction
- [x] **Status Box**: Large centered status label with "READY" state.
- [x] **Progress Bar**: Win32 standard progress indicator.
- [x] **Action Buttons**:
    - [x] **START**: Triggers write/format logic.
    - [x] **CLOSE**: Exits the tab.
- [x] **Footer Navigation (Icons)**:
    - [x] **World/Update**: Check for updates/online help.
    - [x] **Info (i)**: Show hardware/image info.
    - [x] **Settings (Sliders)**: Global app settings.
    - [x] **Log (Document)**: Opens the real-time logging window.
- [x] **Footer Status**: "1 device found" (or appropriate count) status text at the very bottom.

## 🪟 Windows User Experience (Image-Specific)
- [x] **Local Account**: Checkbox + Username text box.
- [x] **Regional Sync**: Checkbox to sync host regional settings.
- [x] **Privacy Bypass**: Checkbox to skip OOBE privacy questions.
- [x] **BitLocker Bypass**: Checkbox to disable automatic encryption.

## 📜 Logging System
- [x] Standalone modal window triggered by the Log Icon.
- [x] Full text area for low-level operation logs.
- [x] **Buttons**: `Clear`, `Save`, `Close`.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
