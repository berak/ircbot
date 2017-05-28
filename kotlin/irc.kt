import java.net.*
import java.io.*
import java.util.*
import kotlin.concurrent.*

fun digit(d: Int): String {
	if (d<10) return "0" + d
	return "" + d
}

@Suppress("DEPRECATION")
fun time(): String {
	var d = Date()
	return "[" + digit(d.getHours()) + ":" + digit(d.getMinutes()) + ":" + digit(d.getSeconds()) + "]"
}

fun main(args: Array<String>) {
	var raw  = false
	var chan = "##electronics"
	//var chan = "#kotlin"
	var nick = "notanabbot"

	// connect:
	var sock = Socket("irc.freenode.net", 6667)
	var bot_in = BufferedReader(InputStreamReader(sock.getInputStream()))
	var bot_out = PrintWriter(sock.getOutputStream())
	bot_out.println("USER " + nick + " 12 * :" + nick)
	bot_out.println("NICK " + nick)
	bot_out.println("JOIN " + chan)
	bot_out.flush()
	println(sock.toString() + "  on " + chan + " as " + nick)

	// console thread:
	var con = System.console()
	thread(block = {
		while(true) {
			var line = con.readLine()
			var msg: String
			if (line.startsWith("/")) {
				msg = line.substring(1)
				when (msg) {
					"quit" -> System.exit(0)
					"raw"  -> raw = ! raw
				}
			} else {
				msg = "PRIVMSG " + chan + " :" + line
			}
			bot_out.println(msg)
			bot_out.flush()
		}
	})

	// main thread, read from sock:
	while(true) {
		var b: String;
		b = bot_in.readLine()

		if (raw) {
			println(b)
		} else { // nicely format human messages:
			var q: String = "PRIVMSG " + chan + " :"
			var l = b.indexOf(q)
			if (l>-1) {
				var p = l + q.length
				if (p < b.length - 1) {
					var from = b.substring(1, b.indexOf("!"))
					var msg  = b.substring(p)
					println( time() + " " + from + "\t: " + msg)
				}
			}
		}
		if (b.startsWith("PING")) {
			bot_out.println("PONG 12345")
			bot_out.flush()
		}
	}
}
