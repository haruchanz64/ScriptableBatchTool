# ScriptableBatchTool

A Unity Editor Window tool designed to streamline the creation and management of multiple ScriptableObject instances.  
Instead of duplicating assets manually, this tool lets you batch create, edit, and export ScriptableObjects from a selected ScriptableObject-derived script.

## Features

- Select any concrete ScriptableObject script and batch create instances.
- Edit multiple ScriptableObjects in one window with a clean UI.
- Prevent duplicate asset names by auto-generating unique, sanitized file names.
- Choose an output folder inside your Unity project's `Assets` directory.
- Scrollable list for easy management of large numbers of entries.
- Safe removal of entries and automatic clearing when changing the script type.

## Installation

There are two ways to install ScriptableBatchTool:

### 1. Using Unity Package Manager (Git URL)

- In Unity, open `Window > Package Manager`.
- Click `+ > Add package from git URL...`.
- Enter `https://github.com/haruchanz64/ScriptableBatchTool.git` and click **Add**.
- Open the tool from `Tools > ScriptableObject Batch Creator`.

### 2. Using the `.zip` file from the repository

- Download the repository as a `.zip` file (`Code > Download ZIP`) and extract it.
- Copy the extracted folder into your project's `Assets` folder (e.g., `Assets/ScriptableBatchTool`). Its `Editor/` assembly definition ensures it is only compiled in the Unity Editor and excluded from player builds.
- Open Unity and wait for it to compile the scripts.
- Open the tool from `Tools > ScriptableObject Batch Creator`.

## Usage

1. Select the C# script that extends **ScriptableObject**.
2. Choose the output folder where assets will be saved (must be inside the `Assets` folder).
3. Add new entries, customize their fields and asset names.
4. When ready, hit **Export All** to save all ScriptableObject assets to the selected folder.

## Requirements

- Unity 2021.1 or higher

## Notes

- The output folder must be inside the `Assets` folder to ensure assets are correctly registered by Unity.
- Asset names are automatically sanitized and made unique to avoid overwriting existing assets.
- Changing the selected script while editing will clear current entries to prevent type mismatches.

## License

[MIT](https://choosealicense.com/licenses/mit/) License. Feel free to use and modify this tool in your projects.

---
Created by [Haru](https://www.github.com/haruchanz64)
