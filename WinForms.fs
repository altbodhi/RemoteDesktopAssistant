namespace RemoteDesktopAssistant.WinForms

open System.Windows.Forms

type DialogButtons(dialog: Form) as x =
    class
        inherit FlowLayoutPanel()
        let accept = new Button(DialogResult = DialogResult.OK, Text = "OK")
        let cancel = new Button(DialogResult = DialogResult.Cancel, Text = "Cancel")
        let empty = new Label(Width = 45)
        do
            x.AutoSize <- true
            x.AutoSizeMode <- AutoSizeMode.GrowAndShrink
            x.WrapContents <- false
            x.Controls.Add empty

            x.Controls.Add accept
            x.Controls.Add cancel

            dialog.AcceptButton <- accept
            dialog.CancelButton <- cancel
    end
