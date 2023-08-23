
set /p original=Drag and Drop ORIGINAL .fbx file and press ENTER: 
set /p ft=Drag and Drop FT .fbx and press ENTER: 
set /p output=Name of output fbx and press ENTER:


hdiffz.exe %original% %ft% DiffFiles\%output%