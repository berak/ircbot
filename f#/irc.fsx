open System
open System.Windows.Forms
open System.Threading

let NICK = "njamr"
let mutable CHANNEL = "#p4p4p4"

let form = new Form()
form.Width  <- 400
form.Height <- 300
form.Visible <- true 
form.Text <- "i r c"

let tabs = new TabControl()
tabs.Dock <- DockStyle.Fill
tabs.Click.Add( fun e ->
  CHANNEL <- tabs.SelectedTab.Text 
  Console.WriteLine("changed to " + CHANNEL) 
  )

let tcp = new System.Net.Sockets.TcpClient("irc.freenode.net",6667)
let strm = tcp.GetStream()
let reader = new System.IO.StreamReader(strm)
let writer = new System.IO.StreamWriter(strm)

let irc_write (s:string) = 
  writer.WriteLine(s)
  writer.Flush()
  Console.WriteLine("> " + s)
  
let irc_read () =
  reader.ReadLine()
  
irc_write("PASS " + "i_am_" + NICK)
irc_write("USER " + NICK + " 12 * :" + NICK)
irc_write("NICK " + NICK);

let newChan (s:string) = 
  let page = new TabPage()
  page.Text <- s
  page.Name <- s
  let text = new RichTextBox()
  text.Dock <- DockStyle.Fill  
  text.Text <- "welcome to " + s + ".\r\n"
  page.Controls.Add(text)    
  tabs.Controls.Add(page)    
  
irc_write("JOIN " + CHANNEL);
newChan(CHANNEL)

let findText(s:string) =
  tabs.Controls.Find(s,true).[0].Controls.[0]

let editB = new TextBox()
editB.Dock <- DockStyle.Bottom  
editB.Text <- "/clear"
editB.KeyDown.Add(fun e -> 
  if (e.KeyValue = 13) then
    let text = findText(CHANNEL)
    if editB.Text.StartsWith("/") then
      if text.Text = "/clear" then 
        text.Text <- ""
      else if editB.Text.StartsWith("/join") then 
        CHANNEL <- editB.Text.Substring(6)
        irc_write("JOIN " + CHANNEL);
        newChan(CHANNEL)
        text.Text <- ""  
      else
        irc_write(editB.Text.Substring(1) + "\r\n")
        text.Text <- text.Text + editB.Text + "\r\n"
    else
      irc_write("PRIVMSG " + CHANNEL + " :" + editB.Text + "\r\n")
      text.Text <- text.Text + "<" + NICK + ">\t" + editB.Text+ "\r\n"
    editB.Text <- ""
  )
form.Controls.Add(editB)    
form.Controls.Add(tabs)


let rd = new Thread(new ThreadStart(fun _ ->
  while(true) do
    let mess = reader.ReadLine()
    Console.WriteLine(mess)
    if mess.StartsWith("PING") then
        irc_write("PONG 12345\r\n")
    let l = mess.IndexOf("PRIVMSG ")
    Console.WriteLine(l>0)
    if l>=0 then
      let text = findText(CHANNEL)
      let ne = mess.IndexOf("!")
      if ne > 0 then
        let n = mess.Substring(1,ne-1)
        let p = mess.Substring(l)
        let c = p.Substring(9)
        let e = p.IndexOf(":")
        let m = p.Substring(e+1)
        text.Text <- text.Text + "<" + n + "> " + m + "\n"
        Console.WriteLine("<" + n + "> " + m)
  ))
rd.Start()

//#if COMPILED
// Run the main code. The attribute marks the startup application thread as "Single 
// Thread Apartment" mode, which is necessary for GUI applications. 
[<STAThread>]    
do Application.Run(form)
//#endif
