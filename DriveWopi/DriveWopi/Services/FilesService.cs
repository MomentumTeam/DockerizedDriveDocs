﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DriveWopi.Models;
using DriveWopi.Exceptions;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

namespace DriveWopi.Services
{
    public class FilesService
    {

        public static string GenerateAuthorizationToken(string userId)
        {
            //TODO

           // return "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjVlNTY4ODMyNDIwM2ZjNDAwNDM1OTFhYSIsImFkZnNJZCI6InQyMzQ1ODc4OUBqZWxsby5jb20iLCJnZW5lc2lzSWQiOiI1ZTU2ODgzMjQyMDNmYzQwMDQzNTkxYWEiLCJuYW1lIjp7ImZpcnN0TmFtZSI6Iteg15nXmden15kiLCJsYXN0TmFtZSI6IteQ15PXmdeT16EifSwiZGlzcGxheU5hbWUiOiJ0MjM0NTg3ODlAamVsbG8uY29tIiwicHJvdmlkZXIiOiJHZW5lc2lzIiwiZW50aXR5VHlwZSI6ImRpZ2ltb24iLCJjdXJyZW50VW5pdCI6Im5pdHJvIHVuaXQiLCJkaXNjaGFyZ2VEYXkiOiIyMDIyLTExLTMwVDIyOjAwOjAwLjAwMFoiLCJyYW5rIjoibWVnYSIsImpvYiI6Iteo15XXpteXIiwicGhvbmVOdW1iZXJzIjpbIjA1Mi0xMjM0NTY3Il0sImFkZHJlc3MiOiLXqNeX15XXkSDXlNee157Xqten15nXnSAzNCIsInBob3RvIjpudWxsLCJqdGkiOiJmZjQ2ODJjMy1lZDI3LTRkODItYmFjNi1iZWFhYTgzNDVmNzAiLCJpYXQiOjE1OTIyMzAxOTUsImV4cCI6MTU5NDgyMjE5NSwiZmlyc3ROYW1lIjoi16DXmdeZ16fXmSIsImxhc3ROYW1lIjoi15DXk9eZ15PXoSJ9.4caytinSKCQMDGSHq0p0Fl--NypOOiKC8Df5W7RCPkY";
            return Config.AuthorizationToken;
        
        
        }

