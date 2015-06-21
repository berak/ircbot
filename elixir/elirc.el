defmodule Irc do
  def connect(chan,nick) do
    opts = [:binary, packet: :line, active: false]
    {:ok, sock} = :gen_tcp.connect('irc.freenode.net', 6667, opts)
    intro = "USER elirc 12 * :elirc\r\n"
         <> "NICK " <> nick <> "\r\n" 
         <> "PASS i_am" <> nick <> "\r\n" 
         <> "JOIN " <> chan <> "\r\n"
    :gen_tcp.send(sock, intro)
    Task.start_link(fn -> rv(sock) end)
    kb(sock)
  end

  defp rv(sock) do
    case :gen_tcp.recv(sock, 0, 99999999) do
      {:ok, "PONG"} ->
        :gen_tcp.send(sock, "PONG, y'old bastard\r\n")
        rv(sock)
      {:ok, m} ->
        IO.puts m
        rv(sock)
      {:error, :timeout} -> 
        :ok
    end
  end
  
  defp kb(sock) do
    m = IO.gets "> "
    cond do
      m == "ok.\n" -> 1
      true -> 
        :ok = :gen_tcp.send(sock, m)
        kb(sock)
    end
  end
end

Irc.connect("#elixir-lang", "elirc")
