/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* Generated By:JavaCC: Do not edit this line. HTMLParser.java */

using System;

namespace Lucene.Net.Demo.Html
{
	
	public class HTMLParser : HTMLParserConstants_Fields
	{
		private void  InitBlock()
		{
			jj_2_rtns = new JJCalls[2];
			jj_ls = new LookaheadSuccess();
		}
		public static int SUMMARY_LENGTH = 200;
		
		internal System.Text.StringBuilder title = new System.Text.StringBuilder(SUMMARY_LENGTH);
		internal System.Text.StringBuilder summary = new System.Text.StringBuilder(SUMMARY_LENGTH * 2);
		internal System.Collections.Specialized.NameValueCollection metaTags = new System.Collections.Specialized.NameValueCollection();
		internal System.String currentMetaTag = null;
		internal System.String currentMetaContent = null;
		internal int length = 0;
		internal bool titleComplete = false;
        internal bool summaryComplete = false;
		internal bool inTitle = false;
		internal bool inMetaTag = false;
		internal bool inStyle = false;
		internal bool afterTag = false;
		internal bool afterSpace = false;
		internal System.String eol = System.Environment.NewLine;
		internal System.IO.StreamReader pipeIn = null;
		internal System.IO.StreamWriter pipeOut;
		private MyPipedInputStream pipeInStream = null;
		private System.IO.StreamWriter pipeOutStream = null;
		
		private class MyPipedInputStream : System.IO.MemoryStream
        {
            long _readPtr = 0;
            long _writePtr = 0;

            public System.IO.Stream BaseStream
            {
                get
                {
                    return this;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                lock (this)
                {
                    base.Seek(_readPtr, System.IO.SeekOrigin.Begin);
                    int x = base.Read(buffer, offset, count);
                    _readPtr += x;
                    return x;
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                lock (this)
                {
                    base.Seek(_writePtr, System.IO.SeekOrigin.Begin);
                    base.Write(buffer, offset, count);
                    _writePtr += count;
                }
            }

            public override void Close()
            {

            }

            public virtual bool Full()
            {
                return false;
            }
        }
		
		/// <deprecated> Use HTMLParser(FileInputStream) instead
		/// </deprecated>
		public HTMLParser(System.IO.FileInfo file):this(new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
		{
		}
		
		public virtual System.String GetTitle()
		{
			if (pipeIn == null)
				GetReader(); // spawn parsing thread
			while (true)
			{
				lock (this)
				{
					if (titleComplete || pipeInStream.Full())
						break;
					System.Threading.Monitor.Wait(this, TimeSpan.FromMilliseconds(10));
				}
			}
			return title.ToString().Trim();
		}
		
		public virtual System.Collections.Specialized.NameValueCollection GetMetaTags()
		{
			if (pipeIn == null)
				GetReader(); // spawn parsing thread
			while (true)
			{
				lock (this)
				{
					if (titleComplete || pipeInStream.Full())
						break;
					System.Threading.Monitor.Wait(this, TimeSpan.FromMilliseconds(10));
				}
			}
			return metaTags;
		}
		
		
		public virtual System.String GetSummary()
		{
			if (pipeIn == null)
				GetReader(); // spawn parsing thread
			while (true)
			{
				lock (this)
				{
					if (summary.Length >= SUMMARY_LENGTH || pipeInStream.Full())
						break;
					System.Threading.Monitor.Wait(this, TimeSpan.FromMilliseconds(10));
				}
			}
			if (summary.Length > SUMMARY_LENGTH)
				summary.Length = SUMMARY_LENGTH;
			
			System.String sum = summary.ToString().Trim();
			System.String tit = GetTitle();
			if (sum.StartsWith(tit) || sum.Equals(""))
				return tit;
			else
				return sum;
		}
		
		public virtual System.IO.StreamReader GetReader()
		{
			if (pipeIn == null)
			{
				pipeInStream = new MyPipedInputStream();
				pipeOutStream = new System.IO.StreamWriter(pipeInStream.BaseStream);
				pipeIn = new System.IO.StreamReader(pipeInStream.BaseStream, System.Text.Encoding.GetEncoding("UTF-16BE"));
				pipeOut = new System.IO.StreamWriter(pipeOutStream.BaseStream, System.Text.Encoding.GetEncoding("UTF-16BE"));
				
				SupportClass.ThreadClass thread = new ParserThread(this);
				thread.Start(); // start parsing
			}
			
			return pipeIn;
		}
		
		internal virtual void  AddToSummary(System.String text)
		{
			if (summary.Length < SUMMARY_LENGTH)
			{
				summary.Append(text);
				if (summary.Length >= SUMMARY_LENGTH)
				{
					lock (this)
					{
                        summaryComplete = true;
						System.Threading.Monitor.PulseAll(this);
					}
				}
			}
		}
		
		internal virtual void  AddText(System.String text)
		{
			if (inStyle)
				return ;
			if (inTitle)
				title.Append(text);
			else
			{
				AddToSummary(text);
				if (!titleComplete && !(title.Length == 0))
				{
					// finished title
					lock (this)
					{
						titleComplete = true; // tell waiting threads
						System.Threading.Monitor.PulseAll(this);
					}
				}
			}
			
			length += text.Length;
			pipeOut.Write(text);
			
			afterSpace = false;
		}
		
		internal virtual void  AddMetaTag()
		{
			metaTags[currentMetaTag] = currentMetaContent;
			currentMetaTag = null;
			currentMetaContent = null;
			return ;
		}
		
		internal virtual void  AddSpace()
		{
			if (!afterSpace)
			{
				if (inTitle)
					title.Append(" ");
				else
					AddToSummary(" ");
				
				System.String space = afterTag?eol:" ";
				length += space.Length;
				pipeOut.Write(space);
				afterSpace = true;
			}
		}
		
		public void  HTMLDocument()
		{
			Token t;
			while (true)
			{
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptStart: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.TagName: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.DeclName: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment1: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment2: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Word: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Entity: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Space: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Punct: 
						;
						break;
					
					default: 
						jj_la1[0] = jj_gen;
						goto label_1_brk;
					
				}
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.TagName: 
						Tag();
						afterTag = true;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.DeclName: 
						t = Decl();
						afterTag = true;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment1: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment2: 
						CommentTag();
						afterTag = true;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptStart: 
						ScriptTag();
						afterTag = true;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Word: 
						t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Word);
						AddText(t.image); afterTag = false;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Entity: 
						t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Entity);
						AddText(Entities.Decode(t.image)); afterTag = false;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Punct: 
						t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Punct);
						AddText(t.image); afterTag = false;
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Space: 
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Space);
						AddSpace(); afterTag = false;
						break;
					
