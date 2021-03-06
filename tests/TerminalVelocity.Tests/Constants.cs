﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Illumina.TerminalVelocity.Tests
{
    public class Constants
    {
        public const string ONE_GIG_FILE_S_SL = @"https://1000genomes.s3.amazonaws.com/release/20110521/ALL.wgs.phase1_release_v3.20101123.snps_indels_sv.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425600785&Signature=KQ3qGSqFYN0z%2BHMTGLAGLUejtBw%3D";
        public const string ONE_GIG_FILE = @"http://1000genomes.s3.amazonaws.com/release/20110521/ALL.wgs.phase1_release_v3.20101123.snps_indels_sv.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425600785&Signature=KQ3qGSqFYN0z%2BHMTGLAGLUejtBw%3D";
        public const string ONE_GIG_REDIRECT = @"http://tinyurl.com/m3x2kz7";
        public const long ONE_GIG_FILE_LENGTH = 1297662912;
        public const string ONE_GIG_CHECKSUM = "24b9f9d41755b841eaf8d0faeab00a6c";//24b9f9d41755b841eaf8d0faeab00a6c
        public const string TWENTY_CHECKSUM = "11db70c5bd445c4b41de6cde9d655ee8";

        public const string TWENTY_MEG_FILE =
            @"https://1000genomes.s3.amazonaws.com/release/20100804/ALL.chrX.BI_Beagle.20100804.sites.vcf.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1425620139&Signature=h%2BIqHbo2%2Bjk0jIbR2qKpE3iS8ts%3D";

        public const string THIRTY_GIG_FILE = @"https://1000genomes.s3.amazonaws.com/data/HG02484/sequence_read/SRR404082_2.filt.fastq.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1362529020&Signature=l%2BS3sA1vkZeFqlZ7lD5HrQmY5is%3D";
        public const int TWENTY_MEG_FILE_LENGTH = 29996532;
        public const string FIVE_MEG_FILE = @"https://cloud-internal-test.s3.amazonaws.com/22ae3296a2e843ff9242a226011c93bf/UnitTestFile_110120?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1440635462&Signature=rUizUG0KuoCrqrU3%2BGOLtQlE%2BcU%3D";
        public const string FIVE_MEG_CHECKSUM = @"ec1a47cd2f40ba8f94ec2be5aef86aba";
        public const int FIVE_MEG_LENGTH = 5242880;

        public const string ZERO_BYTE_FILE =
            @"https://cloud-qa-performancetests.s3.amazonaws.com/zerobytes.txt?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1600561134&Signature=2X3uvjyYmChVwLD5su9boUExzIs%3D";

        public const string ZERO_BYTE_CHECKSUM = "d41d8cd98f00b204e9800998ecf8427e";

        public const string THIRTEEN_GIG_BAD_SAMPLE =
            @"https://cloud-hoth-data-east.s3.amazonaws.com/6c9527dc17b74899813fa22f00a0b64e/Sample-1/RZ-UHR_S1_L001_R2_001.fastq.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1475855036&Signature=5KZsAZKkDh%2BqIieCJCnEYaHHgZc%3D";

        public const long THIRTEEN_GIG_FILE_LENGTH = 14724364564;
        public const string THIRTEEN_GIG_CHECKSUM = "0e4c2cfb3c758a35fe914bb3be2e54a1";

        public const string FOURTEEN_GIG_FILE =
            @"https://cloud-hoth-data-east.s3.amazonaws.com/a3f7efe8c8054c8ca775a22f00b7d2e1/Sample-8/mRNA-Brian_S8_L008_R2_001.fastq.gz?AWSAccessKeyId=AKIAIYDIF27GS5AAXHQQ&Expires=1444261983&Signature=oPtorti3aB2wd0E6grtFkNK%2Bv0Y%3D";
        public const long FOURTEEN_GIG_FILE_LENGTH = 14724364564;
        public const string FOURTEEN_GIG_CHECKSUM = "0e4c2cfb3c758a35fe914bb3be2e54a1";
    }
}
