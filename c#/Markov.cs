using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace markov3
{
    public class Analyzer
    {
        //private struct Dawg
        //{
        //    public int id;
        //    public int count;
        //    public List<string> sen;

        //    public bool equals(Dawg other)
        //    {
        //        return false;
        //    }
        //};

        private struct Sentence
        {
            public int id;
            public string text;
            public string dawg;
            public List<int> words;
            public Sentence(string t, int i)
            {
                id = i;
                text = t;
                dawg = "";
                words = new List<int>();
            }
            public List<int> intersect(Sentence sen)
            {
                List<int> inter = new List<int>();
                foreach (int i in words)
                {
                    if (sen.words.Contains(i))
                    {
                        if (!inter.Contains(i))
                            inter.Add(i);
                    }
                }
                return inter;
            }
        };
        private struct Word
        {
            public int id;
            public int count;
            public List<int> left;
            public List<int> right;
            public List<int> sent;
            public string text;
            public string type;
            public Word(int i, string t)
            {
                id = i;
                count = 0;
                left = new List<int>();
                right = new List<int>();
                sent = new List<int>();
                text = t;
                type = "";
            }
            public override string ToString()
            {
                string r = text + "[" + id + "," + count + "," + left.Count + "," + right.Count + "," + sent.Count + "]" + type;
                return r;
            }
        };


        List<Word> words;
        Hashtable swords;
        List<Sentence> dbs;
//        List<Dawg> dawg;
        string[] white = new string[] { " ", "\t" };
        //string[] white = new string[] { " ", "\t", "!", ".", ",", ";", "?" };
        public Random rand = new Random();
        public int verbosity = 6;
        HashSet<string> dawgs;

        public Analyzer(string infile, string wdb)
        {
            swords = new Hashtable();
            words = new List<Word>();
            dbs = new List<Sentence>();
            dawgs = new HashSet<string>();
            readWordsDB(wdb);
            try
            {
                foreach (string line in File.ReadAllLines(infile))
                {
                    Sentence s = addSentence(line.ToLower());
                    dawgs.Add(s.dawg);
                }
            } 
            catch (Exception) { }
            Console.WriteLine(" dbs : " + dbs.Count);
            Console.WriteLine(" wrd : " + words.Count);
        }

        private void readWordsDB(string infile)
        { 
            try
            {
                foreach (string line in File.ReadAllLines(infile))
                {
                    string[] pts = line.Split(white, StringSplitOptions.RemoveEmptyEntries);

                    //FIXME whtw to do with multiple words, here ?
                    if (pts.Length > 2) 
                        continue;
                    
                    Word w = lookup( pts[0].ToLower() );

                    string t = pts[pts.Length - 1];                   
                    if ( t.StartsWith("|") )
                        t = t.Substring(1);
                    w.type = t;

                    words[w.id] = w;
                }
            }
            catch (Exception) { }
        }
        Word lookup(string s)
        {
            Word w;
            if (!swords.ContainsKey(s))
            {
                int id = words.Count;
                w = new Word(id, s);
                words.Add(w);
                swords[s] = id;
            }
            else
            {
                int id = (int)swords[s];
                w = words[id];
            }
            return w;
        }

        Sentence addSentence(string line)
        {
            //foreach (Sentence s in dbs)
            //{
            //    if (s.text == line )
            //        return s;
            //}

            int id = dbs.Count;
            Sentence sen = new Sentence(line, id);
            Word w2 = new Word(-1, "");
            int sl = -1;
            string[] s0 = line.Split(white, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in s0)
            {
                Word w = lookup(s);
                if (sl != -1 && w2.id != -1)
                {
                    if (!w.left.Contains(sl)) w.left.Add(sl);
                    if (!w2.right.Contains(sl)) w2.right.Add(w.id);
                }
                string t = w.type;
                sen.dawg += t != "" ? t : "?";
                sen.dawg += " ";
                sl = w.id;
                w2 = w;
                w.sent.Add(sen.id);
                sen.words.Add(w.id);
                w.count = w.count + 1;
                words[w.id] = w;
            }
            dbs.Add(sen);
            return sen;
        }

        //bool checkGrammar( string ds, string wt, int pos )
        //{
        //    if (wt == "")
        //        return true;
        //    foreach (string d in dawgs)
        //    {
        //        if (d == "")
        //            continue;
        //        if (!d.StartsWith(ds))
        //            continue;
        //        string[] dsp = d.Split(white, StringSplitOptions.RemoveEmptyEntries);
        //        if (pos >= dsp.Length)
        //            return false;
        //        if (dsp[pos].Length > wt.Length)
        //            if (dsp[pos].Contains(wt))
        //                return true;
        //        if (dsp[pos].Length <= wt.Length)
        //            if (wt.Contains(dsp[pos]))
        //                return true;
        //    }
        //    return false;
        //}
        public string markov(string arg, int ct)
        {
            string[] args = arg.Split(white, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length < 1)
                return "";

            // cache words from input sentence
            List<int> iargs = new List<int>();
            for (int i = 0; i < args.Length; i++)
            {
                if (swords.Contains(args[i]))
                    iargs.Add((int)swords[args[i]]);
            }
            // find word with least entropy:
            int starter = -1;
            int startMin = 10000;
            for (int i = 0; i < iargs.Count; i++)
            {
                int j = iargs[i];
                if (words[j].count < startMin)
                {
                    starter = j;
                    startMin = words[j].count;
                }
               //  Console.WriteLine(words[j].count + "\t" + words[j].text);
            }

            Word w = starter!=-1 ? words[ starter ] : lookup(args[0]);
            int prv = w.id;
            int pos = 0;
            string txt = w.text;
            string ds = w.type;
            // try sometimes to expand to the left:
            //int p = rand.Next(3);
            //if (p > 0 && w.left.Count > 0)
            //{
            //    int i = rand.Next(w.left.Count - 1);
            //    int j = w.left[i];
            //    txt = words[j].text + " " + args[0];
            //}
            Word pw = w;
            while (ct-- > 0)
            {
                if (w.right.Count == 0)
                    break;

                List<int> found = new List<int>();
                foreach (int i in w.right)
                {
                    Word w2 = words[i];
                    //if (!checkGrammar(ds, w2.type, pos))
                    //    continue;
                    // prefer words from input sentence to random
                    if (iargs.Contains(i))
                    {
                        found.RemoveRange(0, found.Count); // use once only
                        found.Add(i);
                        iargs.Remove(i);
                        break; // take that
                    }
                    if (w2.left.Contains(prv))
                    {
                        found.Add(i);
                    }
                }
                if (found.Count == 0)
                    break;

                int j = found[rand.Next(found.Count - 1)];
                w = words[j];
                txt += " " + w.text;
                if (w.text.EndsWith(".") || w.text.EndsWith("!") || w.text.EndsWith("?"))
                    break;
                pos++;
                ds += " " + w.type;
                prv = w.id;
            }
            return (args[0] == txt ? "" : txt);
        }

        public string match(string inp, int id)
        {
            Sentence sen_in = addSentence(inp);
            List<int> found = new List<int>();
            int ml = sen_in.words.Count / 2;
            foreach (Sentence sen in dbs)
            {
                if (sen_in.text == sen.text)
                    continue;
                List<int> inter = sen.intersect(sen_in);
                if (inter.Count > ml) // better weight them here!
                {
                    found.Add(sen.id);
                    //ml = inter.Count;
                }
            }
            if (found.Count > 0)
            {
                int i = rand.Next(found.Count - 1);
                return dbs[found[i]].text;
            }
            return "";
        }

        public string printWord(string s, bool deep)
        {
            return printWord(lookup(s), deep);
        }
        public string printWord(int i, bool deep)
        {
            return printWord(words[i], deep);
        }
        private string printWord(Word w, bool deep)
        {
            string res = "";
            try
            {
                res += w.ToString();
                if (!deep)
                {
                    return res;
                }

                res += ("{");
                foreach (int m in w.left)
                    res += (m + ",");
                res += ("}{");
                foreach (int m in w.right)
                    res += (m + ",");
                res += ("}{");
                foreach (int m in w.sent)
                    res += (m + ",");
                res += ("}");
            }
            catch (Exception)
            {
            }
            return res;
        }

        public string printSentence(int id)
        {
            Sentence s = dbs[id];
            //string res = s.text + "{";
            //foreach (int m in s.words)
            //    res += (m + ",");
            //res += ("}");
            //return res;
            return s.text + "[" + s.dawg + "]";
        }

        public string run(string query)
        {
            if (query == null || query == ".")
                return "";

            string[] sl = query.Split(white, StringSplitOptions.RemoveEmptyEntries);
            if (sl.Length > 0)
            {
                if (sl[0] == "!say")
                {
                    return query.Substring(5);
                }
                if (sl.Length == 2)
                {
                    if (sl[0] == "!s")
                    {
                        try
                        {
                            int idx = Int32.Parse(sl[1]);
                            if (idx > 0)
                                return printSentence(idx);
                        }
                        catch (Exception) // rand sentence containing word
                        {
                            if (swords.Contains(sl[1]))
                            {
                                int i = (int)swords[sl[1]];
                                Word w = words[i];
                                if (w.sent.Count > 0)
                                {
                                    int j = rand.Next(w.sent.Count - 1);
                                    return printSentence(w.sent[j]);
                                }
                            }
                        }
                    }
                    if (sl[0] == "!w")
                    {
                        int idx = 0;
                        try { idx = Int32.Parse(sl[1]); }
                        catch (Exception) { }
                        if (idx > 0)
                            return printWord(idx, false);
                        else
                            return printWord(sl[1], false);
                    }

                    if (sl[0] == "!W")
                    {
                        int idx = 0;
                        try { idx = Int32.Parse(sl[1]); }
                        catch (Exception) { }
                        if (idx > 0)
                            return printWord(idx, true);
                        else
                            return printWord(sl[1], true);
                    }
                    if (sl[0] == "!v")
                    {
                        try { verbosity = Int32.Parse(sl[1]); }
                        catch (Exception) { }
                        return "";
                    }
                }
                if (sl.Length == 1)
                {
                    if (sl[0] == "!i")
                    {
                        return "words: " + words.Count + " " + "sentences: " + dbs.Count + " dawgs: " + dawgs.Count + " verbose: " + verbosity;
                    }
                    if (sl[0] == "!q")
                    {
                        System.Environment.Exit(1);
                        return "bye!";
                    }
                }
            }
            string res = markov(query.ToLower(), 32); // prefered
            //if (res == "")
            //    res = match(query, 1); // ugly fallback

            // learn a new one:            
            addSentence(query.ToLower());

            return res;
        }
    };
}