					default: 
						jj_la1[1] = jj_gen;
						Jj_consume_token(- 1);
						throw new ParseException();
					
				}
			}

label_1_brk: ;
			
			Jj_consume_token(0);
		}
		
		public void  Tag()
		{
			Token t1, t2;
			bool inImg = false;
			t1 = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.TagName);
			System.String tagName = t1.image.ToLower();
			if (Tags.WS_ELEMS.Contains(tagName))
			{
				AddSpace();
			}
			inTitle = tagName.ToUpper().Equals("<title".ToUpper()); // keep track if in <TITLE>
			inMetaTag = tagName.ToUpper().Equals("<META".ToUpper()); // keep track if in <META>
			inStyle = tagName.ToUpper().Equals("<STYLE".ToUpper()); // keep track if in <STYLE>
			inImg = tagName.ToUpper().Equals("<img".ToUpper()); // keep track if in <IMG>
			
			while (true)
			{
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgName: 
						;
						break;
					
					default: 
						jj_la1[2] = jj_gen;
						goto label_2_brk;
					
				}
				t1 = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgName);
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgEquals: 
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgEquals);
						switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
						{
							
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgValue: 
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1: 
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2: 
								t2 = ArgValue();
								if (inImg && t1.image.ToUpper().Equals("alt".ToUpper()) && t2 != null)
									AddText("[" + t2.image + "]");
								
								if (inMetaTag && (t1.image.ToUpper().Equals("name".ToUpper()) || t1.image.ToUpper().Equals("HTTP-EQUIV".ToUpper())) && t2 != null)
								{
									currentMetaTag = t2.image.ToLower();
									if (currentMetaTag != null && currentMetaContent != null)
									{
										AddMetaTag();
									}
								}
								if (inMetaTag && t1.image.ToUpper().Equals("content".ToUpper()) && t2 != null)
								{
									currentMetaContent = t2.image.ToLower();
									if (currentMetaTag != null && currentMetaContent != null)
									{
										AddMetaTag();
									}
								}
								break;
							
							default: 
								jj_la1[3] = jj_gen;
								;
								break;
							
						}
						break;
					
					default: 
						jj_la1[4] = jj_gen;
						;
						break;
					
				}
			}

