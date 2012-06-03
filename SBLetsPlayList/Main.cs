﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.IO;
using xLogger;

namespace SBLetsPlayList
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private static List<string> LPs = new List<string>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Let's Play List Manager";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public string Author
        {
            get
            {
                return "Michael Schwarz";
            }
        }

        public string Website
        {
            get
            {
                return "";
            }
        }

        public string Description
        {
            get
            {
                return "List for managing Let's Play links";
            }
        }

        public void Dispose()
        {
            return;
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                if ( LoadList() == true )
                {
                    Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
                }
                else
                {
                    Logger.WriteLine("*** LP.TXT not found. Ignoring.", ConsoleColor.Yellow);
                }

            }
        }

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( command == "lp" )
            {
                if ( parameters.Length > 0 )
                {
                    if ( parameters[0].Trim() == "add" )
                    {
                        if ( !Host.PluginUserManager.IsOperator(name, channel) )
                        {
                            Host.PluginResponse(channel, "Only @'s can do that.");
                            return;
                        }

                        if ( parameters.Length == 3 )
                        {
                            if ( AddLp(parameters[1], parameters[2]) == true )
                            {
                                Host.PluginResponse(channel, String.Format("{0} has been added!", parameters[1]));
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("Error while adding {0}", parameters[1]));
                            }
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Invalid parameters. Usage: !lp add [name] [youtube url]");
                        }
                    }
                    else if ( parameters[0].Trim() == "del" || parameters[0].Trim() == "remove" )
                    {
                        if ( !Host.PluginUserManager.IsOperator(name, channel) )
                        {
                            Host.PluginResponse(channel, "Only @'s can do that.");
                            return;
                        }

                        if ( parameters.Length == 2 )
                        {
                            DelLp(parameters[1], channel);
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Invalid parameters. Usage: !lp remove|del [name]");
                        }
                    }
                    else
                    {
                        Host.PluginResponse(channel, GetLp(parameters[0]));
                    }
                }
                else
                {
                    Host.PluginResponse(channel, LpList());
                }
            }
        }


        #endregion

        private bool LoadList()
        {
            string input = "";

            if ( File.Exists(Host.PluginBotFolder + "\\lp.txt") )
            {
                StreamReader r = new StreamReader(Host.PluginBotFolder + "\\lp.txt");

                while ( ( input = r.ReadLine() ) != null )
                {
                    LPs.Add(input);
                }

                r.Close();
                r.Dispose();

                return true;
            }
            else
            {
                return false;
            }
        }

        private string LpList()
        {
            string response = "List of Let's Plays: ";

            foreach ( string lp in LPs )
            {
                string[] slp = lp.Split(' ');
                response = response + slp[0] + " ";
            }

            response = response + "( !lp [name] for more info )";

            return response;
        }

        private string GetLp( string name )
        {
            string lp = LPs.Find(delegate( string l )
            {
                return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( lp == null || lp.Trim().Length == 0 )
            {
                return "I don't have any information on " + name;
            }
            else
            {
                return lp;
            }
        }

        private bool AddLp( string name, string ur )
        {
            try
            {
                string newlp = String.Format("{0} {1}", name, ur);
                LPs.Add(newlp);
                LPs.Sort();

                SaveLP();

                return true;
            }
            catch ( Exception e )
            {
                Logger.WriteLine(e.Message);
                return false;
            }
        }

        private void DelLp( string name, string Channel )
        {
            try
            {
                int i = LPs.FindIndex(delegate( string l )
                {
                    return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
                });

                if ( i < 0 )
                {
                    Host.PluginResponse(Channel, "I couldn't find " + name + ". Did you check your spelling?");
                    return;
                }

                LPs.RemoveAt(i);
                SaveLP();

                Host.PluginResponse(Channel, name + " has been removed");
            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** " + e.Message, ConsoleColor.DarkRed);
                Host.PluginResponse(Channel, "Something has gone horribly wrong.");
            }
        }

        private void SaveLP()
        {
            StreamWriter w = new StreamWriter("lp.txt", false);
            foreach ( string lp in LPs )
            {
                w.WriteLine(lp);
            }

            w.Flush();
            w.Close();
            w.Dispose();
        }

    }
}