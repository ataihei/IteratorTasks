if (-not (Test-Path ../../packages)) { mkdir ../../packages }
cp ..\IteratorTasksGenerator\IteratorTasksGenerator\IteratorTasksGenerator\bin\Debug\*.nupkg ../../packages
cp ..\IteratorTasks.Nuget\bin\Debug\*.nupkg ../../packages
