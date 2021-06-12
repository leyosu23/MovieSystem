using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MovieSystem.Models;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;

namespace MovieSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static readonly string bucketName = "yoonseop";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast2;
        private static readonly string accesskey = "AKIAVGK3U2NUBRNMBIPA";
        private static readonly string secretkey = "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";
        private static readonly string keyName = "Movie.csv";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DownloadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(IFormFile file)
        {
            var s3Client = new AmazonS3Client(accesskey, secretkey, bucketRegion);

            var fileTransferUtility = new TransferUtility(s3Client);
            try
            {
                if (file.Length > 0)
                {
                    var filePath = "C:\\Users\\User\\Desktop\\Movie.csv";
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        FilePath = filePath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.  
                        Key = keyName,
                        CannedACL = S3CannedACL.PublicRead
                    };
                    fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                    fileTransferUtilityRequest.Metadata.Add("param2", "Value2");
                    fileTransferUtility.Upload(fileTransferUtilityRequest);
                    fileTransferUtility.Dispose();
                }
                ViewBag.Message = "File Uploaded Successfully!";
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    ViewBag.Message = "Check the provided AWS Credentials.";
                }
                else
                {
                    ViewBag.Message = "Error occurred: " + amazonS3Exception.Message;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DownloadFile(IFormFile file)
        {
            var s3Client = new AmazonS3Client(bucketRegion);
            var filePath = "C:\\Users\\User\\Downloads\\Movie.csv";

            TransferUtility utility = new TransferUtility(s3Client);
            TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest();

            request.BucketName = bucketName;
            request.Key = keyName;
            request.FilePath = filePath;

            utility.Download(request);

            return RedirectToAction("Index");
        }
    }
}
