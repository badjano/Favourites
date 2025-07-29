# Unity Favourites Panel

A Unity editor extension that adds a Favourites panel with a Tree View where you can add categories and drag-and-drop objects from the Hierarchy or Project panel.

## Features

- **Tree View Interface**: Organize your favourite assets in a hierarchical tree structure
- **Drag & Drop Support**: Easily drag objects from Hierarchy or Project panel into categories
- **Category Management**: Add and remove categories with the [+] and [-] buttons
- **Quick Access**: Double-click items to open them, right-click to ping in Project/Hierarchy
- **Cross-Panel Integration**: Drag items from Favourites to other Unity panels (Inspector, etc.)

## Installation

### Via Git URL (Recommended)
1. Open Unity Package Manager (Window > Package Manager)
2. Click the '+' button and select "Add package from git URL"
3. Enter: `https://github.com/badjano/unity-favourites.git`

### Via Local Package
1. Download or clone this repository
2. In Unity Package Manager, click the '+' button and select "Add package from disk"
3. Navigate to the package folder and select the `package.json` file

## Usage

### Opening the Panel
- Go to `Window > Favourites` in the Unity menu bar
- The Favourites panel will open as a dockable window

### Adding Categories
- Click the [+] button in the Favourites panel toolbar
- Enter a name for your new category
- Press Enter or click OK

### Adding Items
- Drag and drop objects from the Hierarchy or Project panel into any category
- Items will be automatically organized under their respective categories

### Managing Items
- **Remove Item**: Select an item and click the [-] button
- **Remove Category**: Select a category and click the [-] button (removes category and all its items)
- **Open Item**: Double-click on any item to open it
- **Ping Item**: Right-click on an item to ping it in the Project or Hierarchy panel
- **Drag to Inspector**: Drag items from Favourites to component properties in the Inspector

### Organization
- Create multiple categories to organize different types of assets
- Drag items between categories to reorganize
- Use descriptive category names for better organization

## Requirements

- Unity 2021.3 or later
- No additional dependencies required

## License

This project is released into the public domain under the [Unlicense](UNLICENSE).

## Contributing

Contributions are welcome! Feel free to submit issues, feature requests, or pull requests.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.
