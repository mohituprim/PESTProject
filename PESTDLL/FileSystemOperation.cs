using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FactSet.Partners.PESTDLL
{
    class FileSystemOperation
    {
        public string CopyDirectory(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
            destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";
            try
            {
                DirectoryInfo sourceDirInfo = new DirectoryInfo(sourcePath);
                DirectoryInfo destinationDirInfo = new DirectoryInfo(destinationPath);
                // Check source directory exists or not
                if (Directory.Exists(sourceDirInfo.FullName) == false)
                {
                    throw new DirectoryNotFoundException("source directory does not exists");
                }
                // Check if the target directory exists; if true, remove it first.
                string targetDirPath = Path.Combine(destinationDirInfo.FullName, sourceDirInfo.Name);
                DirectoryInfo diTargetDir = new DirectoryInfo(targetDirPath);
                if (Directory.Exists(diTargetDir.FullName) == true)
                {
                    diTargetDir.Attributes &= ~FileAttributes.ReadOnly;
                    Directory.Delete(diTargetDir.FullName, true);
                }
                // Create zip file at target
                string zipFilePath = Path.Combine(destinationDirInfo.FullName, sourceDirInfo.Name + ".zip");
                if (File.Exists(zipFilePath) == true)
                {
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(sourceDirInfo.FullName, zipFilePath);
                string extractPath = Path.Combine(destinationDirInfo.FullName, sourceDirInfo.Name);
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                // Remove zip file
                File.Delete(zipFilePath);
                return sourceDirInfo.Name;
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Unable to copy the directory", ex);
            }
        }

        public void RemoveDirectory(string targetDirPath)
        {
            targetDirPath = targetDirPath.EndsWith(@"\") ? targetDirPath : targetDirPath + @"\";
            DeleteDirectory(targetDirPath);
            return;
        }

        public bool CopyFolderContents(string sourcePath, string destinationPath)
        {
            sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
            destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";

            try
            {
                if (Directory.Exists(sourcePath))
                {
                    if (Directory.Exists(destinationPath) == false)
                    {
                        DirectorySecurity securityRules = new DirectorySecurity();
                        IdentityReference everybodyIdentity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                        FileSystemAccessRule rule = new FileSystemAccessRule(
                        everybodyIdentity,
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow);
                        securityRules.AddAccessRule(rule);
                        Directory.CreateDirectory(destinationPath, securityRules);
                    }

                    foreach (string file in Directory.GetFiles(sourcePath))
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        fileInfo.CopyTo(string.Format(@"{0}\{1}", destinationPath, fileInfo.Name), true);
                    }

                    foreach (string directory in Directory.GetDirectories(sourcePath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                        if (CopyFolderContents(directory, destinationPath + directoryInfo.Name) == false)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Unable to copy the directory" + sourcePath, ex);
            }
        }

        public void RenameDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            oldDirectoryPath = oldDirectoryPath.EndsWith(@"\") ? oldDirectoryPath : oldDirectoryPath + @"\";
            newDirectoryPath = newDirectoryPath.EndsWith(@"\") ? newDirectoryPath : newDirectoryPath + @"\";
            try
            {
                DeleteDirectory(newDirectoryPath);
                this.CopyFolderContents(oldDirectoryPath, newDirectoryPath);
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Exception caught while renaming directory", ex);
            }
        }

        public void ClearRepository(string environmentRepositoryPath)
        {
            environmentRepositoryPath = environmentRepositoryPath.EndsWith(@"\") ? environmentRepositoryPath : environmentRepositoryPath + @"\";
            environmentRepositoryPath = Path.Combine(environmentRepositoryPath, "Repository");
            if (Directory.Exists(environmentRepositoryPath) == true)
            {
                DirectoryInfo diRepository = new DirectoryInfo(environmentRepositoryPath);
                foreach (FileInfo file in diRepository.GetFiles())
                {
                    file.Attributes &= ~FileAttributes.Normal;
                    file.Delete();
                }
            }
            else
            {
                throw new InvalidOperationException("Repository does not exist");
            }
        }

        private static void DeleteDirectory(string targetDirPath)
        {
            targetDirPath = targetDirPath.EndsWith(@"\") ? targetDirPath : targetDirPath + @"\";
            DirectoryInfo diTargetDir = new DirectoryInfo(targetDirPath);
            try
            {

                if (Directory.Exists(diTargetDir.FullName) == true)
                {
                    DirectorySecurity securityRules = new DirectorySecurity();
                    IdentityReference everybodyIdentity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                    FileSystemAccessRule rule = new FileSystemAccessRule(
                    everybodyIdentity,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);
                    securityRules.AddAccessRule(rule);
                    Directory.SetAccessControl(diTargetDir.FullName, securityRules);
                    Directory.Delete(diTargetDir.FullName, true);
                }
            }
            catch (IOException)
            {
                diTargetDir.Refresh();
                Directory.Delete(diTargetDir.FullName, true);
            }
            catch (UnauthorizedAccessException)
            {
                diTargetDir.Refresh();
                Directory.Delete(diTargetDir.FullName, true);
            }
            catch (Exception ex)
            {
                throw new System.InvalidOperationException("{0} Unable to Remove the directory" + diTargetDir.FullName, ex);

            }
        }
    }
}