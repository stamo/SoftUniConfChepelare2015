using System;
using System.Configuration;

// Needed for Azure Blob access
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureBlobDemo
{
    /// <summary>
    /// Author: Tamra Myers
    /// Source: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/
    /// Modified by: Stamo Petkov
    /// Date modified: 30.10.2015
    /// SoftUni conference in Chepelare
    /// </summary>
    class PhotoStorage
    {
        private static CloudStorageAccount storageAccount;
        private static CloudBlobClient blobClient;
        private static CloudBlobContainer container;
        private const string ImagePath = @"G:\Pictures\LegionRun\DSC01432.JPG";
        private const string CopyImagePath = @"G:\Downloads\AzureBlobDownloadedCopy.JPG";

        static void Main(string[] args)
        {
            try
            {
                GetStartedDemo();
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }

        private static void GetStartedDemo()
        {
            storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);

            blobClient = storageAccount.CreateCloudBlobClient();

            CreatePublicContainer("photos");

            UploadImage(ImagePath, "DSC01432.JPG");

            DownLoadImage(CopyImagePath, "DSC01432.JPG");

            DeleteContainer();
        }

        /// <summary>
        /// Create the container if it doesn't already exist
        /// and changes its permissions to public.
        /// </summary>
        /// <param name="name">Name of the container. 
        /// Must be a valid DNS name</param>
        private static void CreatePublicContainer(string name)
        {
            container = blobClient.GetContainerReference(name);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            // By default, the new container is private and you must 
            // specify your storage access key to download blobs
            container.SetPermissions(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

            Console.WriteLine("{0} created!", name);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        /// Uploads image to blob storage
        /// </summary>
        /// <param name="imagePath">Path to image to upload</param>
        /// <param name="imageName">Image name to use</param>
        private static void UploadImage(string imagePath, string imageName)
        {
            using (var fileStream = System.IO.File.OpenRead(imagePath))
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

                blockBlob.UploadFromStream(fileStream);
            }

            Console.WriteLine("{0} uploaded!", imageName);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        ///  Downloads image from blob storage
        /// </summary>
        /// <param name="copyImagePath">Path to save image</param>
        /// <param name="imageName">Name of the stored image</param>
        private static void DownLoadImage(string copyImagePath, string imageName)
        {
            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(copyImagePath))
            {
                blockBlob.DownloadToStream(fileStream);
            }

            Console.WriteLine("{0} downloaded to {1}!", imageName, copyImagePath);
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        /// Deletes created container
        /// </summary>
        private static void DeleteContainer()
        {
            container.Delete();

            Console.WriteLine("Container deleted!");
            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
