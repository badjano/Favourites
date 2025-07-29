# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-01-27

### Changed
- **BREAKING CHANGE**: Replaced ScriptableObject storage with persistent data path storage
- Each user now has their own favourites saved in `Application.persistentDataPath`
- Favourites are automatically saved and persist between Unity sessions
- Removed scene-based storage system in favor of centralized user-specific storage
- Updated menu location from `Window > Favourites` to `Tools > Favourites`

### Added
- `FavouritesManager` singleton for centralized data management
- `FavouritesData` class for JSON serialization
- Migration utility to transfer existing ScriptableObject data
- Data management tools (view location, clear all)
- Improved object reference system using GUIDs and paths
- Better error handling for missing or corrupted data

### Removed
- Scene-based `FavouritesContainer` system (kept for backward compatibility with deprecation warnings)
- ScriptableObject asset creation and management
- Scene-specific object tracking

### Migration
- Use `Tools > Favourites > Migrate from ScriptableObject` to transfer existing data
- Old ScriptableObject assets can be safely deleted after migration

## [1.0.0] - 2024-07-28

### Added
- Initial release of Unity Favourites Panel
- Tree View interface for organizing favourite assets
- Drag and drop support from Hierarchy and Project panels
- Category management system with add/remove functionality
- Double-click to open items
- Right-click to ping items in Project/Hierarchy
- Cross-panel drag and drop support
- Editor-only scene object tracking
- Persistent asset storage

### Features
- Favourites panel accessible via Tools > Favourites menu
- Hierarchical organization of assets in categories
- Seamless integration with Unity's existing panels
- Automatic cleanup of scene objects when scenes are closed 