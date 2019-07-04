:- use_module(library(socket)).

irc :-
    NICK="cvtail",
    CHAN="#p4p4p4",
    Adress = 'irc.freenode.net':6667,
    tcp_socket(Socket),
    tcp_connect(Socket, Adress, Read, Write),
    write_list(Write,["PASS i_am_",NICK]),
    write_list(Write,["USER ",NICK," 12 * ",NICK]),
    write_list(Write,["NICK ",NICK]),

    thread_create(irc_read(Read,Write), Th, []),
    konsole(Write).

konsole(Write) :-
    get_char(S),
    put_char(Write, S),
    (S=='\n' ->
        flush_output(Write);
        true),
    konsole(Write).

process(Read,Write,C) :-
    writeln(C),
    split_string(C," ", "", L),
    (member("433",L) ->
        writeln("nickname in use."), abort;
        true),
    (member("PING",L) ->
        irc_write(Write, "PONG !\r\n"), flush_output(Write);
        true),
    true.

write_list(O, [H|T]) :-
    irc_write(O,H),
    length(T,L),
    (L>0 ->
        write_list(O, T);
        write(O,"\r\n"), writeln(""), flush_output(O)),
    true.

irc_write(Write,Str) :-
    write(Str),
    write(Write,Str).

irc_read(Read,Write) :-
    read_string(Read,"\n", "\r", End, C),
    (End>0 ->
        process(Read,Write,C), irc_read(Read,Write);
        true).

