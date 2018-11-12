using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Qs.Enumerators;
using Qs.Parse;
using Qs.Parse.Developed;
using Qs.Pdb;
using Qs.Pdb.CPU;
using Qs.Structures;
using Qs.Utils;
using Qs.Utils.Base;

using Class = Qs.Parse.Developed.Class;
using Label = Qs.Parse.Developed.Label;
using Qs.Utils.Syntax;

namespace Qs
{

    
    public static class Test
    {
        private static byte[] ru = new byte[(int)Math.Pow(16, 7)];
        public static readonly string Prg_Example;

        

        static object key()
        {
          
            var ts = new byte[16];
            var u = 0;

            for (var i = 'A'; i < 'F'; i++)
                ts[u++] = (byte)i;
            for (var i = '0'; i < '9'; i++)
                ts[u++] = (byte) i;
            var f = 0;
            for (var n = 0; n < 16; n++)
                for (var o = 0; o < 16; o++)
                    for (var p = 0; p < 16; p++)
                        for (var k = 0; k < 6; k++)
                            for (var l = 0; l < 6; l++)
                                for (var m = 0; m < 6; m++)
            for (var i = 0; i < 6; i++)
                for (var j = 0; j < 6; j++)
                                    {
                                            ru[f] = ts[i];
                                            ru[++f] = ts[j];
                                            ru[++f] = ts[k];
                                            ru[++f] = ts[l];
                                            ru[++f] = ts[m];
                                            ru[++f] = ts[n];
                                            ru[++f] = ts[o];
                                            ru[++f] = ts[p];
                                            ru[++f] = (byte)'\r';
                                        }
            File.WriteAllBytes("e:\\mot.txt",ru);return null;
        }
        static Test()
        {
            
            var a = uint.MaxValue;uint b = 2;
            int b1 = (short) Math.Abs(b);
            var eq = a != b;
            var _eq = a != b1;
            var __eq = b1 != a;

            var t = _eq.ToString(CultureInfo.InvariantCulture) + __eq.ToString(CultureInfo.InvariantCulture);
            
            const string s1 = @"class AV
                            {
                                
                                int M(int a){return c.aa;}
                                int aa;
                                AV(int l)
                                {
                                    int b=2;
                                    int x=6;
                                    M(b,x,this);
                                }
                            }
                            class c:AV {AV a;}";
            var var = new StringBuilder(@"space ac{");
            for (var i = 0; i < 1; i++)
                var.Append(s1);
            var.Append("}");
            Prg_Example = var.ToString();
        }


        static void t_Created(object sender, FileSystemEventArgs e)
        {
            
        }

        public static ExtendParse[] Update(BasicParse p)
        {
            return new[]
            {
                (ExtendParse) new ArrayCaller(p),
                new Bloc(p),
                new Boucle(p),
                new Class(p),
                new Class(p),
                new ComplexHeritachy(p),
                new Constructor(p),
                new DeclaredParams(p),
                new Do(p),
                new EqAssign(p),
                new Expression(p),
                new For(p),
                new Function(p),
                new Goto(p),
                new If(p),
                new Instruction(p),
                new Label(p),
                new MethodCaller(p),
                new New(p), new Parent(p),
                new Return(p),
                new Return(p),
                new Word(p),
                new Space(p),
                new TypeAssign(p),
                new When(p),
                new While(p),
                new CallParameter(p),
                new SWord(p),
                new SHyratachy(p)
            };
        }

        public static void _Test()
        {
            while (true)
            {
                __test();

                var parse = new BasicParse { Pile = new Pile(Prg_Example) };
                var parent = new Tree(parse.Pile, null, Kind.Program);
                var glob = CurrentScop.Initialize("globe");
                Update(parse);
                var s = new Space(parse);
                if (s.Parse(parent))
                {
                    var byteCode = new ByteCodeMapper(glob);
                    var load = new LoadClasses(byteCode);
                    load.Add(parent[0]);
                    load.Compile();
                    var inst = load.Optimum.Instructs;
                    var sw = new IO.Stream.StreamWriter(true);
                    for (int i = 0; i < inst.Count; i++)
                        inst[i].Push(sw);
                    var tt = inst[0].Length;
                    var sr = new IO.Stream.StreamReader(sw);
                    for (int i = 0; i < inst.Count; i++)
                    {
                        var x = Instruct.Pop(sr);
                        var isieq = x.Equals(inst[i]);
                        if (!isieq) { }
                    }
                }
            }
        }

