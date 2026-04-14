# MetadataEditor Development Roadmap

## Phase 1: Core Engine and ADS Support (Completed)
- [x] Implement NTFS Alternative Data Stream (ADS) enumeration.
- [x] Implement ADS stream manipulation (Read/Write/Delete).
- [x] Add "Custom ADS Field" injection feature.
- [x] Integrate with `IPropertyStore` for native metadata editing.

## Phase 2: Shell Integration and Styling (Completed)
- [x] Create SharpShell Property Sheet Extension ("Metadata" tab).
- [x] Create Global Context Menu ("Open in Metadata Editor").
- [x] Implement Asynchronous "Fill-In" loading for metadata.
- [x] Unified Teal structural theme for PropertyGrid.
- [x] Finalize unique GUIDs for registry-stable shell integration.

## Phase 3: Optimization and Performance (Completed)
- [x] Implement persistent schema cache at `%AppData%`.
- [x] Deep scan (800+ fields) for hidden property discovery.
- [x] Optimize thread safety for static property loading.
- [x] Handle system-wide Explorer restarts during installation.

## Phase 4: Future Features (Planned)
- [ ] **Search and Filter**: Real-time filtering in the PropertyGrid to find specific fields.
- [ ] **Batch Metadata Editing**: Select multiple files and apply a single metadata change to all of them.
- [ ] **Export to JSON/CSV**: Export the metadata and ADS structure of a file for external reporting.
- [ ] **System-Wide Property Aliases**: Allow users to rename canonical property names to something more intuitive.
- [ ] **Full Property System Explorer**: A standalone view of all 1500+ potential properties in the Windows Property Store.

---
© 2026 EliteSoftwareTech Co.  
**Author:** Zachary Whiteman
