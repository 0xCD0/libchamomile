#region Using
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
#endregion

namespace libchamomile.Net {
    public static class FTP {
        #region 로컬 변수
        private static string _hostname;

        public static string Hostname {
            get {
                if (_hostname.StartsWith("ftp://")) {
                    return _hostname;
                }
                else {
                    return "ftp://" + _hostname;
                }
            }
            set {
                _hostname = value;
            }
        }

        private static string _username;
        public static string Username {
            get {
                return (_username == "" ? "anonymous" : _username);
            }
            set {
                _username = value;
            }
        }

        private static string _password;
        public static string Password {
            get {
                return _password;
            }
            set {
                _password = value;
            }
        }

        private static string _currentDirectory = "/";
        public static string CurrentDirectory {
            get {
                return _currentDirectory + ((_currentDirectory.EndsWith("/")) ? "" : "/").ToString();
            }
            set {
                if (!value.StartsWith("/")) {
                    throw (new ApplicationException("Directory should start with /"));
                }
                _currentDirectory = value;
            }
        }
        #endregion

        #region Public 내부 함수
        /// <summary>
        /// FTP에 접속하기 위해 ID와 Password 정보를 기입하는 함수입니다. FTP 사용전에 반드시 호출하여 입력해야합니다.
        /// </summary>
        /// <param name="FTPHostIP">FTP의 주소를 입력합니다. 192.168.0.1과 같은 형태로 입력합니다.</param>
        /// <param name="ID">FTP에서 사용되는 ID를 입력합니다.</param>
        /// <param name="Password">FTP에서 사용되는 Password를 입력합니다.</param>
        public static void SetFTPAccount(string FTPHostIP, string ID, string Password) {
            _hostname = FTPHostIP;
            _username = ID;
            _password = Password;
        }

        /// <summary>
        /// 디렉토리의 리스트를 Generic으로 출력합니다.
        /// </summary>
        /// <param name="directory">디렉토리 이름을 입력합니다.</param>
        /// <returns>디렉토리 List<string>을 반환합니다.</returns>
        public static List<string> ListDirectory(string directory) {
            System.Net.FtpWebRequest ftp = GetRequest(GetDirectory(directory));
            ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectory;

            string str = GetStringResponse(ftp);
            str = str.Replace("\r\n", "\r").TrimEnd('\r');

            List<string> result = new List<string>();
            result.AddRange(str.Split('\r'));
            return result;
        }

        /// <summary>
        /// FTP에 파일을 업로드 합니다.
        /// </summary>
        /// <param name="localFilename">로컬 파일의 상대 경로를 입력합니다.</param>
        /// <param name="targetFilename">파일을 저장할 FTP 경로를 입력합니다. 파일 이름까지 같이 입력하여야 합니다.</param>
        /// <returns>성공 시 true를, 실패시 false를 반환합니다.</returns>
        public static bool Upload(string localFilename, string targetFilename) {
            if (!File.Exists(localFilename)) {
                throw (new ApplicationException("File " + localFilename + " not found"));
            }

            FileInfo fi = new FileInfo(localFilename);
            return Upload(fi, targetFilename);
        }

        public static bool Upload(FileInfo fi, string targetFilename) {
            string target;
            if (targetFilename.Trim() == "") {
                target = CurrentDirectory + fi.Name;
            }
            else if (targetFilename.Contains("/")) {
                target = AdjustDir(targetFilename);
            }
            else {
                target = CurrentDirectory + targetFilename;
            }

            string URI = Hostname + target;
            System.Net.FtpWebRequest ftp = GetRequest(URI);

            ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            ftp.UseBinary = true;

            ftp.ContentLength = fi.Length;

            const int BufferSize = 2048;
            byte[] content = new byte[BufferSize - 1 + 1];
            int dataRead;

            using (FileStream fs = fi.OpenRead()) {
                try {
                    using (Stream rs = ftp.GetRequestStream()) {
                        do {
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                        } while (!(dataRead < BufferSize));
                        rs.Close();
                    }
                }
                catch (Exception) {

                }
                finally {
                    fs.Close();
                }

            }
            ftp = null;
            return true;
        }

        /// <summary>
        /// FTP에서 파일을 다운로드합니다.
        /// </summary>
        /// <param name="sourceFilename">FTP에서 전송받을 파일 경로를 입력합니다.</param>
        /// <param name="localFilename">로컬 영역에 저장될 경로를 입력합니다.</param>
        /// <param name="PermitOverwrite">만약 중복된 파일이 존재할 경우 덮어씌우기를 할지 bool 형태로 입력합니다. True를 입력하면 중복 파일에 덮어씁니다.</param>
        /// <returns>성공 시 true를, 실패 시 false를 반환합니다.</returns>
        public static bool Download(string sourceFilename, string localFilename, bool PermitOverwrite) {
            FileInfo fi = new FileInfo(localFilename);
            return Download(sourceFilename, fi, PermitOverwrite);
        }

