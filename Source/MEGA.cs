using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using System.IO;
using System.IO.Compression;

namespace BackupCameras2
{
    class MEGA
    {
        static MegaApiClient client = new MegaApiClient();

        public static void Login()
        {
            Program.MEGAConnected = true;

            try
            {
                client.Login("henrique-si@hotmail.com", "123mudar");
            }catch(Exception e)
            {
                Program.Write(e.Message, ConsoleColor.Gray);
                Program.MEGAConnected = false;
            }

            if (Program.MEGAConnected)
                Program.Write("MEGA conectado com sucesso.", ConsoleColor.Green);
            else
                Program.Write("Falha ao conectar com MEGA.", ConsoleColor.Red);
        }

        public static void StartUpload()
        {
            string fileName = "";
            Program.UploadingFile = true;

            Program.Write("Sending: " + Program.zipPath, ConsoleColor.Magenta);

            DirectoryCopy(Program.Newest, Program.basePath + @"temp\", true);

            ZipFile.CreateFromDirectory(Program.basePath + @"temp\", Program.zipPath);

            FileInfo flInf = new FileInfo(Program.zipPath); 
            fileName = flInf.Name;

            var nodes = client.GetNodes();

            try
            {
                Node LastFiles = nodes.Single(n => n.Name == fileName);
                client.Delete(LastFiles, false);
            }catch(Exception e)
            {
                
            }

            try
            {  
                Node root = nodes.Single(n => n.Type == NodeType.Root);

                client.Upload(Program.zipPath, root);

                Program.Write("Arquivo enviado com sucesso.", ConsoleColor.Green);

                File.Delete(Program.zipPath);
                Directory.Delete(Program.basePath + @"temp\", true);
            }catch(Exception e)
            {
                Program.Write(e.Message, ConsoleColor.Gray);
            }

            Program.UploadingFile = false;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // pega os sub diretorios
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                return;
            }

            // se o destino não existe... cria
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // copia os arquivos
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // to iterete is human, recurse divine  
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