label_2_brk: ;
			
			Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.TagEnd);
		}
		
		public Token ArgValue()
		{
			Token t = null;
			switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
			{
				
				case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgValue: 
					t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgValue);
					{
						if (true)
							return t;
					}
					break;
				
				default: 
					jj_la1[5] = jj_gen;
					if (Jj_2_1(2))
					{
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1);
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote1);
						{
							if (true)
								return t;
						}
					}
					else
					{
						switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
						{
							
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1: 
								Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1);
								t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Quote1Text);
								Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote1);
								{
									if (true)
										return t;
								}
								break;
							
							default: 
								jj_la1[6] = jj_gen;
								if (Jj_2_2(2))
								{
									Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2);
									Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote2);
									{
										if (true)
											return t;
									}
								}
								else
								{
									switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
									{
										
										case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2: 
											Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2);
											t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Quote2Text);
											Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote2);
											{
												if (true)
													return t;
											}
											break;
										
										default: 
											jj_la1[7] = jj_gen;
											Jj_consume_token(- 1);
											throw new ParseException();
										
									}
								}
								break;
							
						}
					}
					break;
				
			}
			throw new System.ApplicationException("Missing return statement in function");
		}
		
		public Token Decl()
		{
			Token t;
			t = Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.DeclName);
			while (true)
			{
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgName: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgEquals: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgValue: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2: 
						;
						break;
					
					default: 
						jj_la1[8] = jj_gen;
						goto label_3_brk;
					
				}
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgName: 
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgName);
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgValue: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1: 
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2: 
						ArgValue();
						break;
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgEquals: 
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgEquals);
						break;
					
					default: 
						jj_la1[9] = jj_gen;
						Jj_consume_token(- 1);
						throw new ParseException();
					
				}
			}

label_3_brk: ;
			
			Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.TagEnd);
			{
				if (true)
					return t;
			}
			throw new System.ApplicationException("Missing return statement in function");
		}
		
		public void  CommentTag()
		{
			switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
			{
				
				case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment1: 
					Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment1);
					while (true)
					{
						switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
						{
							
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentText1: 
								;
								break;
							
							default: 
								jj_la1[10] = jj_gen;
								goto label_4_brk;
							
						}
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentText1);
					}

label_4_brk: ;
					
					Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentEnd1);
					break;
				
				case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment2: 
					Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.Comment2);
					while (true)
					{
						switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
						{
							
							case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentText2: 
								;
								break;
							
							default: 
								jj_la1[11] = jj_gen;
								goto label_5_brk;
							
						}
						Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentText2);
					}

label_5_brk: ;
					
					Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CommentEnd2);
					break;
				
				default: 
					jj_la1[12] = jj_gen;
					Jj_consume_token(- 1);
					throw new ParseException();
				
			}
		}
		
		public void  ScriptTag()
		{
			Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptStart);
			while (true)
			{
				switch ((jj_ntk == - 1)?Jj_ntk():jj_ntk)
				{
					
					case Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptText: 
						;
						break;
					
					default: 
						jj_la1[13] = jj_gen;
						goto label_6_brk;
					
				}
				Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptText);
			}