        public static bool Download(string sourceFilename, FileInfo targetFI, bool PermitOverwrite) {
            if (targetFI.Exists && !(PermitOverwrite)) {
                throw (new ApplicationException("Target file already exists"));
            }

            string target;
            if (sourceFilename.Trim() == "") {
                throw (new ApplicationException("File not specified"));
            }
            else if (sourceFilename.Contains("/")) {
                target = AdjustDir(sourceFilename);
            }
            else {
                target = CurrentDirectory + sourceFilename;
            }

            string URI = Hostname + target;

            System.Net.FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
            ftp.UseBinary = true;

            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse()) {
                using (Stream responseStream = response.GetResponseStream()) {
                    using (FileStream fs = targetFI.OpenWrite()) {
                        try {
                            byte[] buffer = new byte[2048];
                            int read = 0;
                            do {
                                read = responseStream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, read);
                            } while (!(read == 0));
                            responseStream.Close();
                            fs.Flush();
                            fs.Close();
                        }
                        catch (Exception) {
                            fs.Close();
                            targetFI.Delete();
                            throw;
                        }
                    }
                    responseStream.Close();
                }
                response.Close();
            }
            return true;
        }

        /// <summary>
        /// FTP에서 파일을 삭제합니다.
        /// </summary>
        /// <param name="filename">삭제할 파일 이름을 입력합니다.</param>
        /// <returns>성공시 true, 실패시 false를 반환합니다.</returns>
        public static bool FTPDelete(string filename) {
            string URI = Hostname + GetFullPath(filename);

            System.Net.FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
            try {
                string str = GetStringResponse(ftp);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// FTP에서 해당 파일의 존재 유무를 확인합니다.
        /// </summary>
        /// <param name="filename">파일 이름을 입력합니다.</param>
        /// <returns>성공 시 true, 실패 시 false를 반환합니다.</returns>
        public static bool FTPFileExists(string filename) {
            try {
                long size = GetFileSize(filename);
                return true;

            }
            catch (Exception ex) {
                if (ex is System.Net.WebException) {
                    if (ex.Message.Contains("550")) {
                        return false;
                    }
                    else {
                        throw;
                    }
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// 파일 크기를 얻습니다.
        /// </summary>
        /// <param name="filename">파일 이름을 입력합니다.</param>
        /// <returns>성공 시 해당 파일의 크기를 리턴, 실패 시 null을 리턴합니다.</returns>
        public static long GetFileSize(string filename) {
            string path;
            if (filename.Contains("/")) {
                path = AdjustDir(filename);
            }
            else {
                path = CurrentDirectory + filename;
            }
            string URI = Hostname + path;
            System.Net.FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = System.Net.WebRequestMethods.Ftp.GetFileSize;
            string tmp = GetStringResponse(ftp);
            return GetSize(ftp);
        }

        /// <summary>
        /// FTP 서버 내의 파일 이름을 변경합니다.
        /// </summary>
        /// <param name="sourceFilename">대상 파일의 경로를 입력합니다.</param>
        /// <param name="newName">대상 파일의 경로와 함께 새로운 이름을 입력합니다.</param>
        /// <returns>성공 시 true를, 실패시 false를 리턴합니다.</returns>
        public static bool FTPRename(string sourceFilename, string newName) {
            string source = GetFullPath(sourceFilename);
            if (!FTPFileExists(source)) {
                throw (new FileNotFoundException("File " + source + " not found"));
            }

            string target = GetFullPath(newName);
            if (target == source) {
                throw (new ApplicationException("Source and target are the same"));
            }
            else if (FTPFileExists(target)) {
                throw (new ApplicationException("Target file " + target + " already exists"));
            }

            string URI = Hostname + source;

            System.Net.FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = System.Net.WebRequestMethods.Ftp.Rename;
            ftp.RenameTo = target;

            try {
                string str = GetStringResponse(ftp);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        #endregion

        #region 내부 함수
        private static FtpWebRequest GetRequest(string URI) {
            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
            result.Credentials = GetCredentials();
            result.KeepAlive = false;

            return result;
        }

        private static ICredentials GetCredentials() {
            return new System.Net.NetworkCredential(Username, Password);
        }

        private static string GetFullPath(string file) {
            if (file.Contains("/")) {
                return AdjustDir(file);
            }
            else {
                return CurrentDirectory + file;
            }
        }

        private static string AdjustDir(string path) {
            return ((path.StartsWith("/")) ? "" : "/").ToString() + path;
        }

        private static string GetDirectory(string directory) {
            string URI;
            if (directory == "") {
                URI = Hostname + CurrentDirectory;
                _lastDirectory = CurrentDirectory;
            }
            else {
                if (!directory.StartsWith("/")) {
                    throw (new ApplicationException("Directory should start with /"));
                }
                URI = Hostname + directory;
                _lastDirectory = directory;
            }
            return URI;
        }

        private static string _lastDirectory = "";

        private static string GetStringResponse(FtpWebRequest ftp) {
            string result = "";
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse()) {
                long size = response.ContentLength;
                using (Stream datastream = response.GetResponseStream()) {
                    using (StreamReader sr = new StreamReader(datastream)) {
                        result = sr.ReadToEnd();
                        sr.Close();
                    }

                    datastream.Close();
                }

                response.Close();
            }

            return result;
        }

        private static long GetSize(FtpWebRequest ftp) {
            long size;
            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse()) {
                size = response.ContentLength;
                response.Close();
            }

            return size;
        }

        private static void DirectoryChecker(string path) {
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists == false) {
                info.Create();
            }
        }
        #endregion
    }
}
