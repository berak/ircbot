#ifndef __Thread_onboard__
#define __Thread_onboard__

 #include <windows.h>
// #include <process.h>

class Thread {
protected:
	HANDLE thread;
public:
	Thread() : thread(0) {}
	virtual ~Thread() { stop(); }
	void start();
	void stop();
	void sleep( int millis );
	const bool running() { return (thread != 0); }
	virtual void run() = 0;
};
	

#endif // __Thread_onboard__

