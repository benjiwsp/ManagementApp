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
    public partial class MainWindow : Window
    {
        ArrayList uploadList = new ArrayList();
        private MySqlConnection myConnection;
        private MySqlCommand myCommand;
        static string bucketNames = "starxbucket";
        static string keyName = "";
        static string filePaths = "";
        private MySqlDataReader rdr;
        static IAmazonS3 client;
        public MainWindow()
        {
            InitializeComponent();
            listBoxFiles.AllowDrop = true;
            listBoxFiles.Drop += listBoxFiles_DragDrop;
            listBoxFiles.DragEnter += listBoxFiles_DragEnter;
            string name = ConfigurationManager.AppSettings["connectionString"];
            myConnection = new MySqlConnection(name);
        }

        public void clearUploadList()
        {
            listBoxFiles.Items.Clear();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            clearUploadList();
        }

        private void ListFil_Click(object sender, RoutedEventArgs e)
        {
            OpenConn();
            myCommand = new MySqlCommand("Select * from CustomerFiles", myConnection);
            rdr = myCommand.ExecuteReader();
            while (rdr.Read())
            {
                serverList.Items.Add(rdr["CustomerName"].ToString());
            }
            CloseConnn();
        }
        private void CloseConnn()
        {
            myConnection.Close();
        }
        private void OpenConn()
        {
            myConnection.Open();
        }


        /*
         * function to write an object to S3 bucket, reference from aws website.
         */
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

        /*
         * allow drag and drop function on listbox, drag the upload *folder* onto the list.
         */
        private void listBoxFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                listBoxFiles.Items.Add(file);
        }

        /*
         *  if item drag into the listbox, add onto the list.
         */
        private void listBoxFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }


        /*
         * Click button to upload all the files in the folders listed. Users must drag the whole folder onto the list
         * instead of the files itself. folder will be created on the s3bucket, and uploaded path will be store onto
         * aws RDS. 
         */
        private void UploadBtn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            string folderName = "";
            //       string fullPath = "";
            string s3FullPath = "";
            /*  using (client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1))
              {
                  WritingAnObject(bucketNames, keyName, filePaths);
              }*/
            OpenConn();
            foreach (string folder in listBoxFiles.Items)
            {
                foreach (string file in System.IO.Directory.GetFiles(folder))
                {
                    fileName = System.IO.Path.GetFileName(file);
                    folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(file));
                    keyName = folderName + "/" + fileName;
                    s3FullPath = file.Replace(@"\\", "/");
                    using (client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1))
                    {
                        //     MessageBox.Show(bucketNames + "," + keyName + "," + s3FullPath);
                        WritingAnObject(bucketNames, keyName, s3FullPath);
                        myCommand = new MySqlCommand("insert into CustomerFiles values ('','" + folderName + "','" + keyName + "','" + fileName + "')", myConnection);
                        myCommand.ExecuteNonQuery();
                    }
                }
            }
            clearUploadList();
            CloseConnn();
        }

        /*
         * Download all the files in the selected folder from aws RDS and save it locally.
         * 
         * Note: need to figure out a way to read all the files from the *request.key*, 
         * then set all the keys to the file namem; create a folder locally (check if it already exist).
         */
        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedFolder = "";
            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = "starxbucket";
            request.Key = "clara/18-AE-100297.pdf";
            using (client = new AmazonS3Client(Amazon.RegionEndpoint.APNortheast1))
            {
                GetObjectResponse response = client.GetObject(request);
           
         //       MessageBox.Show("!");

                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    selectedFolder = dialog.SelectedPath.ToString();
                    response.WriteResponseStreamToFile(selectedFolder + "/2.pdf");
                }


            }

        }
    }

}




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

