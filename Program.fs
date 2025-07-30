﻿module RemoteDesktopAssistant.Program

open System.Windows.Forms
open System.Data
open System.IO
open RemoteCore

let dataRowToRemoteHost (row: DataRow) =
    let port =
        match System.UInt16.TryParse(row.Item("Port").ToString()) with
        | (true, v) -> v
        | _ -> 3389us

    let name =
        match row.Item("Name").ToString() with
        | null -> ""
        | s -> s

    let _ = null |> Option.ofObj

    let host =
        match row.Item("Host").ToString() with
        | null -> ""
        | s -> s


    { name = name
      host = host
      port = port }

let db = new DataTable("rdp")
db.Columns.AddRange([| new DataColumn("Name"); new DataColumn("Host"); new DataColumn("Port") |])

if File.Exists("db.xml") then
    db.ReadXml("db.xml") |> ignore
else
    db.Rows.Add([| "Домашний компьютер" :> obj; "home-pc" :> obj; 3389us :> obj |])
    |> ignore

let servers = new ToolStripMenuItem("&Server")
let addServers = servers.DropDownItems.Add("&Add")

let connectServer = servers.DropDownItems.Add("&Connect")
let editServers = servers.DropDownItems.Add("&Edit")
let deleteServers = servers.DropDownItems.Add("&Delete")

let hosts = new ToolStripMenuItem("&Hosts")
let openHosts = hosts.DropDownItems.Add("&Open")
let changeHosts = hosts.DropDownItems.Add("&Change")
let cmdKeys = new ToolStripMenuItem("&CmdKey")
let listCmdKey = cmdKeys.DropDownItems.Add("&List")
let purgeCmdKey = cmdKeys.DropDownItems.Add("&Purge")
let addCmdKey = cmdKeys.DropDownItems.Add("&Add")

openHosts.Click.Add(fun _ -> Utils.viewHosts ())
changeHosts.Click.Add(fun _ -> Utils.changeHosts ())

type MainMenu() as x =
    class
        inherit MenuStrip()

        do
            x.Dock <- DockStyle.Top
            x.LayoutStyle <- ToolStripLayoutStyle.Flow
            x.Items.Add(servers) |> ignore
            x.Items.Add(hosts) |> ignore
            x.Items.Add(cmdKeys) |> ignore
    end

type ConnectionsGrid() as x =
    class
        inherit DataGridView()

        do
            x.DataSource <- db
            x.AllowUserToAddRows <- false
            x.AllowUserToDeleteRows <- false
            let hosts = Hosts.readHostEntries ()

            x.CellFormatting.Add(fun e ->
                if e.RowIndex > x.Rows.Count then
                    ()
                else
                    match x.Rows.Item(e.RowIndex).DataBoundItem with
                    | :? DataRowView as item ->
                        let host = item.Row |> dataRowToRemoteHost
                        let is_new = hosts |> Array.exists (fun h -> h.host = host.host) |> not

                        if is_new then
                            e.CellStyle.BackColor <- System.Drawing.Color.LightYellow
                    | _ -> ())
    end

let panel = new FlowLayoutPanel()
let grid = new ConnectionsGrid()

type AccountEdit(host) as x =
    class
        inherit Form()
        let accept = new Button(DialogResult = DialogResult.OK, Text = "OK")
        let cancel = new Button(DialogResult = DialogResult.Cancel, Text = "Cancel")
        let host = new TextBox(Text = host)
        let user = new TextBox(Text = System.Environment.UserName)
        let pass = new TextBox(PasswordChar = '*')

        do
            x.StartPosition <- FormStartPosition.CenterParent
            x.FormBorderStyle <- FormBorderStyle.FixedDialog
            x.AcceptButton <- accept
            x.CancelButton <- cancel
            let flow = new FlowLayoutPanel()
            flow.Dock <- DockStyle.Fill

            flow.Controls.Add(new Label(Text = "Host:"))
            flow.Controls.Add(host)

            flow.Controls.Add(new Label(Text = "User:"))
            flow.Controls.Add(user)

            flow.Controls.Add(new Label(Text = "Pass:"))
            flow.Controls.Add(pass)

            flow.Controls.Add(accept)
            flow.Controls.Add(cancel)

            x.Controls.Add(flow)

        member x.Edit() = x.ShowDialog() = DialogResult.OK
        member x.User = user.Text
        member x.Pass = pass.Text
    end