        private static void __test()
        {
        //    down();

        }

        public static long GetSize(Uri url)
        {
            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            int ContentLength;
            using (var resp = req.GetResponse())
                return int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength) && ContentLength != -1
                    ? ContentLength
                    : req.ContentLength;
        }
        private static long size;
        public static void MultiDownloading(Uri url, string output, int n_requests)
        {
            var pp = 0L;
            size = GetSize(url);
            long vsize = 0;
            var pas = size/n_requests;

            long np = pas;
            Downloaded = new int[n_requests];
            n_requests--;
            for (var i = 0; i <= n_requests; i++)
            {
                if (i == n_requests)
                    np = size - 1;
                else np += pas;

                start_downloading(url, output,i, pp, np);
                vsize += np - pp + 1;
                pp = np + 1;
            }
            if (vsize != size) throw new Exception();
        }

        private static int[] Downloaded;

        public static double SizeDownloaded
        {
            get
            {
                var t = 0;
                foreach (var t1 in Downloaded)
                    t += t1;

                return t;
            }
        }

        private static List<Thread> threads = new List<Thread>(8);
        private static void start_downloading(Uri url, string output,int index ,long from, long to)
        {
            if (from > to) return;
            var r = new Thread(new ParameterizedThreadStart(delegate
            {
                MyDownloadFile(url, output + @from + "-" + to,index, from, to);
            }));   
            threads.Add(r);
            r.Start(null);
        }
        public static void MyDownloadFile(Uri url, string outputFilePath,int index,long from, long to)
        {
            const int BUFFER_SIZE = 16 * 1024;
            using (var outputFileStream = File.Create(outputFilePath, BUFFER_SIZE))
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.AddRange(from, to);
                Stream responseStream = null;
                WebResponse response = null;
                try
                {
                    using (response = req.GetResponse())
                    using (responseStream = response.GetResponseStream())
                    {
                        var buffer = new byte[BUFFER_SIZE];
                        int bytesRead;
                        if (responseStream.CanSeek)
                            responseStream.Seek(100, SeekOrigin.Begin);
                        do
                        {
                            bytesRead = responseStream.Read(buffer, 0, BUFFER_SIZE);
                            outputFileStream.Write(buffer, 0, bytesRead);
                            Downloaded[index] += bytesRead;
                        } while (bytesRead > 0);
                        return;
                    }
                }
                catch
                {
                    if (responseStream != null) responseStream.Close();
                    if(response!=null) response.Close();
                    outputFileStream.Dispose();
                }
            }
        }
        
        private static void down()
        {
            return;
            var source = new Uri("http://www.eset.com/us/resources/white-papers/Stuxnet_Under_the_Microscope.pdf");
            MultiDownloading(source, "e:\\test.downloads.txt", 8);
            var req =WebRequest.Create(source);
            req.Method = "HEAD";
            DateTime e = DateTime.Now;
            Thread.SpinWait(1000);
            for (int i = 0; i < threads.Count; i++)
            {
                var thread = threads[i];
                if (thread == null || !thread.IsAlive) continue;
                i--;
            }
            
            var fin = DateTime.Now - e;
            fin = fin;
            using (var resp = req.GetResponse())
            {
                int ContentLength;
                var t=resp.Headers.Get("Content-Type");
                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    //Do something useful with ContentLength here 
                }
            }
        }

        public static Tree _Test(char[] args)
        {
            var b = new BasicParse {Pile = new Pile(args)};
            Update(b);
            var s = new Space(b);
            var parent = new Tree(b.Pile, null, Kind.Program);
            s.Parse(parent);
            return parent.Count != 0 ? parent[0] : null;
        }

        public static void Main(string[] args)
        {
            _Test();
        }
    }
}