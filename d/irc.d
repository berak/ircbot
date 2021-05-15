import std.string, std.conv, std.stdio, std.socket;
import core.thread;


class IrcClient : Thread
{
    this()
    {
        super(&reader);
    }

    void connect(string nick)
    {
        sock = new TcpSocket(new InternetAddress("irc.freenode.net",6667));
        sock.send("USER " ~ nick ~ " 12 * :" ~ nick ~ "\r\n");
        sock.send("NICK " ~ nick ~ "\r\n");
    }

    void konsole()
    {
        char [] line, lorig;
        while (sock.isAlive())
        {
            int k = getchar();
            if (k <= 0) continue;
            if (k == '\n' && (!line.empty()))
            {
                line ~= "\r\n";
                sock.send(line);
                line = lorig;
                continue;
            }
            line ~= k;
        }
    }

    void reader()
    {
        char[1024] buf;
        while(sock.isAlive())
        {
            int n = sock.receive(buf);
            if (n <= 0) break;
            auto z = buf[0..n];
            if (z[0..4] == "PING")
            {
                sock.send("PONG : 41423d2d\r\n");
            }
            writefln(z);
        }
    }

private:
    Socket sock;
}

int main(string[] args)
{
    auto irc = new IrcClient();
    irc.connect("zwquiz3");
    irc.start();
    irc.konsole();
    return 0;
}
