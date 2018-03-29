using System;
using System.IO;

namespace FileSystemVisitor
{
    public class FilteredFileSystemElementEventArgs<T> : EventArgs where T : FileSystemInfo
    {
        public T FileSystemElement { get; set; }

        public ActionType ActionType { get; set; }
    }
}
