using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystemVisitor
{
    public class FileSystemVisitor
    {
        private readonly Func<FileSystemInfo, bool> _filter;

        public event EventHandler<EventArgs> Start;
        public event EventHandler<EventArgs> Finish;
        public event EventHandler<FileSystemElementEventArgs<FileInfo>> FileFound;
        public event EventHandler<FileSystemElementEventArgs<DirectoryInfo>> DirectoryFound;
        public event EventHandler<FilteredFileSystemElementEventArgs<FileInfo>> FilteredFileFound;
        public event EventHandler<FilteredFileSystemElementEventArgs<DirectoryInfo>> FilteredDirectoryFound;

        public FileSystemVisitor() { }

        public FileSystemVisitor(Func<FileSystemInfo, bool> filter)
        {
            _filter = filter;
        }

        public IEnumerable<FileSystemInfo> GetFolderElementsSequence(string root)
        {
            if (String.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return null;
            }

            return VisitFileSystem(root);
        }

        private IEnumerable<FileSystemInfo> VisitFileSystem(string root)
        {
            OnEvent(Start, new EventArgs());

            foreach (var item in VisitDirectory(root))
            {
                yield return item;
            }

            OnEvent(Finish, new EventArgs());
        }

        private IEnumerable<FileSystemInfo> VisitDirectory(string root)
        {
            foreach (var file in VisitFiles(root))
            {
                yield return file;
            }

            foreach (var subDirectory in Directory.EnumerateDirectories(root))
            {
                var directoryInfo = new DirectoryInfo(subDirectory);
                var directoryEventArgs = new FileSystemElementEventArgs<DirectoryInfo> { FileSystemElement = directoryInfo };
                OnEvent(DirectoryFound, directoryEventArgs);

                if (directoryEventArgs.ActionType == ActionType.StopSearch)
                {
                    yield break;
                }

                if (directoryEventArgs.ActionType != ActionType.ExcludeElement && _filter == null || _filter(directoryInfo))
                {
                    var filteredDirectoryEventArgs = new FilteredFileSystemElementEventArgs<DirectoryInfo> { FileSystemElement = directoryInfo };
                    OnEvent(FilteredDirectoryFound, filteredDirectoryEventArgs);

                    if (filteredDirectoryEventArgs.ActionType == ActionType.StopSearch)
                    {
                        yield break;
                    }

                    if (filteredDirectoryEventArgs.ActionType != ActionType.ExcludeElement)
                    {
                        yield return directoryInfo;
                    }
                }

                foreach (var item in VisitDirectory(subDirectory))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<FileSystemInfo> VisitFiles(string root)
        {
            foreach (var file in Directory.EnumerateFiles(root))
            {
                var fileInfo = new FileInfo(file);
                var fileEventArgs = new FileSystemElementEventArgs<FileInfo> { FileSystemElement = fileInfo };
                OnEvent(FileFound, fileEventArgs);

                if (fileEventArgs.ActionType == ActionType.StopSearch)
                {
                    yield break;
                }

                if (fileEventArgs.ActionType != ActionType.ExcludeElement && _filter == null || _filter(fileInfo))
                {
                    var filteredFileEventArgs = new FilteredFileSystemElementEventArgs<FileInfo> { FileSystemElement = fileInfo };
                    OnEvent(FilteredFileFound, filteredFileEventArgs);

                    if (filteredFileEventArgs.ActionType == ActionType.StopSearch)
                    {
                        yield break;
                    }

                    if (filteredFileEventArgs.ActionType != ActionType.ExcludeElement)
                    {
                        yield return fileInfo;
                    }
                }

            }
        }

        private void OnEvent<TArgs>(EventHandler<TArgs> someEvent, TArgs args)
        {
            someEvent?.Invoke(this, args);
        }
    }
}
