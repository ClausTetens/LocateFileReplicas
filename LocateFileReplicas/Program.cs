using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LocateFileReplicas {

    class FileEntry {
        public string path { get; private set; }
        public string sha256 { get; private set; }


        private string getHashSha256(string pathToBeHashed) {
            StringBuilder hashedString = new StringBuilder();
            foreach (byte hashedByte in new SHA256Managed().ComputeHash(new System.IO.FileStream(pathToBeHashed, System.IO.FileMode.Open))) {
                hashedString.Append(String.Format("{0:x2}", hashedByte));
            }
            return hashedString.ToString();
        }

        private void handleFile(string path) {
            this.path = path;
            this.sha256 = getHashSha256(path);
        }

        public FileEntry(string path) {
            handleFile(path);
        }

        public FileEntry(string path, string sha256) {
            //Console.WriteLine("Hey!!! You're using a test c'tor: FileEntry(string path, string sha256)");
            this.path = path;
            this.sha256 = sha256;
        }
    }


    class Program {
        private void createFileList(string dir, List<FileEntry> fileEntries) {
            Console.WriteLine("Processing " + dir);
            string[] files = Directory.GetFiles(dir);
            foreach(string file in files) {
                try {
                    fileEntries.Add(new FileEntry(file));
                } catch(UnauthorizedAccessException) {
                    Console.WriteLine("UnauthorizedAccessException: " + file);
                }
                catch (IOException) {
                    Console.WriteLine("IOException: " + file);
                }
            }
            string [] dirs = Directory.GetDirectories(dir);
            foreach(string d in dirs) {
                createFileList(d, fileEntries);
            }
        }

        private List<FileEntry> createDoupletEntryList(List<FileEntry> fileEntries) {
            List<FileEntry> doubletEntries = new List<FileEntry>();
            List<String> sha256List = new List<string>();
            sha256List = fileEntries.GroupBy(e => e.sha256).Where(grp => grp.Count() > 1).Select(grp => grp.Key).ToList();
            foreach(string sha256 in sha256List) {
                doubletEntries.AddRange(fileEntries.Where(e => e.sha256 == sha256));
            }
            doubletEntries.Sort((a, b) => a.sha256.CompareTo(b.sha256));
            return doubletEntries;
        }

        private void writeResultFile(String filename, List<FileEntry> doubletEntries) {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(filename)) {
                foreach (FileEntry doubletEntry in doubletEntries) {
                    file.WriteLine(doubletEntry.path);
                }
            }
        }

        private void doTheJob(string directory) {
            List<FileEntry> fileEntries = new List<FileEntry>();
            List<FileEntry> doubletEntries;
            createFileList(directory, fileEntries);
            doubletEntries = createDoupletEntryList(fileEntries);
            writeResultFile(@"D:\doublet.txt", doubletEntries);
        }

        private void testList() {
            List<FileEntry> fileEntries = new List<FileEntry>();
            fileEntries.Add(new FileEntry("a", "1"));
            fileEntries.Add(new FileEntry("b", "2"));
            fileEntries.Add(new FileEntry("x", "4"));
            fileEntries.Add(new FileEntry("c", "2"));
            fileEntries.Add(new FileEntry("d", "3"));
            fileEntries.Add(new FileEntry("y", "4"));
            fileEntries.Add(new FileEntry("e", "3"));
            fileEntries.Add(new FileEntry("f", "3"));
            fileEntries.Add(new FileEntry("z", "4"));
            fileEntries.Add(new FileEntry("4", "4"));

            List<FileEntry> doubletEntries = createDoupletEntryList(fileEntries);
            foreach(FileEntry fileEntry in doubletEntries) {
                Console.WriteLine(fileEntry.path + " " + fileEntry.sha256);
            }
            Console.ReadKey();

        }

        static void Main(string[] args) {
            //Console.WriteLine("UserName: {0}", Environment.UserName);
            //new Program().testList();
            new Program().doTheJob("D:\\");
        }
    }
}
