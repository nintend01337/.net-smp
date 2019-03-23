using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMusicPlayer
{
    [Serializable]
    public class Setting
    {
        public List<Station> Stations = new List<Station>();
        public int      SelectedIndex { get; set; } = 0;
        public double   VolumeByDefault { get; set; } = 1;           //100%
        public bool     Autoruned { get; set; } = false;

        private static Setting manager = null;          // Чайлд элемент

        public  static Setting Manager
        {
             get
             {
                if (manager == null)
                {
                    manager = new Setting();
                }
                return manager;
             }
            private set { }
        }
        private Setting()
        {
              
        } 

        public void LoadSettings()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var JsonData = JsonConvert.DeserializeObject<Setting>(json);
                    manager = JsonConvert.DeserializeObject<Setting>(json);
               
                }
                catch (Exception e)
                {

                    Console.WriteLine($"Возникла Ошибка при Загрузке Настроек. Вероятнее всего нарушена структура Файла настроек \n \n Ошибка :{e.Message}\nSTACKTRACE: {e.StackTrace}");
                    File.WriteAllText("App Crash.txt",e.Message + e.StackTrace);
                }
            }
            else                                       //если нет файла со станциями то добавим  на проигрывание шо есть
            {
              //manager.Stations.Add(new Station { Name = "", Adress = "" });
                manager.Stations.Add(new Station { Name = "Record Trap",          Adress = "http://air2.radiorecord.ru:805/trap_320" });
                manager.Stations.Add(new Station { Name = "Promo DJ Strange",     Adress = "http://radio.promodj.com:8000/strange-192" });
                manager.Stations.Add(new Station { Name = "DFM Trap",             Adress = "http://icecast.radiodfm.cdnvideo.ru/st07.mp3" });
                manager.Stations.Add(new Station { Name = "DNB RADIO",            Adress = "http://source.dnbradio.com:10128/dnbradio_main.mp3" });
                manager.Stations.Add(new Station { Name = "DNB FM",               Adress = "http://go.dnbfm.ru:8000/play" });
                manager.Stations.Add(new Station { Name = "Пиратская станция",    Adress = "http://air2.radiorecord.ru:805/ps_320" });
                manager.Stations.Add(new Station { Name = "Drop The Bass Radio",  Adress = "http://icecast.dropthebass.ru:8000/stream" });
                manager.Stations.Add(new Station { Name = "RadioEx",              Adress = "http://s2.radioheart.ru:8043/live" });
                manager.Stations.Add(new Station { Name = "Record EDM",           Adress = "http://air2.radiorecord.ru:805/club_320" });
                manager.Stations.Add(new Station { Name = "DubStep Light Radio",  Adress = "http://dubstep-light.info:8000/dubsteplight.mp3?1369207841042" });
                manager.Stations.Add(new Station { Name = "DFM CLUB",             Adress = "http://icecast.radiodfm.cdnvideo.ru/st01.mp3" });
                manager.Stations.Add(new Station { Name = "PromoDj DubStep",      Adress = "http://radio.promodj.com:8000/dubstep-192" });
                manager.Stations.Add(new Station { Name = "Record Deep",          Adress = "http://air.radiorecord.ru:805/deep_320" });
                manager.Stations.Add(new Station { Name = "DFM DEEP",             Adress = "http://icecast.radiodfm.cdnvideo.ru/st24.mp3" });
                manager.Stations.Add(new Station { Name = "Auto Bass Fm",         Adress = "http://195.242.219.208:8300/bass" });
                manager.Stations.Add(new Station { Name = "Record Vip House",     Adress = "http://air.radiorecord.ru:805/vip_320" });
                manager.Stations.Add(new Station { Name = "NewTone Fm",           Adress = "http://play.newtone.fm/live_02" });
                manager.Stations.Add(new Station { Name = "Record Electro",       Adress = "http://air.radiorecord.ru:805/elect_320" });
                manager.Stations.Add(new Station { Name = "Trap FM",              Adress = "http://stream.trap.fm:6002/;" });
                manager.Stations.Add(new Station { Name = "ElectroSpeed Radio",   Adress = "http://s2.radioboss.fm:8091/stream" });
                manager.Stations.Add(new Station { Name = "Record Hardstyle",     Adress = "http://online.radiorecord.ru:8102/teo_128" });
                manager.Stations.Add(new Station { Name = "Record Tropical",      Adress = "http://air.radiorecord.ru:805/trop_320" });
                manager.Stations.Add(new Station { Name = "Record Ibiza",         Adress = "http://air.radiorecord.ru:805/ibiza_320" });
                manager.Stations.Add(new Station { Name = "161 FM (DEEP)",        Adress = "http://stream.161fm.ru:8000/256" });
                manager.Stations.Add(new Station { Name = "Радио Рекорд",         Adress = "http://air.radiorecord.ru:805/rr_320" });

                SaveSettings();                                
            } 
        }

        public void SaveSettings()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

            JsonSerializer jser = new JsonSerializer();
            var json = JsonConvert.SerializeObject(Manager);
            using (StreamWriter sw = new StreamWriter(path))
            {
                using (JsonWriter JWriter = new JsonTextWriter(sw))
                {
                    JWriter.Formatting = Formatting.Indented;
                    JWriter.WriteCommentAsync($"\\***Auto Generated Settings****\\ \n");
                    jser.Serialize(JWriter,Manager);
                    JWriter.WriteComment($" \n Последнее изменение : {DateTime.Now} ");
                }
            }
        }

        public void GetStationList()
        {
            int index = 0;
            Console.WriteLine($"Список Станций : Всего насчитано : {Manager.Stations.Count}\n \n \n ");
            Console.WriteLine(string.Format("#ID        Название     URL Адрес "));
            foreach (Station s in Manager.Stations)
            {
                Console.WriteLine(string.Format($"{index}       {s.Name}        {s.Adress}  "));
                index++;
            }

        }
    }
}
