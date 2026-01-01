module RemoteDesktopAssistant.Init

open System.IO
open System.Text.Json

[<Literal>]
let config = "config.json"

type InitValues =
    { OpenVpnExecutable: string
      VpnConnect: string
      VpnDisconnect: string }

let def =
    { OpenVpnExecutable = @"C:\Program Files\OpenVPN\bin\openvpn-gui.exe"
      VpnConnect = "--command connect VpnName --silent connection 1"
      VpnDisconnect = "--command exit" }

let read () : InitValues =
    if Path.Exists config then
        try
            match File.ReadAllText config |> JsonSerializer.Deserialize<InitValues> with
            | Null -> def
            | NonNull ini -> ini
        with e ->
            System.Diagnostics.Trace.WriteLine e
            def
    else
        def

let write (init: InitValues) =
    let opt = JsonSerializerOptions(JsonSerializerOptions.Default, WriteIndented = true)

    JsonSerializer.Serialize(init, opt)
    |> fun json -> File.WriteAllText(config, json)
