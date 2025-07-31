module RemoteDesktopAssistant.WinForms

open System.Windows.Forms

type DialogButtons(dialog: Form) as x =
    class
        inherit FlowLayoutPanel()
        let accept = new Button(DialogResult = DialogResult.OK, Text = "OK")
        let cancel = new Button(DialogResult = DialogResult.Cancel, Text = "Cancel")
        
        do
            x.AutoSize <- true
            x.AutoSizeMode <- AutoSizeMode.GrowAndShrink
            x.WrapContents <- false
            x.Controls.Add accept
            x.Controls.Add cancel
          
            dialog.AcceptButton <- accept
            dialog.CancelButton <- cancel
    end
