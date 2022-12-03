using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.Lambda.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServerlessDotnetAngularKinesis.Controllers
{
    public class FunctionKinesis
    {
        public string StreamNameData { get; set; } = "MyEventStream";
        public string PartitionKeyData { get; set; } = "AAA";

        public async Task<KinesisShared> PutRecords(EventData input)
        {
            var jsonRs = JsonConvert.SerializeObject(input);
            LambdaLogger.Log(jsonRs);
            byte[] byteArray = Encoding.ASCII.GetBytes(jsonRs);
            var stream = new MemoryStream(byteArray);

            using (var kinesisClient = new AmazonKinesisClient(Amazon.RegionEndpoint.APSoutheast1))
            {
                var rs = await kinesisClient.PutRecordAsync(new PutRecordRequest()
                {
                    StreamName = StreamNameData,
                    PartitionKey = PartitionKeyData,
                    Data = stream,
                });
                return new KinesisShared { ShardId = rs.ShardId, SequenceNumber = rs.SequenceNumber };
            }
        }

        public async Task<List<EventData>> GetAsyncRecord(List<KinesisShared> shr)
        {
            List<EventData> resutl = new List<EventData>();

            using (var kinesisClient = new AmazonKinesisClient(Amazon.RegionEndpoint.APSoutheast1))
            {
                foreach (var item in shr)
                {
                    var getShardIteratorRequest = new GetShardIteratorRequest
                    {
                        StreamName = StreamNameData,
                        ShardId = item.ShardId,
                        StartingSequenceNumber = item.SequenceNumber,
                        ShardIteratorType = ShardIteratorType.AT_SEQUENCE_NUMBER,
                        Timestamp = DateTime.Now.AddHours(-1)
                    };

                    var getShardIteratorResponse = await kinesisClient.GetShardIteratorAsync(getShardIteratorRequest);
                    var shardIterator = getShardIteratorResponse.ShardIterator;

                    var getRecordsRequest = new GetRecordsRequest
                    {
                        Limit = 100,
                        ShardIterator = shardIterator,
                    };

                    var getRecordsResponse = await kinesisClient.GetRecordsAsync(getRecordsRequest);
                    var nextIterator = getRecordsResponse.NextShardIterator;
                    var records = getRecordsResponse.Records;

                    if (records.Count > 0)
                    {
                        foreach (var record in records)
                        {
                            var json = Encoding.UTF8.GetString(record.Data.ToArray());
                            var jsonRs = JsonConvert.DeserializeObject<EventData>(json);
                            resutl.Add(jsonRs);
                        }
                    }
                }
            }

            return resutl.DistinctBy(x => x.ReceivedTime).ToList();
        }

        public async Task<List<EventData>> GetRecord(KinesisShared shr)
        {
            List<EventData> resutl = new List<EventData>();

            using (var kinesisClient = new AmazonKinesisClient(Amazon.RegionEndpoint.APSoutheast1))
            {

                var getShardIteratorRequest = new GetShardIteratorRequest
                {
                    StreamName = StreamNameData,
                    ShardId = shr.ShardId,
                    StartingSequenceNumber = shr.SequenceNumber,
                    ShardIteratorType = ShardIteratorType.AT_SEQUENCE_NUMBER,
                    Timestamp = DateTime.Now.AddHours(-1)
                };

                var getShardIteratorResponse = await kinesisClient.GetShardIteratorAsync(getShardIteratorRequest);
                var shardIterator = getShardIteratorResponse.ShardIterator;


                while (!string.IsNullOrEmpty(shardIterator))
                {
                    var getRecordsRequest = new GetRecordsRequest
                    {
                        Limit = 100,
                        ShardIterator = shardIterator,
                    };

                    var getRecordsResponse = await kinesisClient.GetRecordsAsync(getRecordsRequest);
                    var nextIterator = getRecordsResponse.NextShardIterator;
                    var records = getRecordsResponse.Records;

                    if (records.Count > 0)
                    {
                        foreach (var record in records)
                        {
                            var json = Encoding.UTF8.GetString(record.Data.ToArray());
                            var jsonRs = JsonConvert.DeserializeObject<EventData>(json);
                            resutl.Add(jsonRs);
                        }
                        return resutl;
                    }
                    shardIterator = nextIterator;
                }

            }

            return resutl;
        }

        public async Task<List<EventData>> GetRecords()
        {
            List<EventData> resutl= new List<EventData>();

            using (var kinesisClient = new AmazonKinesisClient(Amazon.RegionEndpoint.APSoutheast1))
            {

                var describeRequest = new DescribeStreamRequest
                {
                    StreamName = StreamNameData,
                };
                var describeResponse = await kinesisClient.DescribeStreamAsync(describeRequest);
                var shards = describeResponse.StreamDescription.Shards;

                foreach (var shard in shards)
                {
                    var getShardIteratorRequest = new GetShardIteratorRequest
                    {
                        StreamName = StreamNameData,
                        ShardId = shard.ShardId,
                        ShardIteratorType = ShardIteratorType.LATEST,
                        Timestamp = DateTime.Now.AddHours(-1)
                    };

                    var getShardIteratorResponse = await kinesisClient.GetShardIteratorAsync(getShardIteratorRequest);
                    var shardIterator = getShardIteratorResponse.ShardIterator;

                    while (!string.IsNullOrEmpty(shardIterator))
                    {
                        var getRecordsRequest = new GetRecordsRequest
                        {
                            Limit = 2,
                            ShardIterator = shardIterator,
                        };

                        var getRecordsResponse = await kinesisClient.GetRecordsAsync(getRecordsRequest);
                        var nextIterator = getRecordsResponse.NextShardIterator;
                        var records = getRecordsResponse.Records;

                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                var json = Encoding.UTF8.GetString(record.Data.ToArray());
                                var jsonRs = JsonConvert.DeserializeObject<EventData>(json);
                                resutl.Add(jsonRs);
                            }
                        }
                        shardIterator = nextIterator;
                    }
                }

            }

            return resutl;
        }

        public async Task<List<EventData>> GetKinesisRecords()
        {
            List<EventData> resutl = new List<EventData>();

            using (var kinesisClient = new AmazonKinesisClient(Amazon.RegionEndpoint.APSoutheast1))
            {

                var describeRequest = new DescribeStreamRequest
                {
                    StreamName = StreamNameData,
                };
                var describeResponse = await kinesisClient.DescribeStreamAsync(describeRequest);
                var shards = describeResponse.StreamDescription.Shards;

                foreach (var shard in shards)
                {
                    var getShardIteratorRequest = new GetShardIteratorRequest
                    {
                        StreamName = StreamNameData,
                        ShardId = shard.ShardId,
                        ShardIteratorType = ShardIteratorType.LATEST,
                        Timestamp = DateTime.Now.AddHours(-1)
                    };

                    var getShardIteratorResponse = await kinesisClient.GetShardIteratorAsync(getShardIteratorRequest);
                    var shardIterator = getShardIteratorResponse.ShardIterator;

                    var getRecordsRequest = new GetRecordsRequest
                    {
                        Limit = 2,
                        ShardIterator = shardIterator,
                    };

                    var getRecordsResponse = await kinesisClient.GetRecordsAsync(getRecordsRequest);
                    var records = getRecordsResponse.Records;

                    if (records.Count > 0)
                    {
                        foreach (var record in records)
                        {
                            var json = Encoding.UTF8.GetString(record.Data.ToArray());
                            var jsonRs = JsonConvert.DeserializeObject<EventData>(json);
                            resutl.Add(jsonRs);
                        }
                    }
                }

            }

            return resutl;
        }
    }
}
