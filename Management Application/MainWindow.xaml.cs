using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using MySql.Data.MySqlClient;
using System.Configuration;

using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
namespace Management_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArrayList uploadList = new ArrayList();
        private MySqlConnection myConnection;
        private MySqlCommand myCommand;
        static string bucketNames = "starxbucket";
        static string keyNames = "testfolder2/testing2.docx";
        static string filePaths = @"E:\documents\resignletter.docx";

        static IAmazonS3 client;
        public MainWindow()
        {
            InitializeComponent();
            listBoxFiles.AllowDrop = true;
            listBoxFiles.Drop += listBoxFiles_DragDrop;
            listBoxFiles.DragEnter += listBoxFiles_DragEnter;
            //     IAmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(RegionEndpoint.EUWest1);


            //       Console.WriteLine("Press any key to continue...");
            //     Console.ReadKey();

            //      WritingAnObject(bucketNames, keyNames, filePaths);


        }
        static void WritingAnObject(string bucketName, string keyName, string filePath)
        {
            try
            {
                PutObjectRequest putRequest1 = new PutObjectRequest
                  {
                      BucketName = bucketName,
                      Key = keyName,
                      ContentBody = "sample text"
                  };


                PutObjectResponse response1 = client.PutObject(putRequest1);


                // 2. Put object-set ContentType and add metadata.
                PutObjectRequest putRequest2 = new PutObjectRequest
                {

                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "text/plain"
                };
                putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");

                PutObjectResponse response2 = client.PutObject(putRequest2);

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                        "For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
            }
        }

        private void listBoxFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                listBoxFiles.Items.Add(file);
        }

        private void listBoxFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }


        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = ConfigurationManager.AppSettings["connectionString"];
            //    MessageBox.Show(name);
            string fileName = "";
            string folderName = "";

            string fullPath = "";
            foreach (string file in listBoxFiles.Items)
            {
                //  uploadList.Add(file);
                fileName = System.IO.Path.GetFileName(file);
                folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(file));
                keyNames = folderName + "/" + fileName;
                MessageBox.Show(file);
                fullPath = file.Replace(@"\\", "/");
                //     MessageBox.Show(file.Replace(@"\", "/"));
                using (client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1))
                //   using (client = new  AmazonS3Client(Amazon.S3.AmazonS3Client..APN1))
                {
                    WritingAnObject(bucketNames, keyNames, fullPath);
                    //         Console.WriteLine("Uploading an object");

                }

                //  MessageBox.Show(file + "           " + keyNames);
            }
            uploadList.Clear();

            MySqlDataReader rdr;
            myConnection = new MySqlConnection(name);
            myConnection.Open();

            myCommand = new MySqlCommand("insert into CustomerDetails values ('', 'starx','999','3');", myConnection);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
            /*
             * //壓縮  
            //己經有確定要壓縮的檔案
            FileStream sourceFile = File.OpenRead(@"C:\sample.xml");
            //壓縮後的檔案名稱
            FileStream destFile = File.Create(@"C:\sample.gz");
            //開始
            GZipStream compStream = new GZipStream(destFile, CompressionMode.Compress, true);
            try
            {
                int theByte = sourceFile.ReadByte();
                while (theByte != -1)
                {
                    compStream.WriteByte((byte)theByte);
                    theByte = sourceFile.ReadByte();
                }
            }
            finally
            {
                compStream.Flush();
                compStream.Dispose();
                sourceFile.Flush();
                sourceFile.Dispose();
                destFile.Flush();
                destFile.Dispose();
            }
          
        
            myCommand = new MySqlCommand ("Select id, Customer from CustomerDetails",myConnection);
            rdr = myCommand.ExecuteReader();
          
                while (rdr.Read())
                {
                    MessageBox.Show(rdr["id"].ToString(), rdr["Customer"].ToString());
                }
            
            rdr.Close();
            myConnection.Close();
            uploadList.Clear();
            listBoxFiles.Items.Clear();

            
               */

        }

        //解壓縮
        protected void Decompress()
        {
            //被壓縮後的檔案
            FileStream sourceFile = File.OpenRead(@"C:\sample.gz");
            //解壓縮後的檔案
            FileStream destFile = File.Create(@"C:\Unsample.xml");

            //開始
            GZipStream compStream = new GZipStream(sourceFile, CompressionMode.Decompress, true);
            try
            {
                int theByte = compStream.ReadByte();
                while (theByte != -1)
                {
                    destFile.WriteByte((byte)theByte);
                    theByte = compStream.ReadByte();
                }
            }
            finally
            {
                compStream.Flush();
                compStream.Dispose();
                sourceFile.Flush();
                sourceFile.Dispose();
                destFile.Flush();
                destFile.Dispose();
            }
        }


    }

}
