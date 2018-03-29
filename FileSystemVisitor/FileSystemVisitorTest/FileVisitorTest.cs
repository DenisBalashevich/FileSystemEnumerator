using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FileSystemVisitorTest
{
    [TestClass]
    public class FileVisitorTest
    {
        private FileSystemVisitor.FileSystemVisitor _fileSystemVisitor;
        private DirectoryTraverserTest _directoryTraverserTest;
        private readonly string testFolderPath = "D:\\New folder (3)\\Hey3\\Test";

        [TestInitialize]
        public void TestInit()
        {
            _fileSystemVisitor = new FileSystemVisitor.FileSystemVisitor();
            _directoryTraverserTest = new DirectoryTraverserTest(testFolderPath);

            string[] testDirs =
            {
                MakePath(testFolderPath, "Test"),
                MakePath(testFolderPath, "Test", "dir1")
            };

            string[] testFiles =
            {
                MakePath(testFolderPath, "Test", "dir1",
                    "file1.txt"),
                MakePath(testFolderPath, "Test", "dir1",
                    "file2.txt")
            };
            _directoryTraverserTest.Setup(testDirs, testFiles);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _directoryTraverserTest.TearDown();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFolderElementsSequence_PassNullPath_ThrowArgumentNullException()
        {
            string path = null;

            _fileSystemVisitor.GetFolderElementsSequence(path);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void GetFolderElementsSequence_PassNullPath_ThrowDirectoryNotFoundException()
        {
            string path = "Menya ne suschestvuet";

            _fileSystemVisitor.GetFolderElementsSequence(path);
        }

        [TestMethod]
        public void GetFolderElementsSequence_Collection()
        {
            var collection = _fileSystemVisitor.GetFolderElementsSequence(testFolderPath).ToArray();

            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Any(t => Directory.Exists(t.FullName)));
        }

        private static string MakePath(params string[] tokens)
        {
            var fullpath = "";
            foreach (string token in tokens)
            {
                fullpath = Path.Combine(fullpath, token);
            }
            return fullpath;
        }
    }

    public class DirectoryTraverserTest
    {
        public string TestFolderPath { get; set; }

        public DirectoryTraverserTest(string testFolderPath)
        {
            TestFolderPath = testFolderPath;
        }
        public void Setup(string[] testDirs, string[] testFiles)
        {
            Directory.CreateDirectory(TestFolderPath);

            foreach (string dir in testDirs)
            {
                Directory.CreateDirectory(dir);
            }

            foreach (string file in testFiles)
            {
                var str = File.Create(file);
                str.Close();
            }
        }

        public void TearDown()
        {
            Directory.Delete(TestFolderPath, true);
        }
    }
}
