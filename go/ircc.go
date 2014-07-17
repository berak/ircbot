package main

import (
	"bufio"
	"flag"
	"fmt"
	"net"
	"os"
	"strings"
)

type IrcBot struct {
	con     *net.TCPConn
	nick    string
	channel string
}

func (bot *IrcBot) Connect(host string, channel string, nick string, pass string) error {
	addr, e := net.ResolveTCPAddr("tcp", host)
	if addr == nil || e != nil {
		fmt.Printf("!addr!%s.\n", e)
		return e
	}

	c, e := net.DialTCP("tcp", nil, addr)
	bot.con = c
	if bot.con == nil || e != nil {
		fmt.Printf("!con!%s.\n", e)
		return e
	}
	fmt.Printf("connected to %s %s as %s\r\n", host, channel, nick)

	bot.Write("PASS " + pass + "\r\n")
	bot.Write("USER " + nick + " 12 * :" + nick + "\r\n")
	bot.Write("NICK " + nick + "\r\n")
	bot.Write("JOIN " + channel + "\r\n")

	bot.nick = nick
	bot.channel = channel
	return nil
}

func (bot *IrcBot) Read() string {
	buf := make([]byte, 8021)
	r, e := bot.con.Read(buf)
	if r < 1 || e != nil {
		fmt.Printf("!read!%s.\n", e)
		return ""
	}
	return string(buf[0:r])
}

func (bot *IrcBot) Write(s string) {
	r, e := bot.con.Write([]byte(s))
	if r < 1 || e != nil {
		fmt.Printf("!read!%s.\n", e)
	}
}

func (bot *IrcBot) Parse(s string) (string, string) {
	if strings.Index(s, "PRIVMSG") < 1 {
		return "", ""
	}
	ss := strings.Split(s, "PRIVMSG ")
	from := strings.Split(ss[0], "!")[0][1:]
	mess := strings.SplitAfterN(ss[1], ":", 2)[1]
	return from, mess
}

func (bot *IrcBot) Listen(raw bool) {
	for {
		s := bot.Read()
		if s == "" {
			return
		}
		if strings.Index(s, "PING") > -1 {
			bot.Write("PONG a Dong!\r\n")
		} else {
			if raw {
				fmt.Printf("%s\r\n", s)
			} else {
				from, mess := bot.Parse(s)
				if from != "" {
					fmt.Printf("%s : %s", from, mess)
				}
			}
		}
	}
}

//
// flags have to go BEFORE other args !
// 
func main() {
	var irc IrcBot
	var channel string = "#aichallenge"
	var nick string = "g0llum"
	var raw *bool = flag.Bool("raw", false, "do not parse input")
	var rdo *bool = flag.Bool("readonly", false, "do not read stdin")
	flag.Parse()

	if flag.NArg() > 0 {
		channel = flag.Arg(0)
	}
	if flag.NArg() > 1 {
		nick = flag.Arg(1)
	}

	e := irc.Connect("irc.freenode.net:6667", channel, nick, "i_am_"+nick)
	if e != nil {
		return
	}
	go irc.Listen(*raw)

	if *rdo {
		for ever := true; ever; {
		}
	} else {
		con := bufio.NewReader(os.Stdin)
		for {
			data, err := con.ReadBytes('\n')
			if err != nil {
				break
			}
			line := string(data)
			if len(line) == 0 {
				continue
			}
			if strings.Index(line, "/") == 0 {
				irc.Write(strings.TrimLeft(line, "/"))
			} else {
				targ := irc.channel
				if strings.Index(line, "#") == 0 {
					ss := strings.Split(line, " ")
					targ = ss[0]
					line = ss[1]
				}
				irc.Write("PRIVMSG " + targ + " :" + line + "\r\n")
			}
		}
	}
}
