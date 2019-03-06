using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using WMPLib;

using System.Speech.Recognition; //for speech recognition
using System.Speech.Synthesis;
using AIMLbot;

namespace JARVIS
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine engine;
        private SpeechSynthesizer synthesizer;

        private Dictionary<string, string> cmd = new Dictionary<string, string>();

        private Bot bot;
        private User user;
        public bool search = false;
        private WindowsMediaPlayer Player;
        private static Process media;


        public void LoadSpeech()
        {
            try
            {
                #region Speech, Speak, chabot
                 
                engine = new SpeechRecognitionEngine(); // create the instance
                engine.SetInputToDefaultAudioDevice();  // define the microphone

                Player = new WMPLib.WindowsMediaPlayer();

                Choices c = new Choices();
                //c.Add(File.ReadAllLines(@"E:\Programs\C#\JARVIS\JARVIS\Grammer\lang.txt"));


                #region Customlanguage
          
                c.Add("one");
                c.Add("two");
                c.Add("naveen");
                c.Add("hello");
                c.Add("ok");
                c.Add("oh!");
                c.Add("yooo");
                c.Add("yes");
                c.Add("No");
                c.Add("cat");
                c.Add("dog");
                c.Add("what is your name?");
                c.Add("how are you");
                c.Add("do you like music");
                #endregion

                var gb = new GrammarBuilder(c);
                   var g = new Grammar(gb);

                // engine.LoadGrammar(new DictationGrammar());
                engine.LoadGrammar(g);

                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);

                engine.RecognizeAsync(RecognizeMode.Multiple);

                bot = new Bot();
                bot.loadSettings();

                user = new User("Pedro", bot);

                bot.isAcceptingUserInput = false;
                bot.loadAIMLFromFiles();
                bot.isAcceptingUserInput = true;

                synthesizer = new SpeechSynthesizer();

                #endregion

                #region Commands 

                cmd.Add("search","SearchFor");
                cmd.Add("Play", "PlayMusic");
                cmd.Add("stop", "stopMusic");
                cmd.Add("fullscreen on", "fullscreenon");
                cmd.Add("fullscreen off", "fullscreenoff");
                cmd.Add("What time is it", "WhatTime");
                cmd.Add("Tell me the time", "WhatTime");
                cmd.Add("What date is it", "WhatDate");
                cmd.Add("Tell me the date", "WhatDate");
                cmd.Add("bye", "Exit");
           

                string[] cmds = cmd.Keys.ToArray();

                Choices c_cmds = new Choices(cmds);

                GrammarBuilder gb_cmds = new GrammarBuilder();
                gb_cmds.Append(c_cmds);

                Grammar g_cmds = new Grammar(gb_cmds);
                g_cmds.Name = "cmds";

                engine.LoadGrammar(g_cmds);

                #endregion
            }
            catch (Exception e)
            {
                MessageBox.Show("Error: " + e.Message);
            }
        }

        public Form1()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech();
        }

        private void rec(object s,SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            string answer = string.Empty;
            //label1.Text = "You: " + e.Result.Text;
            label1.Text = "You: " + speech;

            

            if (e.Result.Confidence > 0.4f)
            {
                
                if(search == true)
                {
                    Process.Start("www.google.com/search?q=" + speech);
                    answer = "Here are some result for "+speech;
                    label2.Text = "Jarvis: " + answer;
                    search = false;
                    
                }
                else
                switch(e.Result.Grammar.Name)
                {
                    case "cmds":
                        try
                        {
                            string cmdType = cmd[speech];
                            if (cmdType == "SearchFor" && search == false)
                            {
                                answer = "What you like to search?";
                                search = true;
                            }
                            else
                            {
                                answer = ProcessCMD(cmdType);
                            }
                           
                        }
                        catch
                        {
                            break;
                        }
                        break;
                  
                    default:
                        answer = GetResponse(speech);
                        break;
                }
                label2.Text = "Jarvis: " + answer;
                synthesizer.SpeakAsync(answer);
            }


        }

        private string ProcessCMD(string cmdType)
        {
            string answer = string.Empty;
            
            switch(cmdType)
            {
                case "PlayMusic":
                    media = Process.Start("wmplayer.exe", @"C:\Users\naVeen\Music\Playlists\mylist.wpl");
                    answer = "Playing..";
                    break;
                case "stopMusic":
                    media.Kill();
                    break;
                case "WhatTime":
                    answer = DateTime.Now.ToShortTimeString();
                    break;
                case "WhatDate":
                    answer = DateTime.Now.ToShortDateString();
                    break;
                case "fullscreenon":
                    this.WindowState = FormWindowState.Maximized;
                    break;
                case "fullscreenoff":
                    this.WindowState = FormWindowState.Normal;
                    break;
                case "Exit":
                    System.Windows.Forms.Application.Exit();
                    answer = "Goodbyeee";
                    break;
            }
            return answer;
        }

        private string GetResponse(string query)
        {
            Request request = new Request(query, user, bot);
            Result result = bot.Chat(request);
            return result.Output;
        }

     
    }
}
