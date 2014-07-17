(ns irc
   (:import (java.net Socket)
            (java.io PrintWriter InputStreamReader BufferedReader)))

(def freenode {:name "irc.freenode.net" :port 6667})

(declare conn-handler)

(defn connect [server]
   (let [socket (Socket. (:name server) (:port server))
         in (BufferedReader. (InputStreamReader. (.getInputStream socket)))
         out (PrintWriter. (.getOutputStream socket))
         conn (ref {:in in :out out})]
		(doto (Thread. #(conn-handler conn)) (.start))
		conn
	)
)

(defn write [conn msg]
	(doto (:out @conn)
		(.println (str msg "\r"))
		(.flush)))


(defn conn-handler [conn]
	(while 
		(nil? (:exit @conn))
		(let [msg (.readLine (:in @conn))]
			(println msg)
			(cond 
		(re-find #"^ERROR :Closing Link:" msg) 
		(dosync (alter conn merge {:exit true}))
		(re-find #"^PING" msg)
		(write conn (str "PONG "  (re-find #":.*" msg)))))))




(defn client [chan nick]
	(def irc (connect freenode))
	(write irc (str "NICK " nick))
	(write irc (str "USER " nick " 0 * :" nick))
	(write irc (str "JOIN " chan))
	(while irc
		(let [msg (read-line)]	
			(if (re-find #"^/" msg) 
				(write irc (subs msg 1)) 
				(write irc (str "PRIVMSG " chan " :" msg))
			)			
		)
	)
)

(client "#p4p4p4" "m0wgl1")
