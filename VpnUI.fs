module RemoteDesktopAssistant.VpnUI

open System.Windows.Forms
module vpn = RemoteDesktopAssistant.OpenVpnGUI

type VpnCommand(title, executable, command) as x =
    class
        inherit Form()
        let exe = new TextBox(Text = executable, Width = 200)
        let cmd = new TextBox(Text = command, Width = 200)
        let dialog = new WinForms.DialogButtons(x)

        do
            x.StartPosition <- FormStartPosition.CenterParent
            x.Text <- title
            x.FormBorderStyle <- FormBorderStyle.FixedDialog
            let flow = new FlowLayoutPanel()
            flow.AutoSizeMode <- AutoSizeMode.GrowAndShrink
            flow.AutoSize <- true
            flow.MaximumSize <- System.Drawing.Size(400, 200)
            flow.Dock <- DockStyle.Fill
            flow.Controls.Add(new Label(Text = "OpenVPN Client:"))
            flow.Controls.Add exe
            flow.Controls.Add(new Label(Text = "Command:"))
            flow.Controls.Add cmd
            flow.Controls.Add dialog
            x.Controls.Add flow
            x.ClientSize <- flow.PreferredSize

        member x.Accept() = x.ShowDialog() = DialogResult.OK
        member x.Executable = exe.Text
        member x.Command = cmd.Text
    end

let vpnActions = new ToolStripMenuItem "&Vpn"
let connect = vpnActions.DropDownItems.Add "&Connect"
let disConnect = vpnActions.DropDownItems.Add "&Disconnect"
let killVpn = vpnActions.DropDownItems.Add "&Kill"

connect.Click.Add(fun e ->
    let ini = Init.read ()

    use cn = new VpnCommand("Connect to VPN", ini.OpenVpnExecutable, ini.VpnConnect)

    if cn.Accept() then
        Init.write (
            { ini with
                VpnConnect = cn.Command
                OpenVpnExecutable = cn.Executable }
        )

        try
            vpn.connect cn.Command |> ignore
        with e ->
            MessageBox.Show e.Message |> ignore)

disConnect.Click.Add(fun e ->
    let ini = Init.read ()

    use cn =
        new VpnCommand("Disconnect from VPN", ini.OpenVpnExecutable, ini.VpnDisconnect)

    if cn.Accept() then
        Init.write (
            { ini with
                VpnDisconnect = cn.Command
                OpenVpnExecutable = cn.Executable }
        )

        try
            vpn.disConnect cn.Command |> ignore
        with e ->
            MessageBox.Show e.Message |> ignore)

killVpn.Click.Add(fun e -> vpn.killProgram () |> ignore)
