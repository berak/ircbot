defmodule Irc do
  def connect(chan,nick) do
    opts = [:binary, packet: :line, active: false]
    case :gen_tcp.connect('irc.freenode.net', 6667, opts) do
      {:ok, sock} -> :gen_tcp.connect('irc.freenode.net', 6667, opts)
        intro = "USER elirc 12 * :elirc\r\n"
             <> "NICK " <> nick <> "\r\n"
             <> "PASS i_am" <> nick <> "\r\n"
             <> "JOIN " <> chan <> "\r\n"
        :gen_tcp.send(sock, intro)
        Task.start_link(fn -> rcv(sock) end)
        kb(sock, chan, nick)
      {:error, msg} -> IO.puts msg
    end
  end

  defp rcv(sock) do
    case :gen_tcp.recv(sock, 0, 99999999) do
      {:ok, m} ->
        IO.puts m
        if String.starts_with?(m, "PING") do
          :gen_tcp.send(sock, "PONG lalala\r\n")
        end
        rcv(sock)
      {:error, reason} ->
        System.halt(1)
    end
  end

  defp kb(sock, chan, nick) do
    m = IO.gets "$ "
    if String.starts_with?(m, "/") do
      s = String.slice(m, 1,10000)
    else
      s = "PRIVMSG " <> chan <> " :" <> m
    end
    :gen_tcp.send(sock, s)
    kb(sock, chan, nick)
  end
end

Irc.connect("#elixir-lang", "elirc")
