/*
* Copyright © 2017 Jesse Nicholson  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using Citadel.Core.Windows.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Te.Citadel.Services;
using Topshelf;

namespace CitadelService.Services
{
    internal class Warden : BaseProtectiveService
    {
        public Warden() : base("Sentinel", true)
        {
            
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return false;
        }

        public override void Shutdown(ExitCodes code)
        {
            // Quit our application with a safe code so that whoever
            // is watching us knows it's time to shut down.
            Environment.Exit((int)code);
        }
    }

    public class WardenProgram
    {
        private static Mutex InstanceMutex;

        static void Main(string[] args)
        {

            string appVerStr = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            System.IO.File.AppendAllText("test.log", string.Format("Our assembly location is {0}\n", assembly.Location));

            try
            {
                string name = "." + System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Version.ToString(); // OFFENDING LINE RIGHT HERE! Causes FileNotFoundException
                appVerStr += name;
            }
            catch (Exception ex)
            {
                appVerStr += ".warden_unknown_version";
                System.IO.File.AppendAllText("test.log", string.Format("exception occurred {0}\n", ex.GetType().Name));
            }

            bool createdNew;
            try
            {
                InstanceMutex = new Mutex(true, string.Format(@"Global\{0}", appVerStr.Replace(" ", "")), out createdNew); // Looks like handling this prevents Warden.exe from crashing?
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("test.log", string.Format("exception occurred while creating instance mutex {0}\n", ex.GetType().Name));
            }

            //try
            //{
                if (createdNew)
                {
                    var exitCode = HostFactory.Run(x =>
                    {
                        //try
                        //{
                            x.Service<Warden>(s =>
                            {
                                //try
                                //{
                                    s.ConstructUsing(name => new Warden());
                                    s.WhenStarted((guardian, hostCtl) => guardian.Start());
                                    s.WhenStopped((guardian, hostCtl) => guardian.Stop());

                                    s.WhenShutdown((guardian, hostCtl) =>
                                    {
                                        hostCtl.RequestAdditionalTime(TimeSpan.FromSeconds(30));
                                        guardian.Shutdown(ExitCodes.ShutdownWithSafeguards);
                                    });
                                /*}
                                catch (Exception e)
                                {
                                    System.IO.File.AppendAllText("test.log", string.Format("Looks like third section crashed. {0}\n", e.ToString()));
                                    throw e;
                                }*/

                            });

                            x.EnableShutdown();
                            x.SetDescription("Content Filtering Enforcement Service");
                            x.SetDisplayName(nameof(Warden));
                            x.SetServiceName(nameof(Warden));
                            x.StartAutomatically();
                            x.RunAsLocalSystem();

                            // We don't need recovery options, because there will be multiple
                            // services all watching eachother that will all record eachother
                            // in the event of a failure or forced termination.
                            /*
                            //http://docs.topshelf-project.com/en/latest/configuration/config_api.html#id1
                            x.EnableServiceRecovery(rc =>
                            {
                                rc.OnCrashOnly();
                                rc.RestartService(0);
                                rc.RestartService(0);
                                rc.RestartService(0);                        
                            });
                            */
                        /*}
                        catch (Exception e)
                        {
                            System.IO.File.AppendAllText("test.log", string.Format("Looks like fourth section crashed. {0}\n", e.ToString()));
                            throw e;
                        }*/


                    });
                }
                else
                {
                    Console.WriteLine("Service already running. Exiting.");

                    // We have to exit with a safe code so that if another
                    // monitor is running, it won't see this process end
                    // and then panic and try to restart it when it's already
                    // running!
                    Environment.Exit((int)ExitCodes.ShutdownWithSafeguards);
                }
            /*} catch (Exception e)
            {
                System.IO.File.AppendAllText("test.log", string.Format("Looks like second section crashed. {0}\n", e.ToString()));
                throw e;
            }*/
        }
    }
}
