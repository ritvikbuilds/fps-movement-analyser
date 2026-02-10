# Application Icon

To add a custom icon to NoteD.exe:

1. Create or obtain a `.ico` file (256x256 recommended, with multiple sizes: 16, 32, 48, 256)
2. Save it as `app.ico` in this directory (`src/NoteD.Cli/app.ico`)
3. Rebuild the project

## Quick icon creation

You can create a simple icon using online tools:
- https://www.favicon.io/ (convert PNG to ICO)
- https://realfavicongenerator.net/
- https://iconverticons.com/online/

## Suggested design

For NoteD, consider:
- A crosshair or target symbol (FPS theme)
- A keyboard key with timing marks
- A stopwatch or timer icon
- Colors: Orange (#FFA500) and Blue (#0066FF) to match the A/D key colors

Until you add an icon, the build will use the default Windows executable icon.


