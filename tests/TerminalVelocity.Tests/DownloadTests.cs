﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApprovalTests.Reporters;
using ICSharpCode.SharpZipLib.GZip;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace Illumina.TerminalVelocity.Tests
{
    [UseReporter(typeof(VisualStudioReporter))]
    public class DownloadTests
    {
        public DownloadTests()
        {
            Debug.Listeners.Add(new DefaultTraceListener());
        }
        private string oneGigFileSSl = @"https://1000genomes.s3.amazonaws.com/release/20110521/ALL.wgs.phase1_release_v3.20101123.snps_indels_sv.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425600785&Signature=KQ3qGSqFYN0z%2BHMTGLAGLUejtBw%3D";
        private string oneGigFile = @"http://1000genomes.s3.amazonaws.com/release/20110521/ALL.wgs.phase1_release_v3.20101123.snps_indels_sv.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425600785&Signature=KQ3qGSqFYN0z%2BHMTGLAGLUejtBw%3D";
        private string oneGigChecksum = "290f8099861e8089cec020508a57d2b2";
        private string twentyChecksum = "11db70c5bd445c4b41de6cde9d655ee8";
        private string twentyMegFile =
            @"https://1000genomes.s3.amazonaws.com/release/20100804/ALL.chrX.BI_Beagle.20100804.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425620139&Signature=h%2BIqHbo2%2Bjk0jIbR2qKpE3iS8ts%3D";
        private string thirtyGigFile = @"https://1000genomes.s3.amazonaws.com/data/HG02484/sequence_read/SRR404082_2.filt.fastq.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1362529020&Signature=l%2BS3sA1vkZeFqlZ7lD5HrQmY5is%3D";

        [Fact]
        public void SimpleGetClientGetsFirst100Bytes()
        {
            var timer = new Stopwatch();
            timer.Start();
            var uri = new Uri(oneGigFile);
            var client = new SimpleHttpGetByRangeClient(uri);
            var response =client.Get(uri, 0, 100);
            timer.Stop();
            Debug.WriteLine(string.Format("total {0}ms or {1}secs", timer.ElapsedMilliseconds, timer.ElapsedMilliseconds/1000));
            Assert.NotNull(response);
            Assert.True(response.ContentLength == 100);
            Assert.True(response.ContentRangeLength == 1284396333);
            Assert.True(response.ContentRangeStart == 0);
            Assert.True(response.ContentRangeStop == 99);
            Assert.True(response.StatusCode == 206);
            Assert.NotNull(response.Content);
            Assert.True(response.ContentLength == response.Content.Length);
        }

        [Fact]
        public void ThrottleDownloadWhenQueueIsFull()
        {
            var parameters = new LargeFileDownloadParameters(new Uri(@"http://www.google.com"), "blah", 1000);
            var dict = new ConcurrentDictionary<int, byte[]>();
            var e = new AutoResetEvent(false);
            
            byte[] sampleResponse = Encoding.UTF8.GetBytes("hello world");
            var mockClient = new Mock<ISimpleHttpGetByRangeClient>();

            mockClient.Setup(x => x.Get(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                      .Returns(new SimpleHttpResponse(206, sampleResponse, null));
            int timesAskedForSlow = -1;
            int current = -1;
            Func<int> getNext = () =>
                                    {
                                        current++;
                                        if (current > 5)
                                            return -1;
                                        return current;
                                    };
            Func<int, bool> shouldSlw = i =>
                                            {
                                                timesAskedForSlow++;
                                                return true;
                                            };

            var task = LargeFileDownloadCommand.CreateDownloadTask(parameters, dict,e, getNext, shouldSlw, clientFactory: (x) => mockClient.Object );
            task.Start();
            task.Wait(2000);
            try
            {
                task.Dispose();
            }catch{}
            Assert.True(timesAskedForSlow > 1);
            Assert.True(getNext() == 2);
            Assert.True(Encoding.UTF8.GetString(dict[0]) == "hello world");

        }

        [Fact]
        public void CancellationTokenWillCancel()
        {
            var parameters = new LargeFileDownloadParameters(new Uri(@"http://www.google.com"), "blah", 1000);
            var dict = new ConcurrentDictionary<int, byte[]>();
            var e = new AutoResetEvent(false);

            byte[] sampleResponse = Encoding.UTF8.GetBytes("hello world");
            var mockClient = new Mock<ISimpleHttpGetByRangeClient>();

            mockClient.Setup(x => x.Get(It.IsAny<Uri>(), It.IsAny<long>(), It.IsAny<long>()))
                      .Returns(() =>
            {
                Thread.Sleep(200);
                return new SimpleHttpResponse(206, sampleResponse, null);
            });

            int timesAskedForSlow = -1;
            int current = -1;
            Func<int> getNext = () =>
            {
                current++;
                if (current > 10)
                    return -1;
                return current;
            };
            Func<int, bool> shouldSlw = i =>
            {
                timesAskedForSlow++;
                return false;
            };
            var tokenSource = new CancellationTokenSource();
            
            var task = LargeFileDownloadCommand.CreateDownloadTask(parameters, dict, e, getNext, shouldSlw, clientFactory: (x) => mockClient.Object, cancellation: tokenSource.Token);
            task.Start();
            Thread.Sleep(500);
            tokenSource.Cancel();
            task.Wait(TimeSpan.FromMinutes(2));
            Assert.True(current != 10); //we shouldn't get to 10 before the cancel works

        }

        [Fact]
        public void CalculateChunkCalcs()
        {
            long fileSize = 29996532;
            int maxChunk = LargeFileDownloadParameters.DEFAULT_MAX_CHUNK_SIZE;
            var chunkCount =LargeFileDownloadCommand.GetChunkCount(fileSize, maxChunk);
            Assert.True(chunkCount == 6);
            long totalBytes = 0;
            long lastChunkStart = 0;
            int lastChunkLength = maxChunk;
            for (int i = 0; i < chunkCount; i++)
            {
                var chunkStart = LargeFileDownloadCommand.GetChunkStart(i, maxChunk);
                var chunkLength = LargeFileDownloadCommand.GetChunkSizeForCurrentChunk(fileSize, maxChunk, i);
                Debug.WriteLine(string.Format("chunk {0} start {1} length {2}", i, chunkStart, chunkLength));
                totalBytes += chunkLength;
                lastChunkStart = chunkStart;
                lastChunkLength = chunkLength;
            }
            Assert.True(totalBytes == fileSize);
            Assert.True(lastChunkStart + lastChunkLength == fileSize);

        }

          [Theory, InlineData(1), InlineData(2),  InlineData(4),  InlineData(8)]
        public void ParallelChunkedDownload(int threadCount)
        {
          
            var uri = new Uri(twentyMegFile);
            var path = SafePath("sites_vcf.gz");
            Action<string> logger = ( message) => Trace.WriteLine(message);
            var timer = new Stopwatch();
              timer.Start();
            ILargeFileDownloadParameters parameters = new LargeFileDownloadParameters(uri, path, 29996532, maxThreads: threadCount);
           Task task = parameters.DownloadAsync(logger: logger);
            task.Wait(TimeSpan.FromMinutes(5));
            timer.Stop();
            Debug.WriteLine("Took {0} threads {1} ms", threadCount, timer.ElapsedMilliseconds);
            //try to open the file
            ValidateGZip(path, parameters.FileSize, twentyChecksum);
        }

          [Theory, InlineData(32)]
          public void ParallelChunkedOneGig(int threadCount)
          {

              var uri = new Uri(oneGigFileSSl);
              var path = SafePath("sites_vcf.gz");
              Action<string> logger = (message) => Trace.WriteLine(message);
              var timer = new Stopwatch();
              timer.Start();
              ILargeFileDownloadParameters parameters = new LargeFileDownloadParameters(uri, path, 1284396333, maxThreads: threadCount);
              Task task = parameters.DownloadAsync(logger: logger);
              task.Wait(TimeSpan.FromMinutes(5));
              timer.Stop();
              Debug.WriteLine("Took {0} threads {1} ms", threadCount, timer.ElapsedMilliseconds);
              //try to open the file
              ValidateGZip(path, parameters.FileSize, oneGigChecksum);
          }

        private static void ValidateGZip(string path, long fileSize, string checksum)
        {
            using (Stream fs = File.OpenRead(path))
            {
                Assert.True(fs.Length == fileSize);
               string actualChecksum = Md5SumByProcess(path);
                Assert.Equal(checksum, actualChecksum);

            }
        }

        [Fact]
        public void ValidateSpeedOfWebRequest()
        {
            var uri = new Uri(twentyMegFile);
            var path = SafePath("sites_vcf.gz");
            Action<string> logger = (message) => Trace.WriteLine(message);
            var timer = new Stopwatch();
            timer.Start();
            var client = new WebClient();
            client.DownloadFile(uri, path);
            timer.Stop();
            Debug.WriteLine("Took {0} threads {1} ms", 1, timer.ElapsedMilliseconds);
            ValidateGZip(path, 29996532, twentyChecksum);
            
        }

        public static string Md5SumByProcess(string file)
        {
            var p = new Process();
            string md5Path = Path.Combine(new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.FullName,"lib","fciv.exe");
            p.StartInfo.FileName = md5Path;
            p.StartInfo.Arguments = string.Format(@"-add ""{0}""", file);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd().Replace(@"//
// File Checksum Integrity Verifier version 2.05.
//
", "");
            return output.Split(' ')[0];
        }

       

        [Fact]
        public void SimpleGetClientCanDownloadTwentyMegFileSynchronously()
        {
            var timer = new Stopwatch();
            var uri = new Uri(twentyMegFile);
            var client = new SimpleHttpGetByRangeClient(uri);
            var path = SafePath("sites_vcf.gz");
            timer.Start();

            using (FileStream output = File.Create(path))
            {
                const int chunksize = 1024*1000*2;
                var response = client.Get(uri, 0, chunksize);
                output.Write(response.Content, 0, (int)response.ContentLength);
              
                long currentFileSize = (int)response.ContentLength;
                long fileSize = response.ContentRangeLength.Value;
                while (currentFileSize < fileSize)
                {
                    SimpleHttpResponse loopResponse;
                    long left = fileSize - currentFileSize;
                    Debug.WriteLine("chunk start {0} length {1} ", currentFileSize, left < chunksize ? left : chunksize);
                    loopResponse = client.Get(new Uri(twentyMegFile), currentFileSize, left < chunksize ? left : chunksize);
                    output.Write(loopResponse.Content, 0, (int)loopResponse.ContentLength);
                    currentFileSize += loopResponse.ContentLength;
                }

            }
            
            timer.Stop();
            Debug.WriteLine("total {0}ms or {1}secs", timer.ElapsedMilliseconds, timer.ElapsedMilliseconds / 1000);
            
            using (Stream fs = File.OpenRead(path))
            {
                using (Stream gzipStream =  new GZipInputStream(fs) )
                {
                    using (var reader = new StreamReader(gzipStream))
                    {
                        reader.Read();
                    }
                }
            }
        }

        private static string SafePath(string fileName)
        {
            string path = Path.Combine(Environment.CurrentDirectory,fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return path;
        }
    }
}