#
# this mainly exploits the rest api from http://gpt.contentyze.com/
#

import socket
import base64
import requests



def bot(inp):
    txt = inp.replace(b"\"",b"\\\"")
    json = b'{"input_text":"'+txt+b'","model":3,"length":120}'
    url = b"http://gpt.contentyze.com/generate"
    try:
        x = requests.post(url,data=json)
        txt = x.text[9:-1].replace("\\n","  ")
        txt = txt.replace("\\\"", "\"")
    except:
    	return "sorry, that was invalid input .."
    return txt

def run_bot():
    channel = b"#opencv"
    nick = b"gpt3"

    #
    # main
    #
    # SASL identification ..
    # (required for bots running on e.g. amazon)
    b64auth = base64.b64encode(nick+b"\x00"+nick+b"\x00"+b"i_am_"+nick)
    irc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    irc.connect(('irc.freenode.net', 6667))
    irc.send(b"CAP REQ :sasl\r\n")
    irc.send(b"NICK " + nick + b"\r\n")
    irc.send(b"USER " + nick + b" 12 * :"+nick+b"\r\n")
    m = irc.recv(512)
    if m.find(b"ACK :sasl"):
        irc.send(b"AUTHENTICATE PLAIN\r\n")
        irc.send(b"AUTHENTICATE "+b64auth+b"\r\n")
        irc.send(b"CAP END\r\n")
    else:
        irc.send(b"PASS i_am_" + nick + b"\r\n")
    irc.send(b"JOIN " + channel + b"\r\n")

    while irc != None:
        m = irc.recv(512)
        if not m:
            break
        if len(m)==0 or m == "\r\n":
            continue
        if m.find(b"PING") == 0:
            irc.send(b"PONG 12345\r\n")
        if m.find(b"\x01VERSION\x01") > 0:
            continue
        me = m[1:m.find(b'!')] # who sent the msg
        pm = m.find(b"PRIVMSG %s :" % channel)
        if pm > 0:
            txt = m[pm+10+len(channel):-2]
            isforme=txt.find(nick)
            if (isforme>=0):
                print("[", str(me,"utf-8"), "]",str(txt,"utf-8"))
                txt = txt[2+isforme+len(nick):]
                txt = bot(txt)
                mess = "PRIVMSG {} : {}: {}\r\n".format(str(channel,"utf-8"),str(me,"utf-8"),txt)
                print(mess)
                irc.send(mess.encode('utf-8'))
        else:
            pm = m.find(b"PRIVMSG %s :" % nick)
            if pm < 0:
                continue
            txt = m[pm+10+len(nick):-2]
            print("(", str(me,"utf-8"), ")", str(txt,"utf-8"))
            txt = bot(txt)
            mess = "PRIVMSG {} : {}\r\n".format(str(me,"utf-8"),txt)
            print(mess)
            irc.send(mess.encode('utf-8'))

    irc.close()

if __name__ == '__main__':
    while(True):
        run_bot()
