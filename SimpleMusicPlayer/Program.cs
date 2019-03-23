using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SimpleMusicPlayer
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public class Music
        {
            private Music()
            {
                //singletone для того чтобы могли обращатся к одному объекту из откудого удобно
            }

            private static MediaPlayer player = null;       //брат близнец.
            public  static MediaPlayer Player
            {
                get
                {
                    if(player == null)
                    {
                       player = new MediaPlayer();
                    }
                    return player;
                }
            }
        }

        public static void DrawHelp()
        {
            Console.WriteLine("Список Команд :  \n");
            Console.WriteLine("Play or -pl  = Воспроизвести");
            Console.WriteLine("Pause or -p  = Приостановить");
            Console.WriteLine("load or -l = Подгрузить настройки");
            Console.WriteLine("save = Сохранить \\ Перезаписать настройки");
            Console.WriteLine("switch <n> or -ch <n>  = Переключить Станцию <номер>");
            Console.WriteLine("autorun  = Добавить \\ удалить из автозапуска");
            Console.WriteLine("clean    = Очистить кеш принудительно");

            Console.WriteLine("done - Выйти из панели ввода команд");
            
         
        }

        public static void Paint()
        {
            string cachepath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);


            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Title  = ("CONSOLE RADIO Ver 1.0");
            Console.WriteLine(@"
         _         _                    _   ___   _  _____ _____ _____ 
  _ __  (_) _ __  | |_  ___  _ __    __| | / _ \ / ||___ /|___ /|___  |
 | '_ \ | || '_ \ | __|/ _ \| '_ \  / _` || | | || |  |_ \  |_ \   / / 
 | | | || || | | || |_|  __/| | | || (_| || |_| || | ___) |___) | / /  
 |_| |_||_||_| |_| \__|\___||_| |_| \__,_| \___/ |_||____/|____/ /_/   
