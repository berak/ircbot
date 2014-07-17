var nick = "prego";
var channels = {"olon":[]};
//var nick = "opencv_logs";
//var channel = "#opencv";

var net = require('net'),
	fs  = require('fs'),
	sys = require('sys');


function rep_log() {
	d = realDate()
	for (c in channels) {
		fname = c + "_" + d.getFullYear() + "_" + (d.getMonth()+1)+"_"+d.getDate()+".txt"
		logfile = fs.openSync(fname, "w+")
		for (l in channels[c]) {
			fs.writeSync(logfile, l );
		}
		channels[c] = []
	}
	setTimeout(rep_log, 1000 *60*5 )
}


var stream = net.createConnection( 6667, "irc.freenode.net" );
stream.addListener('connect', function () {	
	stream.write("NICK " + nick + "\r\n")
	stream.write("USER " + nick + " 12 * :" + nick + "\r\n")
	stream.write("JOIN " + channel + "\r\n")
	rep_log()
});

stream.addListener('data', function (data) {
	var s = data.toString();
	if ( s.match(/^PING/) ) {
		stream.write('PONG bing bang bong.\r\n' );
		return
	}
	for (channel in channels ) {
		tok = "PRIVMSG #" + channel
		if ( s.indexOf(tok + ' :' + nick)>-1 ) {
			stream.write(tok + ' :' + 'channel logs at http://d91bbb62.dotcloud.com/' + channel + ' .\r\n' );
			return
		}
		start = s.indexOf(tok)
		if (start < 0 )
			return
		if( s[0]!=':' )
			return
		e = s.indexOf('!~')
		if ( e < 0 || e > 40 )
			return
		d = realDate()
		m = d.getFullYear() + "." + (d.getMonth()+1)+"."+d.getDate() + " "+d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds() + "  ["
		m += s.substring(1,e) + "]"
		m += s.substring(start+tok.length)
		channels[channel].push( m );
		sys.puts("["+channel+"] " + m );
	}
});

stream.addListener('close', function () {
	stream.end();
	logfile.closeSync()
});



///////////////////////////////////////////////////////////////////////////

var http = require("http"),
    url  = require("url"),
    path = require("path");


http.createServer(function(request, response) {
    var req = url.parse(request.url,true);
    var uri = req.pathname;
	sys.puts(uri)
	if ( uri == "/shell" ) {
		doShell(req.query, response )
		return
	}
	var filename = path.join(process.cwd(), uri);
    path.exists(filename, function(exists) {
    	if(!exists) {
    		response.writeHead(404, "sorry, nothing.", {"Content-Type": "text/plain"});
    		response.end("404 Not Found\n");
    		return;
    	}

        stats = fs.statSync( filename );
        if (stats.isDirectory())
        {
            var b = path.basename(filename);
            fs.readdir(filename,function(err, files){
                var d = "";
                files.forEach( function(f) {
					if (f.indexOf(".txt")>-1)
                    	//d += "<li><a href='"+(b+'/'+f)+"'>"+f+"</a></li>";
                    	d += "<li><a href='/"+f+"'>"+f+"</a></li>";
                });
                response.writeHead(200,"ok",{"Content-Type": "text/html"});
                response.end( "<h2>logs for : " + channel + "</h2><br><p><ul>" + d +  "</ul>" );
            });
            return;
        }
        
    	fs.readFile(filename, "binary", function(err, file) {       
    		if(err) {
    			response.writeHead(500, {"Content-Type": "text/plain"});
    			response.end(filename + " " +  err + "\n");
    			return;
    		}

    		response.writeHead(200);
    		response.end(file, "binary");
    	});
    });
}).listen(8080);


function doShell(  params,  response ) {
	r = ""
	if ( params && params['cmd'] ) {
		sys.puts(params['cmd']);
		try {
			r = eval(params['cmd'])
		} catch(e) {
			r = e
		}
	}
	html  = "<form action='/shell'>"
	html += "<textarea name=cmd rows=4 cols=60></textarea><br>"
	html += "<input type=submit><br>"
	html += "</form><br>"
	html += "<textarea rows=4 cols=60 name=output>"+r+"</textarea><br>"

	response.writeHead(200, {"Content-Type": "text/html"});
	response.end(html);
}
