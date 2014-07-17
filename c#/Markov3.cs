using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace irc
{
    public interface Server
    {
        bool start(Client client, string ircnode, string channel);
    }

    public interface Client
    {
        string nick();
        string run(string line);
    }

    public class Bot : Server
    {
        private Client client = null;
        private TcpClient irc = null;
        private StreamWriter writer;
        private StreamReader reader;

        private static int PORT = 6667;
        private static string SERVER = "irc.freenode.net";
        private static string NICK = "marc0ni"; 
        private static string PASS = "i_am_" + NICK;
        public static string CHANNEL = "#Markoff";
        private string[] moreChannels = new string[] { };
                
        public bool start(Client client, string ircnode, string channel)
        {
            try
            {
                this.client = client;
                CHANNEL = channel;
                SERVER = ircnode;
                NICK = client.nick();

                this.irc = new TcpClient(SERVER, PORT);
                Stream stream = irc.GetStream();
                this.reader = new StreamReader(stream);
                this.writer = new StreamWriter(stream);

                write("PASS " + "(none)");
                write("USER " + NICK + " 12 * :" + NICK  );
                write("NICK " + NICK);
                write("JOIN " + channel);

                listen();
            }
            catch (Exception ex) { Console.WriteLine(ex); Console.WriteLine(SERVER + " " + CHANNEL); return false; }
            return true;
        }

        string read()
        {
            string mess = "";
            try
            {
                mess = reader.ReadLine();
                Console.WriteLine(mess);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return mess;
        }
        public void write(string mess)
        {
            try
            {
                this.writer.WriteLine(mess);
                this.writer.Flush();
                Console.WriteLine(mess);
            }
            catch (Exception e)
            {
                Console.WriteLine(mess + "\n" + e.ToString());
            }
        }
        public void writeMess(string to, string mess)
        {
            if (mess == null || mess.Length < 1)
                return;
            if (mess.Length > 460)
            {
                mess = mess.Substring(0, 460) + "[truncated]";
            }
            write("PRIVMSG " + to + " :" + mess + "\r\n");
        }

        string messFrom(string m, char sep)
        {
            int endName = m.IndexOf(sep) - 1;
            if (endName > 0 && endName < m.Length)
                return m.Substring(1, endName);
            return null;
        }


        void listen()
        {
            bool ready = false;
            string inputLine = null;
            while ((inputLine = read()) != null)
            {
                int pg = inputLine.IndexOf("PING :");
                if (pg >= 0)
                {
                    pg += 6;
                    write("PONG :" + inputLine.Substring(pg, inputLine.Length - pg));
                    continue;
                }
                // skip motd, etc.
                if (inputLine.IndexOf("End of /MOTD command.")>= 1)
                {
                    ready = true;
                    foreach (string c in moreChannels)
                    {
                        write("JOIN " + c);
                    }
                    continue;
                }
                if (ready)
                {

                    string res = client.run(inputLine);
                    if (res != null && res != "")
                    {
                        if (res.StartsWith("/"))
                            write(res.Substring(1));
                        else
                            writeMess(CHANNEL, res);
                    }
                }
            } 
        }
    };

    public class Konsole : irc.Client
    {
        private string me = "", cin = "";
        Thread listen = null;
        Bot server = null;
        public Konsole(Bot serv, string n)
        {
            this.me = n;
            this.server = serv;
            listen = new Thread(new ThreadStart(delegate()
            {
                while (true)
                {
                    cin = Console.ReadLine();
                    if ( cin.StartsWith("/") )
                        server.write( cin.Substring(1) );
                    else
                        server.writeMess(Bot.CHANNEL, cin);
                }
            }));
            listen.Start();
        }
        public string nick() { return me; }
        public string run(string line)
        {
            return "";
        }
    }

 
}

namespace markov3
{
    public class MarkovBot : irc.Client
    {
        markov3.Analyzer analyze;
        private string me = "";
        irc.Bot server = null;
        public MarkovBot(irc.Bot serv, string n, string txt)
        {
            this.analyze = new markov3.Analyzer(txt, "E:\\code\\cs\\net\\gIOrdi\\markov3\\part-of-speech.txt");
            this.me = n;
            this.server = serv;
        }
        public string nick() { return me; }

        public string run(string line)
        {
            string cout = "";
            string[] messit = line.Split(new string[] { "PRIVMSG " }, StringSplitOptions.RemoveEmptyEntries);
            if (messit.Length < 2)
                return "";

            int n = messit[1].IndexOf(":");
            if (n >= 0)
            {
                string s = messit[1].Substring(n + 1);
                bool takeit = (1 == analyze.rand.Next(analyze.verbosity));
                takeit |= s.ToLower().Contains(me.ToLower());
                takeit |= s.Contains("All:");
                n = s.IndexOf(":");
                if (n >= 0 && n <= 12)
                {
                    s = s.Substring(n + 1);
                }
                n = s.IndexOf(",");
                if (n >= 0 && n <= 12)
                {
                    s = s.Substring(n + 1);
                }
                if (takeit)
                {
                    cout = analyze.run(s);
                    Thread.Sleep(300); // catch up on latest
                }
            }
            return cout;
        }
    }
    class Markov3
    {
        static void Main(string[] args)
        {
            if (true)
            {
                irc.Bot bot = new irc.Bot();
                if (args[0] == "con")
                {
                    bot.start(new irc.Konsole(bot, "n3rv0us"), "irc.freenode.net", "#p4p4p4");
                }
                else
                {
                    string a0 = "no.txt";
                    if (args.Length > 0) a0 = args[0];
                    string a1 = "empty";
                    if (args.Length > 1) a1 = args[1];
                    string a2 = "#p4p4p4";
                    if (args.Length > 2) a2 = args[2];
                    bot.start(new MarkovBot(bot, a1, a0), "irc.freenode.net", a2);
                }
            }
            //else 
            //{
            //    Analyzer analyze = new Analyzer("E:\\code\\cs\\net\\gIOrdi\\markov3\\darwin1.txt", "E:\\code\\cs\\net\\gIOrdi\\markov3\\part-of-speech.txt");
            //    while (true)
            //    {
            //        Console.WriteLine(analyze.run(Console.ReadLine()));
            //    }
            //}
        }
    }
}
