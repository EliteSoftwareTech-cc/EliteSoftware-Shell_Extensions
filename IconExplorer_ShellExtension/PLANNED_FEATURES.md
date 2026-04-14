# IconExplorer Roadmap & Planned Features

Bringing 2006 to 2026 one line of code at a time.

## 🚀 Priority: Resource Manipulation
*   **Icon Replacement Engine**: Direct binary replacement of icons within `.dll`, `.exe`, `.mui`, `.mun`, `.cpl`, `.scr`, and `.icl` files.
*   **MUI/MUN Support**: Specialized handling for Windows Multilingual User Interface files to ensure resource integrity after modification.
*   **Resource Container Management**: Ability to add, remove, or swap icon groups regardless of file type, as long as it contains a standard `.rsrc` section.

## 🎨 UI/UX Enhancements
*   **Search & Filter**: Real-time filtering of icons by ID or name (where available).
*   **Drag & Drop**: Drag a file onto the app to open, or drag an icon out of the app to extract.

## 🔗 System Integration
*   **Full Context Menu Integration**: (Current Task) Global right-click "Open in Icon Explorer" for all file system objects.
*   **Shell Icon Overlay**: Visual indicators for files that have been modified by IconExplorer.
*   **Registry Integration**: Unified management of icon associations directly from the UI.

## 🛠️ Infrastructure
*   **Incremental Backups**: Automatic source-code snapshots during significant feature milestones.
*   **Self-Healing Installer**: Intelligent detection and repair of broken COM registrations.
