# Rufus-Style Disk Writing Integration: Feature-Complete Specification

This document serves as the absolute blueprint for the **"Burn & Write"** property sheet panel. Every feature listed here must be implemented to achieve "feature completeness."

## 🚀 Full Feature Specification

### 1. Drive Properties (Target Device)
- **Device Enumeration**:
    - Automatic detection of USB Flash Drives, External HDDs (if enabled), and Optical Media (DVD/CD).
    - Display format: `[Drive Letter] [Volume Label] [Capacity]`.
    - Real-time refresh (detection of drive insertion/removal).
- **Advanced Drive Properties (Toggleable)**:
    - **List USB Hard Drives**: Option to include non-removable USB drives in the device list.
    - **BIOS Fixes**: Apply extra partition alignment and legacy BIOS compatibility fixes.
    - **UEFI Media Validation**: Enable runtime verification for UEFI media.
- **Boot Selection**:
    - **Source Detection**: Automatically selects the ISO/IMG file being right-clicked.
    - **Manual Selection**: "SELECT" button to browse for a different image.
    - **Verification**: Checkmark icon to verify image integrity (MD5/SHA1/SHA256).
- **Partition & Target System**:
    - **Partition Scheme**: Selection between **MBR** and **GPT**.
    - **Target System**: Context-aware selection (e.g., GPT requires UEFI; MBR supports BIOS or UEFI).

### 2. Format Options (Filesystem Configuration)
- **Volume Label**: Input field (max 32 chars for NTFS, 11 for FAT).
- **Filesystem Support**: 
    - **FAT32**: Default for small drives/UEFI compatibility.
    - **NTFS**: Support for large files (>4GB).
    - **exFAT**: Modern cross-platform support.
    - **UDF**: Specific for optical media.
- **Cluster Size**: Full range of allocation unit sizes (Default, 512, 1024, 2048, 4096, 8192, 16K, 32K, 64K).
- **Advanced Format Options (Toggleable)**:
    - **Quick Format**: Fast track formatting (default).
    - **Extended Label/Icon**: Create `autorun.inf` and high-res icon files on the target drive.
    - **Bad Block Check**: Surface scan with user-selectable passes (1, 2, 3, or 4).

### 3. Windows Installation Customization (The "Experience" Dialog)
If a Windows ISO is detected, a modal dialog must appear after clicking "START":
- **Username Creation**: "Create a local account with username: [input]".
- **Regional Settings**: "Set regional options to the same values as this user's".
- **Privacy Bypass**: "Disable data collection (Skip privacy questions)".
- **BitLocker Bypass**: "Disable BitLocker automatic device encryption".

### 4. Status, Progress & Logging
- **Visual Feedback**:
    - Centered "READY" status box with teal/gray transitions.
    - Smooth progress bar (Win32 Common Controls).
- **Interaction Buttons**:
    - **START**: Triggers the dangerous operation (with multi-stage confirmation).
    - **CLOSE**: Safe exit.
- **The "Footer" Tools**:
    - **Log Window**: Dedicated modal showing every command and return code.
    - **Info**: Technical details about the selected image.
    - **Language**: Global localization selector.

## 🛠️ Internal Engine Requirements

### Disk Writing Logic
- **USB Writing**: Use `DeviceIoControl` (IOCTL_DISK_GET_DRIVE_GEOMETRY, FSCTL_LOCK_VOLUME, FSCTL_DISMOUNT_VOLUME) and raw `WriteFile` at sector 0.
- **Optical Burning**: Full IMAPI2 integration for CD/DVD-R/RW support.
- **Partitioning**: Programmatic execution of `diskpart` or direct MBR/GPT table writing.

### Security & Safety
- **Locking**: Must lock the target volume to prevent data corruption.
- **UAC**: Writing to physical drives requires full Administrator elevation.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
