module RemoteDesktopAssistant.OpenVpnGUI

open RemoteDesktopAssistant
open System.IO

let executable () = Init.read().OpenVpnExecutable

let exec args =
    let exe = executable ()
    WinProcess.start exe args false

let connect cmd = exec cmd

let disConnect cmd = exec cmd

let killProgram () =
    let exe = executable ()
    let imageName = Path.GetFileName exe
    WinProcess.start "taskkill" $"/IM {imageName}" false