label_6_brk: ;
			
			Jj_consume_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ScriptEnd);
		}
		
		private bool Jj_2_1(int xla)
		{
			jj_la = xla; jj_lastpos = jj_scanpos = token;
			try
			{
				return !Jj_3_1();
			}
			catch (LookaheadSuccess ls)
			{
				return true;
			}
			finally
			{
				Jj_save(0, xla);
			}
		}
		
		private bool Jj_2_2(int xla)
		{
			jj_la = xla; jj_lastpos = jj_scanpos = token;
			try
			{
				return !Jj_3_2();
			}
			catch (LookaheadSuccess ls)
			{
				return true;
			}
			finally
			{
				Jj_save(1, xla);
			}
		}
		
		private bool Jj_3_1()
		{
			if (Jj_scan_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote1))
				return true;
			if (Jj_scan_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote1))
				return true;
			return false;
		}
		
		private bool Jj_3_2()
		{
			if (Jj_scan_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.ArgQuote2))
				return true;
			if (Jj_scan_token(Lucene.Net.Demo.Html.HTMLParserConstants_Fields.CloseQuote2))
				return true;
			return false;
		}
		
		public HTMLParserTokenManager token_source;
		internal SimpleCharStream jj_input_stream;
		public Token token, jj_nt;
		private int jj_ntk;
		private Token jj_scanpos, jj_lastpos;
		private int jj_la;
		public bool lookingAhead = false;
		private bool jj_semLA;
		private int jj_gen;
		private int[] jj_la1 = new int[14];
		private static int[] jj_la1_0;
		private static void  Jj_la1_0()
		{
			jj_la1_0 = new int[]{0x2c7e, 0x2c7e, 0x10000, 0x380000, 0x20000, 0x80000, 0x100000, 0x200000, 0x3b0000, 0x3b0000, 0x8000000, 0x20000000, 0x30, 0x4000};
		}
		private JJCalls[] jj_2_rtns;
		private bool jj_rescan = false;
		private int jj_gc = 0;
		
		public HTMLParser(System.IO.Stream stream):this(stream, null)
		{
		}
		public HTMLParser(System.IO.Stream stream, System.String encoding)
		{
			InitBlock();
			try
			{
				jj_input_stream = new SimpleCharStream(stream, encoding, 1, 1);
			}
			catch (System.IO.IOException e)
			{
				throw new System.Exception(e.Message, e);
			}
			token_source = new HTMLParserTokenManager(jj_input_stream);
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		public virtual void  ReInit(System.IO.Stream stream)
		{
			ReInit(stream, null);
		}
		public virtual void  ReInit(System.IO.Stream stream, System.String encoding)
		{
			try
			{
				jj_input_stream.ReInit(stream, encoding, 1, 1);
			}
			catch (System.IO.IOException e)
			{
				throw new System.Exception(e.Message, e);
			}
			token_source.ReInit(jj_input_stream);
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		public HTMLParser(System.IO.StreamReader stream)
		{
			InitBlock();
			jj_input_stream = new SimpleCharStream(stream, 1, 1);
			token_source = new HTMLParserTokenManager(jj_input_stream);
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		public virtual void  ReInit(System.IO.StreamReader stream)
		{
			jj_input_stream.ReInit(stream, 1, 1);
			token_source.ReInit(jj_input_stream);
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		public HTMLParser(HTMLParserTokenManager tm)
		{
			InitBlock();
			token_source = tm;
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		public virtual void  ReInit(HTMLParserTokenManager tm)
		{
			token_source = tm;
			token = new Token();
			jj_ntk = - 1;
			jj_gen = 0;
			for (int i = 0; i < 14; i++)
				jj_la1[i] = - 1;
			for (int i = 0; i < jj_2_rtns.Length; i++)
				jj_2_rtns[i] = new JJCalls();
		}
		
		private Token Jj_consume_token(int kind)
		{
			Token oldToken;
			if ((oldToken = token).next != null)
				token = token.next;
			else
				token = token.next = token_source.GetNextToken();
			jj_ntk = - 1;
			if (token.kind == kind)
			{
				jj_gen++;
				if (++jj_gc > 100)
				{
					jj_gc = 0;
					for (int i = 0; i < jj_2_rtns.Length; i++)
					{
						JJCalls c = jj_2_rtns[i];
						while (c != null)
						{
							if (c.gen < jj_gen)
								c.first = null;
							c = c.next;
						}
					}
				}
				return token;
			}
			token = oldToken;
			jj_kind = kind;
			throw GenerateParseException();
		}
		
		[Serializable]
		private sealed class LookaheadSuccess:System.ApplicationException
		{
		}
		
		private LookaheadSuccess jj_ls;
		private bool Jj_scan_token(int kind)
		{
			if (jj_scanpos == jj_lastpos)
			{
				jj_la--;
				if (jj_scanpos.next == null)
				{
					jj_lastpos = jj_scanpos = jj_scanpos.next = token_source.GetNextToken();
				}
				else
				{
					jj_lastpos = jj_scanpos = jj_scanpos.next;
				}
			}
			else
			{
				jj_scanpos = jj_scanpos.next;
			}
			if (jj_rescan)
			{
				int i = 0; Token tok = token;
				while (tok != null && tok != jj_scanpos)
				{
					i++; tok = tok.next;
				}
				if (tok != null)
					Jj_add_error_token(kind, i);
			}
			if (jj_scanpos.kind != kind)
				return true;
			if (jj_la == 0 && jj_scanpos == jj_lastpos)
				throw jj_ls;
			return false;
		}
		
		public Token GetNextToken()
		{
			if (token.next != null)
				token = token.next;
			else
				token = token.next = token_source.GetNextToken();
			jj_ntk = - 1;
			jj_gen++;
			return token;
		}
		
		public Token GetToken(int index)
		{
			Token t = lookingAhead?jj_scanpos:token;
			for (int i = 0; i < index; i++)
			{
				if (t.next != null)
					t = t.next;
				else
					t = t.next = token_source.GetNextToken();
			}
			return t;
		}
		
		private int Jj_ntk()
		{
			if ((jj_nt = token.next) == null)
				return (jj_ntk = (token.next = token_source.GetNextToken()).kind);
			else
				return (jj_ntk = jj_nt.kind);
		}
		
		private System.Collections.ArrayList jj_expentries = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
		private int[] jj_expentry;
		private int jj_kind = - 1;
		private int[] jj_lasttokens = new int[100];
		private int jj_endpos;
		
		private void  Jj_add_error_token(int kind, int pos)
		{
			if (pos >= 100)
				return ;
			if (pos == jj_endpos + 1)
			{
				jj_lasttokens[jj_endpos++] = kind;
			}
			else if (jj_endpos != 0)
			{
				jj_expentry = new int[jj_endpos];
				for (int i = 0; i < jj_endpos; i++)
				{
					jj_expentry[i] = jj_lasttokens[i];
				}
				bool exists = false;
				for (System.Collections.IEnumerator e = jj_expentries.GetEnumerator(); e.MoveNext(); )
				{
					int[] oldentry = (int[]) (e.Current);
					if (oldentry.Length == jj_expentry.Length)
					{
						exists = true;
						for (int i = 0; i < jj_expentry.Length; i++)
						{
							if (oldentry[i] != jj_expentry[i])
							{
								exists = false;
								break;
							}
						}
						if (exists)
							break;
					}
				}
				if (!exists)
					jj_expentries.Add(jj_expentry);
				if (pos != 0)
					jj_lasttokens[(jj_endpos = pos) - 1] = kind;
			}
		}
		
		public virtual ParseException GenerateParseException()
		{
			jj_expentries.Clear();
			bool[] la1tokens = new bool[31];
			for (int i = 0; i < 31; i++)
			{
				la1tokens[i] = false;
			}
			if (jj_kind >= 0)
			{
				la1tokens[jj_kind] = true;
				jj_kind = - 1;
			}
			for (int i = 0; i < 14; i++)
			{
				if (jj_la1[i] == jj_gen)
				{
					for (int j = 0; j < 32; j++)
					{
						if ((jj_la1_0[i] & (1 << j)) != 0)
						{
							la1tokens[j] = true;
						}
					}
				}
			}
			for (int i = 0; i < 31; i++)
			{
				if (la1tokens[i])
				{
					jj_expentry = new int[1];
					jj_expentry[0] = i;
					jj_expentries.Add(jj_expentry);
				}
			}
			jj_endpos = 0;
			Jj_rescan_token();
			Jj_add_error_token(0, 0);
			int[][] exptokseq = new int[jj_expentries.Count][];
			for (int i = 0; i < jj_expentries.Count; i++)
			{
				exptokseq[i] = (int[]) jj_expentries[i];
			}
			return new ParseException(token, exptokseq, Lucene.Net.Demo.Html.HTMLParserConstants_Fields.tokenImage);
		}
		
		public void  Enable_tracing()
		{
		}
		
		public void  Disable_tracing()
		{
		}
		
		private void  Jj_rescan_token()
		{
			jj_rescan = true;
			for (int i = 0; i < 2; i++)
			{
				try
				{
					JJCalls p = jj_2_rtns[i];
					do 
					{
						if (p.gen > jj_gen)
						{
							jj_la = p.arg; jj_lastpos = jj_scanpos = p.first;
							switch (i)
							{
								
								case 0:  Jj_3_1(); break;
								
								case 1:  Jj_3_2(); break;
								}
						}
						p = p.next;
					}
					while (p != null);
				}
				catch (LookaheadSuccess ls)
				{
				}
			}
			jj_rescan = false;
		}
		
		private void  Jj_save(int index, int xla)
		{
			JJCalls p = jj_2_rtns[index];
			while (p.gen > jj_gen)
			{
				if (p.next == null)
				{
					p = p.next = new JJCalls(); break;
				}
				p = p.next;
			}
			p.gen = jj_gen + xla - jj_la; p.first = token; p.arg = xla;
		}
		
		internal sealed class JJCalls
		{
			internal int gen;
			internal Token first;
			internal int arg;
			internal JJCalls next;
		}
		
		//    void handleException(Exception e) {
		//      System.out.println(e.toString());  // print the error message
		//      System.out.println("Skipping...");
		//      Token t;
		//      do {
		//        t = getNextToken();
		//      } while (t.kind != TagEnd);
		//    }
		static HTMLParser()
		{
			{
				Jj_la1_0();
			}
		}
	}
}
