
set /p original=Drag and Drop ORIGINAL meta file and press ENTER: 
set /p ft=Drag and Drop FT version of the meta file and press ENTER: 
set /p output=Name of output fbx and press ENTER:


hdiffz.exe %original% %ft% DiffFiles\%output%