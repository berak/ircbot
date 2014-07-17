import os
import re
import time
import socket
 
#os.fork()

frames = 0
lines = {}
names = []
channel = "#p4p4p4" # "#ubuntu"
nick = 'pz222'

irc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
irc.connect(('irc.freenode.net', 6667))
 
irc.send("NICK " + nick + "\r\n")
irc.send("USER " + nick + " 12 * :"+nick+"\r\n")
irc.send("JOIN " + channel + "\r\n")
#~ irc.send("JOIN " + "#ubuntu" + "\r\n")
#~ irc.send("JOIN " + "#reddit" + "\r\n")
#~ irc.send("JOIN " + "#python" + "\r\n")
#~ irc.send("JOIN " + "#gentoo" + "\r\n")

def sendmsg(s):
   m = "PRIVMSG " + "#p4p4p4" + " :" + s + "\r\n"
   r = irc.send(m)
   print "* "+str(r)+"> " + m
   
def run(s):
   if s == "bye":
      sys.exit(0)
   elif s == "helo":
      sendmsg("heloo")
   elif s == "count":
      sendmsg(str(len(lines)) + " items")
   elif s == "names":
      sendmsg(str(names))
   elif s == "time":
      sendmsg(time.asctime())
   else:
      r=checkLine(s)
      if r:
         sendmsg(r)
      
def splitLine(s):
   b = set()
   for i in s.split(" "):
      if i is not '':
         b.add(i)
   return (b)
   
def checkLine(s):
   b=splitLine(s)
   #~ q=s.split("?")
   #~ if (len(q)<2):           
   if s not in lines:
      lines[s] = b
      #~ print ">>> " + str(len(b)) + " : " + str(len(lines)) + " items."
      
   best=None
   bm=0;   
   for l in lines:
      v = lines[l]
      if v == b:
         continue        
      d = len(v.intersection(b))
      if d > bm:
         best = l
         bm = d
   if ( bm > 3 ):
      print "\n** " + str(bm) + "/" + str(len(b)) + ": " + str(best)
   return best
   

while irc != None:
   frames += 1;
   received = irc.recv(512)
   if not received:
      break
   for buffer in received.split("\r\n"):

    #  print "["+str(frames)+"] " + buffer  

      regex = re.match('(?i)^PING (:[^ ]+)$', buffer)
      if regex is not None:
         irc.send('PONG %s\r\n' % regex.group(1))
         continue

      l = buffer.split('@ ' + channel + " :")
      if len(l)>1:
         names+=(l[1].split(' '))
         #print "names: " + str(len(names))
      l = buffer.split('= ' + channel + " :")
      if len(l)>1:
         names+=(l[1].split(' '))
         #print "names: " + str(len(names))
      
      msg = buffer.split('PRIVMSG ')
      if len(msg)<2:
         continue;
         
      msg = msg[1].split(" :")
      if len(msg)>1:
         r = msg[1]
         l = None
         l2 = r.split(":")
         if ( len(l2)>1 and len(l2[0])<13 ):
            l=l2[0]
            r=l2[1]
         else:
            l2 = r.split(",")
            if ( len(l2)>1 and len(l2[0])<13 ):
               l=l2[0]
               r=l2[1]

         if l in names:
            print "** " + l + " **"
            
         print r

         if l == nick:
            run(r)
         else:
            checkLine(r)
      
      
      
irc.close()

