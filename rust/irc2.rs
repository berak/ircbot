use std::net::{TcpStream};
use std::io::{Read, Write};
use std::str::from_utf8;

fn console(stream: &mut TcpStream) {
	let inp = std::io::stdin();
	loop {
	    let mut buffer = String::new();
	    inp.read_line(&mut buffer).unwrap();
		stream.write((buffer+"\r\n").as_bytes()).unwrap();
	}
}

fn reader (stream: &mut TcpStream) {
    loop {
	    let mut data = [0; 2048];
	    match stream.read(&mut data) {
	        Ok(z) => {
	            let text = from_utf8(&data[0..z]).unwrap();
	            println!("{}", text);
	            if text.starts_with("PING") {
	            	stream.write(b"PONG : 123456\r\n").unwrap();
	          	}
	        },
	        Err(e) => {
	            println!("Failed to receive data: {}", e);
	        }
	    }
	}
}

fn main() {
	match TcpStream::connect("irc.freenode.net:6667") {
        Ok(mut stream) => {
            println!("Successfully connected to server in port 6667");

            stream.write(b"NICK abcd17\r\n").unwrap();
            stream.write(b"USER abcd17 abcd17 + 12 * :abcd17\r\n\r\n").unwrap();

			let mut s2 = stream.try_clone().unwrap();
 			std::thread::spawn(move||{
				console(&mut s2);
			});

			reader(&mut stream);
        },
        Err(e) => {
            println!("Failed to connect: {}", e);
        }
    }
}
