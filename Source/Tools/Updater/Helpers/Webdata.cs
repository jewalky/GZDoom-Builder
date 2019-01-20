using System;
using System.Net;
using System.IO;

namespace mxd.GZDBUpdater
{
    public delegate void BytesDownloadedEventHandler(ByteArgs e);

    public class ByteArgs : EventArgs
    {
	    public int Downloaded;
	    public int Total;
    }

    static class Webdata
    {
        public static event BytesDownloadedEventHandler BytesDownloaded;

        public static bool SaveWebFile(string url, string file, string targetFolder)
        {
            try
            {
	            MemoryStream memoryStream = DownloadWebFile(Path.Combine(url, file));
	            if(memoryStream == null) return false;

                //Convert the downloaded stream to a byte array
                byte[] downloadedData = memoryStream.ToArray();

                //Release resources
                memoryStream.Close();

                //Write bytes to the specified file
                FileStream newFile = new FileStream(targetFolder + file, FileMode.Create);
                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();

                return !MainForm.AppClosing;
            }
            catch(Exception e)
            {
                //We may not be connected to the internet
                //Or the URL may be incorrect
				MainForm.ErrorDescription = "Failed to download the update...\n" + e.Message;
                return false;
            }
        }

		public static MemoryStream DownloadWebFile(string url)
		{
			//open a data stream from the supplied URL
			WebRequest webReq = WebRequest.Create(url);
			WebResponse webResponse;
			try
			{
				webResponse = webReq.GetResponse();
			}
			catch(Exception e)
			{
				MainForm.ErrorDescription = "Failed to retrieve remote revision info...\n" + e.Message;
				return null;
			}
			Stream dataStream = webResponse.GetResponseStream();

			//Download the data in chuncks
			byte[] dataBuffer = new byte[1024];

			//Get the total size of the download
			int dataLength = (int)webResponse.ContentLength;

			//lets declare our downloaded bytes event args
			ByteArgs byteArgs = new ByteArgs();

			byteArgs.Downloaded = 0;
			byteArgs.Total = dataLength;

			//we need to test for a null as if an event is not consumed we will get an exception
			if(BytesDownloaded != null) BytesDownloaded(byteArgs);

			//Download the data
			MemoryStream memoryStream = new MemoryStream();
			while(!MainForm.AppClosing)
			{
				//Let's try and read the data
				int bytesFromStream = dataStream.Read(dataBuffer, 0, dataBuffer.Length);
				if(bytesFromStream == 0)
				{
					byteArgs.Downloaded = dataLength;
					byteArgs.Total = dataLength;
					if(BytesDownloaded != null) BytesDownloaded(byteArgs);

					//Download complete
					break;
				}
				else 
				{
					//Write the downloaded data
					memoryStream.Write(dataBuffer, 0, bytesFromStream);

					byteArgs.Downloaded += bytesFromStream;
					byteArgs.Total = dataLength;
					if(BytesDownloaded != null) BytesDownloaded(byteArgs);
				}
			}

			//Release resources
			dataStream.Close();

			// Rewind and return the stream
			memoryStream.Position = 0;
			return (MainForm.AppClosing ? null : memoryStream);
		}
    }
}
