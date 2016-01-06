using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.IO;
using IniParser;
using IniParser.Model;


namespace BackupCameras2
{
    class Program
    {
        public static bool MEGAConnected = false;
        public static bool UploadingFile = false;

        public static string Newest, zipPath;

        public static string basePath = @"C:\Users\Vitor\Desktop\Cameras\"; // caminho root das cameras
        static int CountCameras = 5; //numero total de cameras

        static bool isEnable = false;

        static DateTime[] LastUpdate = new DateTime[65]; //ultima data de modificação de cada camera
        static DateTime[] LastHour = new DateTime[65]; //ultima pasta de horas dentro da pasta do dia atual

        static void Main(string[] args)
        {
            //variaveis locais
            Thread tLogin = new Thread(new ThreadStart(MEGA.Login));
            System.Timers.Timer aTimer; //chamando o namespace para não dar conflito com as threads (bilbioteca)

            //lista as ultimas modificações das cameras
            FistLook();

            //busca cfg
            if (File.Exists("Config.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Config.ini");

                basePath = data["CamUpload"]["basePath"];
                CountCameras = Convert.ToInt32(data["CamUpload"]["numCameras"]);

                isEnable = true;
            }
            else
            {
                Program.Write("Arquivo de conexão não encontrado.", ConsoleColor.Red);
            }

            if (isEnable) //ve se o config foi encontrado e parseado
            {
                //inicia conexão com MEGA
                Write("Conectando ao MEGA...", ConsoleColor.Yellow);
                tLogin.Start(); //craida uma thread para não travar o programa. Meia tentativa de thread - safe

                //inica timers
                aTimer = new System.Timers.Timer(5000);

                aTimer.Elapsed += LookForUpdates;
                aTimer.Enabled = true;
            }
            Console.ReadLine(); //Para não deixar o programa fechar sozinho. Já que todo o resto está sendo feito em threads.
        }

        public static void LookForUpdates(Object source, ElapsedEventArgs e)
        {
            string NewestNew;
            Thread tUpload = new Thread(new ThreadStart(MEGA.StartUpload));

            DirectoryInfo DirInfo;
            string currentPath;

            if (!MEGAConnected || UploadingFile || !isEnable)
                return;

            for(int currentCam = 0; currentCam <= CountCameras; currentCam++)
            {
                //loop para todas as pastas com imagens
                if (currentCam < 9)
                    currentPath = basePath + @"Camera0" + currentCam.ToString();
                else
                    currentPath = basePath + @"Camera" + currentCam.ToString();

                if (!Directory.Exists(currentPath))
                    continue;

                Newest = MostRecentPath(currentPath); //pega a pasta mais recente dentro da camera atual

                DirInfo = new DirectoryInfo(Newest);
                

                if(LastUpdate[currentCam] != DirInfo.LastWriteTime) //verifica se não esta enviando o mesmo arquivo 2x
                {
                    LastUpdate[currentCam] = DirInfo.LastWriteTime; //atualiza o log
                    
                    //pega a pasta mais recente dentro da pasta mais recente
                    NewestNew = MostRecentPath(DirInfo.FullName);

                    Write("Pasta mais recente: " + NewestNew, ConsoleColor.DarkBlue);

                    /*if (currentCam < 9)
                        zipPath = basePath + @"Camera0" + currentCam.ToString() + "_" + DirInfo.Name + ".zip"; //define saida do .zip
                    else
                        zipPath = basePath + @"Camera" + currentCam.ToString() + "_" + DirInfo.Name + ".zip"; //define saida do .zip

                    tUpload.Start();
                    */
                    return; //sai do timer para inciar o upload
                }
            }
        }

        public static void FistLook()
        {
            //Esta função tem a tarefa de listar os diretorios mais novos ao iniciar o programa para evitar um upload massivo
            DirectoryInfo DirInfo;
            string currentPath, Newest;

            for(int currentCam = 0; currentCam <= CountCameras; currentCam++)
            {
                currentPath = basePath + @"Camera0" + currentCam.ToString();

                if (!Directory.Exists(currentPath))
                    continue;

                Newest = MostRecentPath(currentPath); //pega a pasta mais recente dentro da camera atual

                DirInfo = new DirectoryInfo(Newest);

                LastUpdate[currentCam] = DirInfo.LastWriteTime;
            }

        }

        public static void Write(string msg, System.ConsoleColor clr)
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static string MostRecentPath(string path)
        {
            DateTime lastHigh = new DateTime(1900, 1, 1);
            string highDir = "";
            foreach (string subdir in Directory.GetDirectories(path))
            {
                DirectoryInfo fi1 = new DirectoryInfo(subdir);
                DateTime created = fi1.LastWriteTime;

                if (created > lastHigh)
                {
                    highDir = subdir;
                    lastHigh = created;
                }
            }

            return highDir;
        }
    }
}
