namespace TTtuner_2022_2.Common
{

    // this class was created to represent info about a file for both scoped and non scoped storage 
    // is used on the list adapter that lists files in a dir
    internal class FileInfoItem
    {
        public FileInfoItem(string name, string fullPath, bool isDirectory = false)
        {
            Name = name;
            FullPath = fullPath;
            IsDirectory = isDirectory;
        }

        public string Name { get; set; }
        public string FullPath { get; set; }

        public bool IsDirectory { get;  }



    }


}