        public async static Task<string> getUploadId(FileInfo fileInfo, string authorization, string fileId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string uploadId = "";
                    httpClient.DefaultRequestHeaders.Add("X-Content-Length", fileInfo.Length.ToString());
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    // var fileId = fileInfo.Name.ToString();
                    Console.WriteLine("fileid"+fileId);
                    // int index = fileId.IndexOf(".");
                    // if (index > 0)
                    // {
                    //     fileId = fileId.Substring(0, index);
                    // }
                    if (!Config.Mimetypes.ContainsKey(fileInfo.Extension.ToString().ToLower()))
                    {
                        throw new Exception();
                    }
                    var body = new { title = fileInfo.Name.ToString(), mimeType = Config.Mimetypes[fileInfo.Extension.ToString().ToLower()] };
                    // Construct the JSON to Put.
                    string convertedBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings());
                    HttpContent content = new StringContent(convertedBody, System.Text.Encoding.UTF8, "application/json");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, Config.DriveUrl + "/api/upload/"+fileId);
                    request.Headers.Add("X-Content-Length", fileInfo.Length.ToString());
                    request.Headers.Add("Authorization", authorization);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Content = content;
                    var response = await httpClient.SendAsync(request);
                    HttpHeaders headers = response.Headers;
                    IEnumerable<string> values;
                    if (headers.TryGetValues("X-Uploadid", out values))
                    {
                        uploadId = values.First();
                    }
                    else
                    {
                        throw new Exception();
                    }
                    Console.WriteLine(uploadId);
                    return uploadId;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async static void UpdateFileInDrive(FileInfo fileInfo, string authorization, string fileId)
        {
            try
            {
                string uploadId = await getUploadId(fileInfo, authorization, fileId);
                Console.WriteLine("ok");
                using (var client = new WebClient())
                {
                    Console.WriteLine(fileId);
                    client.Headers.Set("Authorization", authorization);
                    client.Headers.Set("Content-Range", "bytes 0-" + (fileInfo.Length - 1) + "/" + fileInfo.Length);
                    Console.WriteLine(uploadId + "");
                    byte[] responseArray = client.UploadFile(Config.DriveUrl + "/api/upload?uploadType=resumable&uploadId=" + uploadId, fileInfo.FullName);
                    Console.WriteLine("\nResponse Received. The contents of the file uploaded are:\n{0}", System.Text.Encoding.ASCII.GetString(responseArray));
                    Console.WriteLine("UpdateFileInDrive");
                    // return true;
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        public static void DownloadFileFromDrive(string idToDownload, string localPath, string authorization)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Set("Authorization", authorization);
                    //client.Encoding = Encoding.UTF8;
                    //string fileName = Config.Folder+"\\"+localPath;
                    //TODO download from Drive Service
                    //byte[] data = client.DownloadData(Config.DriveServiceUrl+"/files/download/" + idToDownload);//, Config.Folder+"/"+name);
                    client.DownloadFile(Config.DriveUrl + "/api/files/" + idToDownload + "?alt=media", localPath);
                    // BinaryWriter writer = new BinaryWriter(File.OpenWrite(fileName));     
                    //  writer.Write(data);
                    //  writer.Flush();
                    //  writer.Close();
                    //File.WriteAllBytes(Config.Folder+"/"+name,data);
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public static void UpdateFileInDrive(string idToUpdate, string localPath, string authorization)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Set("Authorization", authorization);

                    //client.Encoding = Encoding.UTF8;
                    // string fileName = Config.Folder+"\\"+localPath;
                    //TODO download from Drive Service
                    //byte[] data = client.DownloadData(Config.DriveServiceUrl+"/files/download/" + idToDownload);//, Config.Folder+"/"+name);
                    //client.DownloadFile("http://atan-drv.northeurope.cloudapp.azure.com/api/files/"+idToDownload+"?alt=media", fileName);
                    // BinaryWriter writer = new BinaryWriter(File.OpenWrite(fileName));     
                    //  writer.Write(data);
                    //  writer.Flush();
                    //  writer.Close();
                    //File.WriteAllBytes(Config.Folder+"/"+name,data);
                    Console.WriteLine("hello");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CopyTemplate(string template, string id)
        {
            try
            {
                string source = source = Config.TemplatesFolder + "/" + template;
                string dest = Config.Folder + "/" + id;
                File.Copy(source, dest, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void CreateBlankFile(string path)
        {
            try
            {
                string type = path.Substring(path.LastIndexOf(".") + 1, path.Length - path.LastIndexOf(".") - 1).ToLower();
                string source;
                string dest = path;
                switch (type)
                {
                    case "docx":
                        source = Config.TemplatesFolder + "/blankDocx.docx";
                        break;
                    case "xlsx":
                        source = Config.TemplatesFolder + "/blankXlsx.xlsx";
                        break;
                    default:
                        source = Config.TemplatesFolder + "/blankDocx.docx";
                        break;
                }
                File.Copy(source, dest, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Save(string id, byte[] newContent)
        {
            try
            {
                string filePath = GeneratePath(id);
                FileInfo fileInfo = new FileInfo(filePath);
                using (FileStream fileStream = fileInfo.Open(FileMode.Truncate))
                {
                    fileStream.Write(newContent, 0, newContent.Length);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static byte[] GetByteArrayFromStream(Stream input)
        {
            try
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] GetFileContent(string filePath)
        {
            try
            {
                if (!FileExists(filePath))
                {
                    throw new DriveFileNotFoundException(filePath);
                }
                //string filePath = GeneratePath(id);
                MemoryStream ms = new MemoryStream();
                FileInfo fileInfo = new FileInfo(filePath);
                lock (fileInfo)
                {
                    using (FileStream fileStream = fileInfo.OpenRead())
                    {
                        fileStream.CopyTo(ms);
                    }
                }
                ms.Position = 0;
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static bool FileExists(string filePath)
        {
            try
            {
                //string filePath = GeneratePath(id);
                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void CreateEmptyFile(string path)
        {
            File.Create(path).Dispose();
        }


        public static string GeneratePath(string id)
        {
            return Config.Folder + "/" + id;
        }
    }
}
