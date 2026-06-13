#!/bin/bash
echo "Packaging Velo Terminal into AppImage..."

# 1. Create AppDir structure
mkdir -p VeloTerminal.AppDir/usr/bin
cp ./publish/linux-x64/VeloTerminal VeloTerminal.AppDir/usr/bin/

# 2. Create Desktop Entry
cat <<EOF > VeloTerminal.AppDir/VeloTerminal.desktop
[Desktop Entry]
Name=Velo Terminal
Exec=VeloTerminal
Icon=terminal
Type=Application
Categories=Finance;Utility;
EOF

# 3. Create dummy icon to satisfy AppImage requirements
touch VeloTerminal.AppDir/terminal.png

# 4. Fetch linuxdeploy and build
wget -c -nv "https://github.com/linuxdeploy/linuxdeploy/releases/download/continuous/linuxdeploy-x86_64.AppImage"
chmod +x linuxdeploy-x86_64.AppImage
APPIMAGE_EXTRACT_AND_RUN=1 ./linuxdeploy-x86_64.AppImage --appdir VeloTerminal.AppDir --output appimage