type ConnectionEdit() as x =
    class
        inherit Form()
        let accept = new Button(DialogResult = DialogResult.OK, Text = "OK")
        let cancel = new Button(DialogResult = DialogResult.Cancel, Text = "Cancel")
        let name = new TextBox(Width = 200)
        let host = new TextBox(Width = 200)
        let port = new TextBox()

        do
            x.StartPosition <- FormStartPosition.CenterParent
            x.FormBorderStyle <- FormBorderStyle.FixedDialog
            x.AcceptButton <- accept
            x.CancelButton <- cancel
            let flow = new FlowLayoutPanel()
            flow.Dock <- DockStyle.Fill


            flow.Controls.Add(new Label(Text = "Name:"))
            flow.Controls.Add(name)
            flow.Controls.Add(new Label(Text = "Host:"))
            flow.Controls.Add(host)
            flow.Controls.Add(new Label(Text = "Port:"))
            flow.Controls.Add(port)
            let panel = new FlowLayoutPanel()
            panel.WrapContents <- false
            panel.FlowDirection <- FlowDirection.RightToLeft
            panel.Dock <- DockStyle.Fill
            panel.BackColor <- System.Drawing.Color.Red
            panel.Controls.Add(accept)
            panel.Controls.Add(cancel)
            flow.Controls.Add(panel)
            cancel.Anchor <- AnchorStyles.Right
            accept.Anchor <- AnchorStyles.Right

            x.Controls.Add(flow)
            panel.AutoSize <- true
            panel.AutoSizeMode <- AutoSizeMode.GrowAndShrink

        member x.Name
            with get () = name.Text.Trim()
            and set v = name.Text <- v

        member x.Host
            with get () = host.Text.Trim()
            and set v = host.Text <- v

        member x.Port
            with get () =
                (match System.UInt16.TryParse(port.Text) with
                 | (true, v) -> v
                 | _ -> 0us)
            and set (value: uint16) = port.Text <- $"{value}"

        member x.Edit() = x.ShowDialog() = DialogResult.OK

    end

let current () =
    match grid.CurrentRow with
    | Null -> None
    | NonNull row ->
        match row.DataBoundItem with
        | Null -> None
        | NonNull dbi -> Some (dbi :?> DataRowView).Row

let show (text: string) =
    MessageBox.Show(text, "Rdp Assistant") |> ignore

let question text =
    MessageBox.Show(text, "Rdp Assistant", buttons = MessageBoxButtons.YesNo) = DialogResult.Yes

deleteServers.Click.Add(fun _ ->
    match current () with
    | None -> ()
    | Some r ->
        let h = dataRowToRemoteHost r

        if question $"Удалить {h.name} ({h.host})?" then
            db.Rows.Remove r
            grid.Refresh())

connectServer.Click.Add(fun _ ->
    match current () with
    | None -> ()
    | Some v -> v |> dataRowToRemoteHost |> WinProcess.startConnect |> ignore)

listCmdKey.Click.Add(fun _ ->
    match current () with
    | None -> ()
    | Some v -> v |> dataRowToRemoteHost |> (fun x -> x.host) |> Utils.listCmdKey |> ignore)

purgeCmdKey.Click.Add(fun _ ->
    match current () with
    | None -> ()
    | Some v ->
        v
        |> dataRowToRemoteHost
        |> (fun x ->
            if question $"Удалить учетные данные хоста {x.name}?" then
                (Utils.deleteCmdKey x.host) |> ignore))

grid.Dock <- DockStyle.Fill
panel.Controls.Add grid
panel.BackColor <- System.Drawing.Color.Red
panel.Dock <- DockStyle.Fill
let menu = new MainMenu()

type RdpConnections() as x =
    class
        inherit Form()

        do
            x.Text <- "RDP Assistant"
            x.StartPosition <- FormStartPosition.CenterScreen
            x.Height <- 600
            x.Width <- 800
            x.Controls.Add grid
            x.Controls.Add menu
            x.MainMenuStrip <- menu
    end

let mainForm = new RdpConnections()

addCmdKey.Click.Add(fun _ ->
    match current () with
    | None -> ()
    | Some v ->
        v
        |> dataRowToRemoteHost
        |> (fun x ->
            use acc = new AccountEdit(x.host)

            if acc.Edit() then
                (Utils.addCmdKey x.host acc.User acc.Pass) |> ignore))

addServers.Click.Add(fun e ->
    use connectionEdit = new ConnectionEdit()

    if connectionEdit.Edit() then
        db.Rows.Add(
            [| connectionEdit.Name :> obj
               connectionEdit.Host :> obj
               connectionEdit.Port :> obj |]
        )
        |> ignore

        grid.Refresh())

editServers.Click.Add(fun e ->
    match current () with
    | None -> ()
    | Some dr ->
        let rdeskop = dataRowToRemoteHost dr

        use edit =
            new ConnectionEdit(Name = rdeskop.name, Host = rdeskop.host, Port = rdeskop.port)

        if edit.Edit() then
            dr.BeginEdit()
            dr.Item("Name") <- edit.Name
            dr.Item("Host") <- edit.Host
            dr.Item("Port") <- edit.Port
            dr.EndEdit()
            grid.Refresh())


Application.Run mainForm

db.WriteXml "db.xml"
