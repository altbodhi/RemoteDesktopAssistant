namespace RemoteDesktopAssistant

open System.IO

module RemoteCore =
    type RemoteHost =
        { name: string
          host: string
          port: uint16 }

        member x.fmt = if x.port = 0us then x.host else $"{x.host}:{x.port}"


module Hosts =
    type HostMap = { ip: string; host: string }

    [<Literal>]
    let winHosts = @"C:\Windows\System32\drivers\etc\hosts"


    let readHostEntries () =
        File.ReadAllLines winHosts
        |> Array.filter (fun x -> x.StartsWith("#") |> not)
        |> Array.map (fun x ->
            match x.Split(" ") |> Array.toList with
            | a :: b :: _ -> Some { ip = a; host = b }
            | _ -> None)
        |> Array.choose id

module WinProcess =
    open System.Diagnostics
    open RemoteCore

    let start executable arguments runas =
        let si = ProcessStartInfo()
        si.Arguments <- arguments
        si.FileName <- executable

        if runas then
            si.Verb <- "runas"
            si.UseShellExecute <- true

        let ps = Process.Start si
        ps

    let startConnect (host: RemoteHost) =
        start "mstsc.exe" $"-v:{host.fmt}" false

    let openText path asAdmin =
        start "notepad.exe" $"\"{path}\"" asAdmin



module Utils =
    open Hosts
    open RemoteCore

    let viewHosts () =
        WinProcess.openText winHosts false |> ignore

    let changeHosts () =
        WinProcess.openText winHosts true |> ignore

    let listCmdKey host =
        WinProcess.start "cmd.exe" $"/K cmdkey /list:{host}" true

    let deleteCmdKey host =
        WinProcess.start "cmd.exe" $"/K cmdkey /delete:{host}" true

    let addCmdKey host user pass =
        WinProcess.start "cmd.exe" $"/K cmdkey /generic:{host} /user:{user} /pass:{pass}" true

    let find_in_winHosts (item: RemoteHost) (items: seq<HostMap>) =
        query {
            for e in items do
                where (e.host = item.host)
                select e
        }
        |> Seq.toList
