module RemoteDesktopAssistant.VpnUI

open System.Windows.Forms
module vpn = RemoteDesktopAssistant.OpenVpnGUI

type ConnectionName() as x =
    class
        inherit Form()

        let name = new TextBox()
        let dialog = new WinForms.DialogButtons(x)

        do
            x.Text <- "Enter OpenVPN filename"
            x.FormBorderStyle <- FormBorderStyle.FixedDialog
            let flow = new FlowLayoutPanel()
            flow.AutoSizeMode <- AutoSizeMode.GrowAndShrink
            flow.AutoSize <- true
            flow.MaximumSize <- System.Drawing.Size(300, 0)
            flow.Dock <- DockStyle.Fill
            flow.Controls.Add(new Label(Text = "VPN:"))
            flow.Controls.Add name
            flow.Controls.Add dialog
        end
        member x.Name = x.name.Text

        member x.GetName() =
            if x.ShowDialog() = DialogResult.OK then
                Some x.name.Text
            else
                None
    end

let vpnActions = new ToolStripMenuItem "&Vpn"
let connect = vpnActions.DropDownItems.Add "&Connect"
let disConnect = vpnActions.DropDownItems.Add "&Disconnect"
let killVpn = vpnActions.DropDownItems.Add "&Kill"

connect.Click.Add(fun e -> 
     use cn = new ConnectionName()
     match cn.GetName() with
     |Some name ->   vpn.connect name |> ignore 
     |None -> ())
     
disConnect.Click.Add(fun e -> vpn.killProgram () |> ignore)
killVpn.Click.Add(fun e -> vpn.killProgram () |> ignore)