");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================================================================");
            Console.WriteLine("Для Ввода команд нажмите ENTER. Для Вывода помощи HELP или -h");
            Console.WriteLine("Стрелка вверх -------> Сделать Громче");
            Console.WriteLine("Стрелка вниз  -------> Сделать Тише");
            Console.WriteLine("Стрелка Вправо ------> След Станция");
            Console.WriteLine("Стрелка Влево  ------> Пред Станция");
            Console.WriteLine("M        ------------> Заглушиться");
            Console.WriteLine("==============================================================================");
            Console.WriteLine($"Сейчас играет :  Канал = {Setting.Manager.SelectedIndex} \n {Setting.Manager.Stations[Setting.Manager.SelectedIndex].Name} | {Music.Player.Source} \n ");
            Console.WriteLine($"Громкость Воспроизведения : {Math.Floor(Music.Player.Volume*100)}");
            Console.WriteLine($"Заглушен ? :     {YesNo(Music.Player.IsMuted)}");
            Console.WriteLine($"Размер кеш файлов : {CalculateFolderSize(cachepath)} KB");
            Console.WriteLine("==============================================================================");
        }

        public static void EnterCommand()
        {
            Console.WriteLine("Меню Ввода команд. Напишите done для Выхода из меню ввода");
            while (true)
            {
                var command = Console.ReadLine().ToLower();

                if (command == "done")
                    break;

                if (command.Contains("help") || command == "-h")
                    DrawHelp();

                if (command == "pause" || command == "-p")
                {
                    Music.Player.Pause();
                    Console.WriteLine("Paused");
                }

                if (command == "play" || command == "-pl") 
                {
                    Music.Player.Play();
                    Console.WriteLine("Resumed");
                }

                if (command == "load" || command == "-l")
                {
                    Setting.Manager.LoadSettings();
                    Console.WriteLine("Настройки загружены");
                }

                if (command == "save" || command == "-s")
                {
                    Setting.Manager.SaveSettings();        //override settings
                    Console.WriteLine("Настройки перезаписаны");
                }

                if (command.Contains("switch") || command.Contains("-ch") || command.Contains("ch"))
                {
                    if (command == "switch" || command =="-ch" || command == ("ch")) { SwitchTrack(); }

                  else
                  { 
                    Regex re = new Regex(@"\d+");
                    Match m = re.Match(command);
                    int integer = Convert.ToInt32(m.Value);
                    SwitchTrack(integer);
                  }
                }

                if(command.Contains("list") || command == "-lst" || command.Contains("lst"))
                {
                    Setting.Manager.GetStationList();
                }

                if (command.Contains("clear") || command == "-clr")
                {
                    Console.Clear();
                    Paint();
                }

                if (command == "autorun")
                {
                    MakeAutorun();
                }

                if (command == "clean" || command =="-cln")
                {
                    CleanUp();
                }
            }
        }

        private static void MakeAutorun()
        {
            bool IsAutorunned = Setting.Manager.Autoruned;
            var procname = AppDomain.CurrentDomain.FriendlyName;
            if (!IsAutorunned)
            {
                Microsoft.Win32.RegistryKey myKey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                myKey.SetValue(procname, Application.ExecutablePath + " -h");
                Console.WriteLine("Программа добавлена в автозапуск.");
                Setting.Manager.Autoruned = true;
                Setting.Manager.SaveSettings();
            }
            else
            {
                Microsoft.Win32.RegistryKey myKey =
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                myKey.DeleteValue(procname,false);
                Setting.Manager.Autoruned = false;
                Setting.Manager.SaveSettings();
                Console.WriteLine("Программа удалена из  автозапуска.");
            }
        }

        public static void SwitchTrack()
        {
            Console.Write("Channel num ? ");
            int chann = Convert.ToInt32(Console.ReadLine());
            Setting.Manager.SelectedIndex = chann;
            if (chann >= 0 && chann < Setting.Manager.Stations.Count)
            {
                Music.Player.Open(new Uri(Setting.Manager.Stations[chann].Adress));
                Music.Player.Play();
            }
            else
            {
                Console.WriteLine("Ошибка в индексе");
                return;
            }

            Setting.Manager.SaveSettings();
        }

        public static void SwitchTrack(int index)
        {
            Setting.Manager.SelectedIndex = index;
            if (index >= 0 && index < Setting.Manager.Stations.Count)
            {
                Music.Player.Open(new Uri(Setting.Manager.Stations[index].Adress));
                Music.Player.Play();
            }
            else
            {
                Console.WriteLine("Ошибка в индексе");
                return;
            }

            Setting.Manager.SaveSettings();
        }
        //public static async void Waiter()
        //{
        //     await DrawInfo();

        //}

        //public static async Task<int> DrawInfo()
        //{
        //    await new Task(()=>
        //    {
        //        Paint();
        //    });
        //    return 1;
        //}


        public static  void PullEvent()                 //обработчик нажатия клавишь в главном меню.
        {
                while (true)
                {
                    var key = Console.ReadKey();
                    // Console.WriteLine(string.Format("{0},  + Keycode \n {1}", key.Key, key.KeyChar.ToString()));

                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            Music.Player.Volume += 0.05;
                            Setting.Manager.VolumeByDefault = Music.Player.Volume;
                        // Console.WriteLine("Player Volume :" + Music.Player.Volume.ToString());
                            break;


                    case ConsoleKey.DownArrow:
                            Music.Player.Volume -= 0.05;
                           Setting.Manager.VolumeByDefault = Music.Player.Volume;
                        //       Console.WriteLine("Player Volume :" + Music.Player.Volume.ToString());
                        break;

                    case ConsoleKey.RightArrow:
                        NextChann();
                        break;
                    case ConsoleKey.LeftArrow:
                        PrevChann();
                        break;

                    case ConsoleKey.M:
                        Mute();
                        break;

                    case ConsoleKey.Enter:
                        EnterCommand();
                        break;


                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;
                    }
                         Paint();
                }                                                   //end of while loop
        }

        private static void PrevChann()
        {
            int chann = Setting.Manager.SelectedIndex;
                chann--;
            if (chann < 0)
            {
                chann = Setting.Manager.Stations.IndexOf(Setting.Manager.Stations.Last());
            }

            SwitchTrack(chann);
            Console.WriteLine($"Переключено на {Setting.Manager.Stations[chann].Name}");
        }

        private static void NextChann()
        {
            int chann = Setting.Manager.SelectedIndex;
            int lastIndex = Setting.Manager.Stations.IndexOf(Setting.Manager.Stations.Last());
            chann++;
            if (chann > lastIndex)
            {
                chann = Setting.Manager.Stations.IndexOf(Setting.Manager.Stations.First());
            }
            SwitchTrack(chann);
            Console.WriteLine($"Переключено на {Setting.Manager.Stations[chann].Name}");
        }

        public static void Mute()
        {
            bool muted = Music.Player.IsMuted;
           

            if (!muted)
            {
                Music.Player.IsMuted = true;
            }
            else
            {
                Music.Player.IsMuted = false;
            }
        }

        public static string YesNo(bool target)
        {
                string result;
                if (target) { result = "ДА"; }
                else { result = "НЕТ"; }
                return result;
        }

        static NotifyIcon notifyIcon = new NotifyIcon();
        static bool Visible = true;


        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1); //1 = SW_SHOWNORMAL           
                else ShowWindow(hWnd, 0); //0 = SW_HIDE      
            }
        }

        private static void Initialize()
        {
            Setting.Manager.LoadSettings();
            var station = new Station();
           
            station = Setting.Manager.Stations.ElementAt(Setting.Manager.SelectedIndex);
            Music.Player.Open(new Uri(station.Adress));
            Music.Player.Volume = Setting.Manager.VolumeByDefault;
            Music.Player.Play();
        }


        public static void CleanUp()
        {
            string cachepath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
            string [] dirs = Directory.GetDirectories(cachepath);

            try
            {
                foreach (string dir in dirs)
                {
                    if (dir.Contains("IE5"))
                        Directory.Delete(dir, true);
                }
                Console.WriteLine("Кеш очищен!");
            }
            catch (Exception e )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine("\n");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Кеш НЕ очищен!");
                Console.ForegroundColor = ConsoleColor.Green;          
            }
            
        }

        protected static float CalculateFolderSize(string folder)
        {
            float folderSize = 0.0f;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(folder))
                    return folderSize;
                else
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            if (File.Exists(file))
                            {
                                FileInfo finfo = new FileInfo(file);
                                folderSize += finfo.Length;
                            }
                        }

                        foreach (string dir in Directory.GetDirectories(folder))
                            folderSize += CalculateFolderSize(dir) / 1024 / 1024 ;
                    }
                    catch (NotSupportedException e)
                    {
                        Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
            }
            return folderSize ;
        }

            public static void Main(string[] args)
        {
            CleanUp();
            Initialize();    

            Task.Run(() => {

                Console.WriteLine("Running!");
                notifyIcon.DoubleClick += (s, e) =>
                {
                    Visible = !Visible;
                    SetConsoleWindowVisibility(Visible);
                };
                notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                notifyIcon.Visible = true;
                notifyIcon.Text = Application.ProductName;

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Exit", null, (s, e) => 
                {
                    Application.Exit();
                    Environment.Exit(0);
                });
                notifyIcon.ContextMenuStrip = contextMenu;

                string[] arg = Environment.GetCommandLineArgs();
                if (args.Contains("-h"))
                {
                    Visible = false;
                    SetConsoleWindowVisibility(Visible);
                }

                // Standard message loop to catch click-events on notify icon
                // Code after this method will be running only after Application.Exit()
                Application.Run();

                notifyIcon.Visible = false;
            });
           
        
            Paint();
            PullEvent();
     }

    }
}
