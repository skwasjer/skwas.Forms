# skwas.Forms
Library of Windows Forms controls, components and other useful classes. 

## Requirements

Visual Studio 2015 / C# 6

### skwas.Forms namespace

#### Controls

Name | Description
:---- | :----
[ShieldButton](src/Controls/ShieldButton.cs) | Represents a button that displays a shield to indicate elevated permissions are required. The shield is only displayed when the user does not have Administrator permissions.
[VirtualTreeView](src/Controls/VirtualTreeView.cs) | Displays a hierarchical collection of labeled items, each represented by a [VirtualTreeNode](src/Controls/VirtualTreeNode.cs). The tree nodes are not actually added into the actual tree view until needed (cached internally). This allows a large number of nodes to be added to the tree view very fast, as opposed to the stock TreeView.
[VsToolStrip](src/Controls/VsToolStrip.cs) | Provides a container for Windows toolbar objects, and is drawn using a 3D like style, if visual styles is supported and enabled on the operating system.

#### Components
Name | Related | Description
:---- | :---- | :----
[UndoManager](src/Undo/UndoManager.cs) || Manager for undo/redo operations. Tracks an undo and redo stack of [IUndoAction](src/Undo/IUndoAction.cs) objects.
|| [IUndoAction](src/Undo/IUndoAction.cs) | Interface for creating custom undo/redo actions.
|| [UndoAction](src/Undo/UndoAction.cs) | Abstract base class for creating undo/redo actions.
|| [UndoSetValueAction](src/Undo/UndoSetValueAction.cs) | Undo/redo action for setting a property on an instance.
|| [UndoMethodAction](src/Undo/UndoMethodAction.cs) | Undo/redo action to call custom methods.
[Window](src/Window/Window.cs) || Work in progress. A IWin32Window implementation that supports both managed and native windows, and provides access to alot of information about the window, the process it belongs to, etc. Provides access to several native API's via a similar interface. Very useful for control authors.
[SingleApplicationInstance](src/SingleApplicationInstance.cs) || A component to ensure an application has always at most one instance running. Provides a way to send startup information (command line arguments or otherwise) to the already running instance.

#### Utilities

Name | Description
:---- | :----
[CommandLine](src/CommandLine.cs) |  Command line arguments utility to parse a command line string, or to pack an array of arguments back into a single string.
