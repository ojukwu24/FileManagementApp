using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Syncfusion.EJ2.FileManager.Base;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using FileManagementApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;

namespace FileManagementApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public PhysicalFileProvider operation;
        public string basePath;
        // Root Path in which files and folders are available.
        string root = "wwwroot\\root";
        public HomeController(ILogger<HomeController> logger, IHostEnvironment hostingEnvironment)
        {
            _logger = logger;

            // Map the path of the files to be accessed with the host
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            // Assign the mapped path as root folder
            this.operation.RootFolder(this.basePath + "\\" + this.root);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public object FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            // Restricting modification of the root folder
            if (args.Action == "delete" || args.Action == "rename")
            {
                if ((args.TargetPath == null) && (args.Path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    ErrorDetails er = new ErrorDetails
                    {
                        Code = "401",
                        Message = "Restricted to modify the root folder."
                    };
                    response.Error = er;
                    return this.operation.ToCamelCase(response);
                }
            }
            // Processing the File Manager operations
            switch (args.Action)
            {
                case "read":
                    // Path - Current path; ShowHiddenItems - Boolean value to show/hide hidden items
                    return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
                case "delete":
                    // Path - Current path where of the folder to be deleted; Names - Name of the files to be deleted
                    return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
                case "copy":
                    //  Path - Path from where the file was copied; TargetPath - Path where the file/folder is to be copied; RenameFiles - Files with same name in the copied location that is confirmed for renaming; TargetData - Data of the copied file
                    return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "move":
                    // Path - Path from where the file was cut; TargetPath - Path where the file/folder is to be moved; RenameFiles - Files with same name in the moved location that is confirmed for renaming; TargetData - Data of the moved file
                    return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "details":
                    // Path - Current path where details of file/folder is requested; Name - Names of the requested folders
                    return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names));
                case "create":
                    // Path - Current path where the folder is to be created; Name - Name of the new folder
                    return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
                case "search":
                    // Path - Current path where the search is performed; SearchString - String typed in the searchbox; CaseSensitive - Boolean value which specifies whether the search must be casesensitive
                    return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                case "rename":
                    // Path - Current path of the renamed file; Name - Old file name; NewName - New file name
                    var test = this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
                    return test;
            }
            return null;
        }

        // Processing the Upload operation
        public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action)
        {
            // Here we have restricted the upload operation for our online samples
            if (Response.HttpContext.Request.Host.Value == "ej2.syncfusion.com")
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 403;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File Manager's upload functionality is restricted in the online demo. If you need to test upload functionality, please install Syncfusion Essential Studio on your machine and run the demo";
            }
            // Use below code for performing upload operation
            else
            {
                FileManagerResponse uploadResponse;
                //Invoking upload operation with the required paramaters
                // path - Current path where the file is to uploaded; uploadFiles - Files to be uploaded; action - name of the operation(upload)
                uploadResponse = operation.Upload(path, uploadFiles, action, null);
            }
            return Content("");
        }
        // Processing the Download operation
        public IActionResult Download(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            //Invoking download operation with the required paramaters
            // path - Current path where the file is downloaded; Names - Files to be downloaded;
            return operation.Download(args.Path, args.Names);
        }
        // Processing the GetImage operation
        public IActionResult GetImage(FileManagerDirectoryContent args)
        {
            //Invoking GetImage operation with the required paramaters
            // path - Current path of the image file; Id - Image file id;
            return this.operation.GetImage(args.Path, args.Id, false, null, null);
        }


        public AccessDetails GetRules()
        {
            AccessDetails accessDetails = new AccessDetails();
            List<AccessRule> Rules = new List<AccessRule> {
                // FileRules For Default User
                new AccessRule { Path = "/*.*", Role = "Default User", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                // FileRules For Administrator
                new AccessRule { Path = "/*.*", Role = "Administrator", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, Download = Permission.Allow, IsFile = true },
                // FileRules For Document Manager
                new AccessRule { Path = "/*.*", Role = "Document Manager", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                new AccessRule { Path = "/Documents/*.*", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, Download = Permission.Allow, IsFile = true },
                new AccessRule { Path = "/*.png", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                new AccessRule { Path = "/Documents/*.png", Role = "Document Manager", Read = Permission.Deny, Write = Permission.Allow, Copy = Permission.Deny, Download = Permission.Allow, IsFile = true },
                new AccessRule { Path = "/2.*", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                new AccessRule { Path = "/Documents/2.*", Role = "Document Manager", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Allow, IsFile = true },
                new AccessRule { Path = "/2.png", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Deny, Download = Permission.Allow, IsFile = true },
                new AccessRule { Path = "/Documents/2.png", Role = "Document Manager", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                new AccessRule { Path = "/text", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Deny, Download = Permission.Allow, IsFile = true },
                new AccessRule { Path = "/Documents/text", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Deny, Download = Permission.Deny, IsFile = true },
                // FolderRules For Default User
                new AccessRule { Path = "*", Role = "Default User", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, WriteContents = Permission.Deny, Upload = Permission.Deny, Download = Permission.Deny, IsFile = false },
                new AccessRule { Path = "/", Role = "Default User", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Deny, WriteContents = Permission.Deny, Upload = Permission.Deny, Download = Permission.Deny, IsFile = false },
                // FolderRules For Administrator
                new AccessRule { Path = "*", Role = "Administrator", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Allow, Download = Permission.Deny, IsFile = false },
                new AccessRule { Path = "/", Role = "Administrator", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Allow, Download = Permission.Deny, IsFile = false },
                // FolderRules For Document Manager
                new AccessRule { Path = "*", Role = "Document Manager", Read = Permission.Deny, Write = Permission.Deny, Copy = Permission.Deny, WriteContents = Permission.Deny, Upload = Permission.Deny, Download = Permission.Deny, IsFile = false },
                new AccessRule { Path = "/", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Deny, WriteContents = Permission.Deny, Upload = Permission.Deny, Download = Permission.Deny, IsFile = false },
                new AccessRule { Path = "/Documents", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Deny, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Allow, Download = Permission.Deny, IsFile = false },
                new AccessRule { Path = "/Documents/*", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Allow, Download = Permission.Allow, IsFile = false },
                new AccessRule { Path = "/Music.png", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Allow, Download = Permission.Allow, IsFile = false },
                new AccessRule { Path = "/Documents/Music.png", Role = "Document Manager", Read = Permission.Allow, Write = Permission.Allow, Copy = Permission.Allow, WriteContents = Permission.Allow, Upload = Permission.Deny, Download = Permission.Deny, IsFile = false },
            };
            accessDetails.AccessRules = Rules;
            accessDetails.Role = "Document User";
            return accessDetails;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
