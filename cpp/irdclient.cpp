#include "Birds.h"
#include "Thread.h"

#include <stdio.h>

class IrcListener : public Thread
{
	int sock;
	
public:
	
	IrcListener() 
		: sock(-1) 
	{}

	~IrcListener() 
	{
		Birds::Close(sock);
	}
	
	virtual void run()
	{
		while(true) 
		{
			char *m = Birds::Read(sock);
			if ( ! m ) break;
			
			if ( !strncmp(m, "PING",4) )
			{
				Birds::Write(sock, "PONG 12345 : hiho;) \r\n", 0);
			}
			printf("%s", m );
		}
	}
	
	bool start(char * host, int port)
	{
		sock = Birds::Client(host, port);
		if ( sock==-1 ) return false; // connection fail
		Birds::Write(sock, "NICK b1rd13\r\n", 0);
		Birds::Write(sock, "USER b1rd13 0 * :i am a bot\r\n", 0);
		Birds::Write(sock, "JOIN #pp1234\r\n", 0);
		Thread::start();
		return true;
	}

	int write( const char * chan, const char * mess )
	{
		int bytes=0;
		if ( mess[0] == '/' )
		{
			bytes=Birds::Write(sock, (char *)(mess+1), 0);
		}
		else
		{
			char buf[499];
			sprintf(buf,"PRIVMSG %s :%s\r\n",chan,mess);
			bytes=Birds::Write(sock, buf, 0);
		}
		return bytes;
	}
};

//
// cl client.cpp Thread.cpp birds.cpp ws2_32.lib
//
int main( int argc, char **argv )
{

	IrcListener irc;
	irc.start("irc.freenode.net",6667);
    
    while(true) // input loop
    {
		char buf[500];
		fgets(buf,500,stdin);
		if ( irc.write("#pp1234", buf) < 1 )
			break;
    }
    return 0;
}
