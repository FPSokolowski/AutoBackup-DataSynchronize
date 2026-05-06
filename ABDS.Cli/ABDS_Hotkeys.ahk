#Requires AutoHotkey v2.0

; Dopasuj ścieżkę:
abdsCli := "C:\Tools\ABDS\ABDS.Cli.exe"

^!s:: { ; Ctrl+Alt+S
    Run('"' abdsCli '" sync --all --open-gui', , "Hide")
}

^!b:: { ; Ctrl+Alt+B
    Run('"' abdsCli '" backup --all --open-gui', , "Hide")
}